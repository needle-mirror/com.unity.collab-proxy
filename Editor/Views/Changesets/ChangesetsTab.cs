using System.Collections.Generic;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Codice.Client.Common;
using Codice.CM.Common;
using Codice.CM.Common.Mount;
using PlasticGui;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.Update;
using PlasticGui.WorkspaceWindow.QueryViews;
using PlasticGui.WorkspaceWindow.QueryViews.Changesets;
using GluonGui;
using PlasticGui.WorkspaceWindow.CodeReview;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;
using Unity.PlasticSCM.Editor.Tool;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.UI.Tree;
using Unity.PlasticSCM.Editor.Views.Changesets.Dialogs;
using Unity.PlasticSCM.Editor.Views.Diff;
using Unity.PlasticSCM.Editor.Views.Merge;
using Unity.PlasticSCM.Editor.Views.Properties;

using GluonIncomingChangesUpdater = PlasticGui.Gluon.WorkspaceWindow.IncomingChangesUpdater;
using IGluonUpdateReport = PlasticGui.Gluon.IUpdateReport;
#if !UNITY_6000_0_OR_NEWER
using SplitterState = Unity.PlasticSCM.Editor.UnityInternals.UnityEditor.SplitterState;
#endif

namespace Unity.PlasticSCM.Editor.Views.Changesets
{
    internal partial class ChangesetsTab :
        IRefreshableView,
        IChangesetMenuOperations,
        ChangesetsViewMenu.IMenuOperations,
        ILaunchCodeReviewWindow,
        IGetQueryText,
        IGetFilterText,
        FillChangesetsView.IShowContentView
    {
        internal DateFilter DateFilterForTesting { set { mDateFilter = value; } }
        internal ChangesetsListView Table { get { return mChangesetsListView; } }
        internal IChangesetMenuOperations Operations { get { return this; } }
        internal string EmptyStateMessage { get { return mEmptyStatePanel.Text; } }
        internal DiffPanel DiffPanel { get { return mDiffPanel; } }

        internal interface IRevertToChangesetListener
        {
            void OnSuccessOperation();
        }

        internal ChangesetsTab(
            WorkspaceInfo wkInfo,
            ChangesetInfo changesetToSelect,
            ViewHost viewHost,
            WorkspaceWindow workspaceWindow,
            IViewSwitcher viewSwitcher,
            IMergeViewLauncher mergeViewLauncher,
            IHistoryViewLauncher historyViewLauncher,
            IUpdateReport updateReport,
            IGluonUpdateReport gluonUpdateReport,
            IShelvedChangesUpdater shelvedChangesUpdater,
            IRevertToChangesetListener revertToChangesetListener,
            IAssetStatusCache assetStatusCache,
            ISaveAssets saveAssets,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            IPendingChangesUpdater pendingChangesUpdater,
            IncomingChangesUpdater developerIncomingChangesUpdater,
            GluonIncomingChangesUpdater gluonIncomingChangesUpdater,
            EditorWindow parentWindow,
            bool isGluonMode)
        {
            mWkInfo = wkInfo;
            mViewHost = viewHost;
            mWorkspaceWindow = workspaceWindow;
            mViewSwitcher = viewSwitcher;
            mGluonUpdateReport = gluonUpdateReport;
            mShelvedChangesUpdater = shelvedChangesUpdater;
            mRevertToChangesetListener = revertToChangesetListener;
            mAssetStatusCache = assetStatusCache;
            mSaveAssets = saveAssets;
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mProcessExecutor = processExecutor;
            mWorkspaceOperationsMonitor = workspaceOperationsMonitor;
            mPendingChangesUpdater = pendingChangesUpdater;
            mGluonIncomingChangesUpdater = gluonIncomingChangesUpdater;
            mParentWindow = parentWindow;
            mIsGluonMode = isGluonMode;

            mProgressControls = new ProgressControlsForViews();
            mShelvePendingChangesQuestionerBuilder = new ShelvePendingChangesQuestionerBuilder(parentWindow);
            mEnableSwitchAndShelveFeatureDialog = new EnableSwitchAndShelveFeature(
                PlasticGui.Plastic.API.GetRepositorySpec(mWkInfo),
                mParentWindow);
            mEmptyStatePanel = new EmptyStatePanel(parentWindow.Repaint);

            mFillChangesetsView = new FillChangesetsView(
                wkInfo,
                null,
                null,
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
                mFillChangesetsView);

            mSplitterState = PlasticSplitterGUILayout.InitSplitterState(
                new float[] { 0.50f, 0.50f },
                new int[] { 100, (int)UnityConstants.DIFF_PANEL_MIN_WIDTH },
                new int[] { 100000, 100000 }
            );

            mChangesetOperations = new ChangesetOperations(
                wkInfo,
                workspaceWindow,
                viewSwitcher,
                mergeViewLauncher,
                mProgressControls,
                updateReport,
                new ApplyShelveReport(parentWindow),
                new ContinueWithPendingChangesQuestionerBuilder(viewSwitcher, parentWindow),
                mShelvePendingChangesQuestionerBuilder,
                new ApplyShelveWithConflictsQuestionerBuilder(),
                pendingChangesUpdater,
                developerIncomingChangesUpdater,
                shelvedChangesUpdater,
                null,
                null,
                mEnableSwitchAndShelveFeatureDialog);

            RefreshAndSelect(changesetToSelect);
        }

        internal void OnEnable()
        {
            mDiffPanel.OnEnable();

            mSearchField.downOrUpArrowKeyPressed +=
                SearchField_OnDownOrUpArrowKeyPressed;
        }

        internal void OnDisable()
        {
            mDiffPanel.OnDisable();

            mSearchField.downOrUpArrowKeyPressed -=
                SearchField_OnDownOrUpArrowKeyPressed;

            TreeHeaderSettings.Save(
                mChangesetsListView.multiColumnHeader.state,
                UnityConstants.CHANGESETS_TABLE_SETTINGS_NAME);
        }

        internal void Update()
        {
            mDiffPanel.Update();

            mProgressControls.UpdateProgress(mParentWindow);
        }

        internal void OnGUI()
        {
            PlasticSplitterGUILayout.BeginHorizontalSplit(mSplitterState);

            DoChangesetsArea(
                mChangesetsListView,
                mEmptyStatePanel,
                mDateFilter,
                mSearchField,
                mProgressControls,
                this);

            EditorGUILayout.BeginHorizontal();

            Rect border = GUILayoutUtility.GetRect(1, 0, 1, 100000);
            EditorGUI.DrawRect(border, UnityStyles.Colors.BarBorder);

            DoChangesArea(
                mPropertiesPanel,
                mDiffPanel);

            EditorGUILayout.EndHorizontal();

            PlasticSplitterGUILayout.EndHorizontalSplit();

        }

        internal void SetWorkingObjectInfo(ChangesetInfo changesetInfo)
        {
            mFillChangesetsView.UpdateWorkingObject(changesetInfo);
        }

        internal void SetRevertToChangesetOperationInterfacesForTesting(
            RevertToChangesetOperation.IGetStatusForWorkspace getStatusForWorkspace,
            RevertToChangesetOperation.IUndoCheckoutOperation undoCheckoutOperation,
            RevertToChangesetOperation.IRevertToChangesetMergeController revertToChangesetMergeController)
        {
            mGetStatusForWorkspace = getStatusForWorkspace;
            mUndoCheckoutOperation = undoCheckoutOperation;
            mRevertToChangesetMergeController = revertToChangesetMergeController;
        }

        internal void SetLaunchToolForTesting(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor)
        {
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mProcessExecutor = processExecutor;
        }

        internal void RefreshAndSelect(RepObjectInfo repObj)
        {
            List<object> changesetsToSelect = repObj == null ?
                null : new List<object> { repObj };

            mDiffPanel.ClearInfo();

            mFillChangesetsView.FillView(
                mChangesetsListView,
                mProgressControls,
                null,
                null,
                null,
                changesetsToSelect);
        }

        void IRefreshableView.Refresh()
        {
            RefreshAndSelect(null);
        }

        int IChangesetMenuOperations.GetSelectedChangesetsCount()
        {
            return ChangesetsSelection.GetSelectedChangesetsCount(mChangesetsListView);
        }

        void IChangesetMenuOperations.DiffChangeset()
        {
            LaunchDiffOperations.DiffChangeset(
                mShowDownloadPlasticExeWindow,
                mProcessExecutor,
                ChangesetsSelection.GetSelectedRepository(mChangesetsListView),
                ChangesetsSelection.GetSelectedChangeset(mChangesetsListView),
                mIsGluonMode);
        }

        void IChangesetMenuOperations.DiffSelectedChangesets()
        {
            List<RepObjectInfo> selectedChangesets = ChangesetsSelection.
                GetSelectedRepObjectInfos(mChangesetsListView);

            if (selectedChangesets.Count < 2)
                return;

            LaunchDiffOperations.DiffSelectedChangesets(
                mShowDownloadPlasticExeWindow,
                mProcessExecutor,
                ChangesetsSelection.GetSelectedRepository(mChangesetsListView),
                (ChangesetExtendedInfo)selectedChangesets[0],
                (ChangesetExtendedInfo)selectedChangesets[1],
                mIsGluonMode);
        }

        void IChangesetMenuOperations.SwitchToChangeset()
        {
            SwitchToChangesetForMode();
        }

        void IChangesetMenuOperations.DiffWithAnotherChangeset() { }

        void IChangesetMenuOperations.CreateBranch()
        {
            CreateBranchForMode();
        }

        void IChangesetMenuOperations.LabelChangeset()
        {
            ChangesetLabelData changesetLabelData = LabelChangesetDialog.Label(
                mParentWindow,
                ChangesetsSelection.GetSelectedRepository(mChangesetsListView),
                ChangesetsSelection.GetSelectedChangeset(mChangesetsListView));

            mChangesetOperations.LabelChangeset(changesetLabelData);
        }

        void IChangesetMenuOperations.MergeChangeset()
        {
            mChangesetOperations.MergeChangeset(
                ChangesetsSelection.GetSelectedRepository(mChangesetsListView),
                ChangesetsSelection.GetSelectedChangeset(mChangesetsListView));
        }

        void IChangesetMenuOperations.CherryPickChangeset() { }

        void IChangesetMenuOperations.SubtractiveChangeset() { }

        void IChangesetMenuOperations.SubtractiveChangesetInterval() { }

        void IChangesetMenuOperations.CherryPickChangesetInterval() { }

        void IChangesetMenuOperations.MergeToChangeset() { }

        void IChangesetMenuOperations.MoveChangeset() { }

        void IChangesetMenuOperations.DeleteChangeset() { }

        void IChangesetMenuOperations.BrowseRepositoryOnChangeset() { }

        void IChangesetMenuOperations.CreateCodeReview()
        {
            RepositorySpec repSpec = ChangesetsSelection.GetSelectedRepository(mChangesetsListView);
            ChangesetInfo changesetInfo = ChangesetsSelection.GetSelectedChangeset(mChangesetsListView);

            NewCodeReviewBehavior choice = SelectNewCodeReviewBehavior.For(repSpec.Server);

            switch (choice)
            {
                case NewCodeReviewBehavior.CreateAndOpenInDesktop:
                    mChangesetOperations.CreateCodeReview(repSpec, changesetInfo, this);
                    break;
                case NewCodeReviewBehavior.RequestFromUnityCloud:
                    OpenRequestReviewPage.ForChangeset(repSpec, changesetInfo.ChangesetId);
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

        void IChangesetMenuOperations.RevertToChangeset()
        {
            if (((IChangesetMenuOperations)this).GetSelectedChangesetsCount() != 1)
                return;

            ChangesetExtendedInfo targetChangesetInfo = ((ChangesetsViewMenu.IMenuOperations)this).GetSelectedChangeset();

            RevertToChangesetOperation.RevertTo(
                mWkInfo,
                mViewSwitcher,
                mWorkspaceWindow,
                mProgressControls,
                mGetStatusForWorkspace,
                mUndoCheckoutOperation,
                mRevertToChangesetMergeController,
                GuiMessage.Get(),
                targetChangesetInfo,
                mPendingChangesUpdater,
                RefreshAsset.BeforeLongAssetOperation,
                () => RefreshAsset.AfterLongAssetOperation(mAssetStatusCache),
                mRevertToChangesetListener.OnSuccessOperation);
        }

        void ChangesetsViewMenu.IMenuOperations.DiffBranch()
        {
            LaunchDiffOperations.DiffBranch(
                mShowDownloadPlasticExeWindow,
                mProcessExecutor,
                ChangesetsSelection.GetSelectedRepository(mChangesetsListView),
                ChangesetsSelection.GetSelectedChangeset(mChangesetsListView),
                mIsGluonMode);
        }

        ChangesetExtendedInfo ChangesetsViewMenu.IMenuOperations.GetSelectedChangeset()
        {
            return ChangesetsSelection.GetSelectedChangeset(
                mChangesetsListView);
        }

        string IGetQueryText.Get()
        {
            return GetChangesetsQuery.For(mDateFilter);
        }

        string IGetFilterText.Get()
        {
            return mChangesetsListView.searchString;
        }

        void IGetFilterText.Clear()
        {
            // Not used by the Plugin, needed for the Reset filters button
        }

        void SearchField_OnDownOrUpArrowKeyPressed()
        {
            mChangesetsListView.SetFocusAndEnsureSelectedItem();
        }

        void FillChangesetsView.IShowContentView.ShowContentPanel()
        {
            mEmptyStatePanel.UpdateContent(string.Empty);
        }

        void FillChangesetsView.IShowContentView.ShowEmptyStatePanel(
            string explanationText, bool showResetFilterButton)
        {
            mEmptyStatePanel.UpdateContent(explanationText);
        }

        void OnSelectionChanged()
        {
            List<RepObjectInfo> selectedChangesets = ChangesetsSelection.
                GetSelectedRepObjectInfos(mChangesetsListView);

            if (selectedChangesets.Count != 1)
                return;

            RepositorySpec repSpec = ChangesetsSelection.GetSelectedRepository(
                mChangesetsListView);

            mPropertiesPanel.UpdateInfo(
                selectedChangesets[0], repSpec);

            mDiffPanel.UpdateInfo(
                MountPointWithPath.BuildWorkspaceRootMountPoint(repSpec),
                selectedChangesets[0]);
        }

        static void DoActionsToolbar(
            ProgressControlsForViews progressControls,
            DateFilter dateFilter,
            SearchField searchField,
            ChangesetsListView changesetsListView,
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

            DrawSearchField.For(
                searchField,
                changesetsListView,
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
                    UnityConstants.CHANGESETS_DATE_FILTER_SETTING_NAME);

                view.Refresh();
            }

            GUI.enabled = true;
        }

        static void DoChangesetsArea(
            ChangesetsListView changesetsListView,
            EmptyStatePanel emptyStatePanel,
            DateFilter dateFilter,
            SearchField searchField,
            ProgressControlsForViews progressControls,
            IRefreshableView view)
        {
            EditorGUILayout.BeginVertical();

            DoActionsToolbar(
                progressControls,
                dateFilter,
                searchField,
                changesetsListView,
                view);

            Rect viewRect = OverlayProgress.CaptureViewRectangle();

            GUI.enabled = !progressControls.IsOperationRunning();

            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            changesetsListView.OnGUI(rect);

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

        static void DoChangesArea(
            PropertiesPanel propertiesPanel,
            DiffPanel diffPanel)
        {
            EditorGUILayout.BeginVertical();

            propertiesPanel.OnGUI();
            Rect separatorRect = GUILayoutUtility.GetRect(
                0,
                1,
                GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(separatorRect, UnityStyles.Colors.BarBorder);
            diffPanel.OnGUI();

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
            FillChangesetsView fillChangesetsView)
        {
            mSearchField = new SearchField();
            mSearchField.downOrUpArrowKeyPressed += SearchField_OnDownOrUpArrowKeyPressed;

            DateFilter.Type dateFilterType =
                EnumPopupSetting<DateFilter.Type>.Load(
                    UnityConstants.CHANGESETS_DATE_FILTER_SETTING_NAME,
                    DateFilter.Type.LastMonth);
            mDateFilter = new DateFilter(dateFilterType);

            ChangesetsListHeaderState headerState =
                ChangesetsListHeaderState.GetDefault();
            TreeHeaderSettings.Load(
                headerState,
                UnityConstants.CHANGESETS_TABLE_SETTINGS_NAME,
                (int)ChangesetsListColumn.CreationDate,
                false,
                ChangesetsListHeaderState.GetDefaultVisibleColumns());

            mChangesetsListView = new ChangesetsListView(
                headerState,
                ChangesetsListHeaderState.GetColumnNames(),
                new ChangesetsViewMenu(
                    this,
                    this,
                    fillChangesetsView,
                    mIsGluonMode),
                fillChangesetsView,
                fillChangesetsView,
                selectionChangedAction: OnSelectionChanged,
                doubleClickAction: ((IChangesetMenuOperations)this).DiffChangeset,
                afterItemsChangedAction: fillChangesetsView.ShowContentOrEmptyState);

            mChangesetsListView.Reload();

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
        }

        SplitterState mSplitterState;

        DateFilter mDateFilter;
        SearchField mSearchField;
        ChangesetsListView mChangesetsListView;
        DiffPanel mDiffPanel;
        PropertiesPanel mPropertiesPanel;

        RevertToChangesetOperation.IGetStatusForWorkspace mGetStatusForWorkspace =
            new RevertToChangesetOperation.GetStatusFromWorkspace();
        RevertToChangesetOperation.IUndoCheckoutOperation mUndoCheckoutOperation =
            new RevertToChangesetOperation.UndoCheckout();
        RevertToChangesetOperation.IRevertToChangesetMergeController mRevertToChangesetMergeController =
            new RevertToChangesetOperation.RevertToChangesetMergeController();
        LaunchTool.IProcessExecutor mProcessExecutor;
        LaunchTool.IShowDownloadPlasticExeWindow mShowDownloadPlasticExeWindow;

        readonly ChangesetOperations mChangesetOperations;
        readonly FillChangesetsView mFillChangesetsView;

        readonly EmptyStatePanel mEmptyStatePanel;
        readonly SwitchAndShelve.IEnableSwitchAndShelveFeatureDialog mEnableSwitchAndShelveFeatureDialog;
        readonly bool mIsGluonMode;
        readonly IShelvePendingChangesQuestionerBuilder mShelvePendingChangesQuestionerBuilder;
        readonly IAssetStatusCache mAssetStatusCache;
        readonly ProgressControlsForViews mProgressControls;
        readonly IPendingChangesUpdater mPendingChangesUpdater;
        readonly GluonIncomingChangesUpdater mGluonIncomingChangesUpdater;
        readonly EditorWindow mParentWindow;
        readonly ISaveAssets mSaveAssets;
        readonly WorkspaceOperationsMonitor mWorkspaceOperationsMonitor;
        readonly IRevertToChangesetListener mRevertToChangesetListener;
        readonly WorkspaceInfo mWkInfo;
        readonly WorkspaceWindow mWorkspaceWindow;
        readonly ViewHost mViewHost;
        readonly IShelvedChangesUpdater mShelvedChangesUpdater;
        readonly IGluonUpdateReport mGluonUpdateReport;
        readonly IViewSwitcher mViewSwitcher;
    }
}
