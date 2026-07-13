using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    internal enum DiffRowKind : byte
    {
        ObjectHeader,
        ComponentHeader,
        ContainerHeader,
        PropertyDiff
    }

    internal readonly struct DiffRow
    {
        internal readonly DiffRowKind Kind;
        internal readonly ObjectDiff ObjectDiff;
        internal readonly PropertyDiff PropertyDiff;
        internal readonly int IndentLevel;
        internal readonly bool IsSpacer;
        internal readonly string GroupKey;
        internal readonly string GroupDisplayName;
        internal readonly DiffType GroupDiffType;

        internal DiffRow(
            DiffRowKind kind,
            ObjectDiff objectDiff,
            PropertyDiff propertyDiff,
            int indentLevel = 0,
            bool isSpacer = false)
        {
            Kind = kind;
            ObjectDiff = objectDiff;
            PropertyDiff = propertyDiff;
            IndentLevel = indentLevel;
            IsSpacer = isSpacer;
            GroupKey = null;
            GroupDisplayName = null;
            GroupDiffType = DiffType.Unchanged;
        }

        internal DiffRow(
            DiffRowKind kind,
            ObjectDiff objectDiff,
            string groupKey,
            string groupDisplayName,
            DiffType groupDiffType,
            int indentLevel)
        {
            Kind = kind;
            ObjectDiff = objectDiff;
            PropertyDiff = null;
            IndentLevel = indentLevel;
            IsSpacer = false;
            GroupKey = groupKey;
            GroupDisplayName = groupDisplayName;
            GroupDiffType = groupDiffType;
        }
    }
}
