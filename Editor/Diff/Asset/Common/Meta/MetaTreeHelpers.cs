using System;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.Meta
{
    internal static class MetaTreeHelpers
    {
        internal static PropertyTreeNode FilterIgnoredKeys(PropertyTreeNode root)
        {
            List<PropertyTreeNode> filteredChildren = new List<PropertyTreeNode>();

            foreach (PropertyTreeNode child in root.Children)
            {
                if (!IGNORED_TOP_LEVEL_KEYS.Contains(child.Name))
                    filteredChildren.Add(child);
            }

            return PropertyTreeNode.CreateRoot(filteredChildren);
        }

        static readonly HashSet<string> IGNORED_TOP_LEVEL_KEYS =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "guid",
                "timeCreated",
                "licenseType"
            };
    }
}
