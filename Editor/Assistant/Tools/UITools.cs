#if AIA_PRESENT
using System.Threading.Tasks;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class UITools
    {
        internal static async Task RefreshView(
            ViewType viewType)
        {
            await UnityVersionControlApiProvider.Instance.RefreshView(viewType);
        }

        internal static async Task ShowView(
            ViewType viewType)
        {
            await UnityVersionControlApiProvider.Instance.ShowView(viewType);
        }

        internal static async Task ShowBranchExplorer()
        {
            await UnityVersionControlApiProvider.Instance.ShowBranchExplorer();
        }

        internal static async Task ShowFileHistory(
            string filePath)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            string fullPath = UVCSToolContext.GetFullPath(filePath);
            await UnityVersionControlApiProvider.Instance.ShowFileHistory(wkInfo, fullPath);
        }

        internal static async Task ShowBranchesView(
            string branchName = null)
        {
            if (string.IsNullOrEmpty(branchName))
            {
                await UnityVersionControlApiProvider.Instance.ShowView(ViewType.BranchesView);
                return;
            }

            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.ShowBranchesView(wkInfo, branchName);
        }

        internal static async Task ShowChangesetsView(
            long changesetId = -1)
        {
            if (changesetId == -1)
            {
                await UnityVersionControlApiProvider.Instance.ShowView(ViewType.ChangesetsView);
                return;
            }

            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.ShowChangesetsView(wkInfo, changesetId);
        }

        internal static async Task ShowShelvesView(
            long shelveId = -1)
        {
            if (shelveId == -1)
            {
                await UnityVersionControlApiProvider.Instance.ShowView(ViewType.ShelvesView);
                return;
            }

            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.ShowShelvesView(wkInfo, shelveId);
        }

        internal static async Task LaunchMergeFromBranchUI(
            string branchName)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.LaunchMergeFromBranchUI(wkInfo, branchName);
        }

        internal static async Task LaunchMergeFromChangesetUI(
            long changesetId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.LaunchMergeFromChangesetUI(
                wkInfo, changesetId);
        }

        internal static async Task LaunchApplyShelveUI(
            long shelveId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.LaunchApplyShelveUI(wkInfo, shelveId);
        }
    }
}
#endif
