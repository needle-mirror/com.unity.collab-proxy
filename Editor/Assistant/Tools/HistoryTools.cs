#if AIA_PRESENT
using System.Collections.Generic;
using System.Threading.Tasks;
using Codice.CM.Common;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class HistoryTools
    {
        internal static async Task<List<RepObjectInfo>> GetFileHistory(
            string filePath)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            string fullPath = UVCSToolContext.GetFullPath(filePath);

            return await UnityVersionControlApiProvider.Instance.GetFileHistory(wkInfo, fullPath);
        }
    }
}
#endif
