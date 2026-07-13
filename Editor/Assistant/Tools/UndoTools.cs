#if AIA_PRESENT
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.AI.Assistant.FunctionCalling;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class UndoTools
    {
        internal static async Task UndoChanges(
            ToolExecutionContext context,
            List<string> paths)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await context.Permissions.CheckFileSystemAccess(PermissionItemOperation.Modify, wkInfo.ClientPath);
            var fullPaths = paths.Select(UVCSToolContext.GetFullPath).ToList();
            await UnityVersionControlApiProvider.Instance.UndoChanges(wkInfo, fullPaths);
        }

        internal static async Task UndoUnchanged()
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.UndoUnchanged(wkInfo);
        }

        internal static async Task RevertToRevision(
            ToolExecutionContext context,
            string filePath,
            long revisionId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            string fullPath = UVCSToolContext.GetFullPath(filePath);
            await context.Permissions.CheckFileSystemAccess(PermissionItemOperation.Modify, fullPath);
            await UnityVersionControlApiProvider.Instance.RevertToRevision(wkInfo, fullPath, revisionId);
        }
    }
}
#endif
