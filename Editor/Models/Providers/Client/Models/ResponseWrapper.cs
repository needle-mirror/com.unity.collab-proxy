using System;
using Newtonsoft.Json;

namespace Unity.Cloud.Collaborate.Models.Providers.Client.Models
{
    [JsonObject]
    internal class ResponseException
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
        [JsonProperty]
        public string Message { get; set; }
        [JsonProperty]
        public string Source { get; set; }
        [JsonProperty]
        public string StackTrace { get; set; }
    }

    [JsonObject]
    internal class ResponseWrapper
    {
        [JsonProperty]
        public ResponseException ResponseException { get; set; }
    }

    [JsonObject]
    internal class ResponseWrapper<T>
    {
        [JsonProperty]
        public ResponseException ResponseException { get; set; }
        [JsonProperty]
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
