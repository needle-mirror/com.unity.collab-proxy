using System;
using System.Threading.Tasks;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.Merge;
using Unity.PlasticSCM.Editor.Toolbar;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Api
{
    internal interface IUnityVersionControlUiApi
    {
        void RefreshViews(params ViewType[] viewTypes);
        Task RefreshUIAfterCreateWorkspace(WorkspaceInfo wkInfo);
        Task LaunchSwitchToBranchUI(WorkspaceInfo wkInfo, BranchInfo branchInfo);
        Task RefreshView(ViewType viewType);
        Task ShowView(ViewType viewType);
        Task ShowBranchExplorer();
        Task ShowFileHistory(
            RepositorySpec repSpec,
            long itemId,
            string filePath,
            bool isDirectory);
        Task ShowBranchesView(BranchInfo branchInfo);
        Task ShowChangesetsView(ChangesetInfo changesetInfo);
        Task ShowShelvesView(ChangesetInfo changesetInfo);
        Task MergeFrom(
            WorkspaceInfo wkInfo,
            ObjectInfo objectInfo,
            EnumMergeType mergeType,
            bool showDiscardChangesButton);
    }

    internal class UnityVersionControlUiApi : IUnityVersionControlUiApi
    {
        void IUnityVersionControlUiApi.RefreshViews(params ViewType[] viewTypes)
        {
            RefreshViewsOnUIThread(viewTypes);
        }

        Task IUnityVersionControlUiApi.RefreshUIAfterCreateWorkspace(WorkspaceInfo wkInfo)
        {
            return RunOnUIThread(() =>
            {
                UVCSPlugin.Instance.Enable();
                UVCSWindow window = ShowWindow.UVCS();
                window.ExecuteFullReload();
                GetWindowIfOpened.BranchExplorer()?.OnWorkspaceCreated(wkInfo);
                UVCSToolbar.Controller.SetWorkspace(
                    wkInfo, PlasticGui.Plastic.API.IsGluonWorkspace(wkInfo));
            });
        }

        Task IUnityVersionControlUiApi.LaunchSwitchToBranchUI(
            WorkspaceInfo wkInfo,
            BranchInfo branchInfo)
        {
            return RunOnUIThread(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                UVCSToolbar.Controller.ExecuteSwitchToBranchUI(branchInfo, repSpec);
            });
        }

        Task IUnityVersionControlUiApi.RefreshView(ViewType viewType)
        {
            return RunOnUIThread(() =>
            {
                UVCSWindow window = GetWindowIfOpened.UVCS();
                ViewSwitcher viewSwitcher = window?.IViewSwitcher as ViewSwitcher;

                if (viewSwitcher != null)
                    viewSwitcher.RefreshView(viewType);

                if (viewType == ViewType.BranchExplorerView)
                {
                    IRefreshableView brexView =
                        GetWindowIfOpened.BranchExplorer()?.BranchExplorerView;
                    brexView?.Refresh();
                }
            });
        }

        Task IUnityVersionControlUiApi.ShowView(ViewType viewType)
        {
            return RunOnUIThread(() =>
            {
                UVCSWindow window = ShowWindow.UVCS();
                ViewSwitcher viewSwitcher = window?.IViewSwitcher as ViewSwitcher;

                if (viewSwitcher == null)
                    return;

                switch (viewType)
                {
                    case ViewType.PendingChangesView:
                        viewSwitcher.ShowPendingChangesView();
                        break;
                    case ViewType.IncomingChangesView:
                        viewSwitcher.ShowIncomingChangesView();
                        break;
                    case ViewType.ChangesetsView:
                        viewSwitcher.ShowChangesetsView();
                        break;
                    case ViewType.ShelvesView:
                        viewSwitcher.ShowShelvesView();
                        break;
                    case ViewType.BranchesView:
                        viewSwitcher.ShowBranchesView();
                        break;
                    case ViewType.LabelsView:
                        viewSwitcher.ShowLabelsView();
                        break;
                    case ViewType.LocksView:
                        viewSwitcher.ShowLocksView();
                        break;
                    default:
                        throw new ArgumentException(
                            $"Unsupported view type: {viewType}");
                }
            });
        }

        Task IUnityVersionControlUiApi.ShowBranchExplorer()
        {
            return RunOnUIThread(() => ShowWindow.BranchExplorer());
        }

        Task IUnityVersionControlUiApi.ShowFileHistory(
            RepositorySpec repSpec,
            long itemId,
            string filePath,
            bool isDirectory)
        {
            return RunOnUIThread(() =>
            {
                UVCSWindow window = ShowWindow.UVCS();
                ViewSwitcher viewSwitcher = window?.IViewSwitcher as ViewSwitcher;

                viewSwitcher?.ShowHistoryView(
                    repSpec, itemId, filePath, isDirectory);
            });
        }

        Task IUnityVersionControlUiApi.ShowBranchesView(BranchInfo branchInfo)
        {
            return RunOnUIThread(() =>
            {
                UVCSWindow window = ShowWindow.UVCS();
                ViewSwitcher viewSwitcher = window?.IViewSwitcher as ViewSwitcher;
                viewSwitcher?.ShowBranchesView(branchInfo);
            });
        }

        Task IUnityVersionControlUiApi.ShowChangesetsView(ChangesetInfo changesetInfo)
        {
            return RunOnUIThread(() =>
            {
                UVCSWindow window = ShowWindow.UVCS();
                ViewSwitcher viewSwitcher = window?.IViewSwitcher as ViewSwitcher;
                viewSwitcher?.ShowChangesetsView(changesetInfo);
            });
        }

        Task IUnityVersionControlUiApi.ShowShelvesView(ChangesetInfo shelveInfo)
        {
            return RunOnUIThread(() =>
            {
                UVCSWindow window = ShowWindow.UVCS();
                ViewSwitcher viewSwitcher = window?.IViewSwitcher as ViewSwitcher;
                viewSwitcher?.ShowShelvesView(shelveInfo);
            });
        }

        Task IUnityVersionControlUiApi.MergeFrom(
            WorkspaceInfo wkInfo,
            ObjectInfo objectInfo,
            EnumMergeType mergeType,
            bool showDiscardChangesButton)
        {
            return RunOnUIThread(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                UVCSWindow window = ShowWindow.UVCS();
                window.IMergeViewLauncher.MergeFrom(
                    repSpec, objectInfo, mergeType, showDiscardChangesButton);
            });
        }

        static void RefreshViewsOnUIThread(params ViewType[] viewTypes)
        {
            EditorDispatcher.Dispatch(() =>
            {
                UVCSWindow window = GetWindowIfOpened.UVCS();
                ViewSwitcher viewSwitcher = window?.IViewSwitcher as ViewSwitcher;

                if (viewSwitcher == null)
                    return;

                foreach (ViewType viewType in viewTypes)
                {
                    if (viewType != ViewType.BranchExplorerView)
                    {
                        viewSwitcher.RefreshView(viewType);
                        continue;
                    }

                    IRefreshableView brexView =
                        GetWindowIfOpened.BranchExplorer()?.BranchExplorerView;
                    brexView?.Refresh();
                }
            });
        }

        static Task RunOnUIThread(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            EditorDispatcher.Dispatch(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
