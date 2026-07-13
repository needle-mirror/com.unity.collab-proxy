using UnityEditor;

using Unity.PlasticSCM.Editor.UI;

#if !UNITY_6000_3_OR_NEWER
using SettingsWindow = Unity.PlasticSCM.Editor.UnityInternals.UnityEditor.SettingsWindow;
#endif

namespace Unity.PlasticSCM.Editor.Settings
{
    internal static class OpenUVCSProjectSettings
    {
        internal static void ByDefault()
        {
            OpenVersionControlProjectSettings()?.OpenAllFoldouts();
        }

        internal static void InDiffAndMergeFoldout()
        {
            OpenVersionControlProjectSettings()?.OpenDiffAndMergeFoldout();
        }

        internal static void InShelveAndSwitchFoldout()
        {
            OpenVersionControlProjectSettings()?.OpenShelveAndSwitchFoldout();
        }

        internal static void InOtherFoldout()
        {
            OpenVersionControlProjectSettings()?.OpenOtherFoldout();
        }

        static UVCSProjectSettingsProvider OpenVersionControlProjectSettings()
        {
            SettingsWindow.Show(SettingsScope.Project, UnityConstants.PROJECT_SETTINGS_TAB_PATH);

            return UVCSProjectSettingsProvider.GetIfActive();
        }
    }
}
