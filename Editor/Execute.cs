using System;

using UnityEditor;

namespace Unity.PlasticSCM.Editor
{
    internal static class Execute
    {
        internal static void WhenEditorIsReady(Action action)
        {
            if (PlasticApp.IsUnitTesting)
            {
                action();
                return;
            }

            EditorApplication.update += RunOnceWhenEditorIsReady;

            void RunOnceWhenEditorIsReady()
            {
                // Calls action when the editor is ready (not updating or compiling)
                if (EditorApplication.isUpdating ||
                    EditorApplication.isCompiling)
                {
                    return;
                }

                EditorApplication.update -= RunOnceWhenEditorIsReady;

                action();
            }
        }
    }
}
