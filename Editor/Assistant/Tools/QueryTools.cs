#if AIA_PRESENT
using System.Threading.Tasks;
using Codice.CM.Common;
using PlasticGui;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class QueryTools
    {
        internal static async Task<QueryResult> FindQuery(
            string query)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            return await UnityVersionControlApiProvider.Instance.FindQuery(wkInfo, query);
        }

        internal static async Task<SelectorInformation> GetWorkspaceConfiguration()
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            return await UnityVersionControlApiProvider.Instance.GetSelectorUserInformation(wkInfo);
        }
    }
}
#endif
