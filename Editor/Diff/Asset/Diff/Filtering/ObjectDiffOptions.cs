using System;
using System.Collections.Generic;

using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.Filtering
{
    internal static class ObjectDiffOptions
    {
        internal static List<MultiSelectDropdownItem> ExtractGameObjects(
            IList<ObjectDiff> objectDiffs,
            Func<ObjectDiff, UnityEngine.Texture> iconResolver)
        {
            List<MultiSelectDropdownItem> result = new List<MultiSelectDropdownItem>();
            HashSet<string> seen = new HashSet<string>();

            foreach (ObjectDiff diff in objectDiffs)
            {
                string name = diff.GetObjectName();
                if (string.IsNullOrEmpty(name) || !seen.Add(name))
                    continue;

                result.Add(new MultiSelectDropdownItem(name, iconResolver(diff)));
            }

            return result;
        }

        internal static List<MultiSelectDropdownItem> ExtractTypes(
            IList<ObjectDiff> objectDiffs,
            Func<ObjectDiff, UnityEngine.Texture> iconResolver)
        {
            List<MultiSelectDropdownItem> result = new List<MultiSelectDropdownItem>();
            HashSet<string> seen = new HashSet<string>();

            foreach (ObjectDiff diff in objectDiffs)
            {
                AddTypeIfPresent(diff, result, seen, iconResolver);

                if (diff.ComponentDiffs == null)
                    continue;

                foreach (ObjectDiff comp in diff.ComponentDiffs)
                    AddTypeIfPresent(comp, result, seen, iconResolver);
            }

            return result;
        }

        static void AddTypeIfPresent(
            ObjectDiff diff,
            List<MultiSelectDropdownItem> result,
            HashSet<string> seen,
            Func<ObjectDiff, UnityEngine.Texture> iconResolver)
        {
            if (diff.DiffType == DiffType.Unchanged)
                return;

            string typeName = diff.GetTypeName();
            if (string.IsNullOrEmpty(typeName) || !seen.Add(typeName))
                return;

            result.Add(new MultiSelectDropdownItem(typeName, iconResolver(diff)));
        }
    }
}
