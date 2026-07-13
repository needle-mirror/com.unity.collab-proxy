using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Codice.Client.BaseCommands;
using Codice.Client.Commands;
using Codice.Client.Common;
using Codice.Client.Common.Servers;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.MergeRequest;
using PlasticGui.WorkspaceWindow.MergeRequest.ReviewChanges.Comments;
using PlasticGui.WorkspaceWindow.Merge;
using Unity.PlasticSCM.Editor.Views.CreateWorkspace;
using CodeReviewStatus = Codice.CM.Common.CodeReviewStatus;

namespace Unity.PlasticSCM.Editor.Api
{
    internal interface IUnityVersionControlApi
    {
        Task<List<string>> GetKnownServers();
        Task<List<RepositoryInfo>> GetAllRepositories(string server);
        Task<List<RepositoryInfo>> GetAllProjects(string server);
        Task<WorkspaceInfo> GetWorkspaceFromPath(string path);
        Task CreateRepository(string repositoryName, string server);
        Task<WorkspaceInfo> CreateWorkspace(
            string workspaceName,
            string workspacePath,
            string repositoryName);
        Task PerformInitialCheckin(WorkspaceInfo wkInfo);
        Task RefreshUIAfterCreateWorkspace(WorkspaceInfo wkInfo);
        Task<QueryResult> FindQuery(WorkspaceInfo wkInfo, string query);
        Task<SelectorInformation> GetSelectorUserInformation(WorkspaceInfo wkInfo);
        Task<BranchInfo> GetBranchInfo(
            WorkspaceInfo wkInfo,
            string branchName);
        Task CreateBranch(
            WorkspaceInfo wkInfo,
            string branchName,
            string comment = null,
            string parentBranchName = null,
            long changesetId = -1);
        Task LaunchSwitchToBranchUI(
            WorkspaceInfo wkInfo,
            string branchName);
        Task<List<AnnotatedLine>> GetAnnotations(
            WorkspaceInfo wkInfo,
            string fullPath,
            long revisionId = -1);
        Task<RevisionInfo> GetRevisionInfo(
            WorkspaceInfo wkInfo,
            long revisionId);
        Task<List<ClientDiff>> GetBranchDifferences(
            WorkspaceInfo wkInfo,
            string branchName);
        Task<List<ClientDiff>> GetChangesetDifferences(
            WorkspaceInfo wkInfo,
            long changesetId);
        Task<List<ClientDiff>> GetChangesetsDifferences(
            WorkspaceInfo wkInfo,
            long srcChangesetId,
            long dstChangesetId);
        Task<List<ClientDiff>> GetLabelDifferences(
            WorkspaceInfo wkInfo,
            string labelName);
        Task<List<ClientDiff>> GetShelveDifferences(
            WorkspaceInfo wkInfo,
            long shelveId);
        Task<List<ChangeInfo>> GetPendingChanges(
            WorkspaceInfo wkInfo);
        Task DownloadRevisionsToFiles(
            WorkspaceInfo wkInfo,
            List<long> revisionIds,
            string[] destPaths);
        Task<List<RepObjectInfo>> GetFileHistory(
            WorkspaceInfo wkInfo,
            string fullPath);
        Task<ChangesetInfo> GetChangesetInfo(
            WorkspaceInfo wkInfo,
            long changesetId);
        Task<ReviewInfo> CreateCodeReviewForBranch(
            WorkspaceInfo wkInfo,
            string branchName);
        Task<ReviewInfo> CreateCodeReviewForChangeset(
            WorkspaceInfo wkInfo,
            long changesetId);
        Task<long> AddCodeReviewComment(
            WorkspaceInfo wkInfo,
            long reviewId,
            long revisionId,
            long changesetId,
            string locationSpec,
            string commentText,
            CodeReviewCommentType commentType);
        Task<string> GetCodeReviewLink(
            WorkspaceInfo wkInfo,
            long reviewId);
        Task AddCurrentUserAsReviewer(
            WorkspaceInfo wkInfo,
            long reviewId);
        Task UpdateCurrentUserReviewerStatus(
            WorkspaceInfo wkInfo,
            long reviewId,
            CodeReviewStatus status,
            string comment);

        Task<long> CheckinPendingChanges(
            WorkspaceInfo wkInfo,
            string comment);

        Task<long> ShelvePendingChanges(
            WorkspaceInfo wkInfo,
            string comment,
            bool undoShelvedChanges = false);

        Task<long> ReplyComment(
            WorkspaceInfo wkInfo,
            long reviewId,
            long parentCommentId,
            string commentText,
            CodeReviewCommentType commentType);

        Task ReRequestReview(
            WorkspaceInfo wkInfo,
            long reviewId);

        Task RefreshView(ViewType viewType);
        Task ShowView(ViewType viewType);
        Task ShowBranchExplorer();
        Task ShowFileHistory(WorkspaceInfo wkInfo, string fullPath);
        Task ShowBranchesView(WorkspaceInfo wkInfo, string branchName);
        Task ShowChangesetsView(WorkspaceInfo wkInfo, long changesetId);
        Task ShowShelvesView(WorkspaceInfo wkInfo, long shelveId);
        Task<List<LockInfo>> ListLocks(WorkspaceInfo wkInfo);
        Task<LockRule> GetLockRule(WorkspaceInfo wkInfo);
        Task ReleaseLocks(WorkspaceInfo wkInfo, List<long> lockIds);
        Task RemoveLocks(WorkspaceInfo wkInfo, List<long> lockIds);
        Task UndoChanges(WorkspaceInfo wkInfo, List<string> fullPaths);
        Task UndoUnchanged(WorkspaceInfo wkInfo);
        Task RevertToRevision(WorkspaceInfo wkInfo, string fullPath, long revisionId);
        Task LaunchMergeFromBranchUI(WorkspaceInfo wkInfo, string branchName);
        Task LaunchMergeFromChangesetUI(WorkspaceInfo wkInfo, long changesetId);
        Task LaunchApplyShelveUI(WorkspaceInfo wkInfo, long shelveId);
        Task<MarkerInfo> CreateLabel(
            WorkspaceInfo wkInfo,
            string labelName,
            long changeset,
            string comment);
        Task ApplyLabel(
            WorkspaceInfo wkInfo,
            string labelName);
        Task RenameBranch(
            WorkspaceInfo wkInfo,
            string branchName,
            string newBranchName);
        Task RenameLabel(
            WorkspaceInfo wkInfo,
            string labelName,
            string newLabelName);
        Task DeleteLabel(
            WorkspaceInfo wkInfo,
            string labelName);
    }

    internal class UnityVersionControlApi : IUnityVersionControlApi
    {
        internal UnityVersionControlApi() : this(new UnityVersionControlUiApi()) { }

        internal UnityVersionControlApi(IUnityVersionControlUiApi uiApi)
        {
            mUIApi = uiApi ?? throw new ArgumentNullException(nameof(uiApi));
        }

        Task<List<string>> IUnityVersionControlApi.GetKnownServers()
        {
            return Task.Run(() =>
                {
                    List<string> result = new List<string>();

                    foreach (string server in KnownServers.Get(
                                 PlasticGui.Plastic.WebRestAPI,
                                 CmConnection.Get().GetProfileManager()))
                    {
                        result.Add(ResolveServer.ToDisplayString(server));
                    }

                    return result;
                }
            );
        }

        Task<List<RepositoryInfo>> IUnityVersionControlApi.GetAllRepositories(string server)
        {
            return Task.Run(() => PlasticGui.Plastic.API.GetAllRepositories(
                server, true, RepositoryType.VCS));
        }

        Task<List<RepositoryInfo>> IUnityVersionControlApi.GetAllProjects(string server)
        {
            return Task.Run(() => PlasticGui.Plastic.API.GetAllRepositories(
                server, true, RepositoryType.Project));
        }

        Task<WorkspaceInfo> IUnityVersionControlApi.GetWorkspaceFromPath(string path)
        {
            return Task.Run(() => FindWorkspace.InfoForApplicationPath(
                path, PlasticGui.Plastic.API));
        }

        Task IUnityVersionControlApi.CreateRepository(string repositoryName, string server)
        {
            return Task.Run(() => { PlasticGui.Plastic.API.CreateRepository(server, repositoryName); });
        }

        Task<WorkspaceInfo> IUnityVersionControlApi.CreateWorkspace(
            string workspaceName,
            string workspacePath,
            string repositoryName)
        {
            return Task.Run(() => PlasticGui.Plastic.API.CreateWorkspace(
                workspacePath,
                workspaceName,
                repositoryName));
        }

        Task IUnityVersionControlApi.PerformInitialCheckin(WorkspaceInfo wkInfo)
        {
            return Task.Run(() =>
            {
                PerformInitialCheckin.ForWorkspace(
                    wkInfo, false, PlasticGui.Plastic.API);
            });
        }

        Task IUnityVersionControlApi.RefreshUIAfterCreateWorkspace(WorkspaceInfo wkInfo)
        {
            return mUIApi.RefreshUIAfterCreateWorkspace(wkInfo);
        }

        Task<SelectorInformation> IUnityVersionControlApi.GetSelectorUserInformation(WorkspaceInfo wkInfo)
        {
            return Task.Run(() => PlasticGui.Plastic.API.GetSelectorUserInformation(wkInfo));
        }

        Task<QueryResult> IUnityVersionControlApi.FindQuery(
            WorkspaceInfo wkInfo,
            string query)
        {
            return Task.Run(() => PlasticGui.Plastic.API.FindQuery(wkInfo, query));
        }

        Task<BranchInfo> IUnityVersionControlApi.GetBranchInfo(
            WorkspaceInfo wkInfo,
            string branchName)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                return ResolveBranch(repSpec, branchName);
            });
        }

        Task IUnityVersionControlApi.CreateBranch(
            WorkspaceInfo wkInfo,
            string branchName,
            string comment,
            string parentBranchName,
            long changesetId)
        {
            return Task.Run(() =>
                {
                    try
                    {
                        RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);

                        BranchInfo parentBranch = parentBranchName == null
                            ? PlasticGui.Plastic.API.GetMainBranch(wkInfo)
                            : ResolveBranch(repSpec, parentBranchName);

                        if (changesetId != -1)
                        {
                            PlasticGui.Plastic.API.CreateChildBranchFromChangeset(
                                repSpec,
                                parentBranch,
                                changesetId,
                                branchName,
                                comment);
                            return;
                        }

                        PlasticGui.Plastic.API.CreateChildBranch(
                            repSpec,
                            parentBranch,
                            branchName,
                            comment);
                    }
                    finally
                    {
                        mUIApi.RefreshViews(
                            ViewType.BranchesView,
                            ViewType.BranchExplorerView);
                    }
                }
            );
        }

        Task IUnityVersionControlApi.LaunchSwitchToBranchUI(
            WorkspaceInfo wkInfo,
            string branchName)
        {
            return Task.Run(async () =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                BranchInfo branchInfo = ResolveBranch(repSpec, branchName);
                await mUIApi.LaunchSwitchToBranchUI(wkInfo, branchInfo);
            });
        }

        Task<List<AnnotatedLine>> IUnityVersionControlApi.GetAnnotations(
            WorkspaceInfo wkInfo,
            string fullPath,
            long revisionId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec;
                RevisionInfo revInfo;

                if (revisionId >= 0)
                {
                    repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                    revInfo = PlasticGui.Plastic.API.GetRevisionInfo(repSpec, revisionId);
                }
                else
                {
                    var wkNode = PlasticGui.Plastic.API.GetWorkspaceTreeNode(
                        wkInfo, fullPath);
                    repSpec = wkNode.RepSpec;
                    revInfo = wkNode.RevInfo;
                }

                return PlasticGui.Plastic.API.GetAnnotations(
                    wkInfo,
                    repSpec,
                    revInfo,
                    Path.GetExtension(fullPath),
                    fullPath,
                    null,
                    null);
            });
        }

        Task<RevisionInfo> IUnityVersionControlApi.GetRevisionInfo(
            WorkspaceInfo wkInfo,
            long revisionId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                return PlasticGui.Plastic.API.GetRevisionInfo(repSpec, revisionId);
            });
        }

        Task<List<ClientDiff>> IUnityVersionControlApi.GetBranchDifferences(
            WorkspaceInfo wkInfo,
            string branchName)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                BranchInfo brInfo = ResolveBranch(repSpec, branchName);

                long commonAncestorId = CalculateCommonAncestorIdForDiffs(repSpec, brInfo);

                return commonAncestorId == -1
                    ? PlasticGui.Plastic.API.GetBranchDifferences(repSpec, brInfo)
                    : PlasticGui.Plastic.API.GetBranchDifferencesWithoutMergeTracking(
                        repSpec, brInfo, commonAncestorId);
            });
        }

        Task<List<ClientDiff>> IUnityVersionControlApi.GetChangesetDifferences(
            WorkspaceInfo wkInfo,
            long changesetId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                ChangesetInfo csetInfo = ResolveChangeset(repSpec, changesetId);
                return PlasticGui.Plastic.API.GetChangesetDifferences(repSpec, csetInfo);
            });
        }

        Task<List<ClientDiff>> IUnityVersionControlApi.GetChangesetsDifferences(
            WorkspaceInfo wkInfo,
            long srcChangesetId,
            long dstChangesetId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                ChangesetInfo srcCsetInfo = ResolveChangeset(repSpec, srcChangesetId);
                ChangesetInfo dstCsetInfo = ResolveChangeset(repSpec, dstChangesetId);
                return PlasticGui.Plastic.API.GetChangesetsDifferences(
                    repSpec, srcCsetInfo, dstCsetInfo);
            });
        }

        Task<List<ClientDiff>> IUnityVersionControlApi.GetLabelDifferences(
            WorkspaceInfo wkInfo,
            string labelName)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                MarkerInfo markerInfo = ResolveLabel(repSpec, labelName);
                ChangesetInfo csetInfo = ResolveChangeset(repSpec, markerInfo.Changeset);
                return PlasticGui.Plastic.API.GetChangesetDifferences(repSpec, csetInfo);
            });
        }

        Task<List<ClientDiff>> IUnityVersionControlApi.GetShelveDifferences(
            WorkspaceInfo wkInfo,
            long shelveId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                ChangesetInfo csetInfo = ResolveChangeset(repSpec, shelveId);
                return PlasticGui.Plastic.API.GetChangesetDifferences(repSpec, csetInfo);
            });
        }

        Task<List<ChangeInfo>> IUnityVersionControlApi.GetPendingChanges(
            WorkspaceInfo wkInfo)
        {
            return Task.Run(() => PlasticGui.Plastic.API.GetChanges(
                wkInfo,
                new List<string> { wkInfo.ClientPath },
                WorkspaceStatusOptions.FindAllControlledChanges |
                WorkspaceStatusOptions.FindAllLocalChanges));
        }

        Task IUnityVersionControlApi.DownloadRevisionsToFiles(
            WorkspaceInfo wkInfo,
            List<long> revisionIds,
            string[] destPaths)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                List<RevisionInfo> revInfos = PlasticGui.Plastic.API.GetRevisionsInfo(
                    repSpec, revisionIds);

                RepositorySpec[] repSpecs = new RepositorySpec[revInfos.Count];
                RevisionInfo[] revInfoArray = new RevisionInfo[revInfos.Count];
                for (int i = 0; i < revInfos.Count; i++)
                {
                    repSpecs[i] = repSpec;
                    revInfoArray[i] = revInfos[i];
                }

                PlasticGui.Plastic.API.GetRevisionDatasToFiles(
                    repSpecs, revInfoArray, destPaths);
            });
        }

        Task<List<RepObjectInfo>> IUnityVersionControlApi.GetFileHistory(
            WorkspaceInfo wkInfo,
            string fullPath)
        {
            return Task.Run(() =>
            {
                var wkNode = PlasticGui.Plastic.API.GetWorkspaceTreeNode(
                    wkInfo, fullPath);
                return PlasticGui.Plastic.API.GetHistory(
                    wkNode.RepSpec, wkNode.RevInfo.ItemId);
            });
        }

        Task<ChangesetInfo> IUnityVersionControlApi.GetChangesetInfo(
            WorkspaceInfo wkInfo,
            long changesetId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                return ResolveChangeset(repSpec, changesetId);
            });
        }

        Task<ReviewInfo> IUnityVersionControlApi.CreateCodeReviewForBranch(
            WorkspaceInfo wkInfo,
            string branchName)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                BranchInfo branchInfo = ResolveBranch(repSpec, branchName);
                ReviewInfo reviewInfo = BuildReviewInfo.FromBranch(
                    branchInfo, repSpec.Server);
                return PlasticGui.Plastic.API.CreateReview(repSpec, reviewInfo);
            });
        }

        Task<ReviewInfo> IUnityVersionControlApi.CreateCodeReviewForChangeset(
            WorkspaceInfo wkInfo,
            long changesetId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                ChangesetInfo changesetInfo = ResolveChangeset(repSpec, changesetId);
                ReviewInfo reviewInfo = BuildReviewInfo.FromChangeset(
                    changesetInfo, repSpec.Server);
                return PlasticGui.Plastic.API.CreateReview(repSpec, reviewInfo);
            });
        }

        Task<long> IUnityVersionControlApi.AddCodeReviewComment(
            WorkspaceInfo wkInfo,
            long reviewId,
            long revisionId,
            long changesetId,
            string locationSpec,
            string commentText,
            CodeReviewCommentType commentType)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);

                CodeReviewCommentInfo comment = NewCodeReviewComment.CreateNewComment(
                    commentType,
                    commentText);

                comment.ReviewId = reviewId;
                comment.RevisionId = revisionId;
                comment.ChangesetId = changesetId;
                comment.LocationSpec = locationSpec;

                return PlasticGui.Plastic.API.CreateComment(
                    repSpec, reviewId, comment);
            });
        }

        Task<string> IUnityVersionControlApi.GetCodeReviewLink(
            WorkspaceInfo wkInfo,
            long reviewId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                return GetCodeReviewPlasticLink.From(repSpec, reviewId);
            });
        }

        Task IUnityVersionControlApi.AddCurrentUserAsReviewer(
            WorkspaceInfo wkInfo,
            long reviewId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                SEID currentUser = PlasticGui.Plastic.API.GetRemoteSeidForServer(
                    repSpec.Server);
                PlasticGui.Plastic.API.AddReviewer(repSpec, reviewId, currentUser);
            });
        }

        Task IUnityVersionControlApi.UpdateCurrentUserReviewerStatus(
            WorkspaceInfo wkInfo,
            long reviewId,
            CodeReviewStatus status,
            string comment)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                SEID currentUser = PlasticGui.Plastic.API.GetRemoteSeidForServer(
                    repSpec.Server);
                PlasticGui.Plastic.API.LegacyUpdateReviewer(
                    repSpec, reviewId, currentUser, status, comment);
            });
        }

        Task<long> IUnityVersionControlApi.CheckinPendingChanges(
            WorkspaceInfo wkInfo,
            string comment)
        {
            return Task.Run(() =>
            {
                long result = PlasticGui.Plastic.API.CheckinPendingChanges(wkInfo, comment);

                mUIApi.RefreshViews(
                    ViewType.ChangesetsView,
                    ViewType.PendingChangesView,
                    ViewType.BranchExplorerView);

                return result;
            });
        }

        Task<long> IUnityVersionControlApi.ShelvePendingChanges(
            WorkspaceInfo wkInfo,
            string comment,
            bool undoShelvedChanges)
        {
            ShelveOptions options = ShelveOptions.ThrowOnError;

            if (undoShelvedChanges)
                options |= ShelveOptions.UndoShelvedChanges;

            return Task.Run(() =>
            {
                long result = PlasticGui.Plastic.API.ShelvePendingChangesWithOptions(
                    wkInfo, comment, options);

                if (result == -1)
                    return result;

                mUIApi.RefreshViews(
                    ViewType.ShelvesView,
                    ViewType.PendingChangesView);

                return Math.Abs(result);
            });
        }

        Task<long> IUnityVersionControlApi.ReplyComment(
            WorkspaceInfo wkInfo,
            long reviewId,
            long parentCommentId,
            string commentText,
            CodeReviewCommentType commentType)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);

                CodeReviewCommentInfo parentComment =
                    PlasticGui.Plastic.API.GetReviewComment(
                        repSpec, reviewId, parentCommentId);

                CodeReviewCommentInfo reply =
                    NewCodeReviewComment.CreateNewComment(commentType, commentText);
                reply.ReviewId = reviewId;
                reply.RevisionId = parentComment.RevisionId;
                reply.ChangesetId = parentComment.ChangesetId;
                reply.LocationSpec = parentComment.LocationSpec;
                reply.ParentCommentId = parentCommentId;

                return PlasticGui.Plastic.API.CreateComment(
                    repSpec, reviewId, reply);
            });
        }

        Task IUnityVersionControlApi.ReRequestReview(WorkspaceInfo wkInfo, long reviewId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                List<CodeReviewerInfo> reviewers =
                    PlasticGui.Plastic.API.GetReviewers(repSpec, reviewId);

                foreach (CodeReviewerInfo reviewer in reviewers)
                    PlasticGui.Plastic.API.UpdateReviewer(
                        repSpec,
                        reviewId,
                        reviewer.Reviewer,
                        CodeReviewStatus.UnderReview,
                        statusChangeComment: null);
            });
        }

        Task IUnityVersionControlApi.RefreshView(ViewType viewType)
        {
            return mUIApi.RefreshView(viewType);
        }

        Task IUnityVersionControlApi.ShowView(ViewType viewType)
        {
            return mUIApi.ShowView(viewType);
        }

        Task IUnityVersionControlApi.ShowBranchExplorer()
        {
            return mUIApi.ShowBranchExplorer();
        }

        Task IUnityVersionControlApi.ShowFileHistory(WorkspaceInfo wkInfo, string fullPath)
        {
            return Task.Run(() =>
            {
                var wkNode = PlasticGui.Plastic.API.GetWorkspaceTreeNode(
                    wkInfo, fullPath);
                bool isDirectory = Directory.Exists(fullPath);

                mUIApi.ShowFileHistory(
                    wkNode.RepSpec,
                    wkNode.RevInfo.ItemId,
                    fullPath,
                    isDirectory);
            });
        }

        Task IUnityVersionControlApi.ShowBranchesView(WorkspaceInfo wkInfo, string branchName)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                BranchInfo branchInfo = ResolveBranch(repSpec, branchName);

                mUIApi.ShowBranchesView(branchInfo);
            });
        }

        Task IUnityVersionControlApi.ShowChangesetsView(WorkspaceInfo wkInfo, long changesetId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                ChangesetInfo csetInfo = ResolveChangeset(repSpec, changesetId);

                mUIApi.ShowChangesetsView(csetInfo);
            });
        }

        Task IUnityVersionControlApi.ShowShelvesView(WorkspaceInfo wkInfo, long shelveId)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                ChangesetInfo shelveInfo = ResolveChangeset(repSpec, shelveId);

                mUIApi.ShowShelvesView(shelveInfo);
            });
        }

        Task IUnityVersionControlApi.LaunchMergeFromBranchUI(
            WorkspaceInfo wkInfo,
            string branchName)
        {
            return Task.Run(async () =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                BranchInfo branchInfo = ResolveBranch(repSpec, branchName);
                await mUIApi.MergeFrom(wkInfo, branchInfo, EnumMergeType.BranchLabelMerge, false);
            });
        }

        Task IUnityVersionControlApi.LaunchMergeFromChangesetUI(
            WorkspaceInfo wkInfo,
            long changesetId)
        {
            return Task.Run(async () =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                ChangesetInfo changesetInfo = ResolveChangeset(repSpec, changesetId);
                await mUIApi.MergeFrom(wkInfo, changesetInfo, EnumMergeType.ChangesetMerge, false);
            });
        }

        Task IUnityVersionControlApi.LaunchApplyShelveUI(
            WorkspaceInfo wkInfo,
            long shelveId)
        {
            if (PlasticGui.Plastic.API.IsGluonWorkspace(wkInfo))
                throw new NotSupportedException(
                    "Apply shelve is not supported for Gluon workspaces from the Assistant. " +
                    "Please use the Shelves view context menu instead.");

            return Task.Run(async () =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                ChangesetInfo shelveInfo = ResolveChangeset(repSpec, shelveId);
                await mUIApi.MergeFrom(wkInfo, shelveInfo, EnumMergeType.ChangesetCherryPick, false);
            });
        }

        Task<MarkerInfo> IUnityVersionControlApi.CreateLabel(
            WorkspaceInfo wkInfo,
            string labelName,
            long changeset,
            string comment)
        {
            return Task.Run(() =>
            {
                try
                {
                    RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);

                    if (changeset == -1)
                        changeset = PlasticGui.Plastic.API.GetLoadedChangeset(wkInfo);

                    return PlasticGui.Plastic.API.MkLabel(
                        repSpec, labelName, changeset, comment);
                }
                finally
                {
                    mUIApi.RefreshViews(
                        ViewType.LabelsView,
                        ViewType.BranchExplorerView);
                }
            });
        }

        Task IUnityVersionControlApi.ApplyLabel(
            WorkspaceInfo wkInfo,
            string labelName)
        {
            return Task.Run(() =>
            {
                try
                {
                    RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                    MarkerInfo labelInfo = ResolveLabel(repSpec, labelName);
                    PlasticGui.Plastic.API.ApplyLabelToWorkspace(
                        wkInfo, repSpec, labelInfo);
                }
                finally
                {
                    mUIApi.RefreshViews(
                        ViewType.LabelsView,
                        ViewType.BranchExplorerView);
                }
            });
        }

        Task IUnityVersionControlApi.RenameBranch(
            WorkspaceInfo wkInfo,
            string branchName,
            string newBranchName)
        {
            return Task.Run(() =>
            {
                try
                {
                    RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                    BranchInfo branchInfo = ResolveBranch(repSpec, branchName);
                    PlasticGui.Plastic.API.RenameBranch(
                        wkInfo, repSpec, branchInfo, newBranchName);
                }
                finally
                {
                    mUIApi.RefreshViews(
                        ViewType.BranchesView,
                        ViewType.BranchExplorerView);
                }
            });
        }

        Task IUnityVersionControlApi.RenameLabel(
            WorkspaceInfo wkInfo,
            string labelName,
            string newLabelName)
        {
            return Task.Run(() =>
            {
                try
                {
                    RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                    MarkerInfo labelInfo = ResolveLabel(repSpec, labelName);
                    PlasticGui.Plastic.API.RenameLabel(
                        wkInfo, repSpec, labelInfo, newLabelName);
                }
                finally
                {
                    mUIApi.RefreshViews(
                        ViewType.LabelsView,
                        ViewType.BranchExplorerView);
                }
            });
        }

        Task IUnityVersionControlApi.DeleteLabel(
            WorkspaceInfo wkInfo,
            string labelName)
        {
            return Task.Run(() =>
            {
                try
                {
                    RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                    MarkerInfo labelInfo = ResolveLabel(repSpec, labelName);
                    PlasticGui.Plastic.API.DeleteLabel(repSpec, labelInfo);
                }
                finally
                {
                    mUIApi.RefreshViews(
                        ViewType.LabelsView,
                        ViewType.BranchExplorerView);
                }
            });
        }

        static BranchInfo ResolveBranch(RepositorySpec repSpec, string branchName)
        {
            BranchInfo info = PlasticGui.Plastic.API.GetBranchInfo(repSpec, branchName);
            if (info == null)
                throw new ArgumentException(
                    string.Format("Branch '{0}' not found in repository '{1}'", branchName, repSpec.Name),
                    nameof(branchName));
            return info;
        }

        static ChangesetInfo ResolveChangeset(RepositorySpec repSpec, long changesetId)
        {
            ChangesetInfo info = PlasticGui.Plastic.API.GetChangesetInfoFromId(repSpec, changesetId);
            if (info == null)
                throw new ArgumentException(
                    string.Format("Changeset {0} not found in repository '{1}'", changesetId, repSpec.Name),
                    nameof(changesetId));
            return info;
        }

        static MarkerInfo ResolveLabel(RepositorySpec repSpec, string labelName)
        {
            MarkerInfo info = PlasticGui.Plastic.API.GetMarkerInfoByName(repSpec, labelName);
            if (info == null)
                throw new ArgumentException(
                    string.Format("Label '{0}' not found in repository '{1}'", labelName, repSpec.Name),
                    nameof(labelName));
            return info;
        }

        static long CalculateCommonAncestorIdForDiffs(
            RepositorySpec repSpec,
            BranchInfo brInfo)
        {
            RepositoryInfo repInfo = PlasticGui.Plastic.API.GetRepositoryInfo(repSpec);

            long parentCsetId = PlasticGui.Plastic.API.GetParentChangesetIdForBranch(
                repInfo,
                brInfo);

            if (!PlasticGui.Plastic.API.GetParentBranchCommonAncestorId(
                    repInfo,
                    brInfo,
                    PlasticGui.Plastic.API.GetItemHandler(repInfo.Server),
                    PlasticGui.Plastic.API.GetBranchHandler(repInfo.Server),
                    out var commonAncestorId))
                return -1;

            return parentCsetId != commonAncestorId ? commonAncestorId : -1;
        }

        Task<List<LockInfo>> IUnityVersionControlApi.ListLocks(WorkspaceInfo wkInfo)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                return PlasticGui.Plastic.API.ListLocks(repSpec);
            });
        }

        Task<LockRule> IUnityVersionControlApi.GetLockRule(WorkspaceInfo wkInfo)
        {
            return Task.Run(() =>
            {
                RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                return PlasticGui.Plastic.API.GetLockRule(repSpec);
            });
        }

        Task IUnityVersionControlApi.ReleaseLocks(WorkspaceInfo wkInfo, List<long> lockIds)
        {
            return Task.Run(() =>
            {
                try
                {
                    RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                    List<LockInfo> allLocks = PlasticGui.Plastic.API.ListLocks(repSpec);
                    List<LockInfo> locksToRelease = allLocks.FindAll(
                        l => lockIds.Contains(l.ItemId));
                    PlasticGui.Plastic.API.ReleaseLocks(repSpec, locksToRelease);
                }
                finally
                {
                    mUIApi.RefreshViews(ViewType.LocksView);
                }
            });
        }

        Task IUnityVersionControlApi.RemoveLocks(WorkspaceInfo wkInfo, List<long> lockIds)
        {
            return Task.Run(() =>
            {
                try
                {
                    RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                    List<LockInfo> allLocks = PlasticGui.Plastic.API.ListLocks(repSpec);
                    List<LockInfo> locksToRemove = allLocks.FindAll(
                        l => lockIds.Contains(l.ItemId));
                    PlasticGui.Plastic.API.RemoveLocks(repSpec, locksToRemove);
                }
                finally
                {
                    mUIApi.RefreshViews(ViewType.LocksView);
                }
            });
        }

        Task IUnityVersionControlApi.UndoChanges(WorkspaceInfo wkInfo, List<string> fullPaths)
        {
            return Task.Run(() =>
            {
                try
                {
                    PlasticGui.Plastic.API.UndoCheckout(
                        wkInfo,
                        fullPaths,
                        null,
                        UndoCheckoutModifiers.Recurse);
                }
                finally
                {
                    mUIApi.RefreshViews(
                        ViewType.PendingChangesView,
                        ViewType.BranchExplorerView);
                }
            });
        }

        Task IUnityVersionControlApi.UndoUnchanged(WorkspaceInfo wkInfo)
        {
            return Task.Run(() =>
            {
                try
                {
                    PlasticGui.Plastic.API.UndoUnchanged(
                        wkInfo,
                        new List<string> { wkInfo.ClientPath });
                }
                finally
                {
                    mUIApi.RefreshViews(
                        ViewType.PendingChangesView,
                        ViewType.BranchExplorerView);
                }
            });
        }

        Task IUnityVersionControlApi.RevertToRevision(
            WorkspaceInfo wkInfo,
            string fullPath,
            long revisionId)
        {
            return Task.Run(() =>
            {
                try
                {
                    RepositorySpec repSpec =
                        PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
                    RevisionInfo revInfo =
                        PlasticGui.Plastic.API.GetRevisionInfo(repSpec, revisionId);

                    PlasticGui.Plastic.API.GetRevisionDataToFile(
                        repSpec, revInfo, fullPath);
                }
                finally
                {
                    mUIApi.RefreshViews(ViewType.PendingChangesView);
                }
            });
        }

        readonly IUnityVersionControlUiApi mUIApi;
    }
}
