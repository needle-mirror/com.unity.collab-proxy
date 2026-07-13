using UnityEditor;
using UnityEditor.VersionControl;

using Unity.PlasticSCM.Editor.Settings;

namespace Unity.PlasticSCM.Editor
{
    internal static class VCSBuiltInPlugin
    {
        internal static bool IsEnabled()
        {
            return VersionControlSettings.mode == UVCS_MODE;
        }

        internal static void EnsureProviderAsync()
        {
            if (IsEnabled())
                return;

            if (mIsSetVersionControlProviderScheduled)
                return;

            mIsSetVersionControlProviderScheduled = true;

            // Defer Version Control provider call to ensure Unity has fully resolved the assemblies,
            // which is required for VersionControlManager to locate the [VersionControl] attribute
            // and activate the provider correctly.
            EditorApplication.delayCall += SetVersionControlProvider;
        }

        internal static void SaveModeSetting()
        {
            VersionControlSettings.mode = UVCS_MODE;
            AssetDatabase.SaveAssets();
        }

        internal static bool IsVersionControlProviderEnabled()
        {
            return VersionControlSettings.mode != VISIBLE_META_FILES_MODE &&
                   VersionControlSettings.mode != HIDDEN_META_FILES_MODE;
        }

        static void SetVersionControlProvider()
        {
            mIsSetVersionControlProviderScheduled = false;

            VersionControlManager.SetVersionControl(UVCS_MODE);
        }

        static bool mIsSetVersionControlProviderScheduled;

        const string UVCS_MODE = "Unity Version Control";
        const string VISIBLE_META_FILES_MODE = "Visible Meta Files";
        const string HIDDEN_META_FILES_MODE = "Hidden Meta Files";
    }
}
