#if AIA_PRESENT
using System.Collections.Generic;
using System.Threading.Tasks;
using Codice.CM.Common;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class LockTools
    {
        internal static async Task<List<LockInfo>> ListLocks()
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.ListLocks(wkInfo);
        }

        internal static async Task<LockRule> GetLockRule()
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.GetLockRule(wkInfo);
        }

        internal static async Task ReleaseLocks(
            List<long> lockIds)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.ReleaseLocks(wkInfo, lockIds);
        }

        internal static async Task RemoveLocks(
            List<long> lockIds)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.RemoveLocks(wkInfo, lockIds);
        }
    }
}
#endif
