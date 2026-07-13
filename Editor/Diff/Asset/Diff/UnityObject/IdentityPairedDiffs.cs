using System.Collections.Generic;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;
using UnityEditor;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    internal static class IdentityPairedDiffs
    {
        internal static void Build(
            Dictionary<long, UnityEngine.Object> srcById,
            Dictionary<long, UnityEngine.Object> dstById,
            Dictionary<long, long> srcBlockHashes,
            Dictionary<long, long> dstBlockHashes,
            Dictionary<UnityEngine.Object, DataLossKind> srcDataLoss,
            Dictionary<UnityEngine.Object, DataLossKind> dstDataLoss,
            List<ObjectDiff> objectDiffs)
        {
            HashSet<long> matchedDstIds = new HashSet<long>();

            foreach (KeyValuePair<long, UnityEngine.Object> kvp in srcById)
            {
                if (!dstById.TryGetValue(kvp.Key, out UnityEngine.Object dstObj))
                {
                    AddRemovedDiff(kvp.Value, srcDataLoss, objectDiffs);
                    continue;
                }

                matchedDstIds.Add(kvp.Key);

                if (IsBlockUnchanged(kvp.Key, srcBlockHashes, dstBlockHashes))
                    continue;

                AddModifiedDiff(
                    kvp.Value, dstObj, srcDataLoss, dstDataLoss, objectDiffs);
            }

            foreach (KeyValuePair<long, UnityEngine.Object> kvp in dstById)
            {
                if (!matchedDstIds.Contains(kvp.Key))
                    AddAddedDiff(kvp.Value, dstDataLoss, objectDiffs);
            }
        }

        internal static Dictionary<long, UnityEngine.Object> BuildFileIdMap(
            UnityEngine.Object[] objects)
        {
            // LoadSerializedFileAndForget returns objects in an order that does not
            // match the YAML block order, so we cannot zip by index. Read each
            // object's actual local file ID from its serialized state instead.
            Dictionary<long, UnityEngine.Object> map =
                new Dictionary<long, UnityEngine.Object>(objects.Length);

            foreach (UnityEngine.Object obj in objects)
            {
                if (obj == null)
                    continue;

                long fileId = obj.GetLocalFileId();
                if (fileId == 0)
                    continue;

                map[fileId] = obj;
            }

            return map;
        }

        internal static int CountNonNull(UnityEngine.Object[] objects)
        {
            int n = 0;
            foreach (UnityEngine.Object o in objects)
                if (o != null) n++;
            return n;
        }

        // Foundational diff builders. Used by both this pass and NameMatchedDiffs.
        internal static void AddModifiedDiff(
            UnityEngine.Object srcObj,
            UnityEngine.Object dstObj,
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

            using (SerializedObject srcSO = new SerializedObject(srcObj))
            using (SerializedObject dstSO = new SerializedObject(dstObj))
            {
                PropertyTreeNode srcTree = PropertyTreeBuilder.Build(srcSO);
                PropertyTreeNode dstTree = PropertyTreeBuilder.Build(dstSO);

                diff.PropertyDiffTree = PropertyDiffs.BuildDiffTree(srcTree, dstTree);

                if (diff.PropertyDiffTree == null)
                {
                    // No visible property diff. Keep the entry only when a
                    // per-object DataLossDetection rule already flagged the
                    // object — the loader dropped serialized data and the
                    // empty tree is a symptom of that loss (e.g. AOC with
                    // null controller wipes m_Clips, FallbackEditorWindow
                    // strips everything but base EditorWindow fields).
                    // Otherwise the YAML difference was in fields the
                    // property tree filters out (m_Component reorder,
                    // etc.) and there is nothing for the user to see.
                    if (diff.DataLoss == DataLossKind.None)
                        return;
                }

                diff.DiffType = DiffType.Modified;

                objectDiffs.Add(diff);
            }
        }

        internal static void AddAddedDiff(
            UnityEngine.Object dstObj,
            Dictionary<UnityEngine.Object, DataLossKind> dstDataLoss,
            List<ObjectDiff> objectDiffs)
        {
            ObjectDiff diff = new ObjectDiff
            {
                SrcObject = null,
                DstObject = dstObj,
                DiffType = DiffType.Added,
                DataLoss = DataLossDetection.LookupOrNone(dstDataLoss, dstObj)
            };

            using (SerializedObject dstSO = new SerializedObject(dstObj))
            {
                PropertyTreeNode dstTree = PropertyTreeBuilder.Build(dstSO);
                diff.PropertyDiffTree = PropertyDiffs.BuildDiffTree(null, dstTree);
            }

            objectDiffs.Add(diff);
        }

        internal static void AddRemovedDiff(
            UnityEngine.Object srcObj,
            Dictionary<UnityEngine.Object, DataLossKind> srcDataLoss,
            List<ObjectDiff> objectDiffs)
        {
            ObjectDiff diff = new ObjectDiff
            {
                SrcObject = srcObj,
                DstObject = null,
                DiffType = DiffType.Removed,
                DataLoss = DataLossDetection.LookupOrNone(srcDataLoss, srcObj)
            };

            using (SerializedObject srcSO = new SerializedObject(srcObj))
            {
                PropertyTreeNode srcTree = PropertyTreeBuilder.Build(srcSO);
                diff.PropertyDiffTree = PropertyDiffs.BuildDiffTree(srcTree, null);
            }

            objectDiffs.Add(diff);
        }

        static bool IsBlockUnchanged(
            long fileId,
            Dictionary<long, long> srcBlockHashes,
            Dictionary<long, long> dstBlockHashes)
        {
            return srcBlockHashes.TryGetValue(fileId, out long srcHash)
                && dstBlockHashes.TryGetValue(fileId, out long dstHash)
                && srcHash == dstHash;
        }
    }
}
