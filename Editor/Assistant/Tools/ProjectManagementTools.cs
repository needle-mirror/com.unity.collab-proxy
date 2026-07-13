#if AIA_PRESENT
using System.Collections.Generic;
using System.Threading.Tasks;
using Codice.CM.Common;
using Unity.AI.Assistant.FunctionCalling;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class ProjectManagementTools
    {
        internal static async Task<List<string>> GetKnownUVCSServers()
        {
            return await UnityVersionControlApiProvider.Instance.GetKnownServers();
        }

        internal static async Task<List<RepositoryInfo>> GetAllRepositories(
            string server)
        {
            return await UnityVersionControlApiProvider.Instance.GetAllRepositories(server);
        }

        internal static async Task<List<RepositoryInfo>> GetAllProjects(
            string server)
        {
            return await UnityVersionControlApiProvider.Instance.GetAllProjects(server);
        }

        internal static async Task<WorkspaceInfo> GetWorkspaceFromPath(
            string path)
        {
            return await UnityVersionControlApiProvider.Instance.GetWorkspaceFromPath(path);
        }

        internal static async Task CreateRepository(
            string repositoryName,
            string server)
        {
            await UnityVersionControlApiProvider.Instance.CreateRepository(repositoryName, server);
        }

        internal static async Task<WorkspaceInfo> CreateWorkspace(
            ToolExecutionContext context,
            string workspaceName,
            string workspacePath,
            string repositoryName)
        {
            await context.Permissions.CheckFileSystemAccess(PermissionItemOperation.Modify, workspacePath);

            return await UnityVersionControlApiProvider.Instance.CreateWorkspace(
                workspaceName,
                workspacePath,
                repositoryName);
        }

        internal static async Task PerformInitialCheckin(ToolExecutionContext context)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            await context.Permissions.CheckFileSystemAccess(PermissionItemOperation.Read, wkInfo.ClientPath);

            await UnityVersionControlApiProvider.Instance.PerformInitialCheckin(wkInfo);
        }

        internal static async Task RefreshUIAfterCreateWorkspace()
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            await UnityVersionControlApiProvider.Instance.RefreshUIAfterCreateWorkspace(wkInfo);
        }

        internal static async Task<long> CheckinPendingChanges(
            ToolExecutionContext context,
            string comment)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            await context.Permissions.CheckFileSystemAccess(PermissionItemOperation.Modify, wkInfo.ClientPath);

            return await UnityVersionControlApiProvider.Instance.CheckinPendingChanges(wkInfo, comment);
        }

        internal static async Task<long> ShelvePendingChanges(
            ToolExecutionContext context,
            string comment,
            bool undoPendingChangesAfterShelve = false)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            await context.Permissions.CheckFileSystemAccess(PermissionItemOperation.Modify, wkInfo.ClientPath);

            return await UnityVersionControlApiProvider.Instance.ShelvePendingChanges(
                wkInfo, comment, undoPendingChangesAfterShelve);
        }
    }
}
#endif
