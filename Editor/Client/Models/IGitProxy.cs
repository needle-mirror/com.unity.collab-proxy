using System;
using System.Collections.Generic;

namespace CollabProxy.Models
{
    interface IGitProxy
    {
        bool RepositoryExists();
        void InitializeRepository();
        void RemoveRepository();
        void SetRemoteOrigin(string cloneUrl);
        void SetCurrentHeadAsync(string revisionId, string accessToken);
        void GetWorkingDirectoryChangesAsync();
        void UpdateCachedChangesAsync();
        void UpdateFileStatusAsync(string path);
    }
}
