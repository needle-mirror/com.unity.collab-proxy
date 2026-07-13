#if AIA_PRESENT
using System.Threading.Tasks;
using Codice.CM.Common;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class BranchTools
    {
        internal static async Task<BranchInfo> GetBranchInfo(
            string branchName)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            return await UnityVersionControlApiProvider.Instance.GetBranchInfo(wkInfo, branchName);
        }

        internal static async Task CreateBranch(
            string branchName,
            string comment = null,
            string parentBranchName = null,
            long changesetId = -1)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            await UnityVersionControlApiProvider.Instance.CreateBranch(wkInfo, branchName, comment, parentBranchName, changesetId);
        }

        internal static async Task RenameBranch(
            string branchName,
            string newBranchName)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            await UnityVersionControlApiProvider.Instance.RenameBranch(wkInfo, branchName, newBranchName);
        }

        internal static async Task LaunchSwitchToBranchUI(
            string branchName)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            await UnityVersionControlApiProvider.Instance.LaunchSwitchToBranchUI(wkInfo, branchName);
        }
    }
}
#endif
