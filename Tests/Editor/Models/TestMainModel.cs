using System;
using Unity.Cloud.Collaborate.Models;
using Unity.Cloud.Collaborate.Models.Structures;

namespace Unity.Cloud.Collaborate.Tests.Models
{
    internal class TestMainModel : IMainModel
    {
        public void OnStop()
        {
            throw new NotImplementedException();
        }

        public event Action<bool> ConflictStatusChange = delegate { };
        public event Action<bool> OperationStatusChange = delegate { };
        public event Action<IProgressInfo> OperationProgressChange = delegate { };
        public event Action<IErrorInfo> ErrorOccurred = delegate { };
        public event Action ErrorCleared = delegate { };
        public event Action<bool> RemoteRevisionsAvailabilityChange = delegate { };
        public bool RemoteRevisionsAvailable { get; }
        public bool Conflicted { get; }
        public IProgressInfo ProgressInfo { get; }
        public IErrorInfo ErrorInfo { get; }

        public IHistoryModel ConstructHistoryModel()
        {
            throw new NotImplementedException();
        }

        public IChangesModel ConstructChangesModel()
        {
            throw new NotImplementedException();
        }

        public void ClearError()
        {
            throw new NotImplementedException();
        }

        public void RequestSync()
        {
            throw new NotImplementedException();
        }

        public void RequestCancelJob()
        {
            throw new NotImplementedException();
        }
    }
}
