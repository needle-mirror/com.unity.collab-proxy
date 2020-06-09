using System;
using Unity.Cloud.Collaborate.Models.Providers.Client.Models;

namespace Unity.Cloud.Collaborate.Models.Providers.Client
{
    /// <summary>
    /// This class is responsible for channeling requests to the server and, in the case of synchronous requests,
    /// providing a response
    /// </summary>
    internal class GitProxy : IGitProxy
    {
        readonly CollabProxyClient m_TcpClient;

        public GitProxy(CollabProxyClient tcpClient)
        {
            m_TcpClient = tcpClient ?? throw new ArgumentNullException();
        }

        public bool RepositoryExists()
        {
            return m_TcpClient.CallSynchronous<bool>(nameof(RepositoryExists));
        }

        public void InitializeRepository()
        {
            m_TcpClient.CallSynchronous(nameof(InitializeRepository));
        }

        public void RemoveRepository()
        {
            m_TcpClient.CallSynchronous(nameof(RemoveRepository));
        }

        public void SetRemoteOrigin(string cloneUrl)
        {
            m_TcpClient.CallSynchronous(nameof(SetRemoteOrigin), cloneUrl);
        }

        public void SetCurrentHeadAsync(string revisionId, string accessToken)
        {
            m_TcpClient.CallAsynchronous(nameof(SetCurrentHeadAsync), revisionId, accessToken);
        }

        public void GetWorkingDirectoryChangesAsync()
        {
            m_TcpClient.CallAsynchronous(nameof(GetWorkingDirectoryChangesAsync));
        }

        public void UpdateCachedChangesAsync()
        {
            m_TcpClient.CallAsynchronous(nameof(UpdateCachedChangesAsync));
        }

        public void UpdateFileStatusAsync(string path)
        {
            m_TcpClient.CallAsynchronous(nameof(UpdateFileStatusAsync), path);
        }
    }
}
