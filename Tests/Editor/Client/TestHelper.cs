using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using CollabProxy.Client;
using CollabProxy.Models;
using NetworkCommsDotNet;

namespace CollabProxy.Tests
{
    internal static class TestHelper
    {
        public static int GetPortFromFile()
        {
            int serverPort = 0;
            if (File.Exists(CollabProxyClient.GetPortFilePath()))
            {
                using (StreamReader sr = new StreamReader(CollabProxyClient.GetPortFilePath()))
                {
                    int.TryParse(sr.ReadToEnd(), out serverPort);
                }
            }

            return serverPort;
        }

        static string Serialize(ResponseWrapper response)
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

        static string Serialize<T>(ResponseWrapper<T> response)
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

        public static void RegisterListener(string messageName, Action<string> callback, bool keepExistingListeners = false)
        {
            if (!keepExistingListeners)
            {
                NetworkComms.RemoveGlobalIncomingPacketHandler();
            }

            NetworkComms.AppendGlobalIncomingPacketHandler<string>(messageName,
                (packetHeader, connection, incomingSring) =>
                {
                    callback(incomingSring);
                    connection.SendObject(messageName.ToUpper(), Serialize(new ResponseWrapper()));
                });
        }

        public static void RegisterListener<T>(string messageName, Action<string> callback, bool keepExistingListeners = false)
        {
            if (!keepExistingListeners)
            {
                NetworkComms.RemoveGlobalIncomingPacketHandler();
            }

            NetworkComms.AppendGlobalIncomingPacketHandler<string>(messageName,
                (packetHeader, connection, incomingSring) =>
                {
                    try
                    {
                        callback(incomingSring);
                        connection.SendObject(messageName.ToUpper(), Serialize<T>(new ResponseWrapper<T>()));
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                });
        }
    }
}
