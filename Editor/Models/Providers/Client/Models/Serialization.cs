using System;
using Newtonsoft.Json;

namespace Unity.Cloud.Collaborate.Models.Providers.Client.Models
{
    internal static class Serialization
    {
        public static string SerializeResponse(ResponseWrapper response)
        {
            return JsonConvert.SerializeObject(response);
        }

        public static ResponseWrapper DeserializeResponse(string response)
        {
            return JsonConvert.DeserializeObject<ResponseWrapper>(response);
        }

        public static string SerializeResponse<T>(ResponseWrapper<T> response)
        {
            return JsonConvert.SerializeObject(response);
        }

        public static ResponseWrapper<T> DeserializeResponse<T>(string response)
        {
            return JsonConvert.DeserializeObject<ResponseWrapper<T>>(response);
        }

        public static string Serialize<T>(T t)
        {
            return JsonConvert.SerializeObject(t);
        }

        public static T Deserialize<T>(string response)
        {
            return JsonConvert.DeserializeObject<T>(response);
        }
    }
}
