using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property
{
    internal static class PropertyDiffs
    {
        internal static List<PropertyDiff> Compare(
            PropertyTreeNode src,
            PropertyTreeNode dst)
        {
            return Flatten(BuildDiffTree(src, dst));
        }

        internal static PropertyDiffNode BuildDiffTree(
            PropertyTreeNode src,
            PropertyTreeNode dst)
        {
            return BuildDiffTree(src, dst, DEFAULT_ARRAY_MATCH_THRESHOLD);
        }

        internal static List<PropertyDiff> Flatten(PropertyDiffNode tree)
        {
            List<PropertyDiff> results = new List<PropertyDiff>();
            if (tree != null)
                FlattenInto(tree, results);
            return results;
        }

        static PropertyDiffNode BuildDiffTree(
            PropertyTreeNode src,
            PropertyTreeNode dst,
            double arrayMatchThreshold)
        {
            if (src == null && dst == null)
                return null;

            if (src == null)
                return BuildAllAdded(dst);

            if (dst == null)
                return BuildAllRemoved(src);

            List<PropertyDiffNode> children = new List<PropertyDiffNode>();
            BuildChildren(src, dst, arrayMatchThreshold, children);

            if (children.Count == 0)
                return null;

            StableSortByDiffType(children);
            return PropertyDiffNode.CreateRoot(children);
        }

        static void StableSortByDiffType(List<PropertyDiffNode> children)
        {
            int count = children.Count;
            if (count <= 1)
                return;

            int[] order = new int[count];
            for (int i = 0; i < count; i++)
                order[i] = i;

            System.Array.Sort(order, (a, b) =>
            {
                int delta = GetDiffTypeOrder(children[a].DiffType)
                    - GetDiffTypeOrder(children[b].DiffType);
                if (delta != 0)
                    return delta;
                return a - b;
            });

            PropertyDiffNode[] sorted = new PropertyDiffNode[count];
            for (int i = 0; i < count; i++)
                sorted[i] = children[order[i]];

            for (int i = 0; i < count; i++)
                children[i] = sorted[i];
        }

        static int GetDiffTypeOrder(DiffType diffType)
        {
            switch (diffType)
            {
                case DiffType.Modified: return 0;
                case DiffType.Added: return 1;
                case DiffType.Removed: return 2;
                default: return 3;
            }
        }

        static void FlattenInto(PropertyDiffNode node, List<PropertyDiff> results)
        {
            if (node.Kind == NodeKind.Leaf)
            {
                results.Add(ToFlatDiff(node));
                return;
            }

            foreach (PropertyDiffNode child in node.Children)
                FlattenInto(child, results);
        }

        static PropertyDiff ToFlatDiff(PropertyDiffNode node)
        {
            return new PropertyDiff
            {
                Path = node.Path,
                DisplayName = node.DisplayName,
                TypeTag = node.TypeTag,
                DiffType = node.DiffType,
                SrcValue = node.SrcValue,
                DstValue = node.DstValue,
                SrcTag = node.SrcTag,
                DstTag = node.DstTag
            };
        }

        static void AppendPair(
            PropertyTreeNode src,
            PropertyTreeNode dst,
            double arrayMatchThreshold,
            List<PropertyDiffNode> results)
        {
            if (src.Kind == NodeKind.Leaf && dst.Kind == NodeKind.Leaf)
            {
                if (LeafValuesEqual(src, dst))
                    return;

                results.Add(PropertyDiffNode.CreateLeaf(
                    dst.DisplayName, dst.Path, dst.TypeTag,
                    DiffType.Modified,
                    src.Value, dst.Value, src.Tag, dst.Tag));
                return;
            }

            if (src.Kind == NodeKind.Leaf || dst.Kind == NodeKind.Leaf)
            {
                results.Add(BuildAllRemoved(src));
                results.Add(BuildAllAdded(dst));
                return;
            }

            List<PropertyDiffNode> children = new List<PropertyDiffNode>();
            BuildChildren(src, dst, arrayMatchThreshold, children);

            if (children.Count == 0)
                return;

            StableSortByDiffType(children);

            // When the parent exists on both sides (we're inside AppendPair)
            // but one side was empty, AggregateDiffType would collapse to
            // Added/Removed because every child carries a single DiffType.
            // The parent itself didn't appear or disappear — its contents
            // changed — so the right rollup is Modified.
            DiffType parentDiffType =
                (src.Children.Count == 0) != (dst.Children.Count == 0)
                    ? DiffType.Modified
                    : PropertyDiffNode.AggregateDiffType(children);

            results.Add(PropertyDiffNode.CreateContainer(
                dst.Kind, dst.DisplayName, dst.Path,
                parentDiffType,
                children));
        }

        static void BuildChildren(
            PropertyTreeNode src,
            PropertyTreeNode dst,
            double arrayMatchThreshold,
            List<PropertyDiffNode> results)
        {
            if (src.Kind == NodeKind.Array && dst.Kind == NodeKind.Array)
                BuildArrayChildren(src, dst, arrayMatchThreshold, results);
            else
                BuildNamedChildren(src, dst, arrayMatchThreshold, results);
        }

        static void BuildNamedChildren(
            PropertyTreeNode src,
            PropertyTreeNode dst,
            double arrayMatchThreshold,
            List<PropertyDiffNode> results)
        {
            NameIndex dstIndex = NameIndex.Build(dst.Children);
            HashSet<PropertyTreeNode> matchedDst = new HashSet<PropertyTreeNode>(
                ReferenceEqualityComparer.Instance);

            foreach (PropertyTreeNode srcChild in src.Children)
            {
                PropertyTreeNode dstChild = dstIndex.FindAndRemove(srcChild.Name);

                if (dstChild != null)
                {
                    matchedDst.Add(dstChild);
                    AppendPair(srcChild, dstChild, arrayMatchThreshold, results);
                }
                else
                {
                    results.Add(BuildAllRemoved(srcChild));
                }
            }

            foreach (PropertyTreeNode dstChild in dst.Children)
            {
                if (!matchedDst.Contains(dstChild))
                    results.Add(BuildAllAdded(dstChild));
            }
        }

        static void BuildArrayChildren(
            PropertyTreeNode srcArray,
            PropertyTreeNode dstArray,
            double arrayMatchThreshold,
            List<PropertyDiffNode> results)
        {
            IReadOnlyList<PropertyTreeNode> srcChildren = srcArray.Children;
            IReadOnlyList<PropertyTreeNode> dstChildren = dstArray.Children;

            HashSet<PropertyTreeNode> matchedSrc = new HashSet<PropertyTreeNode>(
                ReferenceEqualityComparer.Instance);
            HashSet<PropertyTreeNode> matchedDst = new HashSet<PropertyTreeNode>(
                ReferenceEqualityComparer.Instance);

            // Pair elements equal at the same index first, so a single value
            // change in an ordered array (e.g. m_Materials[0]) is not drowned
            // out by bag-style matching of duplicates elsewhere.
            int commonLen = System.Math.Min(srcChildren.Count, dstChildren.Count);
            for (int i = 0; i < commonLen; i++)
            {
                PropertyTreeNode srcAtI = srcChildren[i];
                PropertyTreeNode dstAtI = dstChildren[i];

                if (!EqualElementsSearcher.StructuralEquals(srcAtI, dstAtI))
                    continue;

                matchedSrc.Add(srcAtI);
                matchedDst.Add(dstAtI);
            }

            List<PropertyTreeNode> srcRemaining = BuildUnmatched(srcChildren, matchedSrc);
            List<PropertyTreeNode> dstRemaining = BuildUnmatched(dstChildren, matchedDst);

            List<NodeMatch> equalMatches =
                EqualElementsSearcher.Find(srcRemaining, dstRemaining);

            foreach (NodeMatch match in equalMatches)
            {
                matchedSrc.Add(match.Src);
                matchedDst.Add(match.Dst);
            }

            srcRemaining = BuildUnmatched(srcChildren, matchedSrc);
            dstRemaining = BuildUnmatched(dstChildren, matchedDst);

            List<NodeMatch> modifiedMatches =
                ModifiedElementsSearcher.Find(
                    srcRemaining, dstRemaining, arrayMatchThreshold);

            foreach (NodeMatch match in modifiedMatches)
            {
                AppendPair(match.Src, match.Dst, arrayMatchThreshold, results);
                matchedSrc.Add(match.Src);
                matchedDst.Add(match.Dst);
            }

            // Leaves cannot match through the similarity searcher (leaf
            // similarity is 0 or 1). Pair the survivors by their relative
            // order so a single-leaf change becomes Modified, not Add+Remove.
            PairRemainingLeavesByPosition(
                srcChildren, dstChildren, matchedSrc, matchedDst,
                arrayMatchThreshold, results);

            foreach (PropertyTreeNode node in srcChildren)
                if (!matchedSrc.Contains(node))
                    results.Add(BuildAllRemoved(node));

            foreach (PropertyTreeNode node in dstChildren)
                if (!matchedDst.Contains(node))
                    results.Add(BuildAllAdded(node));
        }

        static void PairRemainingLeavesByPosition(
            IReadOnlyList<PropertyTreeNode> srcChildren,
            IReadOnlyList<PropertyTreeNode> dstChildren,
            HashSet<PropertyTreeNode> matchedSrc,
            HashSet<PropertyTreeNode> matchedDst,
            double arrayMatchThreshold,
            List<PropertyDiffNode> results)
        {
            List<PropertyTreeNode> srcLeaves = CollectUnmatchedLeaves(srcChildren, matchedSrc);
            List<PropertyTreeNode> dstLeaves = CollectUnmatchedLeaves(dstChildren, matchedDst);

            int pairCount = System.Math.Min(srcLeaves.Count, dstLeaves.Count);
            for (int i = 0; i < pairCount; i++)
            {
                PropertyTreeNode srcLeaf = srcLeaves[i];
                PropertyTreeNode dstLeaf = dstLeaves[i];

                if (srcLeaf.TypeTag != dstLeaf.TypeTag)
                    continue;

                AppendPair(srcLeaf, dstLeaf, arrayMatchThreshold, results);
                matchedSrc.Add(srcLeaf);
                matchedDst.Add(dstLeaf);
            }
        }

        static List<PropertyTreeNode> CollectUnmatchedLeaves(
            IReadOnlyList<PropertyTreeNode> children,
            HashSet<PropertyTreeNode> matched)
        {
            List<PropertyTreeNode> leaves = new List<PropertyTreeNode>();
            foreach (PropertyTreeNode node in children)
            {
                if (node.Kind != NodeKind.Leaf)
                    continue;
                if (matched.Contains(node))
                    continue;
                leaves.Add(node);
            }
            return leaves;
        }

        static PropertyDiffNode BuildAllAdded(PropertyTreeNode node)
        {
            if (node.Kind == NodeKind.Leaf)
            {
                return PropertyDiffNode.CreateLeaf(
                    node.DisplayName, node.Path, node.TypeTag,
                    DiffType.Added,
                    null, node.Value, null, node.Tag);
            }

            List<PropertyDiffNode> children = new List<PropertyDiffNode>(node.Children.Count);
            foreach (PropertyTreeNode child in node.Children)
                children.Add(BuildAllAdded(child));

            return PropertyDiffNode.CreateContainer(
                node.Kind, node.DisplayName, node.Path,
                DiffType.Added, children);
        }

        static PropertyDiffNode BuildAllRemoved(PropertyTreeNode node)
        {
            if (node.Kind == NodeKind.Leaf)
            {
                return PropertyDiffNode.CreateLeaf(
                    node.DisplayName, node.Path, node.TypeTag,
                    DiffType.Removed,
                    node.Value, null, node.Tag, null);
            }

            List<PropertyDiffNode> children = new List<PropertyDiffNode>(node.Children.Count);
            foreach (PropertyTreeNode child in node.Children)
                children.Add(BuildAllRemoved(child));

            return PropertyDiffNode.CreateContainer(
                node.Kind, node.DisplayName, node.Path,
                DiffType.Removed, children);
        }

        static List<PropertyTreeNode> BuildUnmatched(
            IReadOnlyList<PropertyTreeNode> children,
            HashSet<PropertyTreeNode> matched)
        {
            List<PropertyTreeNode> remaining = new List<PropertyTreeNode>(
                children.Count - matched.Count);

            foreach (PropertyTreeNode node in children)
                if (!matched.Contains(node))
                    remaining.Add(node);

            return remaining;
        }

        internal static bool LeafValuesEqual(PropertyTreeNode src, PropertyTreeNode dst)
        {
            if (src.TypeTag != dst.TypeTag
                || !(src.Tag is LeafPropertyData srcData)
                || !(dst.Tag is LeafPropertyData dstData))
            {
                return src.Value == dst.Value;
            }

            switch (srcData.PropertyType)
            {
                case SerializedPropertyType.Float:
                    return srcData.FloatValue.Equals(dstData.FloatValue);
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.Enum:
                    return srcData.IntValue == dstData.IntValue;
                case SerializedPropertyType.Boolean:
                    return srcData.BoolValue == dstData.BoolValue;
                case SerializedPropertyType.String:
                    return srcData.StringValue == dstData.StringValue;
                case SerializedPropertyType.Vector2:
                    return srcData.Vector2Value.Equals(dstData.Vector2Value);
                case SerializedPropertyType.Vector3:
                    return srcData.Vector3Value.Equals(dstData.Vector3Value);
                case SerializedPropertyType.Vector4:
                    return srcData.Vector4Value.Equals(dstData.Vector4Value);
                case SerializedPropertyType.Vector2Int:
                    return srcData.Vector2IntValue.Equals(dstData.Vector2IntValue);
                case SerializedPropertyType.Vector3Int:
                    return srcData.Vector3IntValue.Equals(dstData.Vector3IntValue);
                case SerializedPropertyType.Color:
                    return srcData.ColorValue.Equals(dstData.ColorValue);
                case SerializedPropertyType.Quaternion:
                    return srcData.QuaternionValue.Equals(dstData.QuaternionValue);
                case SerializedPropertyType.Rect:
                    return srcData.RectValue.Equals(dstData.RectValue);
                case SerializedPropertyType.Bounds:
                    return srcData.BoundsValue.Equals(dstData.BoundsValue);
                case SerializedPropertyType.RectInt:
                    return srcData.RectIntValue.Equals(dstData.RectIntValue);
                case SerializedPropertyType.BoundsInt:
                    return srcData.BoundsIntValue.Equals(dstData.BoundsIntValue);
                case SerializedPropertyType.Hash128:
                    return srcData.Hash128Value.Equals(dstData.Hash128Value);
                case SerializedPropertyType.AnimationCurve:
                    return Equals(srcData.AnimationCurveValue, dstData.AnimationCurveValue);
                case SerializedPropertyType.Gradient:
                    return Equals(srcData.GradientValue, dstData.GradientValue);
                default:
                    return src.Value == dst.Value;
            }
        }

        const double DEFAULT_ARRAY_MATCH_THRESHOLD = 0.5;
    }

    static class EqualElementsSearcher
    {
        internal static List<NodeMatch> Find(
            IReadOnlyList<PropertyTreeNode> srcElements,
            IReadOnlyList<PropertyTreeNode> dstElements)
        {
            List<NodeMatch> matches = new List<NodeMatch>();
            HashSet<PropertyTreeNode> usedDst = new HashSet<PropertyTreeNode>(
                ReferenceEqualityComparer.Instance);

            foreach (PropertyTreeNode src in srcElements)
            {
                foreach (PropertyTreeNode dst in dstElements)
                {
                    if (usedDst.Contains(dst))
                        continue;

                    if (!StructuralEquals(src, dst))
                        continue;

                    matches.Add(new NodeMatch(src, dst));
                    usedDst.Add(dst);
                    break;
                }
            }

            return matches;
        }

        internal static bool StructuralEquals(
            PropertyTreeNode a, PropertyTreeNode b)
        {
            if (a.Kind != b.Kind)
                return false;

            if (a.Kind == NodeKind.Leaf)
                return a.TypeTag == b.TypeTag && PropertyDiffs.LeafValuesEqual(a, b);

            if (a.Children.Count != b.Children.Count)
                return false;

            for (int i = 0; i < a.Children.Count; i++)
            {
                if (a.Kind == NodeKind.Object &&
                    a.Children[i].Name != b.Children[i].Name)
                    return false;

                if (!StructuralEquals(a.Children[i], b.Children[i]))
                    return false;
            }

            return true;
        }
    }

    static class ModifiedElementsSearcher
    {
        internal static List<NodeMatch> Find(
            IReadOnlyList<PropertyTreeNode> srcElements,
            IReadOnlyList<PropertyTreeNode> dstElements,
            double minSimilarityThreshold)
        {
            Dictionary<string, string>[] srcLeaves = SimilarityCalculator.BuildLeafMaps(srcElements);
            Dictionary<string, string>[] dstLeaves = SimilarityCalculator.BuildLeafMaps(dstElements);

            List<NodeMatch> matches = new List<NodeMatch>();
            HashSet<int> usedDst = new HashSet<int>();

            for (int i = 0; i < srcElements.Count; i++)
            {
                int bestIdx = -1;
                double bestSimilarity = minSimilarityThreshold;

                for (int j = 0; j < dstElements.Count; j++)
                {
                    if (usedDst.Contains(j))
                        continue;

                    double similarity = SimilarityCalculator.CalculateFromLeaves(
                        srcLeaves[i], dstLeaves[j]);

                    if (similarity <= bestSimilarity)
                        continue;

                    bestIdx = j;
                    bestSimilarity = similarity;
                }

                if (bestIdx < 0)
                    continue;

                matches.Add(new NodeMatch(srcElements[i], dstElements[bestIdx]));
                usedDst.Add(bestIdx);
            }

            return matches;
        }
    }

    static class SimilarityCalculator
    {
        internal static Dictionary<string, string>[] BuildLeafMaps(
            IReadOnlyList<PropertyTreeNode> elements)
        {
            Dictionary<string, string>[] maps = new Dictionary<string, string>[elements.Count];
            for (int i = 0; i < elements.Count; i++)
            {
                maps[i] = new Dictionary<string, string>();
                CollectRelativeLeaves(elements[i], string.Empty, maps[i]);
            }
            return maps;
        }

        internal static double Calculate(
            PropertyTreeNode a, PropertyTreeNode b)
        {
            if (a.Kind == NodeKind.Leaf && b.Kind == NodeKind.Leaf)
            {
                if (a.TypeTag != b.TypeTag)
                    return 0.0;

                return a.Value == b.Value ? 1.0 : 0.0;
            }

            if (a.Kind == NodeKind.Leaf || b.Kind == NodeKind.Leaf)
                return 0.0;

            Dictionary<string, string> leavesA = new Dictionary<string, string>();
            Dictionary<string, string> leavesB = new Dictionary<string, string>();
            CollectRelativeLeaves(a, string.Empty, leavesA);
            CollectRelativeLeaves(b, string.Empty, leavesB);

            return CalculateFromLeaves(leavesA, leavesB);
        }

        internal static double CalculateFromLeaves(
            Dictionary<string, string> leavesA,
            Dictionary<string, string> leavesB)
        {
            if (leavesA.Count == 0 && leavesB.Count == 0)
                return 1.0;

            int totalPaths = leavesA.Count;
            int matchCount = 0;

            foreach (KeyValuePair<string, string> kvp in leavesA)
            {
                if (leavesB.TryGetValue(kvp.Key, out string valueB))
                {
                    if (kvp.Value == valueB)
                        matchCount++;
                }
            }

            foreach (KeyValuePair<string, string> kvp in leavesB)
            {
                if (!leavesA.ContainsKey(kvp.Key))
                    totalPaths++;
            }

            if (totalPaths == 0)
                return 1.0;

            return (double)matchCount / totalPaths;
        }

        internal static void CollectRelativeLeaves(
            PropertyTreeNode node,
            string prefix,
            Dictionary<string, string> leaves)
        {
            if (node.Kind == NodeKind.Leaf)
            {
                string key = string.IsNullOrEmpty(prefix) ? node.Name : prefix;
                leaves[key] = string.Concat(node.TypeTag, "\0", node.Value ?? string.Empty);
                return;
            }

            foreach (PropertyTreeNode child in node.Children)
            {
                string childPath = string.IsNullOrEmpty(prefix)
                    ? child.Name
                    : prefix + "." + child.Name;

                CollectRelativeLeaves(child, childPath, leaves);
            }
        }
    }

    class NameIndex
    {
        internal static NameIndex Build(IReadOnlyList<PropertyTreeNode> children)
        {
            NameIndex index = new NameIndex();

            foreach (PropertyTreeNode child in children)
            {
                if (!index.mPropertiesByName.TryGetValue(child.Name, out List<PropertyTreeNode> list))
                {
                    list = new List<PropertyTreeNode>();
                    index.mPropertiesByName[child.Name] = list;
                }

                list.Add(child);
            }

            return index;
        }

        internal PropertyTreeNode FindAndRemove(string name)
        {
            if (!mPropertiesByName.TryGetValue(name, out List<PropertyTreeNode> list))
                return null;

            if (!mCursorsByName.TryGetValue(name, out int cursor))
                cursor = 0;

            if (cursor >= list.Count)
                return null;

            mCursorsByName[name] = cursor + 1;
            return list[cursor];
        }

        readonly Dictionary<string, List<PropertyTreeNode>> mPropertiesByName =
            new Dictionary<string, List<PropertyTreeNode>>();
        readonly Dictionary<string, int> mCursorsByName =
            new Dictionary<string, int>();
    }

    class ReferenceEqualityComparer : IEqualityComparer<PropertyTreeNode>
    {
        internal static readonly ReferenceEqualityComparer Instance =
            new ReferenceEqualityComparer();

        public bool Equals(PropertyTreeNode x, PropertyTreeNode y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(PropertyTreeNode obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }

    struct NodeMatch
    {
        internal PropertyTreeNode Src;
        internal PropertyTreeNode Dst;

        internal NodeMatch(PropertyTreeNode src, PropertyTreeNode dst)
        {
            Src = src;
            Dst = dst;
        }
    }
}
