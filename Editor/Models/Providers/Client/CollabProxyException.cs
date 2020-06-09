using System;
using Unity.Cloud.Collaborate.Models.Providers.Client.Models;

namespace Unity.Cloud.Collaborate.Models.Providers.Client
{
    /// <summary>
    /// This class represents exceptions generated from a handler used by the Collab proxy server to fulfill requests
    /// </summary>
    internal class CollabProxyException : Exception
    {
        ResponseException m_ResponseException;

        Exception InnermostException
        {
            get
            {
                Exception e = this;
                while (e.InnerException != null) e = e.InnerException;
                return e;
            }
        }

        public string InnermostExceptionMessage => m_ResponseException != null
            ? m_ResponseException.Message
            : InnermostException.Message;

        public string InnermostExceptionStackTrace => m_ResponseException != null
            ? m_ResponseException.StackTrace
            : InnermostException.StackTrace;

        public CollabProxyException()
        {
        }

        public CollabProxyException(string message)
            : base(message)
        {
        }

        public CollabProxyException(string message, ResponseException responseException)
            : base(message)
        {
            m_ResponseException = responseException;
        }
    }
}
