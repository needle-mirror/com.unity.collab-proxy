using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Codice.CM.Common;
using Codice.CM.Common.Mount;
using PlasticGui;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.QueryViews;
using PlasticGui.WorkspaceWindow.QueryViews.Changesets;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.Tool;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.UI.Tree;
using Unity.PlasticSCM.Editor.Views.Changesets;
using Unity.PlasticSCM.Editor.Views.Properties;

#if !UNITY_6000_3_OR_NEWER
using SplitterState = Unity.PlasticSCM.Editor.UnityInternals.UnityEditor.SplitterState;
#endif

namespace Unity.PlasticSCM.Editor.Views.Diff
{
    internal class ChangesetByChangesetDiffPanel :
        IGetQueryText,
        IGetFilterText,
        FillChangesetsView.IShowContentView
    {
        internal DiffPanel DiffPanel { get { return mDiffPanel; } }

        internal ChangesetByChangesetDiffPanel(
            WorkspaceInfo wkInfo,
            IWorkspaceWindow workspaceWindow,
            IViewSwitcher viewSwitcher,
            IHistoryViewLauncher historyViewLauncher,
            IRefreshView refreshView,
            IAssetStatusCache assetStatusCache,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            IPendingChangesUpdater pendingChangesUpdater,
            IIncomingChangesUpdater developerIncomingChangesUpdater,
            IIncomingChangesUpdater gluonIncomingChangesUpdater,
            EditorWindow parentWindow,
            bool isGluonMode)
        {
            mWkInfo = wkInfo;
            mWorkspaceWindow = workspaceWindow;
            mViewSwitcher = viewSwitcher;
            mHistoryViewLauncher = historyViewLauncher;
            mRefreshView = refreshView;
            mAssetStatusCache = assetStatusCache;
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mPendingChangesUpdater = pendingChangesUpdater;
            mDeveloperIncomingChangesUpdater = developerIncomingChangesUpdater;
            mGluonIncomingChangesUpdater = gluonIncomingChangesUpdater;
            mParentWindow = parentWindow;
            mIsGluonMode = isGluonMode;

            mEmptyStatePanel = new EmptyStatePanel(parentWindow.Repaint);
            mProgressControls = new ProgressControlsForViews();

            mFillChangesetsView = new FillChangesetsView(
                mWkInfo,
                null,
                null,
                this,
                this,
                this);

            BuildComponents();

            mDiffSplitterState = PlasticSplitterGUILayout.InitSplitterState(
                new float[] { 0.50f, 0.50f },
                new int[] { 100, 100 },
                new int[] { 100000, 100000 }
            );

            mPropertiesSplitterState = PlasticSplitterGUILayout.InitSplitterState(
                new float[] { 0.35f, 0.65f },
                new int[] { 75, 75 },
                new int[] { 100000, 100000 }
            );
        }

        internal void ClearInfo()
        {
            mSelectedMountWithPath = null;
            mSelectedRepObjectInfo = null;

            mDiffPanel.ClearInfo();

            mParentWindow.Repaint();
        }

        internal void UpdateInfo(
            MountPointWithPath mountWithPath,
            RepObjectInfo repObjectInfo)
        {
            mSelectedMountWithPath = mountWithPath;
            mSelectedRepObjectInfo = repObjectInfo;

            if (repObjectInfo is BranchInfo)
            {
                RefreshChangesetsList();
            }

            mParentWindow.Repaint();
        }

        internal void OnEnable()
        {
            mDiffPanel.OnEnable();
        }

        internal void OnDisable()
        {
            mDiffPanel.OnDisable();
        }

        internal void Update()
        {
            mProgressControls.UpdateProgress(mParentWindow);
            mDiffPanel.Update();
        }

        internal void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            PlasticSplitterGUILayout.BeginVerticalSplit(mDiffSplitterState);

            DoChangesetsAndPropertiesArea();
            DoDiffArea();

            PlasticSplitterGUILayout.EndVerticalSplit();

            if (mProgressControls.HasNotification())
            {
                DrawProgressForViews.ForNotificationArea(
                    mProgressControls.ProgressData);
            }

            EditorGUILayout.EndVertical();
        }

        string IGetQueryText.Get()
        {
            if (mSelectedRepObjectInfo is BranchInfo)
            {
                return GetChangesetsQuery.For((BranchInfo)mSelectedRepObjectInfo);
            }

            return GetChangesetsQuery.For(new DateFilter(DateFilter.Type.LastMonth));
        }

        string IGetFilterText.Get()
        {
            return mChangesetsListView.searchString;
        }

        void IGetFilterText.Clear() { }

        void FillChangesetsView.IShowContentView.ShowContentPanel()
        {
            mEmptyStatePanel.UpdateContent(string.Empty);
        }

        void FillChangesetsView.IShowContentView.ShowEmptyStatePanel(
            string explanationText, bool showResetFilterButton)
        {
            mEmptyStatePanel.UpdateContent(
                PlasticLocalization.Name.NoChangesetsCreatedYet.GetString());
        }

        void RefreshChangesetsList()
        {
            mPropertiesPanel.ClearInfo();
            mDiffPanel.ClearInfo();

            mFillChangesetsView.FillView(
                mChangesetsListView,
                mProgressControls,
                null,
                null,
                null,
                null);
        }

        void DoChangesetsAndPropertiesArea()
        {
            GUILayout.BeginHorizontal();

            PlasticSplitterGUILayout.BeginHorizontalSplit(mPropertiesSplitterState);

            DoChangesetsArea();
            DoPropertiesArea();

            PlasticSplitterGUILayout.EndHorizontalSplit();

            GUILayout.EndHorizontal();
        }

        void DoChangesetsArea()
        {
            EditorGUILayout.BeginVertical();

            DoChangesetsToolbar();

            Rect viewRect = OverlayProgress.CaptureViewRectangle();

            DoChangesetsListView();

            EditorGUILayout.EndVertical();

            if (mProgressControls.IsOperationRunning())
            {
                OverlayProgress.DoOverlayProgress(
                    viewRect,
                    mProgressControls.ProgressData.ProgressPercent,
                    mProgressControls.ProgressData.ProgressMessage);
            }
        }

        void DoPropertiesArea()
        {
            GUILayout.BeginHorizontal();

            Rect separatorRect = GUILayoutUtility.GetRect(
                1,
                0,
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(separatorRect, UnityStyles.Colors.BarBorder);

            GUILayout.BeginVertical();

            mPropertiesPanel.OnGUI();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        void DoChangesetsToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.FlexibleSpace();

            GUILayout.Space(3);

            DrawSearchField.For(
                mSearchField,
                mChangesetsListView,
                UnityConstants.SEARCH_FIELD_WIDTH);

            EditorGUILayout.EndHorizontal();
        }

        void DoChangesetsListView()
        {
            GUI.enabled = !mProgressControls.IsOperationRunning();

            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            mChangesetsListView.OnGUI(rect);

            if (!mEmptyStatePanel.IsEmpty())
                mEmptyStatePanel.OnGUI(rect);

            GUI.enabled = true;
        }

        void DoDiffArea()
        {
            EditorGUILayout.BeginVertical();

            Rect separatorRect = GUILayoutUtility.GetRect(
                0,
                1,
                GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(separatorRect, UnityStyles.Colors.BarBorder);
            mDiffPanel.OnGUI();

            EditorGUILayout.EndVertical();
        }

        void BuildComponents()
        {
            mSearchField = new SearchField();
            mSearchField.downOrUpArrowKeyPressed += SearchField_OnDownOrUpArrowKeyPressed;

            mChangesetsListView = new ChangesetsListView(
                null,
                ChangesetsListHeaderState.GetColumnNames(),
                null,
                mFillChangesetsView,
                mFillChangesetsView,
                selectionChangedAction: OnChangesetSelectionChanged,
                doubleClickAction: OnChangesetDoubleClick,
                afterItemsChangedAction: mFillChangesetsView.ShowContentOrEmptyState);

            mChangesetsListView.Reload();

            mPropertiesPanel = new PropertiesPanel(
                mParentWindow.Repaint,
                true);

            mDiffPanel = new DiffPanel(
                mWkInfo,
                mWorkspaceWindow,
                mViewSwitcher,
                mHistoryViewLauncher,
                mRefreshView,
                mAssetStatusCache,
                mShowDownloadPlasticExeWindow,
                mPendingChangesUpdater,
                mDeveloperIncomingChangesUpdater,
                mGluonIncomingChangesUpdater,
                mParentWindow,
                mIsGluonMode);
        }

        void SearchField_OnDownOrUpArrowKeyPressed()
        {
            mChangesetsListView.SetFocusAndEnsureSelectedItem();
        }

        void OnChangesetSelectionChanged()
        {
            ChangesetInfo changesetInfo =
                ChangesetsSelection.GetSelectedChangeset(mChangesetsListView);

            if (changesetInfo == null)
                return;

            RepositorySpec repSpec =
                ChangesetsSelection.GetSelectedRepository(mChangesetsListView);

            mPropertiesPanel.UpdateInfo(changesetInfo, repSpec);
            mDiffPanel.UpdateInfo(mSelectedMountWithPath, changesetInfo);
        }

        void OnChangesetDoubleClick() { }

        RepObjectInfo mSelectedRepObjectInfo;
        MountPointWithPath mSelectedMountWithPath;

        SplitterState mDiffSplitterState;
        SplitterState mPropertiesSplitterState;

        SearchField mSearchField;
        ChangesetsListView mChangesetsListView;
        PropertiesPanel mPropertiesPanel;
        DiffPanel mDiffPanel;

        readonly ProgressControlsForViews mProgressControls;
        readonly FillChangesetsView mFillChangesetsView;

        readonly EmptyStatePanel mEmptyStatePanel;
        readonly bool mIsGluonMode;
        readonly EditorWindow mParentWindow;
        readonly IRefreshView mRefreshView;
        readonly LaunchTool.IShowDownloadPlasticExeWindow mShowDownloadPlasticExeWindow;
        readonly IAssetStatusCache mAssetStatusCache;
        readonly IHistoryViewLauncher mHistoryViewLauncher;
        readonly IViewSwitcher mViewSwitcher;
        readonly IWorkspaceWindow mWorkspaceWindow;
        readonly WorkspaceInfo mWkInfo;
        readonly IPendingChangesUpdater mPendingChangesUpdater;
        readonly IIncomingChangesUpdater mDeveloperIncomingChangesUpdater;
        readonly IIncomingChangesUpdater mGluonIncomingChangesUpdater;
    }
}
