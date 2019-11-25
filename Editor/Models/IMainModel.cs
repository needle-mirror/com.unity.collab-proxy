using System;
using Unity.Cloud.Collaborate.Models.Structures;
using JetBrains.Annotations;

namespace Unity.Cloud.Collaborate.Models
{
    internal interface IMainModel : IModel
    {
        /// <summary>
        /// Signal when the local state switches between conflicted or not.
        /// </summary>
        event Action<bool> ConflictStatusChange;

        /// <summary>
        /// Signal when an operation with progress has started or stopped.
        /// </summary>
        event Action<bool> OperationStatusChange;

        /// <summary>
        /// Signal with incremental details of the operation in progress.
        /// </summary>
        event Action<IProgressInfo> OperationProgressChange;

        /// <summary>
        /// Signal when an error has occurred.
        /// </summary>
        event Action<IErrorInfo> ErrorOccurred;

        /// <summary>
        /// Signal when the error has cleared.
        /// </summary>
        event Action ErrorCleared;

        /// <summary>
        /// Signal whether or not the there are remote revisions to be fetched.
        /// </summary>
        event Action<bool> RemoteRevisionsAvailabilityChange;

        /// <summary>
        /// Returns true if there are remote revisions available.
        /// </summary>
        bool RemoteRevisionsAvailable { get; }

        /// <summary>
        /// Returns true if there's a conflict locally.
        /// </summary>
        bool Conflicted { get; }

        /// <summary>
        /// Returns progress info if there is any.
        /// </summary>
        [CanBeNull]
        IProgressInfo ProgressInfo { get; }

        /// <summary>
        /// Returns error info if there is any.
        /// </summary>
        [CanBeNull]
        IErrorInfo ErrorInfo { get; }

        /// <summary>
        /// Returns a history model.
        /// </summary>
        /// <returns>Singleton history model for this main model.</returns>
        [NotNull]
        IHistoryModel ConstructHistoryModel();

        /// <summary>
        /// Returns a Changes model.
        /// </summary>
        /// <returns>Singleton change model for this main model.</returns>
        [NotNull]
        IChangesModel ConstructChangesModel();

        /// <summary>
        /// Clears any set error.
        /// </summary>
        void ClearError();

        /// <summary>
        /// Sync to latest revision.
        /// </summary>
        void RequestSync();

        /// <summary>
        /// Request cancel current job.
        /// </summary>
        void RequestCancelJob();
    }
}
