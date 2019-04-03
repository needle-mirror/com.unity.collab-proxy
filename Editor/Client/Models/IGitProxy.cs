using System;
using System.Collections.Generic;

namespace CollabProxy.Models
{
    internal interface IGitProxy
    {
        bool IsRunningAsyncOperations();
        bool RepositoryExists();
        void InitializeRepository();
        void RemoveRepository();
        void SetRemoteOrigin(string cloneUrl);
        void SetCurrentHead(string revisionId, string accessToken);
        void GetWorkingDirectoryChangesAsync(string callBackName);
    }
}
