using UnityEditor;
using UnityEngine;

namespace Unity.CollabProxy.Compat
{
    [InitializeOnLoad]
    internal static class CompatibilityError
    {
        static CompatibilityError()
        {
#if UNITY_2020_1_OR_NEWER
            const string message = "Collab version 2.0.0-preview is not supported on Unity 2020.1 and above. Please update to Collab version 2.1.0-preview or above to continue using Collaborate.";
            Debug.LogError(message);
            EditorUtility.DisplayDialog("Collaborate", message, "Okay");
#endif
        }

    }
}
