using Unity.PlasticSCM.Editor.Inspector.Properties;
using Unity.PlasticSCM.Editor.Settings;
using Unity.PlasticSCM.Editor.UI;
using UnityEditor;

namespace Unity.PlasticSCM.Editor
{
    internal static class SwitchUVCSPlugin
    {
        internal static UVCSWindow On(UVCSPlugin uvcsPlugin)
        {
            UVCSPluginIsEnabledPreference.Enable();

            uvcsPlugin.Enable();

            UVCSWindow window = ShowWindow.UVCS();

            return window;
        }

        internal static UVCSWindow OnIfNeeded(UVCSPlugin uvcsPlugin)
        {
            if (!UVCSPluginIsEnabledPreference.IsEnabled())
            {
                UVCSWindow result = On(uvcsPlugin);
                ReloadSettings();
                return result;
            }

            return ShowWindow.UVCS();
        }

        internal static void Off(UVCSPlugin uvcsPlugin)
        {
            UVCSPluginIsEnabledPreference.Disable();

            CloseWindowIfOpened.UVCS();
            CloseWindowIfOpened.BranchExplorer();
            CloseWindowIfOpened.Diff();

            CleanSelectionIfNeeded();

            uvcsPlugin.Shutdown();
        }

        static void ReloadSettings()
        {
            UVCSProjectSettingsProvider.GetIfActive()?.ReloadSettings();
        }

        static void CleanSelectionIfNeeded()
        {
            if (!(Selection.activeObject is SelectedRepObjectInfoData))
                return;

            Selection.activeObject = null;
        }
    }
}
