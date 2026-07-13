using UnityEditor;

using UnityEditorWindow = UnityEditor.EditorWindow;

namespace Unity.PlasticSCM.Editor.UnityInternals.UnityEditor
{
    internal class SettingsWindow
    {
        internal object InternalObject { get; }

        internal SettingsWindow(object settingsWindow)
        {
            InternalObject = settingsWindow;
        }

        internal static SettingsWindow Show(SettingsScope scopes, string settingsPath)
        {
            return ShowInternal(scopes, settingsPath);
        }

        internal static bool IsProjectSettingsWindow(UnityEditorWindow editorWindow)
        {
            return IsProjectSettingsWindowInternal(editorWindow);
        }

        internal delegate SettingsWindow ShowDelegate(SettingsScope scopes, string settingsPath);
        internal static ShowDelegate ShowInternal { get; set; }

        internal delegate bool IsProjectSettingsWindowDelegate(UnityEditorWindow editorWindow);
        internal static IsProjectSettingsWindowDelegate IsProjectSettingsWindowInternal { get; set; }
    }
}
