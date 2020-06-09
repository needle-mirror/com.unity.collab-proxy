using System;
using System.IO;
using Unity.Cloud.Collaborate.Models.Providers.Client;
using NUnit.Framework;

namespace Unity.Cloud.Collaborate.Tests.Models.Providers.Client
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
        public void CreateOrMigrateIgnoreFile_WhenIgnoreFileNotExist_CreatesNewIgnoreFile()
        {
            const string baseText = "base";
            Directory.CreateDirectory(Path.GetDirectoryName(IgnoreFileManager.baseCollabIgnoreFile));
            File.Create(IgnoreFileManager.baseCollabIgnoreFile).Close();
            File.WriteAllText(IgnoreFileManager.baseCollabIgnoreFile, baseText);
            var testableIgnoreFileManager = new TestableIgnoreFileManager { IgnoreFileAlreadyExists = false};
            testableIgnoreFileManager.CreateOrMigrateIgnoreFile();
            var ignoreTextAfter = File.ReadAllText(IgnoreFileManager.collabIgnoreFile);
            Assert.AreEqual(baseText, ignoreTextAfter);
            Assert.That(!File.Exists(IgnoreFileManager.collabIgnoreBackup));
            Assert.That(string.IsNullOrEmpty(testableIgnoreFileManager.Error));
        }

        [Test]
        public void CreateOrMigrateIgnoreFile_WhenIgnoreFileAlreadyExists_BacksUpIgnoreFile()
        {
            const string baseText = "base";
            const string ignoreText = "ignore";
            Directory.CreateDirectory(Path.GetDirectoryName(IgnoreFileManager.baseCollabIgnoreFile));
            File.Create(IgnoreFileManager.baseCollabIgnoreFile).Close();
            File.Create(IgnoreFileManager.collabIgnoreFile).Close();
            File.WriteAllText(IgnoreFileManager.baseCollabIgnoreFile, baseText);
            File.WriteAllText(IgnoreFileManager.collabIgnoreFile, ignoreText);
            var testableIgnoreFileManager = new TestableIgnoreFileManager { IgnoreFileAlreadyExists = true};
            testableIgnoreFileManager.CreateOrMigrateIgnoreFile();
            var backupTextAfter = File.ReadAllText(IgnoreFileManager.collabIgnoreBackup);
            Assert.AreEqual(ignoreText, backupTextAfter);
            var ignoreTextAfter = File.ReadAllText(IgnoreFileManager.collabIgnoreFile);
            Assert.AreEqual(baseText, ignoreTextAfter);
            Assert.That(string.IsNullOrEmpty(testableIgnoreFileManager.Error));
        }

        [Test]
        public void CreateOrMigrateIgnoreFile_WhenBackUpFileAlreadyExists_DoesNothing()
        {
            const string backupText = "backup";
            const string ignoreText = "ignore";
            Directory.CreateDirectory(Path.GetDirectoryName(IgnoreFileManager.baseCollabIgnoreFile));
            File.Create(IgnoreFileManager.baseCollabIgnoreFile).Close();
            File.Create(IgnoreFileManager.collabIgnoreFile).Close();
            File.WriteAllText(IgnoreFileManager.collabIgnoreBackup, backupText);
            File.WriteAllText(IgnoreFileManager.collabIgnoreFile, ignoreText);
            var testableIgnoreFileManager = new TestableIgnoreFileManager { IgnoreFileAlreadyExists = true };
            testableIgnoreFileManager.CreateOrMigrateIgnoreFile();
            Assert.That(!string.IsNullOrEmpty(testableIgnoreFileManager.Error));
            var backupTextAfter = File.ReadAllText(IgnoreFileManager.collabIgnoreBackup);
            Assert.AreEqual(backupText, backupTextAfter);
            var ignoreTextAfter = File.ReadAllText(IgnoreFileManager.collabIgnoreFile);
            Assert.AreEqual(ignoreText, ignoreTextAfter);
        }

        [TearDown]
        public void UnsetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(m_OriginalPath);
        }
    }
}
