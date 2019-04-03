using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace CollabProxy.Models
{
    internal static class Serialization
    {
        public static string SerializeResponse(ResponseWrapper response)
        {
            var sb = new StringBuilder();
            var serializer = new DataContractSerializer(typeof(ResponseWrapper));
            using (XmlWriter writer = XmlWriter.Create(sb))
            {
                serializer.WriteObject(writer, response);
                writer.Flush();
                return sb.ToString();
            }
        }

        public static ResponseWrapper DeserializeResponse(string response)
        {
            var serializer = new DataContractSerializer(typeof(ResponseWrapper));
            using (var sr = new StringReader(response))
            {
                using (var reader = XmlReader.Create(sr))
                {
                    return (ResponseWrapper)serializer.ReadObject(reader);
                }
            }
        }

        public static string SerializeResponse<T>(ResponseWrapper<T> response)
        {
            var sb = new StringBuilder();
            var serializer = new DataContractSerializer(typeof(ResponseWrapper<T>));
            using (var writer = XmlWriter.Create(sb))
            {
                serializer.WriteObject(writer, response);
                writer.Flush();
                return sb.ToString();
            }
        }

        public static ResponseWrapper<T> DeserializeResponse<T>(string response)
        {
            var serializer = new DataContractSerializer(typeof(ResponseWrapper<T>));
            using (var sr = new StringReader(response))
            {
                using (var reader = XmlReader.Create(sr))
                {
                    return (ResponseWrapper<T>)serializer.ReadObject(reader);
                }
            }
        }

        public static string Serialize<T>(T t)
        {
            var sb = new StringBuilder();
            var serializer = new DataContractSerializer(typeof(T));
            using (XmlWriter writer = XmlWriter.Create(sb))
            {
                serializer.WriteObject(writer, t);
                writer.Flush();
                return sb.ToString();
            }
        }

        public static T Deserialize<T>(string response)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var sr = new StringReader(response))
            {
                using (var reader = XmlReader.Create(sr))
                {
                    return (T)serializer.ReadObject(reader);
                }
            }
        }
    }
}
