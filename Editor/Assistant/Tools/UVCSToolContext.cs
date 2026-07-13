#if AIA_PRESENT
using System;
using System.IO;
using System.Threading.Tasks;
using Codice.CM.Common;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class UVCSToolContext
    {
        internal static async Task<WorkspaceInfo> GetCurrentWorkspaceAsync()
        {
            WorkspaceInfo wkInfo = await UnityVersionControlApiProvider.Instance
                .GetWorkspaceFromPath(Application.dataPath);

            if (wkInfo == null)
                throw new ArgumentException(
                    "No Unity Version Control workspace was found for the current Unity project. " +
                    "Make sure the project is checked out under a UVCS workspace before invoking UVCS tools.");

            return wkInfo;
        }

        internal static string GetProjectPath()
        {
            return Path.GetDirectoryName(Application.dataPath);
        }

        internal static string GetFullPath(string projectRelativePath)
        {
            return Path.GetFullPath(Path.Combine(GetProjectPath(), projectRelativePath));
        }
    }
}
#endif
