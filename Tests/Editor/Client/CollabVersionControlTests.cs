using System;
using System.IO;
using CollabProxy.Client;
using CollabProxy.Models;
using NUnit.Framework;

namespace CollabProxy.Tests
{
    
    internal class TestableCollabVersionControl : CollabVersionControl
    {
        public string ProjectId { get; set; }
        public string BaseUrl { get; set; }
        public bool OnFileSystemChangedCalled { get; set; }

        public TestableCollabVersionControl(IGitProxy gitProxy) : base(null, gitProxy)
        {
        }

        protected override void OnFileSystemChanged()
        {
            OnFileSystemChangedCalled = true;
        }

        protected override string GetProjectId()
        {
            return ProjectId;
        }
        
        protected override string GetAccessToken()
        {
            return "foobar";
        }
        
        protected override string GetCollabTip()
        {
            return "foobar";
        }
        
        protected override string GetBaseUrl()
        {
            return BaseUrl;
        }
        
        protected override void RegisterListeners()
        {
        }
                
        protected override void DeregisterListeners()
        {
        }

        protected override void UpdateHeadRevision()
        {
        }
        public bool IsJobRunningReturn = false;
        public override bool IsJobRunning
        {
            get { return IsJobRunningReturn; }
        }
    }

    internal class TestableGitProxy : IGitProxy
    {
        public bool IsRunningAsyncOperations()
        {
            return true;
        }
        public bool RepositoryExistsReturn = true;
        public bool RepositoryExists()
        {
            return RepositoryExistsReturn;
        }
        public int InitializeRepositoryCount = 0;
        public void InitializeRepository()
        {
            InitializeRepositoryCount++;
        }
        public void RemoveRepository()
        {
        }
        public int SetRemoteOriginCount = 0;
        public void SetRemoteOrigin(string cloneUrl)
        {
            SetRemoteOriginCount++;
        }
        public void SetCurrentHead(string revisionId, string accessToken)
        {
        }
        public void GetWorkingDirectoryChangesAsync(string callBackName)
        {
        }
    }

    [TestFixture]
    internal class CollabVersionControlTests
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
        public void OnEnableVersionControl_WithRepoNoProjectId_InitializesRepository()
        {
            TestableGitProxy gitProxy = new TestableGitProxy();
            TestableCollabVersionControl collabVersionControl = new TestableCollabVersionControl(gitProxy);
            collabVersionControl.OnEnableVersionControl();
            Assert.That(gitProxy.InitializeRepositoryCount == 1);
        }

        [Test]
        public void OnEnableVersionControl_WithNoRepoWithProjectId_InitializesRepository()
        {
            TestableGitProxy gitProxy = new TestableGitProxy();
            TestableCollabVersionControl collabVersionControl = new TestableCollabVersionControl(gitProxy);
            collabVersionControl.ProjectId = "anything";
            collabVersionControl.OnEnableVersionControl();
            Assert.That(gitProxy.InitializeRepositoryCount == 1);
        }
        
        [Test]
        public void OnEnableVersionControl_WithNoRepoWithProjectId_SetsRemoteOrigin()
        {
            TestableGitProxy gitProxy = new TestableGitProxy();
            gitProxy.RepositoryExistsReturn = false;
            TestableCollabVersionControl collabVersionControl = new TestableCollabVersionControl(gitProxy);
            collabVersionControl.ProjectId = "anything";
            collabVersionControl.BaseUrl = "http://bestsiteever.com";
            collabVersionControl.OnEnableVersionControl();
            Assert.That(gitProxy.SetRemoteOriginCount == 1);
        }
        
        [Test]
        public void OnEnableVersionControl_WithRepoNoProjectId_DoesNotSetRemoteOrigin()
        {
            TestableGitProxy gitProxy = new TestableGitProxy();
            gitProxy.RepositoryExistsReturn = true;
            TestableCollabVersionControl collabVersionControl = new TestableCollabVersionControl(gitProxy);
            collabVersionControl.OnEnableVersionControl();
            Assert.That(gitProxy.SetRemoteOriginCount == 0);
        }
        
        [Test]
        public void OnEnableVersionControl_WithNoRepoWithProjectId_SetsCurrentHead()
        {
            TestableGitProxy gitProxy = new TestableGitProxy();
            gitProxy.RepositoryExistsReturn = false;
            TestableCollabVersionControl collabVersionControl = new TestableCollabVersionControl(gitProxy);
            collabVersionControl.ProjectId = "anything";
            collabVersionControl.BaseUrl = "http://bestsiteever.com";
            collabVersionControl.OnEnableVersionControl();
            Assert.That(gitProxy.SetRemoteOriginCount == 1);
        }
        
        [Test]
        public void OnEnableVersionControl_WithNoRepoWithProjectId_CallsOnFileSystemChanged()
        {
            TestableGitProxy gitProxy = new TestableGitProxy();
            gitProxy.RepositoryExistsReturn = false;
            TestableCollabVersionControl collabVersionControl = new TestableCollabVersionControl(gitProxy);
            collabVersionControl.ProjectId = "anything";
            collabVersionControl.BaseUrl = "http://bestsiteever.com";
            collabVersionControl.OnEnableVersionControl();
            Assert.AreEqual(true, collabVersionControl.OnFileSystemChangedCalled);
        }
        
        [TearDown]
        public void UnsetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(originalPath);
        }
    }
}
