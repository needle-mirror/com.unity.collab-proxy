using System.Collections.Generic;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content.Search
{
    internal class PropertyContentSearchIndex
    {
        internal bool IsActive { get; }

        internal static PropertyContentSearchIndex Build(
            IList<ObjectContent> objectContents, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new PropertyContentSearchIndex(isActive: false, null);

            HashSet<PropertyTreeNode> visible = new HashSet<PropertyTreeNode>();

            foreach (ObjectContent content in objectContents)
            {
                bool objectInScope = SearchMatcher.Contains(
                    content.GetDisplayName(), searchTerm);

                CollectVisibleForObject(
                    content.PropertyTree, searchTerm, objectInScope, visible);

                if (content.ComponentContents == null)
                    continue;

                foreach (ObjectContent comp in content.ComponentContents)
                {
                    bool componentInScope = objectInScope
                        || SearchMatcher.Contains(
                            comp.GetDisplayName(), searchTerm);
                    CollectVisibleForObject(
                        comp.PropertyTree, searchTerm, componentInScope, visible);
                }
            }

            return new PropertyContentSearchIndex(isActive: true, visible);
        }

        static void CollectVisibleForObject(
            PropertyTreeNode root,
            string searchTerm,
            bool ownerNameMatches,
            HashSet<PropertyTreeNode> visible)
        {
            if (root == null)
                return;

            if (ownerNameMatches)
            {
                foreach (PropertyTreeNode child in root.Children)
                    AddSubtree(child, visible);
                return;
            }

            CollectVisible(root, searchTerm, visible);
        }

        internal bool IsNodeVisible(PropertyTreeNode node)
        {
            if (!IsActive)
                return true;

            return mVisibleNodes.Contains(node);
        }

        internal bool SubtreeHasMatch(ObjectContent content)
        {
            if (!IsActive)
                return false;

            if (content.PropertyTree == null)
                return false;

            foreach (PropertyTreeNode child in content.PropertyTree.Children)
            {
                if (mVisibleNodes.Contains(child))
                    return true;
            }
            return false;
        }

        PropertyContentSearchIndex(bool isActive, HashSet<PropertyTreeNode> visibleNodes)
        {
            IsActive = isActive;
            mVisibleNodes = visibleNodes;
        }

        static bool CollectVisible(
            PropertyTreeNode node,
            string searchTerm,
            HashSet<PropertyTreeNode> visible)
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
            foreach (PropertyTreeNode child in node.Children)
            {
                if (CollectVisible(child, searchTerm, visible))
                    anyChildMatches = true;
            }
            if (anyChildMatches)
                visible.Add(node);
            return anyChildMatches;
        }

        static void AddSubtree(
            PropertyTreeNode node, HashSet<PropertyTreeNode> visible)
        {
            visible.Add(node);
            foreach (PropertyTreeNode child in node.Children)
                AddSubtree(child, visible);
        }

        readonly HashSet<PropertyTreeNode> mVisibleNodes;
    }
}
