using Unity.PlasticSCM.Editor.Diff.Asset.Content.Property;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content
{
    internal enum ContentRowKind : byte
    {
        ObjectHeader,
        ComponentHeader,
        ContainerHeader,
        Property
    }

    internal readonly struct ContentRow
    {
        internal readonly ContentRowKind Kind;
        internal readonly ObjectContent ObjectContent;
        internal readonly PropertyContent PropertyContent;
        internal readonly int IndentLevel;
        internal readonly bool IsSpacer;
        internal readonly string GroupKey;
        internal readonly string GroupDisplayName;

        internal ContentRow(
            ContentRowKind kind,
            ObjectContent objectContent,
            PropertyContent propertyContent,
            int indentLevel = 0,
            bool isSpacer = false)
        {
            Kind = kind;
            ObjectContent = objectContent;
            PropertyContent = propertyContent;
            IndentLevel = indentLevel;
            IsSpacer = isSpacer;
            GroupKey = null;
            GroupDisplayName = null;
        }

        internal ContentRow(
            ContentRowKind kind,
            ObjectContent objectContent,
            string groupKey,
            string groupDisplayName,
            int indentLevel)
        {
            Kind = kind;
            ObjectContent = objectContent;
            PropertyContent = null;
            IndentLevel = indentLevel;
            IsSpacer = false;
            GroupKey = groupKey;
            GroupDisplayName = groupDisplayName;
        }
    }
}
