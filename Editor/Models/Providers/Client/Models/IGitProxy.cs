using System;

namespace Unity.Cloud.Collaborate.Models.Providers.Client.Models
{
    internal interface IGitProxy
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
