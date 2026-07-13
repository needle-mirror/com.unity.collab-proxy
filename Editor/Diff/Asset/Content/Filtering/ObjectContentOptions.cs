using System;
using System.Collections.Generic;

using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content.Filtering
{
    internal static class ObjectContentOptions
    {
        internal static List<MultiSelectDropdownItem> ExtractGameObjects(
            IList<ObjectContent> objectContents,
            Func<ObjectContent, UnityEngine.Texture> iconResolver)
        {
            List<MultiSelectDropdownItem> result = new List<MultiSelectDropdownItem>();
            HashSet<string> seen = new HashSet<string>();

            foreach (ObjectContent content in objectContents)
            {
                string name = content.GetObjectName();
                if (string.IsNullOrEmpty(name) || !seen.Add(name))
                    continue;

                result.Add(new MultiSelectDropdownItem(name, iconResolver(content)));
            }

            return result;
        }

        internal static List<MultiSelectDropdownItem> ExtractTypes(
            IList<ObjectContent> objectContents,
            Func<ObjectContent, UnityEngine.Texture> iconResolver)
        {
            List<MultiSelectDropdownItem> result = new List<MultiSelectDropdownItem>();
            HashSet<string> seen = new HashSet<string>();

            foreach (ObjectContent content in objectContents)
            {
                AddTypeIfPresent(content, result, seen, iconResolver);

                if (content.ComponentContents == null)
                    continue;

                foreach (ObjectContent comp in content.ComponentContents)
                    AddTypeIfPresent(comp, result, seen, iconResolver);
            }

            return result;
        }

        static void AddTypeIfPresent(
            ObjectContent content,
            List<MultiSelectDropdownItem> result,
            HashSet<string> seen,
            Func<ObjectContent, UnityEngine.Texture> iconResolver)
        {
            string typeName = content.GetTypeName();
            if (string.IsNullOrEmpty(typeName) || !seen.Add(typeName))
                return;

            result.Add(new MultiSelectDropdownItem(typeName, iconResolver(content)));
        }
    }
}
