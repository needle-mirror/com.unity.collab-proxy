using System.Collections.Generic;
using System.Diagnostics;
using UnityEditorInternal;

using Codice.LogWrapper;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.UnityObject;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    internal static class UnityObjectDiffs
    {
        internal static List<ObjectDiff> BuildDiffData(
            string srcFile,
            string dstFile)
        {
            Stopwatch sw = Stopwatch.StartNew();

            List<ObjectDiff> objectDiffs = new List<ObjectDiff>();

            UnityEngine.Object[] srcObjects = InternalEditorUtility.LoadSerializedFileAndForget(srcFile);
            UnityEngine.Object[] dstObjects = InternalEditorUtility.LoadSerializedFileAndForget(dstFile);

            FileIdInfo srcFileIdInfo = FileIdReader.Read(srcFile);
            FileIdInfo dstFileIdInfo = FileIdReader.Read(dstFile);

            Dictionary<long, UnityEngine.Object> srcById = IdentityPairedDiffs.BuildFileIdMap(srcObjects);
            Dictionary<long, UnityEngine.Object> dstById = IdentityPairedDiffs.BuildFileIdMap(dstObjects);

            // Detect objects where LoadSerializedFileAndForget silently dropped
            // data — missing scripts substituted as UnityEditor.FallbackEditorWindow,
            // MonoBehaviours with unresolved MonoScript references,
            // AnimatorOverrideControllers whose controller couldn't be
            // resolved (BuildAsset wipes m_Clips in that state). These maps
            // are consumed by the diff builders to tag each ObjectDiff with a
            // DataLoss reason. See DataLossDetection for the per-object rules.
            Dictionary<UnityEngine.Object, DataLossKind> srcDataLoss =
                DataLossDetection.DetectPerObject(srcObjects);
            Dictionary<UnityEngine.Object, DataLossKind> dstDataLoss =
                DataLossDetection.DetectPerObject(dstObjects);

            if (CanPairByFileId(srcObjects, dstObjects, srcById, dstById))
            {
                IdentityPairedDiffs.Build(
                    srcById, dstById,
                    srcFileIdInfo.HashByFileId,
                    dstFileIdInfo.HashByFileId,
                    srcDataLoss, dstDataLoss,
                    objectDiffs);

                ComponentReorderDiffs.Append(srcById, dstById, objectDiffs);
                SiblingReorderDiffs.Append(
                    srcObjects, dstObjects, srcById, dstById, objectDiffs);
                ParentChangeDiffs.Append(srcById, dstById, objectDiffs);
            }
            else
            {
                // Component, sibling and parent change detection are skipped on
                // the fallback path because the name-based matcher cannot
                // reliably pair GameObjects across the two sides.
                mLog.InfoFormat("Falling back to name-based diff for files '{0}' and '{1}' " +
                                "because file IDs could not be read", srcFile, dstFile);

                NameMatchedDiffs.Build(
                    srcObjects, dstObjects,
                    srcDataLoss, dstDataLoss,
                    objectDiffs);
            }

            ObjectDiffGrouping.GroupComponentsUnderGameObjects(objectDiffs);

            HierarchyOrderSorting.SortByHierarchyOrder(
                objectDiffs,
                d => d.DstObject ?? d.SrcObject,
                // dst hierarchy first — matches what the user sees in their
                // workspace after pulling/checking out the destination. Then
                // src-only objects (Removed) keep their src-side position.
                dstObjects, srcObjects);

            int dataLossCount = CountObjectsWithDataLoss(objectDiffs);
            if (dataLossCount > 0)
            {
                mLog.InfoFormat(
                    "LoadSerializedFileAndForget dropped data on {0} object(s) " +
                    "while diffing '{1}' and '{2}' — these diffs are incomplete " +
                    "and the user should be steered to text diff",
                    dataLossCount, srcFile, dstFile);
            }

            mLog.DebugFormat(
                "{0} object diffs calculated in {1} ms",
                objectDiffs.Count, sw.ElapsedMilliseconds);

            return objectDiffs;
        }

        static bool CanPairByFileId(
            UnityEngine.Object[] srcObjects,
            UnityEngine.Object[] dstObjects,
            Dictionary<long, UnityEngine.Object> srcById,
            Dictionary<long, UnityEngine.Object> dstById)
        {
            return srcById.Count == IdentityPairedDiffs.CountNonNull(srcObjects)
                && dstById.Count == IdentityPairedDiffs.CountNonNull(dstObjects);
        }

        static int CountObjectsWithDataLoss(List<ObjectDiff> diffs)
        {
            int n = 0;
            foreach (ObjectDiff d in diffs)
            {
                if (d.DataLoss != DataLossKind.None)
                    n++;
            }
            return n;
        }

        static readonly ILog mLog = PlasticApp.GetLogger("UnityObjectDiffs");
    }
}
