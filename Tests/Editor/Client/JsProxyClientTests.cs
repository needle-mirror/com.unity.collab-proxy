using System;
using System.IO;
using CollabProxy.Client;
using CollabProxy.Models;
using NUnit.Framework;

namespace CollabProxy.Tests
{
    [TestFixture]
    internal class JsProxyClientTests
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

        [TestCase(true)]
        [TestCase(false)]
        public void JsProxyClient_WhenJobRunningCalled_ReturnsVersionControlIsJobRunning(bool jobIsRunning)
        {
            TestableGitProxy gitProxy = new TestableGitProxy();
            TestableCollabVersionControl collabVersionControlMock = new TestableCollabVersionControl(gitProxy);
            collabVersionControlMock.IsJobRunningReturn = jobIsRunning;
            JsProxyClient jpc = new JsProxyClient(collabVersionControlMock);
            Assert.AreEqual(jobIsRunning, jpc.IsJobRunning());
        }

        [TearDown]
        public void UnsetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(originalPath);
        }
    }
}
