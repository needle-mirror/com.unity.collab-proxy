using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.Client.BaseCommands;
using Codice.Client.BaseCommands.Differences;
using Codice.Client.Commands;
using Codice.Client.Common;
using Codice.Client.Common.Threading;
using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;
using Codice.CM.Common;
using Codice.CM.Common.Merge;
using Codice.CM.Common.Mount;
using PlasticGui;
using PlasticGui.Diff;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff;
using Unity.PlasticSCM.Editor.Diff.Asset.Content;
using Unity.PlasticSCM.Editor.Diff.History;
using Unity.PlasticSCM.Editor.Diff.Text;
using Unity.PlasticSCM.Editor.Diff.Texture;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using XDiffGui;

using EditorWindow = UnityEditor.EditorWindow;

namespace Unity.PlasticSCM.Editor.Diff
{
    internal enum DiffSource
    {
        None,
        AssetMenu,
        PendingChanges,
        History,
        DiffPanel
    }

    internal interface IUnityDiffWindow
    {
        void ShowDiffFromChange(
            ChangeInfo changeInfo,
            ChangeInfo changedForMoved,
            Action showDiffInDesktopApp);
        void ShowDiffFromHistory(
            HistoryRevision leftRevision,
            HistoryRevision rightRevision,
            RepositorySpec repSpec,
            string cmPath,
            long itemId,
            Action showDiffInDesktopApp);
        void ShowMoveRealizationInfo(MoveRealizationInfo moveRealizationInfo);
        void ShowRemovedRealizationInfo();
        void ShowDiffFromDiff(
            MountPoint mount,
            Difference diff,
            Action showDiffInDesktopApp);
        void ShowDiffFromDiffInfo(
            DiffInfo diffInfo,
            Action showDiffInDesktopApp);
        void ClearDiffViewerCache();
        void ClearIfShownFrom(DiffSource source);
    }

    internal class UnityDiffWindow :
        EditorWindow,
        IAfterSaveChangesListener,
        IUnityDiffWindow,
        IHasCustomMenu
    {
        void IUnityDiffWindow.ShowDiffFromChange(
            ChangeInfo changeInfo,
            ChangeInfo changedForMoved,
            Action showDiffInDesktopApp)
        {
            mCurrentMount = null;
            mCurrentDiff = null;
            mShowDiffInDesktopApp = showDiffInDesktopApp;
            mCurrentSource = DiffSource.PendingChanges;

            SaveChange.IfDirty(changeInfo);

            ShowDiffsFromAsyncData(() => mDiffViewerDataProvider.GetDiffViewerInfo(
                changeInfo, changedForMoved));
        }

        void IUnityDiffWindow.ShowDiffFromHistory(
            HistoryRevision leftRevision,
            HistoryRevision rightRevision,
            RepositorySpec repSpec,
            string cmPath,
            long itemId,
            Action showDiffInDesktopApp)
        {
            mCurrentMount = null;
            mCurrentDiff = null;
            mShowDiffInDesktopApp = showDiffInDesktopApp;
            mCurrentSource = DiffSource.History;

            ShowDiffsFromAsyncData(() => mDiffViewerDataProvider.GetDiffViewerInfo(
                leftRevision, rightRevision, repSpec, cmPath, itemId));
        }

        void IUnityDiffWindow.ShowMoveRealizationInfo(MoveRealizationInfo moveRealizationInfo)
        {
            mCurrentMount = null;
            mCurrentDiff = null;
            mShownData = null;
            mShowDiffInDesktopApp = null;
            mCurrentSource = DiffSource.History;

            if (mMoveRealizationInfoDetailsPanel == null)
                mMoveRealizationInfoDetailsPanel = BuildMoveRealizationInfoDetailsPanel();

            mMoveRealizationInfoDetailsPanel.SetData(moveRealizationInfo);

            ReplaceView(mMoveRealizationInfoDetailsPanel);
        }

        void IUnityDiffWindow.ShowRemovedRealizationInfo()
        {
            mCurrentMount = null;
            mCurrentDiff = null;
            mShownData = null;
            mShowDiffInDesktopApp = null;
            mCurrentSource = DiffSource.History;

            ShowMessage(
                PlasticLocalization.Name.ThisItemWasDeleted.GetString());
        }

        void IUnityDiffWindow.ShowDiffFromDiff(
            MountPoint mount,
            Difference diff,
            Action showDiffInDesktopApp)
        {
            mCurrentMount = mount;
            mCurrentDiff = diff;
            mShowDiffInDesktopApp = showDiffInDesktopApp;
            mCurrentSource = DiffSource.DiffPanel;

            ShowDiffsFromAsyncData(() => mDiffViewerDataProvider.GetDiffViewerInfo(
                mount, diff));
        }

        void IUnityDiffWindow.ShowDiffFromDiffInfo(
            DiffInfo diffInfo,
            Action showDiffInDesktopApp)
        {
            mShowDiffInDesktopApp = showDiffInDesktopApp;
            mCurrentSource = DiffSource.AssetMenu;

            ShowDiffs(mDiffViewerDataProvider.GetDiffViewerInfo(diffInfo));
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            GUIContent openDiffInDesktopAppMenuItemContent = new GUIContent(
                PlasticLocalization.Name.OpenDiffInDesktopApp.GetString());

            if (mShowDiffInDesktopApp == null)
            {
                menu.AddDisabledItem(openDiffInDesktopAppMenuItemContent, false);
                return;
            }

            menu.AddItem(openDiffInDesktopAppMenuItemContent, false, () => mShowDiffInDesktopApp());
        }

        void IUnityDiffWindow.ClearDiffViewerCache()
        {
            mDiffViewerDataProvider?.Clear();
        }

        void IUnityDiffWindow.ClearIfShownFrom(DiffSource source)
        {
            if (mCurrentSource != source)
                return;

            mCurrentSource = DiffSource.None;
            mCurrentMount = null;
            mCurrentDiff = null;
            mShownData = null;
            mShowDiffInDesktopApp = null;
            mDiffViewerDataProvider?.Clear();

            ShowEmptyState();
        }

        void IAfterSaveChangesListener.AfterSaveChanges(string file)
        {
            UpdateCachedDiffViewEntry(file);
            mRefreshAssetsPanel.ShowIfNeeded();
        }

        void UpdateCachedDiffViewEntry(string file)
        {
            if (mCurrentMount == null || mCurrentDiff == null)
                return;

            if (mDiffViewerDataProvider == null)
                return;

            DiffViewEntry editedEntry = mDiffViewerDataProvider.GetCachedDiffViewEntry(
                mCurrentMount, mCurrentDiff);

            if (editedEntry == null)
                return;

            editedEntry.Path = file;
            editedEntry.Right.Owner = UserInfo.Get().GetCurrentUserName();
            editedEntry.Right.bLocalData = true;
        }

        void OnDisable()
        {
            mContentControl?.Dispose();
            mDiffControl?.Dispose();
            mImageContentControl?.Dispose();
            mImageDiffControl?.Dispose();
            mAssetDiffControl?.Dispose();
            mAssetContentControl?.Dispose();

            mDiffViewerDataProvider?.Clear();
            mDiffViewManager?.Clear();
        }

        void OnEnable()
        {
            UVCSPlugin uvcsPlugin = UVCSPlugin.Instance;

            titleContent.image = Images.GetDiffIcon();

            uvcsPlugin.Enable();

            if (!uvcsPlugin.ConnectionMonitor.IsConnected)
                return;

            mWkInfo = FindWorkspace.InfoForApplicationPath(
                ApplicationDataPath.Get(), PlasticGui.Plastic.API);

            if (mWkInfo == null)
                return;

            Init();
            BuildComponents();
        }

        void Init()
        {
            mBranchResolver = new BranchResolver();

            mDiffViewManager = new DiffViewManager(
                mWkInfo,
                mBranchResolver,
                new UnityBigFileChecker(),
                null);

            mDiffViewerDataProvider = PlasticGui.Plastic.API.GetDiffViewerDataProvider(
                mWkInfo,
                mDiffViewManager,
                mBranchResolver,
                null);

            mBigFileDownloader = mDiffViewManager.BigFileDownloader;
            mBigFileChecker = mDiffViewManager.BigFileChecker;
        }

        void BuildComponents()
        {
            mRefreshAssetsPanel = new RefreshAssetsPanel();
            mOverlayProgressControls = new OverlayProgressControls();

            rootVisualElement.Add(mRefreshAssetsPanel);
            rootVisualElement.Add(mOverlayProgressControls);

            ShowEmptyState();
        }

        void ShowDiffsFromAsyncData(Func<DiffViewerData> getDiffViewerData)
        {
            int requestId = System.Threading.Interlocked.Increment(
                ref mCurrentDiffRequestId);

            mOverlayProgressControls.ShowProgress(
                PlasticLocalization.Name.Loading.GetString(),
                TimeSpan.FromMilliseconds(500));

            DiffViewerData data = null;
            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    if (requestId != mCurrentDiffRequestId)
                        return;

                    data = getDiffViewerData();
                },
                /*afterOperationDelegate*/ delegate
                {
                    ((IProgressControls)mOverlayProgressControls).HideProgress();

                    if (requestId != mCurrentDiffRequestId)
                        return;

                    if (waiter.Exception != null)
                    {
                        ExceptionsHandler.DisplayException(waiter.Exception);
                        return;
                    }

                    ShowDiffs(data);
                });
        }

        void ShowDiffs(DiffViewerData data)
        {
            if (data == null)
                return;

            if (IsAlreadyShown(mShownData, data))
                return;

            mShownData = data;

            if (data.Left == null)
            {
                ShowMessage(data.Message);
                return;
            }

            if (data.Right == null)
            {
                ShowContent(data);
                return;
            }

            ShowDiff(data);
        }

        void ShowContent(DiffViewerData data)
        {
            if (data.IsSupportedImage())
            {
                ShowImageContentControl(data);
                return;
            }

            if (data.IsSerializedAsset())
            {
                ShowAssetContentControl(data);
                return;
            }

            ShowContentControl(data);
        }

        void ShowDiff(DiffViewerData data)
        {
            if (data.IsSupportedImage())
            {
                ShowImageDiffControl(data);
                return;
            }

            if (data.IsSerializedAsset())
            {
                ShowAssetDiffControl(data);
                return;
            }

            ShowDiffControl(data);
        }

        void ShowContentControl(DiffViewerData data)
        {
            if (mContentControl == null)
                mContentControl = BuildContentControl();

            mContentControl.ShowData(data);

            ReplaceView(mContentControl);
        }

        void ShowDiffControl(DiffViewerData data)
        {
            if (mDiffControl == null)
                mDiffControl = BuildDiffControl();

            mDiffControl.ShowData(data);

            ReplaceView(mDiffControl);
        }

        void ShowAssetContentControl(DiffViewerData data)
        {
            if (mAssetContentControl == null)
                mAssetContentControl = BuildAssetContentControl();

            mAssetContentControl.ShowData(data);

            ReplaceView(mAssetContentControl);
        }

        void ShowAssetDiffControl(DiffViewerData data)
        {
            if (mAssetDiffControl == null)
                mAssetDiffControl = BuildAssetDiffControl();

            mAssetDiffControl.ShowData(data);

            ReplaceView(mAssetDiffControl);
        }

        void ShowImageContentControl(DiffViewerData data)
        {
            if (mImageContentControl == null)
                mImageContentControl = BuildImageContentControl();

            mImageContentControl.ShowData(data);

            ReplaceView(mImageContentControl);
        }

        void ShowImageDiffControl(DiffViewerData data)
        {
            if (mImageDiffControl == null)
                mImageDiffControl = BuildImageDiffControl();

            mImageDiffControl.ShowData(data);

            ReplaceView(mImageDiffControl);
        }

        void ShowMessage(string message)
        {
            if (mDiffMessagePanel == null)
                mDiffMessagePanel = BuildDiffMessagePanel();

            mDiffMessagePanel.ShowMessage(message);

            ReplaceView(mDiffMessagePanel);
        }

        void ShowEmptyState()
        {
            ShowMessage(
                PlasticLocalization.Name.SelectFileToSeeDifferences.GetString());
        }

        ContentControl BuildContentControl()
        {
            return new ContentControl(
                this,
                mBigFileDownloader,
                mBigFileChecker);
        }

        DiffControl BuildDiffControl()
        {
            return new DiffControl(
                this,
                mBigFileDownloader,
                mBigFileChecker);
        }

        AssetContentControl BuildAssetContentControl()
        {
            return new AssetContentControl(
                mBigFileDownloader,
                mBigFileChecker);
        }

        AssetDiffControl BuildAssetDiffControl()
        {
            return new AssetDiffControl(
                mBigFileDownloader,
                mBigFileChecker,
                onViewAsTextDiff: () => ShowDiffControl(mShownData));
        }

        ImageContentControl BuildImageContentControl()
        {
            return new ImageContentControl(
                mBigFileDownloader,
                mBigFileChecker);
        }

        ImageDiffControl BuildImageDiffControl()
        {
            return new ImageDiffControl(
                mBigFileDownloader,
                mBigFileChecker);
        }

        DiffMessagePanel BuildDiffMessagePanel()
        {
            return new DiffMessagePanel();
        }

        MoveRealizationInfoDetailsPanel BuildMoveRealizationInfoDetailsPanel()
        {
            return new MoveRealizationInfoDetailsPanel();
        }

        void ReplaceView(VisualElement view)
        {
            if (mCurrentView != null && mCurrentView.Equals(view))
                return;

            if (rootVisualElement.Contains(mCurrentView))
                rootVisualElement.Remove(mCurrentView);

            rootVisualElement.Insert(0, view);
            mCurrentView = view;
        }

        static bool IsAlreadyShown(DiffViewerData shown, DiffViewerData candidate)
        {
            if (shown == null)
                return false;

            if (ReferenceEquals(shown, candidate))
                return true;

            return EntryEquals(shown.Left, candidate.Left)
                   && EntryEquals(shown.Right, candidate.Right);
        }

        static bool EntryEquals(EntryData a, EntryData b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            return a.File == b.File && a.SymbolicName == b.SymbolicName;
        }

        WorkspaceInfo mWkInfo;

        VisualElement mCurrentView;
        RefreshAssetsPanel mRefreshAssetsPanel;
        ContentControl mContentControl;
        DiffControl mDiffControl;

        ImageContentControl mImageContentControl;
        ImageDiffControl mImageDiffControl;

        AssetContentControl mAssetContentControl;
        AssetDiffControl mAssetDiffControl;
        MoveRealizationInfoDetailsPanel mMoveRealizationInfoDetailsPanel;
        DiffMessagePanel mDiffMessagePanel;
        BranchResolver mBranchResolver;
        //IRunAfterSetDifferencesInfo mRunAfterSetDifferencesInfo;

        MountPoint mCurrentMount;
        Difference mCurrentDiff;
        Action mShowDiffInDesktopApp;
        DiffViewerData mShownData;
        DiffSource mCurrentSource;

        DiffViewManager mDiffViewManager;
        //readonly IContainer mParentContainer;
        int mCurrentDiffRequestId;
        IBigFileDownloader mBigFileDownloader;
        IBigFileChecker mBigFileChecker;
        IDiffViewerDataProvider mDiffViewerDataProvider;
        OverlayProgressControls mOverlayProgressControls;
    }
}
