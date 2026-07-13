namespace Unity.PlasticSCM.Editor.Diff.Asset.Common
{
    internal static class ObjectExtensions
    {
        internal static int GetObjectId(this UnityEngine.Object obj)
        {
#if UNITY_6000_4_OR_NEWER
            return obj.GetEntityId().GetHashCode();
#else
            return obj.GetInstanceID();
#endif
        }

        internal static long GetLocalFileId(this UnityEngine.Object obj)
        {
            return (long)UnityInternals.UnityEditor.Unsupported.GetFileIDHint(obj);
        }
    }
}
