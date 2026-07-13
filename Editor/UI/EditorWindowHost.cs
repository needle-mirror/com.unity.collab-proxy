using Unity.PlasticSCM.Editor.UnityInternals.UnityEditor;

using UnityObject = UnityEngine.Object;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class EditorWindowHost
    {
        internal static bool IsDetached(UnityEditor.EditorWindow window)
        {
            if (window == null)
                return false;

            if (UnityEditorExtensions.m_ParentInternal == null)
                return false;

            return window.m_Parent() == null;
        }

        internal static void DestroyUvcWindowIfDetached()
        {
            if (!UnityEditor.EditorWindow.HasOpenInstances<UVCSWindow>())
                return;

            UVCSWindow window = UnityEditor.EditorWindow.GetWindow<UVCSWindow>(null, false);

            if (!IsDetached(window))
                return;

            UnityObject.DestroyImmediate(window);
        }
    }
}
