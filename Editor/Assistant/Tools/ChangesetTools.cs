#if AIA_PRESENT
using System.Threading.Tasks;
using Codice.CM.Common;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class ChangesetTools
    {
        internal static async Task<ChangesetInfo> GetChangesetInfo(
            long changesetId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.GetChangesetInfo(wkInfo, changesetId);
        }
    }
}
#endif
