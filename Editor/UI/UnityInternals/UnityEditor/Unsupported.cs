namespace Unity.PlasticSCM.Editor.UnityInternals.UnityEditor
{
    internal static class Unsupported
    {
        internal delegate ulong GetFileIDHintDelegate(UnityEngine.Object obj);

        internal static GetFileIDHintDelegate GetFileIDHint { get; set; }
    }
}
