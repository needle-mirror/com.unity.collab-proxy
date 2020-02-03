using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Unity.Cloud.Collaborate.Models.Providers.Client;
using Unity.Cloud.Collaborate.Models.Providers.Client.Models;
using NUnit.Framework;

namespace Unity.Cloud.Collaborate.Tests.Models.Providers.Client
{
    [TestFixture]
    internal class GitProxyTests
    {
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
        public void InitializeRepository_WhenCalled_SendsTcpMessage()
        {
            var methodCalled = false;
            TestHelper.RegisterListener("initializerepository", val => methodCalled = true);
            var theClient = new TestableCollabProxyClient();
            theClient.StartOrConnectToServer();
            IGitProxy theProxy = new GitProxy(theClient);
            // Make sure nothing happened up to this point
            Assert.IsFalse(methodCalled);
            theProxy.InitializeRepository();
            Assert.IsTrue(methodCalled);
        }

        [Test]
        public void SetRemoteOrigin_WhenCalled_SendsTcpMessage()
        {
            var methodCalled = false;
            TestHelper.RegisterListener("setremoteorigin", val => methodCalled = true);
            var theClient = new TestableCollabProxyClient();
            theClient.StartOrConnectToServer();
            IGitProxy theProxy = new GitProxy(theClient);
            // Make sure nothing happened up to this point
            Assert.IsFalse(methodCalled);
            theProxy.SetRemoteOrigin("");
            Assert.IsTrue(methodCalled);
        }

        [Test]
        public void GetWorkingDirectoryChangesAsync_WhenCalled_SendsTcpMessage()
        {
            var methodCalled = false;
            var resetHandle = new ManualResetEvent(false);
            TestHelper.RegisterListener("GetWorkingDirectoryChangesAsync".ToLower(), val =>
                {
                    methodCalled = true;
                    resetHandle.Set();
                });
            var theClient = new TestableCollabProxyClient();
            theClient.StartOrConnectToServer();
            IGitProxy theProxy = new GitProxy(theClient);
            // Make sure nothing happened up to this point
            Assert.IsFalse(methodCalled);
            theProxy.GetWorkingDirectoryChangesAsync();
            resetHandle.WaitOne();
            Assert.IsTrue(methodCalled);
        }

        [TearDown]
        public void UnsetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(m_OriginalPath);
        }
    }
}
