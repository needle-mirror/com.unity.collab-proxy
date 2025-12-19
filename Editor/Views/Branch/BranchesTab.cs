using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Codice.Client.Common.EventTracking;
using Codice.CM.Common;
using Codice.CM.Common.Mount;
using GluonGui;
using PlasticGui;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.CodeReview;
using PlasticGui.WorkspaceWindow.QueryViews;
using PlasticGui.WorkspaceWindow.QueryViews.Branches;
using PlasticGui.WorkspaceWindow.Update;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.Tool;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.UI.Tree;
using Unity.PlasticSCM.Editor.Views.Branches.Dialogs;
using Unity.PlasticSCM.Editor.Views.Diff;
using Unity.PlasticSCM.Editor.Views.Merge;
using Unity.PlasticSCM.Editor.Views.Properties;

using GluonIncomingChangesUpdater = PlasticGui.Gluon.WorkspaceWindow.IncomingChangesUpdater;
using IGluonUpdateReport = PlasticGui.Gluon.IUpdateReport;
#if !UNITY_6000_0_OR_NEWER
using SplitterState = Unity.PlasticSCM.Editor.UnityInternals.UnityEditor.SplitterState;
#endif

namespace Unity.PlasticSCM.Editor.Views.Branches
{
    internal partial class BranchesTab :
        IRefreshableView,
        IQueryRefreshableView,
        IBranchMenuOperations,
        ILaunchCodeReviewWindow,
        IGetQueryText,
        IGetFilter,
        FillBranchesView.IShowContentView,
        FillBranchesView.IShowHiddenBranchesButton
    {
        internal string EmptyStateMessage { get { return mEmptyStatePanel.Text; } }
        internal bool ShowHiddenBranchesForTesting { set { mShowHiddenBranches = value; } }
        internal DateFilter DateFilterForTesting { set { mDateFilter = value; } }
        internal bool IsChangesetByChangesetModeForTesting
        {
            get { return mIsChangesetByChangesetMode; }
            set { mIsChangesetByChangesetMode = value; }
        }
        internal IBranchMenuOperations Operations { get { return this; } }
        internal BranchesListView Table { get { return mBranchesListView; } }
        internal DiffPanel DiffPanel { get { return mDiffPanel; } }

        internal BranchesTab(
            WorkspaceInfo wkInfo,
            ViewHost viewHost,
            WorkspaceWindow workspaceWindow,
            IViewSwitcher viewSwitcher,
            IMergeViewLauncher mergeViewLauncher,
            IHistoryViewLauncher historyViewLauncher,
            IUpdateReport updateReport,
            IGluonUpdateReport gluonUpdateReport,
            IShelvedChangesUpdater shelvedChangesUpdater,
            IAssetStatusCache assetStatusCache,
            ISaveAssets saveAssets,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            IPendingChangesUpdater pendingChangesUpdater,
            IncomingChangesUpdater developerIncomingChangesUpdater,
            GluonIncomingChangesUpdater gluonIncomingChangesUpdater,
            EditorWindow parentWindow,
            bool isGluonMode,
            bool showHiddenBranches)
        {
            mWkInfo = wkInfo;
            mViewHost = viewHost;
            mWorkspaceWindow = workspaceWindow;
            mGluonUpdateReport = gluonUpdateReport;
            mShelvedChangesUpdater = shelvedChangesUpdater;
            mAssetStatusCache = assetStatusCache;
            mSaveAssets = saveAssets;
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mProcessExecutor = processExecutor;
            mWorkspaceOperationsMonitor = workspaceOperationsMonitor;
            mPendingChangesUpdater = pendingChangesUpdater;
            mDeveloperIncomingChangesUpdater = developerIncomingChangesUpdater;
            mGluonIncomingChangesUpdater = gluonIncomingChangesUpdater;
            mParentWindow = parentWindow;
            mIsGluonMode = isGluonMode;
            mShowHiddenBranches = showHiddenBranches;

            mProgressControls = new ProgressControlsForViews();
            mShelvePendingChangesQuestionerBuilder =
                new ShelvePendingChangesQuestionerBuilder(parentWindow);
            mEnableSwitchAndShelveFeatureDialog = new EnableSwitchAndShelveFeature(
                PlasticGui.Plastic.API.GetRepositorySpec(mWkInfo),
                mParentWindow);
            mEmptyStatePanel = new EmptyStatePanel(parentWindow.Repaint);

            mFillBranchesView = new FillBranchesView(
                wkInfo,
                null,
                null,
                this,
                this,
                this,
                this);

            BuildComponents(
                wkInfo,
                workspaceWindow,
                workspaceWindow,
                viewSwitcher,
                historyViewLauncher,
                pendingChangesUpdater,
                developerIncomingChangesUpdater,
                gluonIncomingChangesUpdater,
                parentWindow,
                mFillBranchesView);

            mSplitterState = PlasticSplitterGUILayout.InitSplitterState(
                new float[] { 0.50f, 0.50f },
                new int[] { 100, (int)UnityConstants.BROWSE_REPOSITORY_PANEL_MIN_WIDTH },
                new int[] { 100000, 100000 }
            );

            mBranchOperations = new BranchOperations(
                wkInfo,
                workspaceWindow,
                mergeViewLauncher,
                this,
                ViewType.BranchesView,
                mProgressControls,
                updateReport,
                new ApplyShelveReport(parentWindow),
                new ContinueWithPendingChangesQuestionerBuilder(viewSwitcher, parentWindow),
                mShelvePendingChangesQuestionerBuilder,
                new ApplyShelveWithConflictsQuestionerBuilder(),
                pendingChangesUpdater,
                developerIncomingChangesUpdater,
                shelvedChangesUpdater,
                mEnableSwitchAndShelveFeatureDialog);

            ((IRefreshableView)this).Refresh();
        }

        internal void OnEnable()
        {
            mDiffPanel.OnEnable();
            mChangesetByChangesetDiffPanel.OnEnable();

            mSearchField.downOrUpArrowKeyPressed +=
                SearchField_OnDownOrUpArrowKeyPressed;
        }

        internal void OnDisable()
        {
            mDiffPanel.OnDisable();
            mChangesetByChangesetDiffPanel.OnDisable();

            mSearchField.downOrUpArrowKeyPressed -=
                SearchField_OnDownOrUpArrowKeyPressed;

            TreeHeaderSettings.Save(
                mBranchesListView.multiColumnHeader.state,
                UnityConstants.BRANCHES_TABLE_SETTINGS_NAME);
        }

        internal void Update()
        {
            mDiffPanel.Update();
            mChangesetByChangesetDiffPanel.Update();

            mProgressControls.UpdateProgress(mParentWindow);
        }

        internal SerializableBranchesTabState GetSerializableState()
        {
            return new SerializableBranchesTabState(mShowHiddenBranches);
        }

        internal void OnGUI()
        {
            PlasticSplitterGUILayout.BeginHorizontalSplit(mSplitterState);

            DoBranchesArea(
                mBranchesListView,
                mEmptyStatePanel,
                mDateFilter,
                ref mShowHiddenBranches,
                mSearchField,
                mProgressControls,
                this);

            EditorGUILayout.BeginHorizontal();

            Rect border = GUILayoutUtility.GetRect(1, 0, 1, 100000);
            EditorGUI.DrawRect(border, UnityStyles.Colors.BarBorder);

            DoChangesArea();

            EditorGUILayout.EndHorizontal();

            PlasticSplitterGUILayout.EndHorizontalSplit();
        }

        internal void SetWorkingObjectInfo(BranchInfo branchInfo)
        {
            mFillBranchesView.UpdateWorkingObject(branchInfo, mWkInfo);
        }

        internal void SetLaunchToolForTesting(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor)
        {
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mProcessExecutor = processExecutor;
        }

        void IRefreshableView.Refresh()
        {
            // VCS-1005209 - There are scenarios where the list of branches need to check for incoming changes.
            // For example, deleting the active branch will automatically switch your workspace to the parent changeset,
            // which might have incoming changes.
            if (mDeveloperIncomingChangesUpdater != null)
                mDeveloperIncomingChangesUpdater.Update(DateTime.Now);

            if (mGluonIncomingChangesUpdater != null)
                mGluonIncomingChangesUpdater.Update(DateTime.Now);

            RefreshAndSelect(null);
        }

        //IQueryRefreshableView
        public void RefreshAndSelect(RepObjectInfo objectToSelect)
        {
            List<IPlasticTreeNode> branchesToSelect = objectToSelect == null ?
                null : new List<IPlasticTreeNode> { new BranchTreeNode(
                    null, (BranchInfo)objectToSelect, null) };

            mDiffPanel.ClearInfo();
            mChangesetByChangesetDiffPanel.ClearInfo();

            mFillBranchesView.FillView(
                mBranchesListView,
                mProgressControls,
                null,
                null,
                null,
                branchesToSelect,
                FillBranchesView.ViewMode.List,
                mShowHiddenBranches);
        }

        BranchInfo IBranchMenuOperations.GetSelectedBranch()
        {
            return BranchesSelection.GetSelectedBranch(mBranchesListView);
        }

        int IBranchMenuOperations.GetSelectedBranchesCount()
        {
            return BranchesSelection.GetSelectedBranchesCount(mBranchesListView);
        }

        bool IBranchMenuOperations.AreHiddenBranchesShown()
        {
            return mShowHiddenBranches;
        }

        void IBranchMenuOperations.CreateBranch()
        {
            CreateBranchForMode();
        }

        void IBranchMenuOperations.CreateTopLevelBranch() { }

        void IBranchMenuOperations.SwitchToBranch()
        {
            SwitchToBranchForMode();
        }

        void IBranchMenuOperations.MergeBranch()
        {
            mBranchOperations.MergeBranch(
                BranchesSelection.GetSelectedRepository(mBranchesListView),
                BranchesSelection.GetSelectedBranch(mBranchesListView));
        }

        void IBranchMenuOperations.CherrypickBranch() { }

        void IBranchMenuOperations.MergeToBranch() { }

        void IBranchMenuOperations.PullBranch() { }

        void IBranchMenuOperations.PullRemoteBranch() { }

        void IBranchMenuOperations.SyncWithGit() { }

        void IBranchMenuOperations.PushBranch() { }

        void IBranchMenuOperations.DiffBranch()
        {
            LaunchDiffOperations.DiffBranch(
                mShowDownloadPlasticExeWindow,
                mProcessExecutor,
                BranchesSelection.GetSelectedRepository(mBranchesListView),
                BranchesSelection.GetSelectedBranch(mBranchesListView),
                mIsGluonMode);
        }

        void IBranchMenuOperations.DiffWithAnotherBranch() { }

        void IBranchMenuOperations.ViewChangesets() { }

        void IBranchMenuOperations.RenameBranch()
        {
            RepositorySpec repSpec = BranchesSelection.GetSelectedRepository(mBranchesListView);
            BranchInfo branchInfo = BranchesSelection.GetSelectedBranch(mBranchesListView);

            BranchRenameData branchRenameData = RenameBranchDialog.GetBranchRenameData(
                repSpec,
                branchInfo,
                mParentWindow);

            mBranchOperations.RenameBranch(branchRenameData);
        }

        void IBranchMenuOperations.HideUnhideBranch()
        {
            if (mShowHiddenBranches)
            {
                mBranchOperations.UnhideBranch(
                    BranchesSelection.GetSelectedRepositories(mBranchesListView),
                    BranchesSelection.GetSelectedBranches(mBranchesListView));
                return;
            }

            mBranchOperations.HideBranch(
                BranchesSelection.GetSelectedRepositories(mBranchesListView),
                BranchesSelection.GetSelectedBranches(mBranchesListView));
        }

        void IBranchMenuOperations.DeleteBranch()
        {
            var branchesToDelete = BranchesSelection.GetSelectedBranches(mBranchesListView);

            if (!DeleteBranchDialog.ConfirmDelete(mParentWindow, branchesToDelete))
                return;

            mBranchOperations.DeleteBranch(
                BranchesSelection.GetSelectedRepositories(mBranchesListView),
                branchesToDelete,
                DeleteBranchOptions.IncludeChangesets,
                !mShowHiddenBranches);
        }

        void IBranchMenuOperations.CreateCodeReview()
        {
            RepositorySpec repSpec = BranchesSelection.GetSelectedRepository(mBranchesListView);
            BranchInfo branchInfo = BranchesSelection.GetSelectedBranch(mBranchesListView);

            NewCodeReviewBehavior choice = SelectNewCodeReviewBehavior.For(repSpec.Server);

            switch (choice)
            {
                case NewCodeReviewBehavior.CreateAndOpenInDesktop:
                    mBranchOperations.CreateCodeReview(repSpec, branchInfo, this);
                    break;
                case NewCodeReviewBehavior.RequestFromUnityCloud:
                    OpenRequestReviewPage.ForBranch(repSpec, branchInfo.BranchId);
                    break;
                case NewCodeReviewBehavior.Ask:
                default:
                    break;
            }
        }

        void ILaunchCodeReviewWindow.Show(
            WorkspaceInfo wkInfo,
            RepositorySpec repSpec,
            ReviewInfo reviewInfo,
            RepObjectInfo repObjectInfo,
            bool bShowReviewChangesTab)
        {
            LaunchTool.OpenCodeReview(
                mShowDownloadPlasticExeWindow,
                mProcessExecutor,
                repSpec,
                reviewInfo.Id,
                mIsGluonMode);
        }

        void IBranchMenuOperations.ViewPermissions() { }

        string IGetQueryText.Get()
        {
            return QueryConstants.BuildBranchesQuery(
                mDateFilter.GetLayoutFilter(), mShowHiddenBranches);
        }

        Filter IGetFilter.Get()
        {
            return new Filter(mBranchesListView.searchString);
        }

        void IGetFilter.Clear()
        {
            // Not used by the Plugin, needed for the Reset filters button
        }

        void FillBranchesView.IShowContentView.ShowContentPanel()
        {
            mEmptyStatePanel.UpdateContent(string.Empty);
        }

        void FillBranchesView.IShowContentView.ShowEmptyStatePanel(
            string explanationText, bool showResetFilterButton)
        {
            mEmptyStatePanel.UpdateContent(explanationText);
        }

        bool FillBranchesView.IShowHiddenBranchesButton.IsChecked
        {
            get { return mShowHiddenBranches; }
            set { mShowHiddenBranches = value; }
        }

        void SearchField_OnDownOrUpArrowKeyPressed()
        {
            mBranchesListView.SetFocusAndEnsureSelectedItem();
        }

        void OnSelectionChanged()
        {
            List<RepObjectInfo> selectedBranches = BranchesSelection.
                GetSelectedRepObjectInfos(mBranchesListView);

            if (selectedBranches.Count != 1)
                return;

            RepositorySpec repSpec = BranchesSelection.GetSelectedRepository(mBranchesListView);
            MountPointWithPath mountPoint = MountPointWithPath.BuildWorkspaceRootMountPoint(repSpec);

            if (mIsChangesetByChangesetMode)
                mChangesetByChangesetDiffPanel.UpdateInfo(mountPoint, selectedBranches[0]);
            else
            {
                mPropertiesPanel.UpdateInfo(selectedBranches[0], repSpec);
                mDiffPanel.UpdateInfo(mountPoint, selectedBranches[0]);
            }
        }

        void DoDiffModeButtons()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.FlexibleSpace();

            GUI.enabled = !mProgressControls.IsOperationRunning();

            bool wasChangesetByChangesetMode = mIsChangesetByChangesetMode;

            if (GUILayout.Toggle(
                !mIsChangesetByChangesetMode,
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.DiffEntireBranch),
                EditorStyles.toolbarButton))
            {
                mIsChangesetByChangesetMode = false;
            }

            if (GUILayout.Toggle(
                mIsChangesetByChangesetMode,
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.DiffByChangeset),
                EditorStyles.toolbarButton))
            {
                mIsChangesetByChangesetMode = true;
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            // If mode changed, update the panel with the currently selected branch
            if (wasChangesetByChangesetMode != mIsChangesetByChangesetMode)
            {
                List<RepObjectInfo> selectedBranches = BranchesSelection.
                    GetSelectedRepObjectInfos(mBranchesListView);

                if (selectedBranches.Count == 1)
                {
                    RepositorySpec repSpec = BranchesSelection.GetSelectedRepository(mBranchesListView);
                    MountPointWithPath mountPoint = MountPointWithPath.BuildWorkspaceRootMountPoint(repSpec);

                    if (mIsChangesetByChangesetMode)
                        mChangesetByChangesetDiffPanel.UpdateInfo(mountPoint, selectedBranches[0]);
                    else
                    {
                        mPropertiesPanel.UpdateInfo(selectedBranches[0], repSpec);
                        mDiffPanel.UpdateInfo(mountPoint, selectedBranches[0]);
                    }
                }
            }
        }

        static void DoBranchesArea(
            BranchesListView branchesListView,
            EmptyStatePanel emptyStatePanel,
            DateFilter dateFilter,
            ref bool showHiddenBranchesButton,
            SearchField searchField,
            ProgressControlsForViews progressControls,
            IRefreshableView view)
        {
            EditorGUILayout.BeginVertical();

            DoActionsToolbar(
                progressControls,
                dateFilter,
                ref showHiddenBranchesButton,
                searchField,
                branchesListView,
                view);

            Rect viewRect = OverlayProgress.CaptureViewRectangle();

            GUI.enabled = !progressControls.IsOperationRunning();

            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            branchesListView.OnGUI(rect);

            if (!emptyStatePanel.IsEmpty())
                emptyStatePanel.OnGUI(rect);

            GUI.enabled = true;

            EditorGUILayout.EndVertical();

            if (progressControls.IsOperationRunning())
            {
                OverlayProgress.DoOverlayProgress(
                    viewRect,
                    progressControls.ProgressData.ProgressPercent,
                    progressControls.ProgressData.ProgressMessage);
            }
        }

        static void DoActionsToolbar(
            ProgressControlsForViews progressControls,
            DateFilter dateFilter,
            ref bool showHiddenBranchesButton,
            SearchField searchField,
            BranchesListView branchesListView,
            IRefreshableView view)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(
                    new GUIContent(Images.GetRefreshIcon(),
                        PlasticLocalization.Name.RefreshButton.GetString()),
                    UnityStyles.ToolbarButtonLeft,
                    GUILayout.Width(UnityConstants.TOOLBAR_ICON_BUTTON_WIDTH)))
            {
                view.Refresh();
            }

            DrawDateFilter(progressControls, dateFilter, view);

            GUILayout.FlexibleSpace();

            DrawShowHiddenBranchesButton(
                progressControls, ref showHiddenBranchesButton, branchesListView, view);

            GUILayout.Space(2);

            DrawSearchField.For(
                searchField,
                branchesListView,
                UnityConstants.SEARCH_FIELD_WIDTH);

            EditorGUILayout.EndHorizontal();
        }

        static void DrawDateFilter(
            ProgressControlsForViews progressControls,
            DateFilter dateFilter,
            IRefreshableView view)
        {
            GUI.enabled = !progressControls.IsOperationRunning();

            EditorGUI.BeginChangeCheck();

            dateFilter.FilterType = (DateFilter.Type)
                EditorGUILayout.EnumPopup(
                    dateFilter.FilterType,
                    EditorStyles.toolbarDropDown,
                    GUILayout.Width(100));

            if (EditorGUI.EndChangeCheck())
            {
                EnumPopupSetting<DateFilter.Type>.Save(
                    dateFilter.FilterType,
                    UnityConstants.BRANCHES_DATE_FILTER_SETTING_NAME);

                view.Refresh();
            }

            GUI.enabled = true;
        }

        static void DrawShowHiddenBranchesButton(
            ProgressControlsForViews progressControls,
            ref bool showHiddenBranches,
            BranchesListView branchesListView,
            IRefreshableView view)
        {
            GUI.enabled = !progressControls.IsOperationRunning();

            EditorGUI.BeginChangeCheck();

            showHiddenBranches = GUILayout.Toggle(
                showHiddenBranches,
                new GUIContent(
                    showHiddenBranches ?
                        Images.GetUnhideIcon() :
                        Images.GetHideIcon(),
                    showHiddenBranches ?
                        PlasticLocalization.Name.DontShowHiddenBranchesTooltip.GetString() :
                        PlasticLocalization.Name.ShowHiddenBranchesTooltip.GetString()),
                EditorStyles.toolbarButton,
                GUILayout.Width(UnityConstants.TOOLBAR_ICON_BUTTON_WIDTH));

            if (EditorGUI.EndChangeCheck())
            {
                TrackFeatureUseEvent.For(
                    BranchesSelection.GetSelectedRepository(branchesListView),
                    TrackFeatureUseEvent.Features.Branches.ToggleShowHiddenBranches);

                view.Refresh();
            }

            GUI.enabled = true;
        }

        void DoChangesArea()
        {
            EditorGUILayout.BeginVertical();

            DoDiffModeButtons();

            if (mIsChangesetByChangesetMode)
                mChangesetByChangesetDiffPanel.OnGUI();
            else
            {
                mPropertiesPanel.OnGUI();
                Rect separatorRect = GUILayoutUtility.GetRect(
                    0,
                    1,
                    GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(separatorRect, UnityStyles.Colors.BarBorder);
                mDiffPanel.OnGUI();
            }

            EditorGUILayout.EndVertical();
        }

        void BuildComponents(
            WorkspaceInfo wkInfo,
            IWorkspaceWindow workspaceWindow,
            IRefreshView refreshView,
            IViewSwitcher viewSwitcher,
            IHistoryViewLauncher historyViewLauncher,
            IPendingChangesUpdater pendingChangesUpdater,
            IncomingChangesUpdater developerIncomingChangesUpdater,
            GluonIncomingChangesUpdater gluonIncomingChangesUpdater,
            EditorWindow parentWindow,
            FillBranchesView fillBranchesView)
        {
            mSearchField = new SearchField();
            mSearchField.downOrUpArrowKeyPressed += SearchField_OnDownOrUpArrowKeyPressed;

            DateFilter.Type dateFilterType =
                EnumPopupSetting<DateFilter.Type>.Load(
                    UnityConstants.BRANCHES_DATE_FILTER_SETTING_NAME,
                    DateFilter.Type.LastMonth);
            mDateFilter = new DateFilter(dateFilterType);

            BranchesListHeaderState headerState =
                BranchesListHeaderState.GetDefault();

            TreeHeaderSettings.Load(headerState,
                UnityConstants.BRANCHES_TABLE_SETTINGS_NAME,
                (int)BranchesListColumn.CreationDate, false);

            mBranchesListView = new BranchesListView(
                headerState,
                BranchesListHeaderState.GetColumnNames(),
                new BranchesViewMenu(this, mGluonIncomingChangesUpdater != null),
                fillBranchesView,
                fillBranchesView,
                () => mShowHiddenBranches,
                selectionChangedAction: OnSelectionChanged,
                doubleClickAction: ((IBranchMenuOperations)this).DiffBranch,
                afterItemsChangedAction: fillBranchesView.ShowContentOrEmptyState);
            mBranchesListView.Reload();

            mPropertiesPanel = new PropertiesPanel(mParentWindow.Repaint);

            mDiffPanel = new DiffPanel(
                wkInfo,
                workspaceWindow,
                viewSwitcher,
                historyViewLauncher,
                refreshView,
                mAssetStatusCache,
                mShowDownloadPlasticExeWindow,
                pendingChangesUpdater,
                developerIncomingChangesUpdater,
                gluonIncomingChangesUpdater,
                parentWindow,
                mIsGluonMode);

            mChangesetByChangesetDiffPanel = new ChangesetByChangesetDiffPanel(
                wkInfo,
                workspaceWindow,
                viewSwitcher,
                historyViewLauncher,
                refreshView,
                mAssetStatusCache,
                mShowDownloadPlasticExeWindow,
                pendingChangesUpdater,
                developerIncomingChangesUpdater,
                gluonIncomingChangesUpdater,
                parentWindow,
                mIsGluonMode);
        }
        bool mShowHiddenBranches;

        DateFilter mDateFilter;
        SearchField mSearchField;
        BranchesListView mBranchesListView;
        DiffPanel mDiffPanel;
        PropertiesPanel mPropertiesPanel;
        ChangesetByChangesetDiffPanel mChangesetByChangesetDiffPanel;
        bool mIsChangesetByChangesetMode = false;

        LaunchTool.IProcessExecutor mProcessExecutor;
        LaunchTool.IShowDownloadPlasticExeWindow mShowDownloadPlasticExeWindow;

        readonly SplitterState mSplitterState;
        readonly BranchOperations mBranchOperations;
        readonly FillBranchesView mFillBranchesView;
        readonly EmptyStatePanel mEmptyStatePanel;
        readonly SwitchAndShelve.IEnableSwitchAndShelveFeatureDialog mEnableSwitchAndShelveFeatureDialog;
        readonly IShelvePendingChangesQuestionerBuilder mShelvePendingChangesQuestionerBuilder;
        readonly ProgressControlsForViews mProgressControls;
        readonly bool mIsGluonMode;
        readonly EditorWindow mParentWindow;
        readonly IPendingChangesUpdater mPendingChangesUpdater;
        readonly GluonIncomingChangesUpdater mGluonIncomingChangesUpdater;
        readonly IncomingChangesUpdater mDeveloperIncomingChangesUpdater;
        readonly WorkspaceOperationsMonitor mWorkspaceOperationsMonitor;
        readonly ISaveAssets mSaveAssets;
        readonly IAssetStatusCache mAssetStatusCache;
        readonly IShelvedChangesUpdater mShelvedChangesUpdater;
        readonly IGluonUpdateReport mGluonUpdateReport;
        readonly WorkspaceWindow mWorkspaceWindow;
        readonly ViewHost mViewHost;
        readonly WorkspaceInfo mWkInfo;
    }
}
