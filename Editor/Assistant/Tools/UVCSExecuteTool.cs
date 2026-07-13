#if AIA_PRESENT
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlasticGui;
using Unity.AI.Assistant.FunctionCalling;
using Unity.Plastic.Newtonsoft.Json.Linq;
using Unity.PlasticSCM.Editor.Assistant.UI.Interactions;
using CodeReviewStatus = Codice.CM.Common.CodeReviewStatus;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class UVCSExecuteTool
    {
        [AgentTool(
            "Single entry point for ALL Unity Version Control (UVCS) operations. "
            + "Pick the 'operation' to run and pass its arguments as a JSON object string in 'parametersJson'. "
            + "Each operation maps to a specific UVCS capability (querying branches/changesets/history/reviews/locks/labels, "
            + "diffing, checkin/shelve, undo/revert, navigating the UVCS UI, and asking the user to choose or confirm). "
            + "Refer to the UVCS tool reference for the exact JSON parameter shape and return value of each operation. "
            + "Example: operation=FindQuery, parametersJson={\"query\":\"find branch where name like 'scm%'\"}. "
            + "Operations that take no parameters can omit 'parametersJson'.",
            "Unity.UVCS.Execute")]
        internal static async Task<object> Execute(
            ToolExecutionContext context,
            [ToolParameter("The UVCS operation to run. Must be one of the listed values.")]
            UVCSOperation operation,
            [ToolParameter("JSON object string with the parameters for the chosen operation "
                + "(e.g. {\"branchName\":\"/main/task001\"}). See the UVCS tool reference for the exact "
                + "shape of each operation. Omit for operations that take no parameters.")]
            string parametersJson = null)
        {
            JObject p = ParseParams(operation, parametersJson);

            switch (operation)
            {
                // Onboarding / project management
                case UVCSOperation.GetKnownServers:
                    return await ProjectManagementTools.GetKnownUVCSServers();
                case UVCSOperation.GetAllRepositories:
                    return await ProjectManagementTools.GetAllRepositories(
                        GetParam<string>(p, operation, "server"));
                case UVCSOperation.GetAllProjects:
                    return await ProjectManagementTools.GetAllProjects(
                        GetParam<string>(p, operation, "server"));
                case UVCSOperation.GetWorkspaceFromPath:
                    return await ProjectManagementTools.GetWorkspaceFromPath(
                        GetParam<string>(p, operation, "path"));
                case UVCSOperation.CreateRepository:
                    await ProjectManagementTools.CreateRepository(
                        GetParam<string>(p, operation, "repositoryName"),
                        GetParam<string>(p, operation, "server"));
                    return null;
                case UVCSOperation.CreateWorkspace:
                    return await ProjectManagementTools.CreateWorkspace(
                        context,
                        GetParam<string>(p, operation, "workspaceName"),
                        GetParam<string>(p, operation, "workspacePath"),
                        GetParam<string>(p, operation, "repositoryName"));
                case UVCSOperation.PerformInitialCheckin:
                    await ProjectManagementTools.PerformInitialCheckin(context);
                    return null;
                case UVCSOperation.RefreshUIAfterCreateWorkspace:
                    await ProjectManagementTools.RefreshUIAfterCreateWorkspace();
                    return null;
                case UVCSOperation.CheckinPendingChanges:
                    return await ProjectManagementTools.CheckinPendingChanges(
                        context, GetParam<string>(p, operation, "comment"));
                case UVCSOperation.ShelvePendingChanges:
                    return await ProjectManagementTools.ShelvePendingChanges(
                        context,
                        GetParam<string>(p, operation, "comment"),
                        GetOptionalParam(p, "undoPendingChangesAfterShelve", false));

                // Query
                case UVCSOperation.FindQuery:
                    return await QueryTools.FindQuery(GetParam<string>(p, operation, "query"));
                case UVCSOperation.GetWorkspaceConfiguration:
                    return await QueryTools.GetWorkspaceConfiguration();

                // Branches
                case UVCSOperation.GetBranchInfo:
                    return await BranchTools.GetBranchInfo(GetParam<string>(p, operation, "branchName"));
                case UVCSOperation.CreateBranch:
                    await BranchTools.CreateBranch(
                        GetParam<string>(p, operation, "branchName"),
                        GetOptionalParam<string>(p, "comment", null),
                        GetOptionalParam<string>(p, "parentBranchName", null),
                        GetOptionalParam(p, "changesetId", -1L));
                    return null;
                case UVCSOperation.RenameBranch:
                    await BranchTools.RenameBranch(
                        GetParam<string>(p, operation, "branchName"),
                        GetParam<string>(p, operation, "newBranchName"));
                    return null;
                case UVCSOperation.LaunchSwitchToBranchUI:
                    await BranchTools.LaunchSwitchToBranchUI(GetParam<string>(p, operation, "branchName"));
                    return null;

                // Annotate / revisions
                case UVCSOperation.GetFileAnnotations:
                    return await AnnotateTools.GetFileAnnotations(
                        GetParam<string>(p, operation, "filePath"),
                        GetOptionalParam(p, "revisionId", -1L));
                case UVCSOperation.GetRevisionInfo:
                    return await AnnotateTools.GetRevisionInfo(GetParam<long>(p, operation, "revisionId"));

                // History
                case UVCSOperation.GetFileHistory:
                    return await HistoryTools.GetFileHistory(GetParam<string>(p, operation, "filePath"));

                // Diff
                case UVCSOperation.GetBranchDifferences:
                    return await DiffTools.GetBranchDifferences(GetParam<string>(p, operation, "branchName"));
                case UVCSOperation.GetChangesetDifferences:
                    return await DiffTools.GetChangesetDifferences(GetParam<long>(p, operation, "changesetId"));
                case UVCSOperation.GetChangesetsDifferences:
                    return await DiffTools.GetChangesetsDifferences(
                        GetParam<long>(p, operation, "srcChangesetId"),
                        GetParam<long>(p, operation, "dstChangesetId"));
                case UVCSOperation.GetLabelDifferences:
                    return await DiffTools.GetLabelDifferences(GetParam<string>(p, operation, "labelName"));
                case UVCSOperation.GetShelveDifferences:
                    return await DiffTools.GetShelveDifferences(GetParam<long>(p, operation, "shelveId"));
                case UVCSOperation.GetPendingChanges:
                    return await DiffTools.GetPendingChanges();
                case UVCSOperation.DownloadRevisionsToFiles:
                    return await DiffTools.DownloadRevisionsToFiles(
                        context, GetParam<List<long>>(p, operation, "revisionIds"));
                case UVCSOperation.CleanupDiffTempFiles:
                    await DiffTools.CleanupDiffTempFiles(context);
                    return null;

                // Changesets
                case UVCSOperation.GetChangesetInfo:
                    return await ChangesetTools.GetChangesetInfo(GetParam<long>(p, operation, "changesetId"));

                // Code review
                case UVCSOperation.CreateCodeReviewForBranch:
                    return await CodeReviewTools.CreateCodeReviewForBranch(
                        GetParam<string>(p, operation, "branchName"));
                case UVCSOperation.CreateCodeReviewForChangeset:
                    return await CodeReviewTools.CreateCodeReviewForChangeset(
                        GetParam<long>(p, operation, "changesetId"));
                case UVCSOperation.CreateConversationComment:
                    return await CodeReviewTools.CreateConversationComment(
                        GetParam<long>(p, operation, "reviewId"),
                        GetParam<string>(p, operation, "commentText"));
                case UVCSOperation.CreateCodeComment:
                    return await CodeReviewTools.CreateCodeComment(
                        GetParam<long>(p, operation, "reviewId"),
                        GetParam<long>(p, operation, "revisionId"),
                        GetParam<long>(p, operation, "changesetId"),
                        GetParam<string>(p, operation, "locationSpec"),
                        GetParam<string>(p, operation, "commentText"),
                        GetParam<CodeReviewTools.CodeCommentType>(p, operation, "commentType"));
                case UVCSOperation.GetCodeReviewLink:
                    return await CodeReviewTools.GetCodeReviewLink(GetParam<long>(p, operation, "reviewId"));
                case UVCSOperation.AddCurrentUserAsReviewer:
                    await CodeReviewTools.AddCurrentUserAsReviewer(GetParam<long>(p, operation, "reviewId"));
                    return null;
                case UVCSOperation.SetReviewStatus:
                    await CodeReviewTools.SetReviewStatus(
                        GetParam<long>(p, operation, "reviewId"),
                        GetParam<CodeReviewStatus>(p, operation, "status"),
                        GetParam<string>(p, operation, "comment"));
                    return null;
                case UVCSOperation.GetReviewsInStatus:
                    return await CodeReviewTools.GetReviewsInStatus(
                        GetParam<CodeReviewStatus>(p, operation, "status"),
                        GetParam<long>(p, operation, "targetId"));
                case UVCSOperation.GetReviewComments:
                    return await CodeReviewTools.GetReviewComments(GetParam<long>(p, operation, "reviewId"));
                case UVCSOperation.ReplyComment:
                    return await CodeReviewTools.ReplyComment(
                        GetParam<long>(p, operation, "reviewId"),
                        GetParam<long>(p, operation, "parentCommentId"),
                        GetParam<string>(p, operation, "commentText"),
                        GetParam<CodeReviewTools.ReplyCommentType>(p, operation, "replyType"));
                case UVCSOperation.ReRequestReview:
                    await CodeReviewTools.ReRequestReview(GetParam<long>(p, operation, "reviewId"));
                    return null;

                // Labels
                case UVCSOperation.CreateLabel:
                    return await LabelTools.CreateLabel(
                        GetParam<string>(p, operation, "labelName"),
                        GetParam<string>(p, operation, "comment"),
                        GetOptionalParam(p, "changesetId", -1L));
                case UVCSOperation.RenameLabel:
                    await LabelTools.RenameLabel(
                        GetParam<string>(p, operation, "labelName"),
                        GetParam<string>(p, operation, "newLabelName"));
                    return null;
                case UVCSOperation.ApplyLabel:
                    await LabelTools.ApplyLabel(GetParam<string>(p, operation, "labelName"));
                    return null;
                case UVCSOperation.DeleteLabel:
                    await LabelTools.DeleteLabel(GetParam<string>(p, operation, "labelName"));
                    return null;

                // Locks
                case UVCSOperation.ListLocks:
                    return await LockTools.ListLocks();
                case UVCSOperation.GetLockRule:
                    return await LockTools.GetLockRule();
                case UVCSOperation.ReleaseLocks:
                    await LockTools.ReleaseLocks(GetParam<List<long>>(p, operation, "lockIds"));
                    return null;
                case UVCSOperation.RemoveLocks:
                    await LockTools.RemoveLocks(GetParam<List<long>>(p, operation, "lockIds"));
                    return null;

                // Undo / revert
                case UVCSOperation.UndoChanges:
                    await UndoTools.UndoChanges(context, GetParam<List<string>>(p, operation, "paths"));
                    return null;
                case UVCSOperation.UndoUnchanged:
                    await UndoTools.UndoUnchanged();
                    return null;
                case UVCSOperation.RevertToRevision:
                    await UndoTools.RevertToRevision(
                        context,
                        GetParam<string>(p, operation, "filePath"),
                        GetParam<long>(p, operation, "revisionId"));
                    return null;

                // UI navigation
                case UVCSOperation.RefreshView:
                    await UITools.RefreshView(GetParam<ViewType>(p, operation, "viewType"));
                    return null;
                case UVCSOperation.ShowView:
                    await UITools.ShowView(GetParam<ViewType>(p, operation, "viewType"));
                    return null;
                case UVCSOperation.ShowBranchExplorer:
                    await UITools.ShowBranchExplorer();
                    return null;
                case UVCSOperation.ShowFileHistory:
                    await UITools.ShowFileHistory(GetParam<string>(p, operation, "filePath"));
                    return null;
                case UVCSOperation.ShowBranchesView:
                    await UITools.ShowBranchesView(GetOptionalParam<string>(p, "branchName", null));
                    return null;
                case UVCSOperation.ShowChangesetsView:
                    await UITools.ShowChangesetsView(GetOptionalParam(p, "changesetId", -1L));
                    return null;
                case UVCSOperation.ShowShelvesView:
                    await UITools.ShowShelvesView(GetOptionalParam(p, "shelveId", -1L));
                    return null;
                case UVCSOperation.LaunchMergeFromBranchUI:
                    await UITools.LaunchMergeFromBranchUI(GetParam<string>(p, operation, "branchName"));
                    return null;
                case UVCSOperation.LaunchMergeFromChangesetUI:
                    await UITools.LaunchMergeFromChangesetUI(GetParam<long>(p, operation, "changesetId"));
                    return null;
                case UVCSOperation.LaunchApplyShelveUI:
                    await UITools.LaunchApplyShelveUI(GetParam<long>(p, operation, "shelveId"));
                    return null;

                // User interaction
                case UVCSOperation.SelectFromList:
                    return await InteractionTools.SelectFromList(
                        context,
                        GetParam<string>(p, operation, "title"),
                        GetParam<string>(p, operation, "message"),
                        GetParam<string>(p, operation, "label"),
                        GetParam<List<string>>(p, operation, "choices"));
                case UVCSOperation.SelectOption:
                    return await InteractionTools.SelectOption(
                        context,
                        GetParam<string>(p, operation, "title"),
                        GetParam<string>(p, operation, "message"),
                        GetParam<List<OptionChoice>>(p, operation, "options"));
                case UVCSOperation.AskForConfirmation:
                    return await InteractionTools.AskForConfirmation(
                        context,
                        GetParam<string>(p, operation, "action"),
                        GetParam<string>(p, operation, "detail"));

                default:
                    throw new ArgumentException(
                        "Unknown UVCS operation '" + operation + "'.");
            }
        }

        static JObject ParseParams(UVCSOperation operation, string parametersJson)
        {
            if (string.IsNullOrWhiteSpace(parametersJson))
                return new JObject();

            try
            {
                return JObject.Parse(parametersJson);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    "Could not parse parametersJson for operation '" + operation + "': " + ex.Message +
                    ". Provide a JSON object, for example {\"branchName\":\"/main/task001\"}.");
            }
        }

        static T GetParam<T>(JObject p, UVCSOperation operation, string name)
        {
            JToken token = p[name];
            if (token == null || token.Type == JTokenType.Null)
                throw new ArgumentException(
                    "Operation '" + operation + "' requires parameter '" + name + "' in parametersJson.");

            try
            {
                return token.ToObject<T>();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    "Parameter '" + name + "' for operation '" + operation + "' is invalid: " + ex.Message);
            }
        }

        static T GetOptionalParam<T>(JObject p, string name, T defaultValue)
        {
            JToken token = p[name];
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            return token.ToObject<T>();
        }
    }
}
#endif
