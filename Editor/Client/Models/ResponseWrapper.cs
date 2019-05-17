using System;
using System.Runtime.Serialization;

namespace CollabProxy.Models
{
    [DataContract(Namespace = "", Name = "ResponseException")]
    class ResponseException
    {
        public ResponseException(Exception e)
        {
            Message = e.Message;
            Source = e.Source;
            StackTrace = e.StackTrace;
        }

        public ResponseException()
        {
            Message = string.Empty;
            Source = string.Empty;
            StackTrace = string.Empty;
        }
        [DataMember(Name = "Message")]
        public string Message { get; set; }
        [DataMember(Name = "Source")]
        public string Source { get; set; }
        [DataMember(Name = "StackTrace")]
        public string StackTrace { get; set; }
    }

    [DataContract(Namespace = "", Name = "ResponseWrapper")]
    class ResponseWrapper
    {
        [DataMember(Name = "ResponseException")]
        public ResponseException ResponseException { get; set; }
    }

    [DataContract(Namespace = "", Name = "ResponseWrapperOfT")]
    class ResponseWrapper<T>
    {
        [DataMember(Name = "ResponseException")]
        public ResponseException ResponseException { get; set; }
        [DataMember(Name = "ResponseObject")]
        public T ResponseObject { get; set; }

        public ResponseWrapper()
        {
        }

        public ResponseWrapper(ResponseWrapper response)
        {
            ResponseException = response.ResponseException;
        }
    }
}
