using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor
{
    internal static class FindUnityObject
    {
        internal static Object ForInstanceID(int instanceID)
        {
#if UNITY_6000_3_OR_NEWER
            // TODO: use EntityId.From(instanceID) when it becomes available publicly
            #pragma warning disable CS0618 // Type or member is obsolete
            return EditorUtility.EntityIdToObject(instanceID);
            #pragma warning restore CS0618 // Type or member is obsolete
#else
            return EditorUtility.InstanceIDToObject(instanceID);
#endif
        }
    }
}
