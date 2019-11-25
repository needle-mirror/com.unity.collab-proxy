using Unity.Cloud.Collaborate.Assets;
using Unity.Cloud.Collaborate.Components;
using Unity.Cloud.Collaborate.Models;
using Unity.Cloud.Collaborate.Models.Structures;
using Unity.Cloud.Collaborate.Views;

namespace Unity.Cloud.Collaborate.Presenters
{
    internal class MainPresenter : IMainPresenter
    {
        readonly IMainView m_View;
        readonly IMainModel m_Model;

        const string k_ErrorOccuredId = "error_occured";
        const string k_ConflictsDetectedId = "conflicts_detected";
        const string k_RevisionsAvailableId = "revisions_available";

        public MainPresenter(IMainView view, IMainModel model)
        {
            m_View = view;
            m_Model = model;
        }

        /// <inheritdoc />
        public void Start()
        {
            // Setup listeners.
            m_Model.ConflictStatusChange += OnConflictStatusChange;
            m_Model.OperationStatusChange += OnOperationStatusChange;
            m_Model.OperationProgressChange += OnOperationProgressChange;
            m_Model.ErrorOccurred += OnErrorOccurred;
            m_Model.ErrorCleared += OnErrorCleared;
            m_Model.RemoteRevisionsAvailabilityChange += OnRemoteRevisionsAvailabilityChange;

            // Get initial values.
            OnConflictStatusChange(m_Model.Conflicted);
            OnRemoteRevisionsAvailabilityChange(m_Model.RemoteRevisionsAvailable);

            // Update progress info.
            var progressInfo = m_Model.ProgressInfo;
            if (progressInfo != null)
            {
                OnOperationStatusChange(true);
                OnOperationProgressChange(m_Model.ProgressInfo);
            }
            else
            {
                OnOperationStatusChange(false);
            }

            // Update error info.
            var errorInfo = m_Model.ErrorInfo;
            if (errorInfo != null)
            {
                OnErrorOccurred(errorInfo);
            }
            else
            {
                OnErrorCleared();
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            m_Model.ConflictStatusChange -= OnConflictStatusChange;
            m_Model.OperationStatusChange -= OnOperationStatusChange;
            m_Model.OperationProgressChange -= OnOperationProgressChange;
            m_Model.ErrorOccurred -= OnErrorOccurred;
            m_Model.ErrorCleared -= OnErrorCleared;
            m_Model.RemoteRevisionsAvailabilityChange -= OnRemoteRevisionsAvailabilityChange;
        }

        /// <inheritdoc />
        public IHistoryPresenter AssignHistoryPresenter(IHistoryView view)
        {
            var presenter = new HistoryPresenter(view, m_Model.ConstructHistoryModel());
            view.Presenter = presenter;
            return presenter;
        }

        /// <inheritdoc />
        public IChangesPresenter AssignHistoryPresenter(IChangesView view)
        {
            var presenter = new ChangesPresenter(view, m_Model.ConstructChangesModel(), m_Model);
            view.Presenter = presenter;
            return presenter;
        }

        /// <inheritdoc />
        public void RequestCancelJob()
        {
            m_Model.RequestCancelJob();
        }

        /// <summary>
        /// Display an alert if there is conflicts detected.
        /// </summary>
        /// <param name="conflicts">True if conflicts exist.</param>
        void OnConflictStatusChange(bool conflicts)
        {
            if (conflicts)
            {
                m_View.AddAlert(k_ConflictsDetectedId, AlertBox.AlertLevel.Alert, StringAssets.conflictsDetected);
            }
            else
            {
                m_View.RemoveAlert(k_ConflictsDetectedId);
            }
        }

        /// <summary>
        /// Display a progress bar if an operation has started.
        /// </summary>
        /// <param name="inProgress"></param>
        void OnOperationStatusChange(bool inProgress)
        {
            if (inProgress)
            {
                m_View.AddOperationProgress();
            }
            else
            {
                m_View.RemoveOperationProgress();
            }
        }

        /// <summary>
        /// Update progress bar with incremental details.
        /// </summary>
        /// <param name="progressInfo"></param>
        void OnOperationProgressChange(IProgressInfo progressInfo)
        {
            m_View.SetOperationProgress(progressInfo.Title, progressInfo.Details,
                progressInfo.PercentageComplete, progressInfo.CurrentCount,
                progressInfo.TotalCount, progressInfo.PercentageProgressType, progressInfo.CanCancel);
        }

        /// <summary>
        /// Display an error.
        /// </summary>
        /// <param name="errorInfo"></param>
        void OnErrorOccurred(IErrorInfo errorInfo)
        {
            if (errorInfo.Behaviour == ErrorInfoBehavior.Alert)
            {
                m_View.AddAlert(k_ErrorOccuredId, AlertBox.AlertLevel.Alert, errorInfo.Message, (StringAssets.clear, m_Model.ClearError));
            }
        }

        /// <summary>
        /// Clear the error state.
        /// </summary>
        void OnErrorCleared()
        {
            m_View.RemoveAlert(k_ErrorOccuredId);
        }

        /// <summary>
        /// Show or clear the revisions to fetch alert based on whether or not they are available.
        /// </summary>
        /// <param name="remoteRevisionsAvailable">True if there are remote revisions to pull down.</param>
        void OnRemoteRevisionsAvailabilityChange(bool remoteRevisionsAvailable)
        {
            if (remoteRevisionsAvailable)
            {
                m_View.AddAlert(k_RevisionsAvailableId, AlertBox.AlertLevel.Info, StringAssets.syncRemoteRevisionsMessage, (StringAssets.sync, m_Model.RequestSync));
            }
            else
            {
                m_View.RemoveAlert(k_RevisionsAvailableId);
            }
        }
    }
}
