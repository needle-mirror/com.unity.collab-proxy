using System;
using System.Collections.Generic;

using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.UnityObject
{
    // Sorts top-level items by their natural scene/prefab hierarchy order
    // (DFS over scene roots and their children). Without this pass, items
    // come out in the unspecified order LoadSerializedFileAndForget returns
    // objects in, not the order the user sees in the Unity Hierarchy panel.
    // Items whose object is not part of the hierarchy (settings, etc.) land
    // at the end, preserving their relative input order.
    internal static class HierarchyOrderSorting
    {
        internal static void SortByHierarchyOrder<T>(
            List<T> items,
            Func<T, UnityEngine.Object> objectSelector,
            params UnityEngine.Object[][] objectArrays)
        {
            if (items.Count <= 1)
                return;

            Dictionary<long, int> orderByFileId = BuildHierarchyOrder(objectArrays);

            int trailingOrder = orderByFileId.Count;
            int n = items.Count;

            int[] sortKeys = new int[n];
            int[] indices = new int[n];

            for (int i = 0; i < n; i++)
            {
                sortKeys[i] = ResolveSortKey(
                    objectSelector(items[i]), orderByFileId, trailingOrder);
                indices[i] = i;
            }

            System.Array.Sort(indices, (a, b) =>
            {
                int cmp = sortKeys[a].CompareTo(sortKeys[b]);
                if (cmp != 0)
                    return cmp;
                return a.CompareTo(b);
            });

            T[] reordered = new T[n];
            for (int i = 0; i < n; i++)
                reordered[i] = items[indices[i]];

            items.Clear();
            items.AddRange(reordered);
        }

        static int ResolveSortKey(
            UnityEngine.Object obj,
            Dictionary<long, int> orderByFileId,
            int trailingOrder)
        {
            if (obj == null)
                return trailingOrder;

            long fileId = obj.GetLocalFileId();
            if (fileId == 0)
                return trailingOrder;

            return orderByFileId.TryGetValue(fileId, out int order)
                ? order
                : trailingOrder;
        }

        static Dictionary<long, int> BuildHierarchyOrder(
            UnityEngine.Object[][] objectArrays)
        {
            Dictionary<long, int> result = new Dictionary<long, int>();
            int counter = 0;

            foreach (UnityEngine.Object[] objects in objectArrays)
            {
                foreach (GameObject root in CollectRootGameObjects(objects))
                    VisitGameObjectDfs(root, result, ref counter);
            }

            return result;
        }

        static void VisitGameObjectDfs(
            GameObject go,
            Dictionary<long, int> result,
            ref int counter)
        {
            if (go == null)
                return;

            long fileId = go.GetLocalFileId();
            if (fileId != 0 && !result.ContainsKey(fileId))
                result[fileId] = counter++;

            Transform transform = go.transform;
            if (transform == null)
                return;

            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != null)
                    VisitGameObjectDfs(child.gameObject, result, ref counter);
            }
        }

        static List<GameObject> CollectRootGameObjects(UnityEngine.Object[] objects)
        {
            UnityEngine.Object sceneRoots = SceneRootsReader.Find(objects);
            if (sceneRoots != null)
                return SceneRootsReader.ReadRootGameObjects(sceneRoots);

            // Prefab (no SceneRoots) — fall back to Transforms with no parent.
            List<GameObject> roots = new List<GameObject>();
            foreach (UnityEngine.Object obj in objects)
            {
                if (!(obj is Transform t))
                    continue;
                if (t.parent != null)
                    continue;
                if (t.gameObject == null)
                    continue;

                roots.Add(t.gameObject);
            }
            return roots;
        }
    }
}
