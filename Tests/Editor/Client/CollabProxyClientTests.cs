using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using CollabProxy.Client;
using CollabProxy.Models;
using NetworkCommsDotNet.Connections;
using NUnit.Framework;

namespace CollabProxy.Tests
{
    internal class TestableCollabProxyClient : CollabProxyClient
    {
        protected override void SpawnServer()
        {
            Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Loopback, 0));
            int listenPort =
                ((IPEndPoint) Connection.ExistingLocalListenEndPoints(ConnectionType.TCP).First()).Port;
            Directory.CreateDirectory("Library");
            WritePortToFile(listenPort);
        }
        
        public void ResetPort(int value)
        {
            WritePortToFile(value);
        }
        
        public bool IsConnected()
        {
            return TcpConnection.ConnectionAlive();
        }
    }
    
    [TestFixture]
    internal class CollabProxyClientTests
    {
        const int k_ResetPortValue = 80;
        string originalPath;
        
        [SetUp]
        public void SetCurrentDirectory()
        {
            originalPath = Directory.GetCurrentDirectory();
            string tempDir = Guid.NewGuid().ToString();
            Directory.SetCurrentDirectory(Path.GetTempPath());
            Directory.CreateDirectory(tempDir);
            Directory.SetCurrentDirectory(tempDir);
        }
        
        [Test]
        public void CollabProxyClient_InstantiatedWithoutPortCache_CachesNewPort()
        {
            Assert.IsFalse(File.Exists(CollabProxyClient.GetPortFilePath()));
            TestableCollabProxyClient theClient = new TestableCollabProxyClient();
            Assert.IsTrue(File.Exists(CollabProxyClient.GetPortFilePath()));
            Assert.AreNotEqual(0, TestHelper.GetPortFromFile());
        }
        
        [Test]
        public void CollabProxyClient_InstantiatedWithoutPortCache_HasValidConnection()
        {
            TestableCollabProxyClient theClient = new TestableCollabProxyClient();
            Assert.IsTrue(theClient.IsConnected());
        }
        
        [Test]
        public void CollabProxyClient_InstantiatedWithGoodPortCache_DoesNotCacheNewPort()
        {
            TestableCollabProxyClient firstClient = new TestableCollabProxyClient();
            int firstPort = TestHelper.GetPortFromFile();
            TestableCollabProxyClient secondClient = new TestableCollabProxyClient();
            int secondPort = TestHelper.GetPortFromFile();
            Assert.AreEqual(firstPort, secondPort);
        }
        
        [Test]
        public void CollabProxyClient_InstantiatedWithGoodPortCache_HasValidConnection()
        {
            TestableCollabProxyClient firstClient = new TestableCollabProxyClient();
            TestableCollabProxyClient secondClient = new TestableCollabProxyClient();
            Assert.IsTrue(secondClient.IsConnected());
        }
        
        [Test]
        public void CollabProxyClient_InstantiatedWithBadPortCache_CachesNewPort()
        {
            TestableCollabProxyClient firstClient = new TestableCollabProxyClient();
            Connection.StopListening();
            firstClient.ResetPort(k_ResetPortValue);
            TestableCollabProxyClient secondClient = new TestableCollabProxyClient();
            int secondPort = TestHelper.GetPortFromFile();
            Assert.AreNotEqual(k_ResetPortValue, secondPort);
        }
        
        [Test]
        public void CollabProxyClient_InstantiatedWithBadPortCache_HasValidConnection()
        {
            TestableCollabProxyClient firstClient = new TestableCollabProxyClient();
            Connection.StopListening();
            TestableCollabProxyClient secondClient = new TestableCollabProxyClient();
            Assert.IsTrue(secondClient.IsConnected());
        }

        [Test]
        public void CallSynchronous_WhenCalledWithoutParams_SendsTcpMessageWithoutParams()
        {
            string result = "";
            bool methodCalled = false;
            TestableCollabProxyClient theClient = new TestableCollabProxyClient();
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
            string result = "";
            bool methodCalled = false;
            TestableCollabProxyClient theClient = new TestableCollabProxyClient();
            // Register a listener on the TCP connection
            TestHelper.RegisterListener("randomness", val =>
            {
                methodCalled = true;
                result = val;
            });
            theClient.CallSynchronous("Randomness", "foo", "bar");
            Assert.IsTrue(methodCalled);
            Object[] objects = Serialization.Deserialize<Object[]>(result);
            Assert.AreEqual(objects[0], "foo");
            Assert.AreEqual(objects[1], "bar");
        }

        [Test, Timeout(2000)]
        public void CallAsynchronous_WhenCalledWithoutParams_SendsTcpMessageWithoutParams()
        {
            ManualResetEvent resetHandle = new ManualResetEvent(false);
            string result = "";
            bool methodCalled = false;
            TestableCollabProxyClient theClient = new TestableCollabProxyClient();
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
            ManualResetEvent resetHandle = new ManualResetEvent(false);
            string result = "";
            bool methodCalled = false;
            TestableCollabProxyClient theClient = new TestableCollabProxyClient();
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
            Object[] objects = Serialization.Deserialize<Object[]>(result);
            Assert.AreEqual(objects[0], "foo");
            Assert.AreEqual(objects[1], "bar");
        }
        
        [TearDown]
        public void UnsetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(originalPath);
        }
    }
}
