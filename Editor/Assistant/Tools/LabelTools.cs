#if AIA_PRESENT
using System.Threading.Tasks;
using Codice.CM.Common;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class LabelTools
    {
        internal static async Task<MarkerInfo> CreateLabel(
            string labelName,
            string comment,
            long changesetId = -1)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

            var markerInfo = await UnityVersionControlApiProvider.Instance.CreateLabel(
                wkInfo, labelName, changesetId, comment);
            return markerInfo;
        }

        internal static async Task RenameLabel(
            string labelName,
            string newLabelName)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.RenameLabel(wkInfo, labelName, newLabelName);
        }

        internal static async Task ApplyLabel(
            string labelName)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.ApplyLabel(wkInfo, labelName);
        }

        internal static async Task DeleteLabel(
            string labelName)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.DeleteLabel(wkInfo, labelName);
        }
    }
}
#endif
