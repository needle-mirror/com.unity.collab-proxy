using System.Collections.Generic;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.Property
{
    internal enum NodeKind : byte
    {
        Leaf,
        Object,
        Array
    }

    internal class PropertyTreeNode
    {
        internal string Name { get; }
        internal string Path { get; }
        internal string DisplayName { get; }
        internal NodeKind Kind { get; }
        internal string TypeTag { get; }
        internal string Value { get; }
        internal IReadOnlyList<PropertyTreeNode> Children => mChildren;
        internal object Tag { get; }

        internal PropertyTreeNode(
            string name,
            string path,
            string displayName,
            NodeKind kind,
            string typeTag,
            string value,
            List<PropertyTreeNode> children,
            object tag)
        {
            Name = name ?? string.Empty;
            Path = path ?? string.Empty;
            DisplayName = displayName ?? name ?? string.Empty;
            Kind = kind;
            TypeTag = typeTag ?? string.Empty;
            Value = value;
            mChildren = children ?? EMPTY_CHILDREN;
            Tag = tag;
        }

        internal static PropertyTreeNode CreateLeaf(
            string name,
            string path,
            string typeTag,
            string value,
            string displayName = null,
            object tag = null)
        {
            return new PropertyTreeNode(
                name, path, displayName, NodeKind.Leaf, typeTag, value, null, tag);
        }

        internal static PropertyTreeNode CreateObject(
            string name,
            string path,
            List<PropertyTreeNode> children,
            string displayName = null,
            object tag = null)
        {
            return new PropertyTreeNode(
                name, path, displayName, NodeKind.Object, "Generic", null, children, tag);
        }

        internal static PropertyTreeNode CreateArray(
            string name,
            string path,
            List<PropertyTreeNode> children,
            string displayName = null,
            object tag = null)
        {
            return new PropertyTreeNode(
                name, path, displayName, NodeKind.Array, "Array", null, children, tag);
        }

        internal static PropertyTreeNode CreateRoot(List<PropertyTreeNode> children)
        {
            return CreateObject(string.Empty, string.Empty, children);
        }

        static readonly List<PropertyTreeNode> EMPTY_CHILDREN = new List<PropertyTreeNode>();
        readonly List<PropertyTreeNode> mChildren;
    }
}
