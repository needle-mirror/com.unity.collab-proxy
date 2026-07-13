using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Codice.LogWrapper;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.Meta;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content.Meta
{
    internal static class MetaFileContents
    {
        internal static List<ObjectContent> BuildContentData(string file)
        {
            Stopwatch sw = Stopwatch.StartNew();

            string metaFileName = Path.GetFileName(file);

            PropertyTreeNode tree = MetaPropertyTreeBuilder.Build(
                File.ReadAllBytes(file));
            PropertyTreeNode filteredTree = MetaTreeHelpers.FilterIgnoredKeys(tree);

            if (filteredTree.Children.Count == 0)
                return EmptyList;

            // Each top-level YAML key becomes its own ObjectContent — analogous
            // to a Component in a UnityObject diff. For an Object/Array key
            // the section's tree carries the key's children directly (the
            // "AudioImporter" wrapper is hoisted into the section header so
            // it doesn't duplicate as the first child row).
            List<ObjectContent> result = new List<ObjectContent>(
                filteredTree.Children.Count);
            foreach (PropertyTreeNode section in filteredTree.Children)
            {
                result.Add(new ObjectContent
                {
                    MetaFileName = metaFileName,
                    MetaSectionName = section.DisplayName,
                    PropertyTree = WrapAsSectionRoot(section)
                });
            }

            mLog.DebugFormat(
                "{0} meta sections calculated in {1} ms for '{2}'",
                result.Count, sw.ElapsedMilliseconds, metaFileName);

            return result;
        }

        static PropertyTreeNode WrapAsSectionRoot(PropertyTreeNode section)
        {
            if (section.Kind == NodeKind.Leaf)
            {
                return PropertyTreeNode.CreateRoot(
                    new List<PropertyTreeNode> { section });
            }

            return PropertyTreeNode.CreateRoot(
                new List<PropertyTreeNode>(section.Children));
        }

        static readonly List<ObjectContent> EmptyList =
            new List<ObjectContent>();

        static readonly ILog mLog = PlasticApp.GetLogger("MetaFileContents");
    }
}
