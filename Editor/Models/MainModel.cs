using System;
using Unity.Cloud.Collaborate.Models.Api;
using Unity.Cloud.Collaborate.Models.Structures;
using Unity.Cloud.Collaborate.UserInterface;

namespace Unity.Cloud.Collaborate.Models
{
    internal class MainModel : IMainModel
    {
        readonly ISourceControlProvider m_Provider;

        /// <inheritdoc />
        public event Action<bool> ConflictStatusChange;

        /// <inheritdoc />
        public event Action<bool> OperationStatusChange;

        /// <inheritdoc />
        public event Action<IProgressInfo> OperationProgressChange;

        /// <inheritdoc />
        public event Action<IErrorInfo> ErrorOccurred;

        /// <inheritdoc />
        public event Action ErrorCleared;

        /// <inheritdoc />
        public event Action<bool> RemoteRevisionsAvailabilityChange;

        public MainModel(ISourceControlProvider provider)
        {
            m_Provider = provider;

            // Setup events
            m_Provider.UpdatedOperationStatus += OnUpdatedOperationStatus;
            m_Provider.UpdatedOperationProgress += OnUpdatedOperationProgress;
            m_Provider.ErrorOccurred += OnErrorOccurred;
            m_Provider.ErrorCleared += OnErrorCleared;
            m_Provider.UpdatedConflictState += OnUpdatedConflictState;
            m_Provider.UpdatedRemoteRevisionsAvailability += OnUpdatedRemoteRevisionsAvailability;
            WindowCache.Instance.BeforeSerialize += OnStop;
        }

        /// <inheritdoc />
        public void OnStop()
        {
            WindowCache.Instance.BeforeSerialize -= OnStop;
            m_Provider.UpdatedOperationStatus -= OnUpdatedOperationStatus;
            m_Provider.UpdatedOperationProgress -= OnUpdatedOperationProgress;
            m_Provider.ErrorOccurred -= OnErrorOccurred;
            m_Provider.ErrorCleared -= OnErrorCleared;
            m_Provider.UpdatedConflictState -= OnUpdatedConflictState;
            m_Provider.UpdatedRemoteRevisionsAvailability -= OnUpdatedRemoteRevisionsAvailability;
        }

        /// <inheritdoc />
        public bool RemoteRevisionsAvailable => m_Provider.GetRemoteRevisionAvailability();

        /// <inheritdoc />
        public bool Conflicted => m_Provider.GetConflictedState();

        /// <inheritdoc />
        public IProgressInfo ProgressInfo => m_Provider.GetProgressState();

        /// <inheritdoc />
        public IErrorInfo ErrorInfo => m_Provider.GetErrorState();

        /// <inheritdoc />
        public IHistoryModel ConstructHistoryModel()
        {
            return new HistoryModel(m_Provider);
        }

        /// <inheritdoc />
        public IChangesModel ConstructChangesModel()
        {
            return new ChangesModel(m_Provider);
        }

        /// <inheritdoc />
        public void ClearError()
        {
            m_Provider.ClearError();
        }

        /// <inheritdoc />
        public void RequestSync()
        {
            m_Provider.RequestSync();
        }

        /// <inheritdoc />
        public void RequestCancelJob()
        {
            m_Provider.RequestCancelJob();
        }

        /// <summary>
        /// Event handler for when the availability of remote revisions changes.
        /// </summary>
        /// <param name="available">New availability status.</param>
        void OnUpdatedRemoteRevisionsAvailability(bool available)
        {
            RemoteRevisionsAvailabilityChange?.Invoke(available);
        }

        /// <summary>
        /// Event handler for when the conflicted status changes.
        /// </summary>
        /// <param name="conflicted">New conflicted status.</param>
        void OnUpdatedConflictState(bool conflicted)
        {
            ConflictStatusChange?.Invoke(conflicted);
        }

        void OnUpdatedOperationStatus(bool inProgress)
        {
            OperationStatusChange?.Invoke(inProgress);
        }

        void OnUpdatedOperationProgress(IProgressInfo progressInfo)
        {
            OperationProgressChange?.Invoke(progressInfo);
        }

        void OnErrorOccurred(IErrorInfo errorInfo)
        {
            ErrorOccurred?.Invoke(errorInfo);
        }

        void OnErrorCleared()
        {
            ErrorCleared?.Invoke();
        }
    }
}
