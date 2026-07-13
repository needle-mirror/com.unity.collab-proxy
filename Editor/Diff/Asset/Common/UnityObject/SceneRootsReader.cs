using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.UnityObject
{
    // SceneRoots is an internal Unity type without a public managed wrapper,
    // so we identify it structurally by the presence of an `m_Roots` array
    // property of object references. Prefabs have no SceneRoots.
    internal static class SceneRootsReader
    {
        internal static UnityEngine.Object Find(UnityEngine.Object[] objects)
        {
            foreach (UnityEngine.Object obj in objects)
            {
                if (obj == null)
                    continue;

                if (obj is GameObject || obj is Component)
                    continue;

                using (SerializedObject so = new SerializedObject(obj))
                {
                    SerializedProperty roots = so.FindProperty("m_Roots");
                    if (roots != null && roots.isArray)
                        return obj;
                }
            }

            return null;
        }

        internal static List<GameObject> ReadRootGameObjects(
            UnityEngine.Object sceneRootsObj)
        {
            List<GameObject> result = new List<GameObject>();

            using (SerializedObject so = new SerializedObject(sceneRootsObj))
            {
                SerializedProperty roots = so.FindProperty("m_Roots");
                if (roots == null || !roots.isArray)
                    return result;

                for (int i = 0; i < roots.arraySize; i++)
                {
                    SerializedProperty element = roots.GetArrayElementAtIndex(i);
                    if (element.objectReferenceValue is Transform t
                        && t.gameObject != null)
                    {
                        result.Add(t.gameObject);
                    }
                }
            }

            return result;
        }
    }
}
