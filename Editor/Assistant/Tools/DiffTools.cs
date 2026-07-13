#if AIA_PRESENT
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Codice.Client.BaseCommands;
using Codice.Client.Commands;
using Unity.AI.Assistant.FunctionCalling;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class DiffTools
    {
        internal static async Task<List<ClientDiff>> GetBranchDifferences(
            string branchName)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.GetBranchDifferences(wkInfo, branchName);
        }

        internal static async Task<List<ClientDiff>> GetChangesetDifferences(
            long changesetId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.GetChangesetDifferences(wkInfo, changesetId);
        }

        internal static async Task<List<ClientDiff>> GetChangesetsDifferences(
            long srcChangesetId,
            long dstChangesetId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.GetChangesetsDifferences(
                wkInfo, srcChangesetId, dstChangesetId);
        }

        internal static async Task<List<ClientDiff>> GetLabelDifferences(
            string labelName)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.GetLabelDifferences(wkInfo, labelName);
        }

        internal static async Task<List<ClientDiff>> GetShelveDifferences(
            long shelveId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.GetShelveDifferences(wkInfo, shelveId);
        }

        internal static async Task<List<ChangeInfo>> GetPendingChanges()
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.GetPendingChanges(wkInfo);
        }

        internal static async Task<Dictionary<long, string>> DownloadRevisionsToFiles(
            ToolExecutionContext context,
            List<long> revisionIds)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            string tmpDir = GetTempDirName();
            await context.Permissions.CheckFileSystemAccess(PermissionItemOperation.Create, tmpDir);
            Directory.CreateDirectory(tmpDir);

            string[] destPaths = revisionIds
                .Select(id => Path.Combine(tmpDir, "rev_" + id))
                .ToArray();

            await UnityVersionControlApiProvider.Instance.DownloadRevisionsToFiles(
                wkInfo, revisionIds, destPaths);

            var result = new Dictionary<long, string>();
            for (int i = 0; i < revisionIds.Count; i++)
                result[revisionIds[i]] = destPaths[i];

            return result;
        }

        internal static async Task CleanupDiffTempFiles(ToolExecutionContext context)
        {
            try
            {
                string tmpDir = GetTempDirName();
                if (Directory.Exists(tmpDir))
                {
                    await context.Permissions.CheckFileSystemAccess(PermissionItemOperation.Delete, tmpDir);
                    Directory.Delete(tmpDir, true);
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to clean up UVCS diff temp files: {ex.Message}");
            }
        }

        static string GetTempDirName()
        {
            return Path.Combine(UVCSToolContext.GetProjectPath(), TempDirName, DiffTempDirName);
        }

        const string TempDirName = "Temp";
        const string DiffTempDirName = "UVCSDiff";
    }
}
#endif
