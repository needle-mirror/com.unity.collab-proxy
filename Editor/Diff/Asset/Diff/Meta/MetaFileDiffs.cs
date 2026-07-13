using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Codice.LogWrapper;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.Meta;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.Meta
{
    internal static class MetaFileDiffs
    {
        internal static List<ObjectDiff> BuildDiffData(
            string srcFile,
            string dstFile)
        {
            Stopwatch sw = Stopwatch.StartNew();

            string metaFileName = Path.GetFileName(srcFile);

            PropertyTreeNode filteredSrc = MetaTreeHelpers.FilterIgnoredKeys(
                MetaPropertyTreeBuilder.Build(File.ReadAllBytes(srcFile)));
            PropertyTreeNode filteredDst = MetaTreeHelpers.FilterIgnoredKeys(
                MetaPropertyTreeBuilder.Build(File.ReadAllBytes(dstFile)));

            PropertyDiffNode diffTree = PropertyDiffs.BuildDiffTree(
                filteredSrc, filteredDst);

            if (diffTree == null || diffTree.Children.Count == 0)
                return EmptyList;

            // Each top-level YAML key that changed becomes its own ObjectDiff
            // — analogous to a Component in a UnityObject diff. The section's
            // diff tree carries the key's children directly so the renderer
            // doesn't duplicate "AudioImporter" as both the section header and
            // the first child row.
            List<ObjectDiff> result = new List<ObjectDiff>(
                diffTree.Children.Count);
            foreach (PropertyDiffNode section in diffTree.Children)
            {
                result.Add(new ObjectDiff
                {
                    MetaFileName = metaFileName,
                    MetaSectionName = section.DisplayName,
                    DiffType = section.DiffType,
                    PropertyDiffTree = WrapAsSectionRoot(section)
                });
            }

            mLog.DebugFormat(
                "{0} meta diff sections calculated in {1} ms for '{2}'",
                result.Count, sw.ElapsedMilliseconds, metaFileName);

            return result;
        }

        static PropertyDiffNode WrapAsSectionRoot(PropertyDiffNode section)
        {
            if (section.Kind == NodeKind.Leaf)
            {
                return PropertyDiffNode.CreateRoot(
                    new List<PropertyDiffNode> { section });
            }

            return PropertyDiffNode.CreateRoot(
                new List<PropertyDiffNode>(section.Children));
        }

        static readonly List<ObjectDiff> EmptyList =
            new List<ObjectDiff>();

        static readonly ILog mLog = PlasticApp.GetLogger("MetaFileDiffs");
    }
}
