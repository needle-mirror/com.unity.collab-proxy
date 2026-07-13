using System.Collections.Generic;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.Filtering
{
    internal static class ObjectDiffFilters
    {
        internal static bool PassesGameObjectFilter(
            ObjectDiff diff, HashSet<string> selected)
        {
            if (selected == null)
                return true;

            string name = diff.GetObjectName();
            return name != null && selected.Contains(name);
        }

        internal static bool PassesTypeFilter(
            ObjectDiff diff, HashSet<string> selected)
        {
            if (selected == null)
                return true;

            string typeName = diff.GetTypeName();
            return typeName != null && selected.Contains(typeName);
        }
    }
}
