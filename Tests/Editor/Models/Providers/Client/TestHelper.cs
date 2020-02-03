using System;
using System.IO;
using Unity.Cloud.Collaborate.Models.Providers.Client;
using Unity.Cloud.Collaborate.Models.Providers.Client.Models;
using NetworkCommsDotNet;
using Newtonsoft.Json;

namespace Unity.Cloud.Collaborate.Tests.Models.Providers.Client
{
    internal static class TestHelper
    {
        public static int GetPortFromFile()
        {
            var serverPort = 0;
            if (File.Exists(CollabProxyClient.GetPortFilePath()))
            {
                using (var sr = new StreamReader(CollabProxyClient.GetPortFilePath()))
                {
                    int.TryParse(sr.ReadToEnd(), out serverPort);
                }
            }

            return serverPort;
        }

        static string Serialize(ResponseWrapper response)
        {
            return JsonConvert.SerializeObject(response);
        }

        static string Serialize<T>(ResponseWrapper<T> response)
        {
            return JsonConvert.SerializeObject(response);
        }

        public static void RegisterListener(string messageName, Action<string> callback, bool keepExistingListeners = false)
        {
            if (!keepExistingListeners)
            {
                NetworkComms.RemoveGlobalIncomingPacketHandler();
            }

            NetworkComms.AppendGlobalIncomingPacketHandler<string>(messageName,
                (packetHeader, connection, incomingString) =>
                {
                    callback(incomingString);
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
                (packetHeader, connection, incomingString) =>
                {
                    callback(incomingString);
                    connection.SendObject(messageName.ToUpper(), Serialize(new ResponseWrapper<T>()));
                });
        }
    }
}
