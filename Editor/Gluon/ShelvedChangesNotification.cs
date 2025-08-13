using UnityEditor;
using UnityEngine;

using Codice.Client.Common.EventTracking;

using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.Topbar;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.Tool;
using Unity.PlasticSCM.Editor.StatusBar;
using Unity.PlasticSCM.Editor.UI;

using GluonShelveOperations = GluonGui.WorkspaceWindow.Views.Shelves.ShelveOperations;

namespace Unity.PlasticSCM.Editor.Gluon
{
    internal class ShelvedChangesNotification :
        WindowStatusBar.IShelvedChangesNotification,
        CheckShelvedChanges.IUpdateShelvedChangesNotification
    {
        internal ShelvedChangesNotification(
            WorkspaceInfo wkInfo,
            RepositorySpec repSpec,
            ViewSwitcher viewSwitcher,
            IAssetStatusCache assetStatusCache,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            UVCSWindow uvcsWindow)
        {
            mWkInfo = wkInfo;
            mRepSpec = repSpec;
            mViewSwitcher = viewSwitcher;
            mAssetStatusCache = assetStatusCache;
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mUVCSWindow = uvcsWindow;
        }

        bool WindowStatusBar.IShelvedChangesNotification.HasNotification
        {
            get { return mHasNotification; }
        }

        void WindowStatusBar.IShelvedChangesNotification.SetWorkspaceWindow(
            WorkspaceWindow workspaceWindow)
        {
            mWorkspaceWindow = workspaceWindow;
        }

        void WindowStatusBar.IShelvedChangesNotification.SetShelvedChangesUpdater(
            IShelvedChangesUpdater shelvedChangesUpdater)
        {
            mShelvedChangesUpdater = shelvedChangesUpdater;
        }

        void WindowStatusBar.IShelvedChangesNotification.OnGUI()
        {
            Texture2D icon = Images.GetInfoBellNotificationIcon();

            WindowStatusBar.DrawIcon(icon, UnityConstants.STATUS_BAR_ICON_SIZE - 2);

            WindowStatusBar.DrawNotification(new GUIContentNotification(
                new GUIContent(
                    PlasticLocalization.Name.ShelvedChanges.GetString(),
                    PlasticLocalization.Name.ShelvedChangesExplanation.GetString())));

            GenericMenu discardShelveDropdownMenu = new GenericMenu();

            discardShelveDropdownMenu.AddItem(
                new GUIContent(PlasticLocalization.Name.Apply.GetString()),
                false,
                ApplyPartialShelveset);

            discardShelveDropdownMenu.AddItem(
                new GUIContent(PlasticLocalization.Name.DiscardShelvedChanges.GetString()),
                false,
                DiscardShelvedChanges);

            DrawActionButtonWithMenu.For(
                PlasticLocalization.Name.ViewButton.GetString(),
                PlasticLocalization.Name.ViewShelvedChangesButtonExplanation.GetString(),
                ShowShelvesView,
                discardShelveDropdownMenu);
        }

        void CheckShelvedChanges.IUpdateShelvedChangesNotification.Hide(
            WorkspaceInfo wkInfo)
        {
            if (!wkInfo.Equals(mWkInfo))
                return;

            mShelveInfo = null;

            mHasNotification = false;

            mUVCSWindow.Repaint();
        }

        void CheckShelvedChanges.IUpdateShelvedChangesNotification.Show(
            WorkspaceInfo wkInfo,
            RepositorySpec repSpec,
            ChangesetInfo shelveInfo)
        {
            if (!wkInfo.Equals(mWkInfo))
                return;

            mShelveInfo = shelveInfo;

            mHasNotification = true;

            mUVCSWindow.Repaint();
        }

        void ApplyPartialShelveset()
        {
            GluonShelveOperations.ApplyPartialShelveset(
                mWkInfo,
                mShelveInfo,
                mWorkspaceWindow,
                PlasticExeLauncher.BuildForResolveConflicts(
                    mWkInfo, true, mShowDownloadPlasticExeWindow),
                mViewSwitcher.ShelvesTab,
                mViewSwitcher.ShelvesTab.ProgressControls,
                mViewSwitcher.PendingChangesTab,
                mWorkspaceWindow.GluonProgressOperationHandler,
                mWorkspaceWindow.GluonProgressOperationHandler,
                mShelvedChangesUpdater,
                RefreshAsset.BeforeLongAssetOperation,
                () => RefreshAsset.AfterLongAssetOperation(mAssetStatusCache));
        }

        void DiscardShelvedChanges()
        {
            ShelvedChangesNotificationPanelOperations.DiscardShelvedChanges(
                mWkInfo,
                mShelveInfo,
                this,
                mShelvedChangesUpdater,
                null,
                mWorkspaceWindow);
        }

        void ShowShelvesView()
        {
            TrackFeatureUseEvent.For(
                mRepSpec,
                TrackFeatureUseEvent.Features.SwitchAndShelve.ShowShelvesViewFromNotification);

            mViewSwitcher.ShowShelvesView(mShelveInfo);
        }

        bool mHasNotification;
        ChangesetInfo mShelveInfo;

        WorkspaceWindow mWorkspaceWindow;
        IShelvedChangesUpdater mShelvedChangesUpdater;

        readonly WorkspaceInfo mWkInfo;
        readonly RepositorySpec mRepSpec;
        readonly ViewSwitcher mViewSwitcher;
        readonly IAssetStatusCache mAssetStatusCache;
        readonly LaunchTool.IShowDownloadPlasticExeWindow mShowDownloadPlasticExeWindow;
        readonly UVCSWindow mUVCSWindow;
    }
}
