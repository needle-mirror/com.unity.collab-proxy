using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Cloud.Collaborate.Models.Providers.Client
{
    internal class IgnoreFileManager
    {
        public const string collabIgnoreFile = ".collabignore";
        public const string collabIgnoreBackup = "collabignore.txt";
        public const string baseCollabIgnoreFile = "Packages/com.unity.collab-proxy/.collabignore";

        public static IgnoreFileManager Instance { get; } = new IgnoreFileManager();

        protected virtual bool IgnoreFileExists()
        {
            return File.Exists(collabIgnoreFile);
        }

        protected virtual void DebugLogError(string error)
        {
            Debug.LogError(error);
        }

        void MigrateIgnoreFile()
        {
            if (File.Exists(collabIgnoreBackup))
            {
                DebugLogError($"Backup file {collabIgnoreBackup} already exists - aborting migration");
                return;
            }
            // Copy the old file to the backup location
            File.Move(collabIgnoreFile, collabIgnoreBackup);
            File.Delete(collabIgnoreFile);
            // Create the new ignore file
            CreateIgnoreFile();
        }

        static void CreateIgnoreFile()
        {
            if (!File.Exists(GetBaseCollabIgnorePath())) return;
            File.Move(GetBaseCollabIgnorePath(), collabIgnoreFile);
        }

        static string GetBaseCollabIgnorePath()
        {
            return Path.GetFullPath(baseCollabIgnoreFile.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }

        protected virtual bool IgnoreFileMigrationDialog()
        {
            return EditorUtility.DisplayDialog(L10n.Tr("Existing Collab Ignore File Found"),
                L10n.Tr($"Collab would like to replace your existing .collabignore file - a backup called {collabIgnoreBackup} will be created"),
                L10n.Tr("Continue"),
                L10n.Tr("Keep Existing File"));
        }

        public void CreateOrMigrateIgnoreFile()
        {
            if (IgnoreFileExists())
            {
                MigrateIgnoreFile();
            }
            else
            {
                CreateIgnoreFile();
            }
        }
    }
}
