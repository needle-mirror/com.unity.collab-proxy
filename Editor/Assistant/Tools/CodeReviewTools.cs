#if AIA_PRESENT
using System.Collections.Generic;
using System.Threading.Tasks;
using Codice.CM.Common;
using PlasticGui.WorkspaceWindow.Configuration;
using CodeReviewStatus = Codice.CM.Common.CodeReviewStatus;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    static class CodeReviewTools
    {
        internal enum CodeCommentType
        {
            Change,
            Question
        }

        internal enum ReplyCommentType
        {
            Answer,
            Discard
        }

        internal static async Task<ReviewInfo> CreateCodeReviewForBranch(
            string branchName)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.CreateCodeReviewForBranch(wkInfo, branchName);
        }

        internal static async Task<ReviewInfo> CreateCodeReviewForChangeset(
            long changesetId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.CreateCodeReviewForChangeset(
                wkInfo, changesetId);
        }

        internal static async Task<long> CreateConversationComment(
            long reviewId,
            string commentText)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.AddCodeReviewComment(
                wkInfo, reviewId, -1, -1, "-1", commentText,
                CodeReviewCommentType.Conversation);
        }

        internal static async Task<long> CreateCodeComment(
            long reviewId,
            long revisionId,
            long changesetId,
            string locationSpec,
            string commentText,
            CodeCommentType commentType)
        {
            CodeReviewCommentType type = commentType == CodeCommentType.Change
                ? CodeReviewCommentType.Change
                : CodeReviewCommentType.Question;

            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.AddCodeReviewComment(
                wkInfo, reviewId, revisionId, changesetId, locationSpec, commentText, type);
        }

        internal static async Task<string> GetCodeReviewLink(
            long reviewId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.GetCodeReviewLink(wkInfo, reviewId);
        }

        internal static async Task AddCurrentUserAsReviewer(
            long reviewId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.AddCurrentUserAsReviewer(wkInfo, reviewId);
        }

        internal static async Task SetReviewStatus(
            long reviewId,
            CodeReviewStatus status,
            string comment)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.UpdateCurrentUserReviewerStatus(
                wkInfo, reviewId, status, comment);
        }

        internal static async Task<List<ReviewInfo>> GetReviewsInStatus(
            CodeReviewStatus status,
            long targetId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            var queryResult = await UnityVersionControlApiProvider.Instance.FindQuery(
                wkInfo,
                "find review where status = " + (int)status +
                " and targetid = " + targetId);

            var result = new List<ReviewInfo>();
            if (queryResult.Result.Length > 0 && queryResult.Result[0] != null)
            {
                foreach (ReviewInfo review in queryResult.Result[0])
                    result.Add(review);
            }
            return result;
        }

        internal static async Task<List<CodeReviewCommentInfo>> GetReviewComments(
            long reviewId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            var queryResult = await UnityVersionControlApiProvider.Instance.FindQuery(
                wkInfo, "find changereviewcomment where reviewid = " + reviewId);

            var result = new List<CodeReviewCommentInfo>();
            if (queryResult.Result.Length > 0 && queryResult.Result[0] != null)
            {
                foreach (CodeReviewCommentInfo comment in queryResult.Result[0])
                    result.Add(comment);
            }
            return result;
        }

        internal static async Task ReRequestReview(
            long reviewId)
        {
            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            await UnityVersionControlApiProvider.Instance.ReRequestReview(wkInfo, reviewId);
        }

        internal static async Task<long> ReplyComment(
            long reviewId,
            long parentCommentId,
            string commentText,
            ReplyCommentType replyType)
        {
            CodeReviewCommentType type = replyType == ReplyCommentType.Discard
                ? CodeReviewCommentType.Discarded
                : CodeReviewCommentType.Comment;

            var wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();
            return await UnityVersionControlApiProvider.Instance.ReplyComment(
                wkInfo, reviewId, parentCommentId, commentText, type);
        }
    }
}
#endif
