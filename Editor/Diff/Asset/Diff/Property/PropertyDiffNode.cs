using System.Collections.Generic;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property
{
    internal class PropertyDiffNode
    {
        internal NodeKind Kind { get; }
        internal string Path { get; }
        internal string DisplayName { get; }
        internal string TypeTag { get; }
        internal DiffType DiffType { get; }
        internal string SrcValue { get; }
        internal string DstValue { get; }
        internal object SrcTag { get; }
        internal object DstTag { get; }
        internal IReadOnlyList<PropertyDiffNode> Children => mChildren;
        internal int LeafCount { get; }
        internal DiffTypeFlags DescendantDiffTypes { get; }

        internal PropertyDiffNode(
            NodeKind kind,
            string path,
            string displayName,
            string typeTag,
            DiffType diffType,
            string srcValue,
            string dstValue,
            object srcTag,
            object dstTag,
            List<PropertyDiffNode> children)
        {
            Kind = kind;
            Path = path ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            TypeTag = typeTag ?? string.Empty;
            DiffType = diffType;
            SrcValue = srcValue;
            DstValue = dstValue;
            SrcTag = srcTag;
            DstTag = dstTag;
            mChildren = children ?? EMPTY_CHILDREN;

            if (kind == NodeKind.Leaf)
            {
                LeafCount = 1;
                DescendantDiffTypes = ToFlag(diffType);
                return;
            }

            int leafCount = 0;
            DiffTypeFlags flags = DiffTypeFlags.None;
            foreach (PropertyDiffNode child in mChildren)
            {
                leafCount += child.LeafCount;
                flags |= child.DescendantDiffTypes;
            }
            LeafCount = leafCount;
            DescendantDiffTypes = flags;
        }

        internal bool HasDescendantOfType(DiffType type)
        {
            return (DescendantDiffTypes & ToFlag(type)) != DiffTypeFlags.None;
        }

        internal static PropertyDiffNode CreateLeaf(
            string name,
            string path,
            string typeTag,
            DiffType diffType,
            string srcValue,
            string dstValue,
            object srcTag,
            object dstTag)
        {
            return new PropertyDiffNode(
                NodeKind.Leaf, path, name, typeTag, diffType,
                srcValue, dstValue, srcTag, dstTag, null);
        }

        internal static PropertyDiffNode CreateContainer(
            NodeKind kind,
            string name,
            string path,
            DiffType diffType,
            List<PropertyDiffNode> children)
        {
            return new PropertyDiffNode(
                kind, path, name, GetContainerTypeTag(kind), diffType,
                null, null, null, null, children);
        }

        internal static PropertyDiffNode CreateRoot(List<PropertyDiffNode> children)
        {
            return CreateContainer(
                NodeKind.Object, string.Empty, string.Empty,
                AggregateDiffType(children), children);
        }

        internal static DiffType AggregateDiffType(IReadOnlyList<PropertyDiffNode> children)
        {
            if (children == null || children.Count == 0)
                return DiffType.Unchanged;

            DiffTypeFlags flags = DiffTypeFlags.None;
            foreach (PropertyDiffNode child in children)
                flags |= child.DescendantDiffTypes;

            int kinds = 0;
            if ((flags & DiffTypeFlags.Modified) != 0) kinds++;
            if ((flags & DiffTypeFlags.Added) != 0) kinds++;
            if ((flags & DiffTypeFlags.Removed) != 0) kinds++;

            if (kinds > 1)
                return DiffType.Modified;
            if ((flags & DiffTypeFlags.Modified) != 0) return DiffType.Modified;
            if ((flags & DiffTypeFlags.Added) != 0) return DiffType.Added;
            if ((flags & DiffTypeFlags.Removed) != 0) return DiffType.Removed;
            return DiffType.Unchanged;
        }

        static string GetContainerTypeTag(NodeKind kind)
        {
            return kind == NodeKind.Array ? "Array" : "Generic";
        }

        static DiffTypeFlags ToFlag(DiffType diffType)
        {
            switch (diffType)
            {
                case DiffType.Modified: return DiffTypeFlags.Modified;
                case DiffType.Added: return DiffTypeFlags.Added;
                case DiffType.Removed: return DiffTypeFlags.Removed;
                default: return DiffTypeFlags.None;
            }
        }

        static readonly List<PropertyDiffNode> EMPTY_CHILDREN = new List<PropertyDiffNode>();
        readonly List<PropertyDiffNode> mChildren;
    }

    [System.Flags]
    internal enum DiffTypeFlags : byte
    {
        None = 0,
        Modified = 1,
        Added = 2,
        Removed = 4
    }
}
