using Unity.PlasticSCM.Editor.Diff.Asset.Content;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff;
using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common
{
    internal static class ObjectIconResolver
    {
        internal static UnityEngine.Texture GetIcon(ObjectContent objContent)
        {
            if (objContent.IsMetaFileObject)
                return GetMetaIcon(objContent.MetaSectionName);

            return GetObjectIcon(objContent.Object);
        }

        internal static UnityEngine.Texture GetIcon(ObjectDiff objDiff)
        {
            if (objDiff.IsMetaFileObject)
                return GetMetaIcon(objDiff.MetaSectionName);

            return GetObjectIcon(objDiff.SrcObject ?? objDiff.DstObject);
        }

        static UnityEngine.Texture GetObjectIcon(UnityEngine.Object obj)
        {
            if (obj == null)
                return GetFallbackIcon();

            GUIContent content = EditorGUIUtility.ObjectContent(obj, obj.GetType());
            return content.image != null ? content.image : GetFallbackIcon();
        }

        // Section names arrive nicified ("Audio Importer"), but our match
        // tokens are the unspaced YAML keys ("audioimporter"). Normalise by
        // stripping whitespace before the lowercase contains-check.
        static UnityEngine.Texture GetMetaIcon(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
                return GetFallbackIcon();

            string key = sectionName.Replace(" ", "").ToLowerInvariant();

            if (key.Contains("textureimporter"))
                return TryGetIcon("Texture Icon");
            if (key.Contains("audioimporter"))
                return TryGetIcon("AudioClip Icon");
            if (key.Contains("modelimporter"))
                return TryGetIcon("PrefabModel Icon");
            if (key.Contains("sprite"))
                return TryGetIcon("Sprite Icon");
            if (key.Contains("platformsettings"))
                return TryGetIcon("BuildSettings.Editor.Small");

            return TryGetIcon("TextAsset Icon");
        }

        static UnityEngine.Texture TryGetIcon(string iconName)
        {
            GUIContent content = EditorGUIUtility.IconContent(iconName);
            if (content == null || content.image == null)
                return GetFallbackIcon();
            return content.image;
        }

        static UnityEngine.Texture GetFallbackIcon()
        {
            return EditorGUIUtility.IconContent("DefaultAsset Icon").image;
        }
    }
}
