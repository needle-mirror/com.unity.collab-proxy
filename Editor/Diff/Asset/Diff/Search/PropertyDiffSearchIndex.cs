using System.Collections.Generic;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.Search
{
    internal class PropertyDiffSearchIndex
    {
        internal bool IsActive { get; }

        internal static PropertyDiffSearchIndex Build(
            IList<ObjectDiff> objectDiffs, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new PropertyDiffSearchIndex(isActive: false, null);

            HashSet<PropertyDiffNode> visible = new HashSet<PropertyDiffNode>();

            foreach (ObjectDiff objDiff in objectDiffs)
            {
                bool objectInScope = ObjectNameMatches(objDiff, searchTerm);

                CollectVisibleForObject(
                    objDiff.PropertyDiffTree, searchTerm, objectInScope, visible);

                if (objDiff.ComponentDiffs == null)
                    continue;

                foreach (ObjectDiff comp in objDiff.ComponentDiffs)
                {
                    bool componentInScope = objectInScope
                        || ObjectNameMatches(comp, searchTerm);
                    CollectVisibleForObject(
                        comp.PropertyDiffTree, searchTerm, componentInScope, visible);
                }
            }

            return new PropertyDiffSearchIndex(isActive: true, visible);
        }

        static bool ObjectNameMatches(ObjectDiff diff, string searchTerm)
        {
            return SearchMatcher.Contains(diff.GetSrcDisplayName(), searchTerm)
                || SearchMatcher.Contains(diff.GetDstDisplayName(), searchTerm);
        }

        static void CollectVisibleForObject(
            PropertyDiffNode root,
            string searchTerm,
            bool ownerNameMatches,
            HashSet<PropertyDiffNode> visible)
        {
            if (root == null)
                return;

            if (ownerNameMatches)
            {
                foreach (PropertyDiffNode child in root.Children)
                    AddSubtree(child, visible);
                return;
            }

            CollectVisible(root, searchTerm, visible);
        }

        internal bool IsNodeVisible(PropertyDiffNode node)
        {
            if (!IsActive)
                return true;

            return mVisibleNodes.Contains(node);
        }

        internal bool SubtreeHasMatch(ObjectDiff diff)
        {
            if (!IsActive)
                return false;

            if (diff.PropertyDiffTree == null)
                return false;

            foreach (PropertyDiffNode child in diff.PropertyDiffTree.Children)
            {
                if (mVisibleNodes.Contains(child))
                    return true;
            }
            return false;
        }

        PropertyDiffSearchIndex(bool isActive, HashSet<PropertyDiffNode> visibleNodes)
        {
            IsActive = isActive;
            mVisibleNodes = visibleNodes;
        }

        static bool CollectVisible(
            PropertyDiffNode node,
            string searchTerm,
            HashSet<PropertyDiffNode> visible)
        {
            if (node == null)
                return false;

            if (node.Kind == NodeKind.Leaf)
            {
                string label = node.DisplayName ?? node.Path;
                bool matches =
                    SearchMatcher.Contains(label, searchTerm) ||
                    SearchMatcher.Contains(node.Path, searchTerm);
                if (matches)
                    visible.Add(node);
                return matches;
            }

            bool nodeMatches =
                SearchMatcher.Contains(node.DisplayName, searchTerm) ||
                SearchMatcher.Contains(node.Path, searchTerm);

            if (nodeMatches)
            {
                AddSubtree(node, visible);
                return true;
            }

            bool anyChildMatches = false;
            foreach (PropertyDiffNode child in node.Children)
            {
                if (CollectVisible(child, searchTerm, visible))
                    anyChildMatches = true;
            }
            if (anyChildMatches)
                visible.Add(node);
            return anyChildMatches;
        }

        static void AddSubtree(
            PropertyDiffNode node, HashSet<PropertyDiffNode> visible)
        {
            visible.Add(node);
            foreach (PropertyDiffNode child in node.Children)
                AddSubtree(child, visible);
        }

        readonly HashSet<PropertyDiffNode> mVisibleNodes;
    }
}
