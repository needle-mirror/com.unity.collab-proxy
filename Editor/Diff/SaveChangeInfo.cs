using Codice.Client.BaseCommands;
using Unity.PlasticSCM.Editor;
using Unity.PlasticSCM.Editor.AssetUtils;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Unity.PlasticSCM.Editor.Diff
{
    internal static class SaveChange
    {
        internal static void IfDirty(ChangeInfo changeInfo)
        {
            string changeFullPath = changeInfo.GetFullPath();

            if (string.IsNullOrEmpty(changeFullPath))
                return;

            if (MetaPath.IsMetaPath(changeFullPath))
                changeFullPath = MetaPath.GetPathFromMetaPath(changeFullPath);

            string relativePath = AssetsPath.GetRelativePath(changeFullPath);

            if (string.IsNullOrEmpty(relativePath))
                return;

            if (relativePath.EndsWith(".unity"))
            {
                SaveDirtyScene(relativePath);
                return;
            }

            SaveDirtyNonSceneAsset(relativePath);
        }

        static void SaveDirtyScene(string scenePath)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (!scene.isDirty)
                    continue;

                if (scene.path != scenePath)
                    continue;

                EditorSceneManager.SaveScene(scene);
                return;
            }
        }

        static void SaveDirtyNonSceneAsset(string relativePath)
        {
            UnityEngine.Object asset =
                UnityEditor.AssetDatabase.LoadMainAssetAtPath(relativePath);

            if (asset == null)
                return;

            UnityEditor.AssetDatabase.SaveAssetIfDirty(asset);
        }
    }
}
