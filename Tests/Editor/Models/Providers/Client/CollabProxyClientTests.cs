using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Unity.Cloud.Collaborate.Models.Providers.Client;
using Unity.Cloud.Collaborate.Models.Providers.Client.Models;
using NetworkCommsDotNet.Connections;
using NUnit.Framework;

namespace Unity.Cloud.Collaborate.Tests.Models.Providers.Client
{
    internal class TestableCollabProxyClient : CollabProxyClient
    {
        protected override void SpawnServer()
        {
            Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Loopback, 0));
            var listenPort =
                ((IPEndPoint) Connection.ExistingLocalListenEndPoints(ConnectionType.TCP).First()).Port;
            Directory.CreateDirectory("Library");
            WritePortToFile(listenPort);
        }

        public static void ResetPort(int value)
        {
            WritePortToFile(value);
        }

        public bool IsConnected()
        {
            return TcpConnection?.ConnectionAlive() ?? false;
        }
    }

    [TestFixture]
    internal class CollabProxyClientTests
    {
        const int k_ResetPortValue = 80;
        string m_OriginalPath;

        [SetUp]
        public void SetCurrentDirectory()
        {
            m_OriginalPath = Directory.GetCurrentDirectory();
            var tempDir = Guid.NewGuid().ToString();
            Directory.SetCurrentDirectory(Path.GetTempPath());
            Directory.CreateDirectory(tempDir);
            Directory.SetCurrentDirectory(tempDir);
        }

        [Test]
        public void CollabProxyClient_InstantiatedWithoutPortCache_CachesNewPort()
        {
            Assert.IsFalse(File.Exists(CollabProxyClient.GetPortFilePath()));
            var theClient = new TestableCollabProxyClient();
            theClient.StartOrConnectToServer();
            Assert.IsTrue(File.Exists(CollabProxyClient.GetPortFilePath()));
            Assert.AreNotEqual(0, TestHelper.GetPortFromFile());
        }

        [Test]
        public void CollabProxyClient_InstantiatedWithoutPortCache_HasValidConnection()
        {
            var theClient = new TestableCollabProxyClient();
            theClient.StartOrConnectToServer();
            Assert.IsTrue(theClient.IsConnected());
        }

        [Test]
        public void CollabProxyClient_InstantiatedWithGoodPortCache_DoesNotCacheNewPort()
        {
            var firstClient = new TestableCollabProxyClient();
            var firstPort = TestHelper.GetPortFromFile();
            var secondClient = new TestableCollabProxyClient();
            var secondPort = TestHelper.GetPortFromFile();
            Assert.AreEqual(firstPort, secondPort);
        }

        [Test]
        public void CollabProxyClient_InstantiatedWithGoodPortCache_HasValidConnection()
        {
            var firstClient = new TestableCollabProxyClient();
            firstClient.StartOrConnectToServer();
            var secondClient = new TestableCollabProxyClient();
            secondClient.StartOrConnectToServer();
            Assert.IsTrue(secondClient.IsConnected());
        }

        [Test]
        public void CollabProxyClient_InstantiatedWithBadPortCache_CachesNewPort()
        {
            var firstClient = new TestableCollabProxyClient();
            firstClient.StartOrConnectToServer();
            Connection.StopListening();
            TestableCollabProxyClient.ResetPort(k_ResetPortValue);
            var secondClient = new TestableCollabProxyClient();
            secondClient.StartOrConnectToServer();
            var secondPort = TestHelper.GetPortFromFile();
            Assert.AreNotEqual(k_ResetPortValue, secondPort);
        }

        [Test]
        public void CollabProxyClient_InstantiatedWithBadPortCache_HasValidConnection()
        {
            var firstClient = new TestableCollabProxyClient();
            firstClient.StartOrConnectToServer();
            Connection.StopListening();
            var secondClient = new TestableCollabProxyClient();
            secondClient.StartOrConnectToServer();
            Assert.IsTrue(secondClient.IsConnected());
        }

        [Test]
        public void CallSynchronous_WhenCalledWithoutParams_SendsTcpMessageWithoutParams()
        {
            var result = "";
            var methodCalled = false;
            var theClient = new TestableCollabProxyClient();
            theClient.StartOrConnectToServer();
            // Register a listener on the TCP connection
            TestHelper.RegisterListener("randomness", val =>
            {
                methodCalled = true;
                result = val;
            });
            theClient.CallSynchronous("Randomness");
            Assert.IsTrue(methodCalled);
            Assert.AreEqual("", result);
        }

        [Test]
        public void CallSynchronous_WhenCalledWithParams_SendsTcpMessageWithParams()
        {
            var result = "";
            var methodCalled = false;
            var theClient = new TestableCollabProxyClient();
            theClient.StartOrConnectToServer();
            // Register a listener on the TCP connection
            TestHelper.RegisterListener("randomness", val =>
            {
                methodCalled = true;
                result = val;
            });
            theClient.CallSynchronous("Randomness", "foo", "bar");
            Assert.IsTrue(methodCalled);
            var objects = Serialization.Deserialize<Object[]>(result);
            Assert.AreEqual(objects[0], "foo");
            Assert.AreEqual(objects[1], "bar");
        }

        [Test, Timeout(2000)]
        public void CallAsynchronous_WhenCalledWithoutParams_SendsTcpMessageWithoutParams()
        {
            var resetHandle = new ManualResetEvent(false);
            var result = "";
            var methodCalled = false;
            var theClient = new TestableCollabProxyClient();
            theClient.StartOrConnectToServer();
            // Register a listener on the TCP connection
            TestHelper.RegisterListener("randomness", val =>
            {
                methodCalled = true;
                result = val;
                resetHandle.Set();
            });
            theClient.CallAsynchronous("Randomness");
            resetHandle.WaitOne();
            Assert.IsTrue(methodCalled);
            Assert.AreEqual("", result);
        }

        [Test, Timeout(2000)]
        public void CallAsynchronous_WhenCalledWithParams_SendsTcpMessageWithParams()
        {
            var resetHandle = new ManualResetEvent(false);
            var result = "";
            var methodCalled = false;
            var theClient = new TestableCollabProxyClient();
            theClient.StartOrConnectToServer();
            // Register a listener on the TCP connection
            TestHelper.RegisterListener("randomness", val =>
            {
                methodCalled = true;
                result = val;
                resetHandle.Set();
            });
            theClient.CallAsynchronous("Randomness", "foo", "bar");
            resetHandle.WaitOne();
            Assert.IsTrue(methodCalled);
            var objects = Serialization.Deserialize<object[]>(result);
            Assert.AreEqual(objects[0], "foo");
            Assert.AreEqual(objects[1], "bar");
        }

        [TearDown]
        public void UnsetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(m_OriginalPath);
        }
    }
}
