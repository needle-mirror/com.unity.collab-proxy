using System.Collections.Generic;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content.Filtering
{
    internal static class ObjectContentFilters
    {
        internal static bool PassesGameObjectFilter(
            ObjectContent content, HashSet<string> selected)
        {
            if (selected == null)
                return true;

            string name = content.GetObjectName();
            return name != null && selected.Contains(name);
        }

        internal static bool PassesTypeFilter(
            ObjectContent content, HashSet<string> selected)
        {
            if (selected == null)
                return true;

            string typeName = content.GetTypeName();
            return typeName != null && selected.Contains(typeName);
        }
    }
}
