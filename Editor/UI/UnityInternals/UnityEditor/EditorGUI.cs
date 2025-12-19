using System.Reflection;

using UnityEngine;
using UnityEngine.Internal;

using UnityEditorGUI = UnityEditor.EditorGUI;

namespace Unity.PlasticSCM.Editor.UnityInternals.UnityEditor
{
    internal static class EditorGUI
    {
        // Delegate that matches the signature of ScrollableTextAreaInternal
        internal delegate string ScrollableTextAreaInternalDelegate(
            Rect position,
            string text,
            ref Vector2 scrollPosition,
            GUIStyle style);

        // This will be set by Unity.Cloud.Collaborate assembly
        internal static ScrollableTextAreaInternalDelegate ScrollableTextAreaInternal { get; set; }

        internal static TextEditor activeEditor
        {
            get
            {
                FieldInfo activeEditorField = typeof(UnityEditorGUI).GetField(
                    "activeEditor",
                    BindingFlags.Static | BindingFlags.NonPublic);
                return activeEditorField?.GetValue(null) as TextEditor;
            }
        }

        internal static void LabelField(
            Rect position, string label, [DefaultValue("EditorStyles.label")] GUIStyle style)
        {
            UnityEditorGUI.LabelField(position, label, style);
        }
    }
}
