using System;
using System.Collections.Generic;
using System.Text;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;
using UnityEditor;
using UnityEngine;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    // Fallback path used when the m_LocalIdentfierInFile property cannot be
    // read for every object (i.e. binary serialization or unexpected schema).
    // Pairs objects by hierarchical name/path key and falls back to property
    // similarity for the leftovers.
    internal static class NameMatchedDiffs
    {
        internal static void Build(
            UnityEngine.Object[] srcObjects,
            UnityEngine.Object[] dstObjects,
            Dictionary<UnityEngine.Object, DataLossKind> srcDataLoss,
            Dictionary<UnityEngine.Object, DataLossKind> dstDataLoss,
            List<ObjectDiff> objectDiffs)
        {
            Dictionary<string, List<UnityEngine.Object>> srcByKey = GroupByKey(srcObjects, GetObjectKey);
            Dictionary<string, List<UnityEngine.Object>> dstByKey = GroupByKey(dstObjects, GetObjectKey);

            List<UnityEngine.Object> unmatchedSrc = new List<UnityEngine.Object>();
            List<UnityEngine.Object> unmatchedDst = new List<UnityEngine.Object>();

            HashSet<string> allKeys = new HashSet<string>(srcByKey.Keys);
            allKeys.UnionWith(dstByKey.Keys);

            foreach (string key in allKeys)
            {
                srcByKey.TryGetValue(key, out List<UnityEngine.Object> srcList);
                dstByKey.TryGetValue(key, out List<UnityEngine.Object> dstList);

                srcList = srcList ?? EmptyList;
                dstList = dstList ?? EmptyList;

                int srcCount = srcList.Count;
                int dstCount = dstList.Count;
                int matchCount = Math.Min(srcCount, dstCount);

                for (int i = 0; i < matchCount; i++)
                    IdentityPairedDiffs.AddModifiedDiff(
                        srcList[i], dstList[i],
                        srcDataLoss, dstDataLoss,
                        objectDiffs);

                for (int i = matchCount; i < srcCount; i++)
                    unmatchedSrc.Add(srcList[i]);

                for (int i = matchCount; i < dstCount; i++)
                    unmatchedDst.Add(dstList[i]);
            }

            MatchUnmatchedBySimilarity(
                unmatchedSrc, unmatchedDst,
                srcDataLoss, dstDataLoss,
                objectDiffs);
        }

        static string GetObjectKey(UnityEngine.Object obj)
        {
            if (obj == null) return string.Empty;

            string typeName = obj.GetType().Name;
            string objName = obj.name;

            if (obj is GameObject go)
            {
                Transform transform = go.transform;
                int depth = 0;
                Transform t = transform;
                while (t.parent != null) { depth++; t = t.parent; }

                if (depth == 0)
                    return string.Concat(typeName, ":", objName);

                StringBuilder sb = new StringBuilder(
                    typeName.Length + 1 + (depth + 1) * 16);
                sb.Append(typeName).Append(':');

                AppendHierarchyPath(sb, transform);

                return sb.ToString();
            }

            return string.Concat(typeName, ":", objName);
        }

        static void AppendHierarchyPath(StringBuilder sb, Transform transform)
        {
            if (transform.parent != null)
            {
                AppendHierarchyPath(sb, transform.parent);
                sb.Append('/');
            }
            sb.Append(transform.name);
        }

        static void MatchUnmatchedBySimilarity(
            List<UnityEngine.Object> unmatchedSrc,
            List<UnityEngine.Object> unmatchedDst,
            Dictionary<UnityEngine.Object, DataLossKind> srcDataLoss,
            Dictionary<UnityEngine.Object, DataLossKind> dstDataLoss,
            List<ObjectDiff> objectDiffs)
        {
            if (unmatchedSrc.Count == 0 || unmatchedDst.Count == 0)
            {
                AddAsAddedRemoved(
                    unmatchedSrc, unmatchedDst,
                    srcDataLoss, dstDataLoss,
                    objectDiffs);
                return;
            }

            Dictionary<string, List<UnityEngine.Object>> srcByType = GroupByKey(unmatchedSrc, o => o.GetType().Name);
            Dictionary<string, List<UnityEngine.Object>> dstByType = GroupByKey(unmatchedDst, o => o.GetType().Name);

            HashSet<UnityEngine.Object> matchedSrc = new HashSet<UnityEngine.Object>();
            HashSet<UnityEngine.Object> matchedDst = new HashSet<UnityEngine.Object>();

            foreach (KeyValuePair<string, List<UnityEngine.Object>> kvp in srcByType)
            {
                if (!dstByType.TryGetValue(kvp.Key, out List<UnityEngine.Object> dstTypeList))
                    continue;

                List<ObjectTreeEntry> srcEntries = BuildTreeEntries(kvp.Value);
                List<ObjectTreeEntry> dstEntries = BuildTreeEntries(dstTypeList);

                IReadOnlyList<PropertyTreeNode> srcTrees = ExtractTrees(srcEntries);
                IReadOnlyList<PropertyTreeNode> dstTrees = ExtractTrees(dstEntries);
                Dictionary<string, string>[] srcLeaves = SimilarityCalculator.BuildLeafMaps(srcTrees);
                Dictionary<string, string>[] dstLeaves = SimilarityCalculator.BuildLeafMaps(dstTrees);

                HashSet<int> usedDst = new HashSet<int>();

                for (int i = 0; i < srcEntries.Count; i++)
                {
                    int bestIdx = -1;
                    double bestSimilarity = OBJECT_MATCH_THRESHOLD;

                    for (int j = 0; j < dstEntries.Count; j++)
                    {
                        if (usedDst.Contains(j))
                            continue;

                        double similarity = SimilarityCalculator.CalculateFromLeaves(
                            srcLeaves[i], dstLeaves[j]);

                        if (similarity > bestSimilarity)
                        {
                            bestIdx = j;
                            bestSimilarity = similarity;
                        }
                    }

                    if (bestIdx < 0)
                        continue;

                    usedDst.Add(bestIdx);
                    matchedSrc.Add(srcEntries[i].Object);
                    matchedDst.Add(dstEntries[bestIdx].Object);

                    AddModifiedDiffFromTrees(
                        srcEntries[i].Object, dstEntries[bestIdx].Object,
                        srcEntries[i].Tree, dstEntries[bestIdx].Tree,
                        srcDataLoss, dstDataLoss,
                        objectDiffs);
                }
            }

            foreach (UnityEngine.Object srcObj in unmatchedSrc)
            {
                if (!matchedSrc.Contains(srcObj))
                    IdentityPairedDiffs.AddRemovedDiff(srcObj, srcDataLoss, objectDiffs);
            }

            foreach (UnityEngine.Object dstObj in unmatchedDst)
            {
                if (!matchedDst.Contains(dstObj))
                    IdentityPairedDiffs.AddAddedDiff(dstObj, dstDataLoss, objectDiffs);
            }
        }

        static void AddAsAddedRemoved(
            List<UnityEngine.Object> srcObjects,
            List<UnityEngine.Object> dstObjects,
            Dictionary<UnityEngine.Object, DataLossKind> srcDataLoss,
            Dictionary<UnityEngine.Object, DataLossKind> dstDataLoss,
            List<ObjectDiff> objectDiffs)
        {
            foreach (UnityEngine.Object srcObj in srcObjects)
                IdentityPairedDiffs.AddRemovedDiff(srcObj, srcDataLoss, objectDiffs);

            foreach (UnityEngine.Object dstObj in dstObjects)
                IdentityPairedDiffs.AddAddedDiff(dstObj, dstDataLoss, objectDiffs);
        }

        static void AddModifiedDiffFromTrees(
            UnityEngine.Object srcObj,
            UnityEngine.Object dstObj,
            PropertyTreeNode srcTree,
            PropertyTreeNode dstTree,
            Dictionary<UnityEngine.Object, DataLossKind> srcDataLoss,
            Dictionary<UnityEngine.Object, DataLossKind> dstDataLoss,
            List<ObjectDiff> objectDiffs)
        {
            ObjectDiff diff = new ObjectDiff
            {
                SrcObject = srcObj,
                DstObject = dstObj,
                DataLoss = DataLossDetection.PickMoreSpecific(
                    DataLossDetection.LookupOrNone(srcDataLoss, srcObj),
                    DataLossDetection.LookupOrNone(dstDataLoss, dstObj))
            };

            diff.PropertyDiffTree = PropertyDiffs.BuildDiffTree(srcTree, dstTree);

            if (diff.PropertyDiffTree == null && diff.DataLoss == DataLossKind.None)
                return;

            diff.DiffType = DiffType.Modified;

            objectDiffs.Add(diff);
        }

        static List<ObjectTreeEntry> BuildTreeEntries(
            List<UnityEngine.Object> objects)
        {
            List<ObjectTreeEntry> entries = new List<ObjectTreeEntry>(objects.Count);

            foreach (UnityEngine.Object obj in objects)
            {
                using (SerializedObject so = new SerializedObject(obj))
                {
                    entries.Add(new ObjectTreeEntry
                    {
                        Object = obj,
                        Tree = PropertyTreeBuilder.Build(so)
                    });
                }
            }

            return entries;
        }

        static IReadOnlyList<PropertyTreeNode> ExtractTrees(
            List<ObjectTreeEntry> entries)
        {
            PropertyTreeNode[] trees = new PropertyTreeNode[entries.Count];
            for (int i = 0; i < entries.Count; i++)
                trees[i] = entries[i].Tree;
            return trees;
        }

        static Dictionary<string, List<T>> GroupByKey<T>(
            IList<T> items, Func<T, string> keySelector)
        {
            Dictionary<string, List<T>> map = new Dictionary<string, List<T>>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                string key = keySelector(items[i]);
                if (!map.TryGetValue(key, out List<T> list))
                {
                    list = new List<T>();
                    map[key] = list;
                }
                list.Add(items[i]);
            }
            return map;
        }

        struct ObjectTreeEntry
        {
            internal UnityEngine.Object Object;
            internal PropertyTreeNode Tree;
        }

        static readonly List<UnityEngine.Object> EmptyList =
            new List<UnityEngine.Object>();

        const double OBJECT_MATCH_THRESHOLD = 0.5;
    }
}
