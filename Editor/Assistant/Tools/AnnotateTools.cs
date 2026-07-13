#if AIA_PRESENT
using System.Collections.Generic;
using System.Threading.Tasks;
using Codice.Client.Commands;
using Codice.CM.Common;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class AnnotateTools
    {
        internal static async Task<List<AnnotatedLine>> GetFileAnnotations(
            string filePath,
            long revisionId = -1)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            string fullPath = UVCSToolContext.GetFullPath(filePath);

            return await UnityVersionControlApiProvider.Instance.GetAnnotations(wkInfo, fullPath, revisionId);
        }

        internal static async Task<RevisionInfo> GetRevisionInfo(
            long revisionId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            return await UnityVersionControlApiProvider.Instance.GetRevisionInfo(wkInfo, revisionId);
        }
    }
}
#endif
