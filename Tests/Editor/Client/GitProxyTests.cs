using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CollabProxy.Models;
using NUnit.Framework;

namespace CollabProxy.Tests
{
    [TestFixture]
    internal class GitProxyTests
    {
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
        public void InitializeRepository_WhenCalled_SendsTcpMessage()
        {
            bool methodCalled = false;
            TestableCollabProxyClient theClient = new TestableCollabProxyClient();
            TestHelper.RegisterListener("initializerepository", val => { methodCalled = true; });
            IGitProxy theProxy = new GitProxy(theClient);
            // Make sure nothing happened up to this point
            Assert.IsFalse(methodCalled);
            theProxy.InitializeRepository();
            Assert.IsTrue(methodCalled);
        }

        [Test]
        public void SetRemoteOrigin_WhenCalled_SendsTcpMessage()
        {
            bool methodCalled = false;
            TestableCollabProxyClient theClient = new TestableCollabProxyClient();
            TestHelper.RegisterListener("setremoteorigin", val => { methodCalled = true; });
            IGitProxy theProxy = new GitProxy(theClient);
            // Make sure nothing happened up to this point
            Assert.IsFalse(methodCalled);
            theProxy.SetRemoteOrigin("");
            Assert.IsTrue(methodCalled);
        }

        [Test]
        public void GetWorkingDirectoryChangesAsync_WhenCalled_SendsTcpMessage()
        {
            bool methodCalled = false;
            TestableCollabProxyClient theClient = new TestableCollabProxyClient();

            ManualResetEvent resetHandle = new ManualResetEvent(false);
            TestHelper.RegisterListener("GetWorkingDirectoryChangesAsync".ToLower(),
                val =>
                {
                    methodCalled = true;
                    resetHandle.Set();
                });

            IGitProxy theProxy = new GitProxy(theClient);
            // Make sure nothing happened up to this point
            Assert.IsFalse(methodCalled);
            theProxy.GetWorkingDirectoryChangesAsync("callbackName");
            resetHandle.WaitOne();
            Assert.IsTrue(methodCalled);
        }

        [TearDown]
        public void UnsetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(originalPath);
        }
    }
}