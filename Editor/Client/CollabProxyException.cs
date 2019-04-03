using System;
using CollabProxy.Models;

namespace CollabProxy
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

        public string InnermostExceptionMessage
        {
            get
            {
                if (m_ResponseException != null)
                {
                    return m_ResponseException.Message;
                }
                return InnermostException.Message;
            }
        }

        public string InnermostExceptionStackTrace
        {
            get
            {
                if (m_ResponseException != null)
                {
                    return m_ResponseException.StackTrace;
                }
                return InnermostException.StackTrace;
            }

        }

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