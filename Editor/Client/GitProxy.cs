using System;
using System.Collections.Generic;
using System.Reflection;
using CollabProxy.Client;
using CollabProxy.Models;

namespace CollabProxy
{
    /// <summary>
    /// This class is responsible for channeling requests to the server and, in the case of synchronous requests,
    /// providing a response
    /// </summary>
    internal class GitProxy : IGitProxy
    {
        const string k_SetCurrentHeadMessage = "SETCURRENTHEAD";
        readonly CollabProxyClient m_TcpClient;

        public Action OnUpdateHeadListener
        {
            set { m_TcpClient.RegisterListener(k_SetCurrentHeadMessage, value); }
        }

        public GitProxy(CollabProxyClient tcpClient)
        {
            if (tcpClient == null)
            {
                throw new ArgumentNullException();
            }

            m_TcpClient = tcpClient;
        }

        public bool IsRunningAsyncOperations()
        {
            return m_TcpClient.CallSynchronous<bool>(MethodBase.GetCurrentMethod().Name);
        }

        public bool RepositoryExists()
        {
            return m_TcpClient.CallSynchronous<bool>(MethodBase.GetCurrentMethod().Name);
        }

        public void InitializeRepository()
        {
            m_TcpClient.CallSynchronous(MethodBase.GetCurrentMethod().Name);
        }

        public void RemoveRepository()
        {
            m_TcpClient.CallSynchronous(MethodBase.GetCurrentMethod().Name);
        }

        public void SetRemoteOrigin(string cloneUrl)
        {
            m_TcpClient.CallSynchronous(MethodBase.GetCurrentMethod().Name, cloneUrl);
        }

        public void SetCurrentHead(string revisionId, string accessToken)
        {
            m_TcpClient.CallAsynchronous(MethodBase.GetCurrentMethod().Name, revisionId, accessToken);
        }

        public void GetWorkingDirectoryChangesAsync(string callBackName)
        {
            m_TcpClient.CallAsynchronous(MethodBase.GetCurrentMethod().Name, callBackName);
        }
    }
}
