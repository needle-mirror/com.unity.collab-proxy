using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using CollabProxy.Models;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections.TCP;
using UnityEditor;

namespace CollabProxy.Client
{
    /// <summary>
    /// This class is used to communicate with the server which runs as an external process
    /// </summary>
    class CollabProxyClient
    {
        const int k_TimeoutMillis = 5000;
        const string k_MonoPath = "MONO_PATH";
        const string k_PortFilePath = "Library/.collabproxyport";
        const string k_ServerExePath = "Packages/com.unity.collab-proxy/.Server/Unity.CollabProxy.Server.exe";
        const string k_ServerErrorMsg = "Proxy server encountered an error";

        protected TCPConnection TcpConnection { get; private set; }

        /// <summary>
        /// When this class is instantiated, it connects to an existing server - if one is available - or spawns
        /// a new server and connects to it
        /// </summary>
        public CollabProxyClient()
        {
            if (IsCachedServerAvailable())
            {
                try
                {
                    TcpConnection = ConnectUsingCache();
                }
                catch (ConnectionSetupException)
                {
                    SpawnServer();
                    TcpConnection = ConnectUsingCache();
                }
            }
            else
            {
                SpawnServer();
                TcpConnection = ConnectUsingCache();
            }
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        /// <summary>
        /// Called to connect to either an existing server, or a newly spawned one if none exists.
        /// </summary>
        void ReconnectToServer()
        {
            if (TcpConnection != null)
            {
                try
                {
                    TcpConnection.EstablishConnection();
                }
                catch
                {
                    throw new Exception("Can’t connect: try entering / exiting play mode");
                }
            }
            else
            {
                throw new Exception("Can’t connect: try entering / exiting play mode");
            }
        }

        /// <summary>
        /// Called before the assembly reloads -- responsible for shutting down network comms safely
        /// </summary>
        static void OnBeforeAssemblyReload()
        {
            NetworkComms.Shutdown();
        }

        /// <summary>
        /// Make a synchronous call to the server using the given method name as event names, as well as the given
        /// parameters (if any), blocks on a return message, used for passing exceptions - no data response expected
        /// </summary>
        /// <param name="methodName">The method to execute on the server</param>
        /// <param name="objectsToSend">Objects that will be sent in this call (can be null)</param>
        public void CallSynchronous(string methodName, params Object[] objectsToSend)
        {
            if (!TcpConnection.ConnectionAlive())
                ReconnectToServer();
            string xml;
            if (objectsToSend == null || objectsToSend.Length.Equals(0))
            {
                xml = TcpConnection.SendReceiveObject<string>(methodName.ToLower(), methodName.ToUpper(), k_TimeoutMillis);
            }
            else
            {
                xml = TcpConnection.SendReceiveObject<string, string>(methodName.ToLower(),
                    methodName.ToUpper(), k_TimeoutMillis, Serialization.Serialize(objectsToSend));
            }

            HandleResponse(xml);
        }

        /// <summary>
        /// Make a synchronous call to the server using the given method name as event names, as well as the given
        /// parameters (if any), blocks on a return message, used for passing exceptions - no data response expected
        /// </summary>
        /// <param name="methodName">The method to execute on the server</param>
        /// <param name="objectsToSend">Objects that will be sent in this call (can be null)</param>
        /// <returns>Object of provided type T.</returns>
        public T CallSynchronous<T>(string methodName, params object[] objectsToSend)
        {
            if (!TcpConnection.ConnectionAlive())
                ReconnectToServer();
            string xml;
            if (objectsToSend == null || objectsToSend.Length.Equals(0))
            {
                xml = TcpConnection.SendReceiveObject<string>(methodName.ToLower(), methodName.ToUpper(), k_TimeoutMillis);
            }
            else
            {
                xml = TcpConnection.SendReceiveObject<string, string>(methodName.ToLower(),
                    methodName.ToUpper(), k_TimeoutMillis, Serialization.Serialize(objectsToSend));
            }

            return HandleResponse<T>(xml);
        }

        /// <summary>
        /// Checks if an exception had been thrown on the proxy, rethrowing the exception on the client.
        /// </summary>
        /// <param name="xml">serialized response string</param>
        /// <returns></returns>
        static void HandleResponse(string xml)
        {
            ResponseWrapper result = Serialization.DeserializeResponse(xml);

            if (result == null)
            {
                throw new NullReferenceException();
            }

            if (result.ResponseException != null)
            {
                throw new CollabProxyException(k_ServerErrorMsg, result.ResponseException);
            }
        }

        /// <summary>
        /// Checks if an exception had been thrown on the proxy, rethrowing the exception on the client.
        /// Deserialize a response to the specified type.
        /// </summary>
        /// <param name="xml">serialized response string</param>
        /// <returns></returns>
        static T HandleResponse<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            try
            {
                var result = Serialization.DeserializeResponse<T>(xml);

                if (result == null)
                {
                    throw new NullReferenceException();
                }

                if (result.ResponseException != null)
                {
                    throw new CollabProxyException(k_ServerErrorMsg, result.ResponseException);
                }

                return result.ResponseObject;
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
                ResponseWrapper baseResponse = Serialization.DeserializeResponse(xml);
                throw new CollabProxyException(k_ServerErrorMsg, baseResponse.ResponseException);
            }
        }

        /// <summary>
        /// Make an asynchronous call to the server using the given method name as event name, as well as the given
        /// parameters (if any)
        /// </summary>
        /// <param name="methodName">The method to execute on the server</param>
        /// <param name="objectsToSend">Objects that will be sent in this call (can be null)</param>
        public void CallAsynchronous(string methodName, params object[] objectsToSend)
        {
            if (!TcpConnection.ConnectionAlive())
                ReconnectToServer();
            if (objectsToSend == null || objectsToSend.Length.Equals(0))
            {
                TcpConnection.SendObject(methodName.ToLower());
            }
            else
            {
                TcpConnection.SendObject(methodName.ToLower(), Serialization.Serialize(objectsToSend));
            }
        }

        /// <summary>
        /// Register a listener for messages of a given type that are sent from the server to the client.
        /// </summary>
        /// <param name="message">The name of the message to listen to.</param>
        /// <param name="handler">The action to execute when the message is received.</param>
        /// <param name="exceptionHandler">The action to execute when the message is an exception.</param>
        public void RegisterListener(string message, Action handler, Action<Exception> exceptionHandler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler", "Registering a listener requires valid handler");
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message", "Registering a listener requires valid message");
            }

            var method = message.ToUpper();

            if (TcpConnection.IncomingPacketHandlerExists(method))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Cannot register a second packet handler with the same name {0}", message);
                throw new InvalidOperationException(sb.ToString());
            }

            TcpConnection.AppendIncomingPacketHandler<string>(method, (packetHeader, connection, incoming) =>
            {
                try
                {
                    HandleResponse(incoming);
                    handler();
                }
                catch (Exception e)
                {
                    exceptionHandler(e);
                    // This is needed, otherwise the connection to the proxy server drops.
                    throw;
                }
            }, NetworkComms.DefaultSendReceiveOptions);
        }

        /// <summary>
        /// Register a listener for messages of a given type that are sent from the server to the client.
        /// </summary>
        /// <param name="message">The name of the message to listen to.</param>
        /// <param name="handler">The action to execute when the message is received.</param>
        /// <param name="exceptionHandler">The action to execute when the message is an exception.</param>
        public void RegisterListener<T>(string message, Action<T> handler, Action<Exception> exceptionHandler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler", "Registering a listener requires valid handler");
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message", "Registering a listener requires valid message");
            }

            var method = message.ToUpper();

            if (TcpConnection.IncomingPacketHandlerExists(method))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Cannot register a second packet handler with the same name {0}", message);
                throw new InvalidOperationException(sb.ToString());
            }

            TcpConnection.AppendIncomingPacketHandler<string>(method, (packetHeader, connection, incoming) =>
            {
                try
                {
                    var payload = HandleResponse<T>(incoming);
                    handler(payload);
                }
                catch (Exception e)
                {
                    exceptionHandler(e);
                    // This is needed, otherwise the connection to the proxy server drops.
                    throw;
                }
            }, NetworkComms.DefaultSendReceiveOptions);
        }

        /// <summary>
        /// Deregister all handlers that have been registered on this connection
        /// </summary>
        public void DeregisterListeners()
        {
            TcpConnection.RemoveIncomingPacketHandler();
        }


        /// <summary>
        /// Check whether the port file exists and contains a valid port, return True if it does, False otherwise
        /// </summary>
        /// <returns>Whether a port number has been cached</returns>
        static bool IsCachedServerAvailable()
        {
            return GetCachedPort() != 0;
        }

        /// <summary>
        /// Spawns a new instance of the server and caches the resulting port in the port file
        /// </summary>
        protected virtual void SpawnServer()
        {
            int serverPort;
            Process serverProcess = MonoProcessUtility.PrepareMonoProcessBleedingEdge(Directory.GetCurrentDirectory());
            serverProcess.StartInfo.Arguments = string.Format("\"{0}\" {1}", GetServerExePath(), Process.GetCurrentProcess().Id);
            serverProcess.StartInfo.EnvironmentVariables[k_MonoPath] = null;
            serverProcess.Start();

            string serverOutput = serverProcess.StandardOutput.ReadLine();

            int.TryParse(serverOutput, out serverPort);
            WritePortToFile(serverPort);
        }

        /// <summary>
        /// Writes the given port to the port file, if non-zero
        /// </summary>
        /// <param name="serverPort">The server port to cache</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when input does not contain valid port</exception>
        protected static void WritePortToFile(int serverPort)
        {
            if (serverPort == 0)
            {
                throw new ArgumentException("Expected server to return a valid port");
            }
            using (FileStream fileStream = File.Open(GetPortFilePath(), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    sw.WriteLine(serverPort);
                }
            }
        }

        /// <summary>
        /// Establishes a connection to the server using the cached port number
        /// </summary>
        /// <returns>TCPConnection object for client-server communication</returns>
        static TCPConnection ConnectUsingCache()
        {
            int serverPort = GetCachedPort();
            ConnectionInfo connInfo = new ConnectionInfo(IPAddress.Loopback.ToString(), serverPort);
            return TCPConnection.GetConnection(connInfo);
        }

        /// <summary>
        /// Get the cached port number, if one is found, or 0 otherwise
        /// </summary>
        /// <returns>The cached port number, default 0</returns>
        static int GetCachedPort()
        {
            int serverPort = 0;
            string portFile = GetPortFilePath();
            if (File.Exists(portFile))
            {
                using (StreamReader sr = new StreamReader(portFile))
                {
                    int.TryParse(sr.ReadToEnd(), out serverPort);
                }
            }

            return serverPort;
        }

        /// <summary>
        /// Get the path to the Git proxy port file
        /// </summary>
        /// <returns>Full path to the Git proxy port file</returns>
        public static string GetPortFilePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(),
                k_PortFilePath.Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Get the path to the Git proxy server executable
        /// </summary>
        /// <returns>Full path to the Git proxy server executable</returns>
        static string GetServerExePath()
        {
            // GetFullPath will get us the actual location of the package, even if it isn't in the project dir
            return Path.GetFullPath(k_ServerExePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }
    }
}
