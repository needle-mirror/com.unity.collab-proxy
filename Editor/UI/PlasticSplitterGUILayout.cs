using UnityEditor;

#if !UNITY_6000_3_OR_NEWER
using SplitterState = Unity.PlasticSCM.Editor.UnityInternals.UnityEditor.SplitterState;
using SplitterGUILayout = Unity.PlasticSCM.Editor.UnityInternals.UnityEditor.SplitterGUILayout;
#endif

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class PlasticSplitterGUILayout
    {
        internal static void BeginHorizontalSplit(SplitterState splitterState)
        {
            SplitterGUILayout.BeginHorizontalSplit(splitterState);
        }

        internal static void EndHorizontalSplit()
        {
            SplitterGUILayout.EndHorizontalSplit();
        }

        internal static void BeginVerticalSplit(SplitterState splitterState)
        {
            SplitterGUILayout.BeginVerticalSplit(splitterState);
        }

        internal static void EndVerticalSplit()
        {
            SplitterGUILayout.EndVerticalSplit();
        }

        internal static SplitterState InitSplitterState(
            float[] relativeSizes, int[] minSizes, int[] maxSizes)
        {
            return new SplitterState(relativeSizes, minSizes, maxSizes);
        }

        internal static float[] GetRelativeSizes(SplitterState splitterState)
        {
            return splitterState.relativeSizes;
        }
    }
}
