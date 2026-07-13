using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal static class DraftStorage
    {
        internal static void SaveDraft(string pathForEdition, string content)
        {
            if (string.IsNullOrEmpty(pathForEdition))
                return;

            try
            {
                string draftDir = GetDraftDirectory();
                Directory.CreateDirectory(draftDir);

                string draftPath = GetDraftPath(pathForEdition);
                File.WriteAllText(draftPath, content, Encoding.UTF8);
            }
            catch (Exception)
            {
            }
        }

        internal static bool TryLoadDraft(
            string pathForEdition, out string content)
        {
            content = null;

            if (string.IsNullOrEmpty(pathForEdition))
                return false;

            try
            {
                string draftPath = GetDraftPath(pathForEdition);

                if (!File.Exists(draftPath))
                    return false;

                content = File.ReadAllText(draftPath, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static void DeleteDraft(string pathForEdition)
        {
            if (string.IsNullOrEmpty(pathForEdition))
                return;

            try
            {
                string draftPath = GetDraftPath(pathForEdition);

                if (File.Exists(draftPath))
                    File.Delete(draftPath);
            }
            catch (Exception)
            {
            }
        }

        static string GetDraftPath(string pathForEdition)
        {
            string hash = ComputeHash(pathForEdition);
            return Path.Combine(GetDraftDirectory(), hash + DRAFT_EXTENSION);
        }

        static string GetDraftDirectory()
        {
            return Path.Combine(Path.GetTempPath(), DRAFT_DIRECTORY_NAME);
        }

        static string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                    sb.Append(hash[i].ToString("x2"));

                return sb.ToString();
            }
        }

        const string DRAFT_DIRECTORY_NAME = "plastic-diff-drafts";
        const string DRAFT_EXTENSION = ".draft";
    }
}
