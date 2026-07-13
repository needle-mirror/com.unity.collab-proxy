using System;

using UnityEditor;

using Unity.PlasticSCM.Editor.Diff;
using Unity.PlasticSCM.Editor.CloudDrive;
using Unity.PlasticSCM.Editor.Views.BranchExplorer;
using Unity.PlasticSCM.Editor.Views.BranchExplorer.Options;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class GetWindowIfOpened
    {
        internal static UVCSWindow UVCS()
        {
            if (!EditorWindow.HasOpenInstances<UVCSWindow>())
                return null;

            UVCSWindow window = EditorWindow.GetWindow<UVCSWindow>(null, false);

            if (EditorWindowHost.IsDetached(window))
                return null;

            return window;
        }

        internal static BranchExplorerWindow BranchExplorer()
        {
            if (!EditorWindow.HasOpenInstances<BranchExplorerWindow>())
                return null;

            return EditorWindow.GetWindow<BranchExplorerWindow>(null, false);
        }

        internal static BranchExplorerOptionsWindow BranchExplorerOptions()
        {
            if (!EditorWindow.HasOpenInstances<BranchExplorerOptionsWindow>())
                return null;

            return EditorWindow.GetWindow<BranchExplorerOptionsWindow>(null, false);
        }

        internal static CloudDriveWindow CloudDrive()
        {
            if (!EditorWindow.HasOpenInstances<CloudDriveWindow>())
                return null;

            return EditorWindow.GetWindow<CloudDriveWindow>(null, false);
        }

        internal static void SetGetDiffWindowForTesting(Func<IUnityDiffWindow> getDiffWindowForTesting)
        {
            mGetDiffWindowForTesting = getDiffWindowForTesting;
        }

        internal static IUnityDiffWindow Diff()
        {
            if (mGetDiffWindowForTesting != null)
                return mGetDiffWindowForTesting();

            if (!EditorWindow.HasOpenInstances<UnityDiffWindow>())
                return null;

            return EditorWindow.GetWindow<UnityDiffWindow>(null, false);
        }

        static Func<IUnityDiffWindow> mGetDiffWindowForTesting;
    }
}
