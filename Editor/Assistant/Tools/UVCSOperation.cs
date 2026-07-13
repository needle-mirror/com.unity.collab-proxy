#if AIA_PRESENT
namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    // The set of operations dispatched by UVCSExecuteTool.Execute.
    // Each value maps to a specific UVCS tool method. The member names are part of the
    // tool contract: they are exposed to the LLM as the allowed values of the 'operation'
    // parameter, and the skill/evals refer to them as Unity.UVCS.Execute(<Operation>).
    enum UVCSOperation
    {
        // Onboarding / project management
        GetKnownServers,
        GetAllRepositories,
        GetAllProjects,
        GetWorkspaceFromPath,
        CreateRepository,
        CreateWorkspace,
        PerformInitialCheckin,
        RefreshUIAfterCreateWorkspace,
        CheckinPendingChanges,
        ShelvePendingChanges,

        // Query
        FindQuery,
        GetWorkspaceConfiguration,

        // Branches
        GetBranchInfo,
        CreateBranch,
        RenameBranch,
        LaunchSwitchToBranchUI,

        // Annotate / revisions
        GetFileAnnotations,
        GetRevisionInfo,

        // History
        GetFileHistory,

        // Diff
        GetBranchDifferences,
        GetChangesetDifferences,
        GetChangesetsDifferences,
        GetLabelDifferences,
        GetShelveDifferences,
        GetPendingChanges,
        DownloadRevisionsToFiles,
        CleanupDiffTempFiles,

        // Changesets
        GetChangesetInfo,

        // Code review
        CreateCodeReviewForBranch,
        CreateCodeReviewForChangeset,
        CreateConversationComment,
        CreateCodeComment,
        GetCodeReviewLink,
        AddCurrentUserAsReviewer,
        SetReviewStatus,
        GetReviewsInStatus,
        GetReviewComments,
        ReplyComment,
        ReRequestReview,

        // Labels
        CreateLabel,
        RenameLabel,
        ApplyLabel,
        DeleteLabel,

        // Locks
        ListLocks,
        GetLockRule,
        ReleaseLocks,
        RemoveLocks,

        // Undo / revert
        UndoChanges,
        UndoUnchanged,
        RevertToRevision,

        // UI navigation
        RefreshView,
        ShowView,
        ShowBranchExplorer,
        ShowFileHistory,
        ShowBranchesView,
        ShowChangesetsView,
        ShowShelvesView,
        LaunchMergeFromBranchUI,
        LaunchMergeFromChangesetUI,
        LaunchApplyShelveUI,

        // User interaction
        SelectFromList,
        SelectOption,
        AskForConfirmation
    }
}
#endif
