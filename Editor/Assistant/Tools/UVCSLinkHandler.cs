#if AIA_PRESENT
using System;
using PlasticGui;
using Unity.AI.Assistant;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    // URL schema for uvcs:// links:
    //
    // show/changeset/<id>            → Open Changesets view, select changeset
    // show/branch/<branchName>       → Open Branches view, select branch
    // show/shelve/<id>               → Open Shelves view, select shelve
    // show/history/<filePath>        → Open file history view
    //                                  (filePath is project-relative, e.g. Assets/Foo.cs)
    //
    // open/<ViewType>                → Open a UVCS view tab (uses ViewType enum names)
    // open/BranchExplorerView        → Open the Branch Explorer window
    //
    // refresh/<ViewType>             → Refresh a UVCS view
    //
    // merge/branch/<branchName>      → Launch merge from branch
    // merge/changeset/<id>           → Launch merge from changeset
    // merge/shelve/<id>              → Launch apply shelve UI

    [LinkHandler(UVCS_PREFIX)]
    internal class UVCSLinkHandler : ILinkHandler
    {
        const string UVCS_PREFIX = "uvcs";

        internal const string k_MalformedLinkMsg =
            @"Invalid UVCS link ""{0}"": the link format is not recognized.";

        const string k_ActionShow = "show";
        const string k_ActionOpen = "open";
        const string k_ActionRefresh = "refresh";
        const string k_ActionMerge = "merge";

        const string k_EntityChangeset = "changeset";
        const string k_EntityBranch = "branch";
        const string k_EntityShelve = "shelve";
        const string k_EntityHistory = "history";

        void ILinkHandler.Handle(ILinkHandler.Context context, string prefix, string url)
        {
            if (prefix != UVCS_PREFIX)
                return;

            HandleLink(url);
        }

        internal static void HandleLink(string url)
        {
            var parts = url.Split(new[] { '/' }, 2);
            if (parts.Length < 2)
            {
                LogMalformedLink(url);
                return;
            }

            string action = parts[0];
            string remainder = parts[1];

            switch (action)
            {
                case k_ActionShow:
                    HandleShow(remainder, url);
                    break;
                case k_ActionOpen:
                    HandleOpen(remainder, url);
                    break;
                case k_ActionRefresh:
                    HandleRefresh(remainder, url);
                    break;
                case k_ActionMerge:
                    HandleMerge(remainder, url);
                    break;
                default:
                    LogMalformedLink(url);
                    break;
            }
        }

        static void HandleShow(string remainder, string fullUrl)
        {
            var parts = remainder.Split(new[] { '/' }, 2);
            if (parts.Length < 2 || string.IsNullOrEmpty(parts[1]))
            {
                LogMalformedLink(fullUrl);
                return;
            }

            string entityType = parts[0];
            string entityValue = parts[1];

            switch (entityType)
            {
                case k_EntityChangeset:
                    if (!long.TryParse(entityValue, out long changesetId))
                    {
                        LogMalformedLink(fullUrl);
                        return;
                    }
                    FireAndForget(async () =>
                    {
                        var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
                        await UnityVersionControlApiProvider.Instance.ShowChangesetsView(wkInfo, changesetId);
                    });
                    break;
                case k_EntityBranch:
                    string branchName = "/" + entityValue;
                    FireAndForget(async () =>
                    {
                        var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
                        await UnityVersionControlApiProvider.Instance.ShowBranchesView(wkInfo, branchName);
                    });
                    break;
                case k_EntityShelve:
                    if (!long.TryParse(entityValue, out long shelveId))
                    {
                        LogMalformedLink(fullUrl);
                        return;
                    }
                    FireAndForget(async () =>
                    {
                        var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
                        await UnityVersionControlApiProvider.Instance.ShowShelvesView(wkInfo, shelveId);
                    });
                    break;
                case k_EntityHistory:
                    string fullPath = UVCSToolContext.GetFullPath(entityValue);
                    FireAndForget(async () =>
                    {
                        var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
                        await UnityVersionControlApiProvider.Instance.ShowFileHistory(wkInfo, fullPath);
                    });
                    break;
                default:
                    LogMalformedLink(fullUrl);
                    break;
            }
        }

        static void HandleOpen(string remainder, string fullUrl)
        {
            var parts = remainder.Split(new[] { '/' }, 2);
            string viewName = parts[0];

            if (!TryParseViewType(viewName, out ViewType viewType))
            {
                LogMalformedLink(fullUrl);
                return;
            }

            switch (viewType)
            {
                case ViewType.BranchExplorerView:
                    FireAndForget(async () =>
                    {
                        await UnityVersionControlApiProvider.Instance.ShowBranchExplorer();
                    });
                    break;
                default:
                    FireAndForget(async () =>
                    {
                        await UnityVersionControlApiProvider.Instance.ShowView(viewType);
                    });
                    break;
            }
        }

        static void HandleRefresh(string remainder, string fullUrl)
        {
            if (!TryParseViewType(remainder, out ViewType viewType))
            {
                LogMalformedLink(fullUrl);
                return;
            }
            FireAndForget(async () =>
            {
                await UnityVersionControlApiProvider.Instance.RefreshView(viewType);
            });
        }

        static void HandleMerge(string remainder, string fullUrl)
        {
            var parts = remainder.Split(new[] { '/' }, 2);
            if (parts.Length < 2 || string.IsNullOrEmpty(parts[1]))
            {
                LogMalformedLink(fullUrl);
                return;
            }

            string entityType = parts[0];
            string entityValue = parts[1];

            switch (entityType)
            {
                case k_EntityBranch:
                    string branchName = "/" + entityValue;
                    FireAndForget(async () =>
                    {
                        var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
                        await UnityVersionControlApiProvider.Instance.LaunchMergeFromBranchUI(wkInfo, branchName);
                    });
                    break;
                case k_EntityChangeset:
                    if (!long.TryParse(entityValue, out long changesetId))
                    {
                        LogMalformedLink(fullUrl);
                        return;
                    }
                    FireAndForget(async () =>
                    {
                        var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
                        await UnityVersionControlApiProvider.Instance.LaunchMergeFromChangesetUI(
                            wkInfo, changesetId);
                    });
                    break;
                case k_EntityShelve:
                    if (!long.TryParse(entityValue, out long shelveId))
                    {
                        LogMalformedLink(fullUrl);
                        return;
                    }
                    FireAndForget(async () =>
                    {
                        var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
                        await UnityVersionControlApiProvider.Instance.LaunchApplyShelveUI(wkInfo, shelveId);
                    });
                    break;
                default:
                    LogMalformedLink(fullUrl);
                    break;
            }
        }

        static bool TryParseViewType(string value, out ViewType viewType)
        {
            return Enum.TryParse(value, out viewType)
                && Enum.IsDefined(typeof(ViewType), viewType);
        }

        static void LogMalformedLink(string url)
        {
            Debug.LogWarningFormat(k_MalformedLinkMsg, url);
        }

        static async void FireAndForget(Func<System.Threading.Tasks.Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
#endif
