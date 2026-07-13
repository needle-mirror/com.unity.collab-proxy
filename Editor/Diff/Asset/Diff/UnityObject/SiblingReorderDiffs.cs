using System.Collections.Generic;
using UnityEngine;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.UnityObject;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    // Detects sibling reorders at any level of the GameObject hierarchy:
    // both the scene-root level (via SceneRoots.m_Roots) and inside every
    // Transform's m_Children list. The same reorder algorithm used for
    // components is applied per parent — common siblings (present on both
    // sides) define the reference order, additions/removals don't count.
    internal static class SiblingReorderDiffs
    {
        internal static void Append(
            UnityEngine.Object[] srcObjects,
            UnityEngine.Object[] dstObjects,
            Dictionary<long, UnityEngine.Object> srcById,
            Dictionary<long, UnityEngine.Object> dstById,
            List<ObjectDiff> objectDiffs)
        {
            Dictionary<GameObject, ObjectDiff> bySrcGameObject =
                new Dictionary<GameObject, ObjectDiff>();
            Dictionary<GameObject, ObjectDiff> byDstGameObject =
                new Dictionary<GameObject, ObjectDiff>();

            foreach (ObjectDiff diff in objectDiffs)
            {
                if (diff.SrcObject is GameObject sg) bySrcGameObject[sg] = diff;
                if (diff.DstObject is GameObject dg) byDstGameObject[dg] = diff;
            }

            // Scene-root level (not modeled as a Transform parent).
            UnityEngine.Object srcSceneRoots = SceneRootsReader.Find(srcObjects);
            UnityEngine.Object dstSceneRoots = SceneRootsReader.Find(dstObjects);

            if (srcSceneRoots != null && dstSceneRoots != null)
            {
                EmitDiffs(
                    SceneRootsReader.ReadRootGameObjects(srcSceneRoots),
                    SceneRootsReader.ReadRootGameObjects(dstSceneRoots),
                    bySrcGameObject, byDstGameObject, objectDiffs);
            }

            // Every paired Transform's m_Children. Covers prefabs (no SceneRoots)
            // and nested hierarchy reorders inside scenes.
            foreach (KeyValuePair<long, UnityEngine.Object> kvp in srcById)
            {
                if (!(kvp.Value is Transform srcParent))
                    continue;
                if (!dstById.TryGetValue(kvp.Key, out UnityEngine.Object dstObj))
                    continue;
                if (!(dstObj is Transform dstParent))
                    continue;

                if (srcParent == null || dstParent == null)
                    continue;

                EmitDiffs(
                    ReadChildGameObjects(srcParent),
                    ReadChildGameObjects(dstParent),
                    bySrcGameObject, byDstGameObject, objectDiffs);
            }
        }

        static void EmitDiffs(
            List<GameObject> srcOrdered,
            List<GameObject> dstOrdered,
            Dictionary<GameObject, ObjectDiff> bySrcGameObject,
            Dictionary<GameObject, ObjectDiff> byDstGameObject,
            List<ObjectDiff> objectDiffs)
        {
            if (srcOrdered.Count == 0 || dstOrdered.Count == 0)
                return;

            Dictionary<long, GameObject> srcByFileId = BuildGameObjectFileIdMap(srcOrdered);
            Dictionary<long, GameObject> dstByFileId = BuildGameObjectFileIdMap(dstOrdered);

            HashSet<long> common = new HashSet<long>();
            foreach (long fileId in srcByFileId.Keys)
            {
                if (dstByFileId.ContainsKey(fileId))
                    common.Add(fileId);
            }

            // Position is measured against the relative order of siblings
            // present on both sides — additions and removals don't count as
            // "everyone moved".
            if (common.Count < 2)
                return;

            List<long> srcCommonOrder = FilterByCommonFileId(srcOrdered, common);
            List<long> dstCommonOrder = FilterByCommonFileId(dstOrdered, common);

            Dictionary<long, int> srcPos = ToPositionIndex(srcCommonOrder);
            Dictionary<long, int> dstPos = ToPositionIndex(dstCommonOrder);

            foreach (long fileId in common)
            {
                int delta = dstPos[fileId] - srcPos[fileId];
                if (delta == 0)
                    continue;

                GameObject srcGo = srcByFileId[fileId];
                GameObject dstGo = dstByFileId[fileId];

                ObjectDiff existing = null;
                if (srcGo != null) bySrcGameObject.TryGetValue(srcGo, out existing);
                if (existing == null && dstGo != null)
                    byDstGameObject.TryGetValue(dstGo, out existing);

                if (existing != null)
                {
                    existing.PositionDelta = delta;
                    continue;
                }

                // Sibling moved but its content is unchanged — emit a
                // Modified diff with no PropertyDiffs so the row still appears.
                ObjectDiff newDiff = new ObjectDiff
                {
                    SrcObject = srcGo,
                    DstObject = dstGo,
                    DiffType = DiffType.Modified,
                    PositionDelta = delta
                };
                objectDiffs.Add(newDiff);

                if (srcGo != null) bySrcGameObject[srcGo] = newDiff;
                if (dstGo != null) byDstGameObject[dstGo] = newDiff;
            }
        }

        static List<GameObject> ReadChildGameObjects(Transform parent)
        {
            int count = parent.childCount;
            List<GameObject> result = new List<GameObject>(count);

            for (int i = 0; i < count; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null && child.gameObject != null)
                    result.Add(child.gameObject);
            }

            return result;
        }

        static Dictionary<long, GameObject> BuildGameObjectFileIdMap(List<GameObject> gameObjects)
        {
            Dictionary<long, GameObject> map = new Dictionary<long, GameObject>(gameObjects.Count);

            foreach (GameObject go in gameObjects)
            {
                if (go == null)
                    continue;

                long fileId = go.GetLocalFileId();
                if (fileId == 0)
                    continue;

                map[fileId] = go;
            }

            return map;
        }

        static List<long> FilterByCommonFileId(
            List<GameObject> gameObjects, HashSet<long> common)
        {
            List<long> result = new List<long>(gameObjects.Count);
            foreach (GameObject go in gameObjects)
            {
                if (go == null)
                    continue;

                long fileId = go.GetLocalFileId();
                if (common.Contains(fileId))
                    result.Add(fileId);
            }
            return result;
        }

        static Dictionary<long, int> ToPositionIndex(List<long> orderedFileIds)
        {
            Dictionary<long, int> result = new Dictionary<long, int>(orderedFileIds.Count);
            for (int i = 0; i < orderedFileIds.Count; i++)
                result[orderedFileIds[i]] = i;
            return result;
        }
    }
}
