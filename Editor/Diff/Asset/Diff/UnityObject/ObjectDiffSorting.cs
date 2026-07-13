using System.Collections.Generic;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    internal static class ObjectDiffSorting
    {
        internal static int GetOrder(DiffType diffType)
        {
            switch (diffType)
            {
                case DiffType.Modified: return 0;
                case DiffType.Added: return 1;
                case DiffType.Removed: return 2;
                default: return 3;
            }
        }

        internal class PropertyDiffComparer : IComparer<PropertyDiff>
        {
            internal static readonly PropertyDiffComparer Instance = new PropertyDiffComparer();

            public int Compare(PropertyDiff x, PropertyDiff y)
            {
                return GetOrder(x.DiffType).CompareTo(GetOrder(y.DiffType));
            }
        }
    }
}
