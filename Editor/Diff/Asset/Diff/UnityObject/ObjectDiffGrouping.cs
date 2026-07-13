using System.Collections.Generic;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    // Restructures a flat list of ObjectDiffs into a tree where each Component
    // diff is nested under its owning GameObject diff (creating a synthetic
    // Unchanged container when the GameObject itself has no own changes).
    // Sorts each GameObject's components to match the inspector order.
    internal static class ObjectDiffGrouping
    {
        internal static void GroupComponentsUnderGameObjects(List<ObjectDiff> objectDiffs)
        {
            Dictionary<GameObject, ObjectDiff> goDiffs = new Dictionary<GameObject, ObjectDiff>();

            foreach (ObjectDiff diff in objectDiffs)
            {
                if (diff.SrcObject is GameObject srcGo) goDiffs[srcGo] = diff;
                if (diff.DstObject is GameObject dstGo) goDiffs[dstGo] = diff;
            }

            List<ObjectDiff> topLevel = new List<ObjectDiff>(objectDiffs.Count);

            foreach (ObjectDiff diff in objectDiffs)
            {
                Component c = (diff.DstObject ?? diff.SrcObject) as Component;
                if (c == null)
                {
                    topLevel.Add(diff);
                    continue;
                }

                ObjectDiff parent = GetOrCreateGameObjectDiff(diff, goDiffs, topLevel);
                if (parent.ComponentDiffs == null)
                    parent.ComponentDiffs = new List<ObjectDiff>();
                parent.ComponentDiffs.Add(diff);
            }

            objectDiffs.Clear();
            objectDiffs.AddRange(topLevel);

            foreach (ObjectDiff diff in objectDiffs)
                SortComponentsByGameObjectOrder(diff);
        }

        static ObjectDiff GetOrCreateGameObjectDiff(
            ObjectDiff componentDiff,
            Dictionary<GameObject, ObjectDiff> goDiffs,
            List<ObjectDiff> topLevel)
        {
            GameObject srcParent = (componentDiff.SrcObject as Component)?.gameObject;
            GameObject dstParent = (componentDiff.DstObject as Component)?.gameObject;

            if (srcParent != null && goDiffs.TryGetValue(srcParent, out ObjectDiff existing))
                return existing;
            if (dstParent != null && goDiffs.TryGetValue(dstParent, out existing))
                return existing;

            ObjectDiff container = new ObjectDiff
            {
                SrcObject = srcParent,
                DstObject = dstParent,
                DiffType = DiffType.Unchanged
            };

            if (srcParent != null) goDiffs[srcParent] = container;
            if (dstParent != null) goDiffs[dstParent] = container;
            topLevel.Add(container);

            return container;
        }

        static void SortComponentsByGameObjectOrder(ObjectDiff gameObjectDiff)
        {
            if (gameObjectDiff.ComponentDiffs == null)
                return;

            GameObject go = (gameObjectDiff.DstObject ?? gameObjectDiff.SrcObject) as GameObject;
            if (go == null)
                return;

            Component[] orderedComponents = go.GetComponents<Component>();
            Dictionary<Component, int> orderIndex = new Dictionary<Component, int>(
                orderedComponents.Length);

            for (int i = 0; i < orderedComponents.Length; i++)
            {
                if (orderedComponents[i] != null)
                    orderIndex[orderedComponents[i]] = i;
            }

            gameObjectDiff.ComponentDiffs.Sort((a, b) =>
            {
                int ia = GetComponentOrder(a, orderIndex);
                int ib = GetComponentOrder(b, orderIndex);
                return ia.CompareTo(ib);
            });
        }

        static int GetComponentOrder(
            ObjectDiff diff,
            Dictionary<Component, int> orderIndex)
        {
            if (diff.DstObject is Component dstComp
                && orderIndex.TryGetValue(dstComp, out int dstIdx))
                return dstIdx;

            if (diff.SrcObject is Component srcComp
                && orderIndex.TryGetValue(srcComp, out int srcIdx))
                return srcIdx;

            return int.MaxValue;
        }
    }
}
