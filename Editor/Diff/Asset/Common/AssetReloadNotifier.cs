using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common
{
    class AssetReloadNotifier : AssetPostprocessor
    {
        internal static event Action<HashSet<string>> AssetsChanged;

        internal static bool IsWatchedPath(
            string file, HashSet<string> changedFullPaths)
        {
            if (string.IsNullOrEmpty(file))
                return false;

            if (file.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
                return false;

            string fullPath = Path.GetFullPath(file);

            if (changedFullPaths.Contains(fullPath))
                return true;

            if (!MetaPath.IsMetaPath(file))
                return false;

            string assetFullPath = Path.GetFullPath(
                MetaPath.GetPathFromMetaPath(file));

            return changedFullPaths.Contains(assetFullPath);
        }

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            Action<HashSet<string>> handler = AssetsChanged;

            if (handler == null)
                return;

            HashSet<string> changedFullPaths = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            AddFullPaths(changedFullPaths, importedAssets);
            AddFullPaths(changedFullPaths, deletedAssets);
            AddFullPaths(changedFullPaths, movedAssets);
            AddFullPaths(changedFullPaths, movedFromAssetPaths);

            if (changedFullPaths.Count == 0)
                return;

            handler(changedFullPaths);
        }

        static void AddFullPaths(HashSet<string> set, string[] assetPaths)
        {
            foreach (string assetPath in assetPaths)
                set.Add(Path.GetFullPath(assetPath));
        }
    }
}
