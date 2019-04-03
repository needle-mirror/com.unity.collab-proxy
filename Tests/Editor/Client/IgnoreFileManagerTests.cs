using System;
using System.IO;
using CollabProxy.Client;
using NUnit.Framework;

namespace CollabProxy.Tests
{
    internal class TestableIgnoreFileManager : IgnoreFileManager
    {
        public bool IgnoreFileAlreadyExists { get; set; }
        
        protected override bool IgnoreFileExists()
        {
            return IgnoreFileAlreadyExists;
        }

        public string Error { get; set; }

        protected override void DebugLogError(string error)
        {
            Error = error;
        }
    }
    
    [TestFixture]
    internal class IgnoreFileManagerTests
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
        public void CreateOrMigrateIgnoreFile_WhenIgnoreFileNotExist_CreatesNewIgnoreFile()
        {
            string baseText = "base";
            Directory.CreateDirectory(Path.GetDirectoryName(IgnoreFileManager.k_BaseCollabIgnoreFile));
            File.Create(IgnoreFileManager.k_BaseCollabIgnoreFile).Close();
            File.WriteAllText(IgnoreFileManager.k_BaseCollabIgnoreFile, baseText);
            TestableIgnoreFileManager testableIgnoreFileManager = new TestableIgnoreFileManager { IgnoreFileAlreadyExists = false};
            testableIgnoreFileManager.CreateOrMigrateIgnoreFile();
            string ignoreTextAfter = File.ReadAllText(IgnoreFileManager.k_CollabIgnoreFile);
            Assert.AreEqual(baseText, ignoreTextAfter);
            Assert.That(!File.Exists(IgnoreFileManager.k_CollabIgnoreBackup));
            Assert.That(string.IsNullOrEmpty(testableIgnoreFileManager.Error));
        }
        
        [Test]
        public void CreateOrMigrateIgnoreFile_WhenIgnoreFileAlreadyExists_BacksUpIgnoreFile()
        {
            string baseText = "base";
            string ignoreText = "ignore";
            Directory.CreateDirectory(Path.GetDirectoryName(IgnoreFileManager.k_BaseCollabIgnoreFile));
            File.Create(IgnoreFileManager.k_BaseCollabIgnoreFile).Close();
            File.Create(IgnoreFileManager.k_CollabIgnoreFile).Close();
            File.WriteAllText(IgnoreFileManager.k_BaseCollabIgnoreFile, baseText);
            File.WriteAllText(IgnoreFileManager.k_CollabIgnoreFile, ignoreText);
            TestableIgnoreFileManager testableIgnoreFileManager = new TestableIgnoreFileManager { IgnoreFileAlreadyExists = true};
            testableIgnoreFileManager.CreateOrMigrateIgnoreFile();
            string backupTextAfter = File.ReadAllText(IgnoreFileManager.k_CollabIgnoreBackup);
            Assert.AreEqual(ignoreText, backupTextAfter);
            string ignoreTextAfter = File.ReadAllText(IgnoreFileManager.k_CollabIgnoreFile);
            Assert.AreEqual(baseText, ignoreTextAfter);
            Assert.That(string.IsNullOrEmpty(testableIgnoreFileManager.Error));
        }

        [Test]
        public void CreateOrMigrateIgnoreFile_WhenBackUpFileAlreadyExists_DoesNothing()
        {
            string backupText = "backup";
            string ignoreText = "ignore";
            Directory.CreateDirectory(Path.GetDirectoryName(IgnoreFileManager.k_BaseCollabIgnoreFile));
            File.Create(IgnoreFileManager.k_BaseCollabIgnoreFile).Close();
            File.Create(IgnoreFileManager.k_CollabIgnoreFile).Close();
            File.WriteAllText(IgnoreFileManager.k_CollabIgnoreBackup, backupText);
            File.WriteAllText(IgnoreFileManager.k_CollabIgnoreFile, ignoreText);
            TestableIgnoreFileManager testableIgnoreFileManager = new TestableIgnoreFileManager { IgnoreFileAlreadyExists = true };
            testableIgnoreFileManager.CreateOrMigrateIgnoreFile();
            Assert.That(!string.IsNullOrEmpty(testableIgnoreFileManager.Error));
            string backupTextAfter = File.ReadAllText(IgnoreFileManager.k_CollabIgnoreBackup);
            Assert.AreEqual(backupText, backupTextAfter);
            string ignoreTextAfter = File.ReadAllText(IgnoreFileManager.k_CollabIgnoreFile);
            Assert.AreEqual(ignoreText, ignoreTextAfter);
        }

        [TearDown]
        public void UnsetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(originalPath);
        }
    }
}
