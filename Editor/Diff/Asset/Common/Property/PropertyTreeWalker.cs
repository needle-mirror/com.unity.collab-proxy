using System;
using System.Collections.Generic;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.Property
{
    // Walks a property tree (PropertyTreeNode or PropertyDiffNode) and emits
    // rows via caller-supplied callbacks. Replaces the previously duplicated
    // AddTreeNode recursion in AssetDiffControl and AssetContentControl with
    // a single generic walker, parameterised on the node type via accessors
    // (mirroring the HierarchyOrderSorting<T> precedent in this codebase).
    internal class PropertyTreeWalker<TNode>
    {
        internal PropertyTreeWalker(
            Func<TNode, NodeKind> getKind,
            Func<TNode, IReadOnlyList<TNode>> getChildren)
        {
            mGetKind = getKind;
            mGetChildren = getChildren;
        }

        // Visits every container in the tree and registers its group key,
        // regardless of visibility or expansion state. Run before Walk so
        // that CollapseAll can collapse groups that are currently hidden by
        // filter / search — otherwise those groups would silently fall out
        // of mKnownGroups and re-appear expanded when the filter clears.
        internal void CollectGroupKeys(
            IReadOnlyList<TNode> roots,
            Func<TNode, string> buildGroupKey,
            Action<string> registerGroupKey)
        {
            foreach (TNode node in roots)
                CollectGroupKeysFromNode(node, buildGroupKey, registerGroupKey);
        }

        internal void Walk(
            IReadOnlyList<TNode> roots,
            int baseIndent,
            Func<TNode, bool> isVisible,
            Func<TNode, string> buildGroupKey,
            Func<string, bool> isExpanded,
            Action<TNode, int> emitLeaf,
            Action<TNode, int, string> emitContainerHeader)
        {
            foreach (TNode node in roots)
                WalkNode(
                    node, baseIndent,
                    isVisible, buildGroupKey, isExpanded,
                    emitLeaf, emitContainerHeader);
        }

        void CollectGroupKeysFromNode(
            TNode node,
            Func<TNode, string> buildGroupKey,
            Action<string> registerGroupKey)
        {
            if (mGetKind(node) == NodeKind.Leaf)
                return;

            IReadOnlyList<TNode> children = mGetChildren(node);
            if (children.Count == 0)
                return;

            registerGroupKey(buildGroupKey(node));

            foreach (TNode child in children)
                CollectGroupKeysFromNode(child, buildGroupKey, registerGroupKey);
        }

        void WalkNode(
            TNode node,
            int indent,
            Func<TNode, bool> isVisible,
            Func<TNode, string> buildGroupKey,
            Func<string, bool> isExpanded,
            Action<TNode, int> emitLeaf,
            Action<TNode, int, string> emitContainerHeader)
        {
            if (!isVisible(node))
                return;

            if (mGetKind(node) == NodeKind.Leaf)
            {
                emitLeaf(node, indent);
                return;
            }

            IReadOnlyList<TNode> children = mGetChildren(node);
            string fullKey = buildGroupKey(node);
            emitContainerHeader(node, indent, fullKey);

            if (children.Count == 0 || !isExpanded(fullKey))
                return;

            foreach (TNode child in children)
                WalkNode(
                    child, indent + 1,
                    isVisible, buildGroupKey, isExpanded,
                    emitLeaf, emitContainerHeader);
        }

        readonly Func<TNode, NodeKind> mGetKind;
        readonly Func<TNode, IReadOnlyList<TNode>> mGetChildren;
    }
}
