using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CollabProxy.Client
{
    internal class IgnoreFileManager
    {
        public const string k_CollabIgnoreFile = ".collabignore";
        public const string k_CollabIgnoreBackup = "collabignore.txt";
        public const string k_BaseCollabIgnoreFile = "Packages/com.unity.collab-proxy/.collabignore";
        
        static IgnoreFileManager s_IgnoreFileManager = new IgnoreFileManager();
        public static IgnoreFileManager Instance
        {
            get { return s_IgnoreFileManager; }
        }

        protected virtual bool IgnoreFileExists()
        {
            return File.Exists(k_CollabIgnoreFile);
        }

        protected virtual void DebugLogError(string error)
        {
            Debug.LogError(error);
        }

        void MigrateIgnoreFile()
        {
            if (File.Exists(k_CollabIgnoreBackup))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Backup file {0} already exists - aborting migration", k_CollabIgnoreBackup);
                DebugLogError(sb.ToString());
                return;
            }
            // Copy the old file to the backup location
            File.Move(k_CollabIgnoreFile, k_CollabIgnoreBackup);
            File.Delete(k_CollabIgnoreFile);
            // Create the new ignore file
            CreateIgnoreFile();
        }

        void CreateIgnoreFile()
        {
            if (!File.Exists(GetBaseCollabIgnorePath())) return;
            File.Move(GetBaseCollabIgnorePath(), k_CollabIgnoreFile);
        }

        string GetBaseCollabIgnorePath()
        {
            return Path.GetFullPath(k_BaseCollabIgnoreFile.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }
        
        protected virtual bool IgnoreFileMigrationDialog()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Collab would like to replace your existing .collabignore file - a backup called {0} will be created", k_CollabIgnoreBackup);
            return EditorUtility.DisplayDialog(L10n.Tr("Existing Collab Ignore File Found"),
                L10n.Tr(sb.ToString()),
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
