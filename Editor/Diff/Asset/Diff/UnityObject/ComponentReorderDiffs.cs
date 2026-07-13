using System.Collections.Generic;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    // Detects component reorders inside each paired GameObject. Components
    // are matched by type-name labels (with #N suffix to disambiguate
    // duplicates), and "common" labels — present on both sides — define the
    // reference order. Pure moves emit a Modified ObjectDiff with no
    // PropertyDiffs and a non-zero PositionDelta.
    internal static class ComponentReorderDiffs
    {
        internal static void Append(
            Dictionary<long, UnityEngine.Object> srcById,
            Dictionary<long, UnityEngine.Object> dstById,
            List<ObjectDiff> objectDiffs)
        {
            // Index existing Component diffs so a reorder annotation can update
            // them in place instead of creating a duplicate row.
            Dictionary<Component, ObjectDiff> bySrcComponent =
                new Dictionary<Component, ObjectDiff>();
            Dictionary<Component, ObjectDiff> byDstComponent =
                new Dictionary<Component, ObjectDiff>();

            foreach (ObjectDiff diff in objectDiffs)
            {
                if (diff.SrcObject is Component sc) bySrcComponent[sc] = diff;
                if (diff.DstObject is Component dc) byDstComponent[dc] = diff;
            }

            foreach (KeyValuePair<long, UnityEngine.Object> kvp in srcById)
            {
                if (!(kvp.Value is GameObject srcGo))
                    continue;
                if (!dstById.TryGetValue(kvp.Key, out UnityEngine.Object dstObj))
                    continue;
                if (!(dstObj is GameObject dstGo))
                    continue;

                AppendReordersForGameObjectPair(
                    srcGo, dstGo,
                    bySrcComponent, byDstComponent,
                    objectDiffs);
            }
        }

        static void AppendReordersForGameObjectPair(
            GameObject srcGo,
            GameObject dstGo,
            Dictionary<Component, ObjectDiff> bySrcComponent,
            Dictionary<Component, ObjectDiff> byDstComponent,
            List<ObjectDiff> objectDiffs)
        {
            List<LabeledComponent> srcLabeled = BuildLabeledComponents(
                srcGo.GetComponents<Component>());
            List<LabeledComponent> dstLabeled = BuildLabeledComponents(
                dstGo.GetComponents<Component>());

            HashSet<string> srcLabelSet = new HashSet<string>();
            foreach (LabeledComponent lc in srcLabeled)
                srcLabelSet.Add(lc.Label);

            HashSet<string> common = new HashSet<string>();
            foreach (LabeledComponent lc in dstLabeled)
            {
                if (srcLabelSet.Contains(lc.Label))
                    common.Add(lc.Label);
            }

            if (common.Count < 2)
                return;

            // Position is measured against the relative order of components
            // present on both sides — additions and removals don't count as
            // "everyone moved".
            List<LabeledComponent> srcCommon = FilterByCommon(srcLabeled, common);
            List<LabeledComponent> dstCommon = FilterByCommon(dstLabeled, common);

            Dictionary<string, int> srcPos = BuildPositionIndex(srcCommon);
            Dictionary<string, int> dstPos = BuildPositionIndex(dstCommon);

            foreach (string label in common)
            {
                int delta = dstPos[label] - srcPos[label];
                if (delta == 0)
                    continue;

                Component srcComp = srcCommon[srcPos[label]].Component;
                Component dstComp = dstCommon[dstPos[label]].Component;

                ObjectDiff existing = null;
                if (srcComp != null) bySrcComponent.TryGetValue(srcComp, out existing);
                if (existing == null && dstComp != null)
                    byDstComponent.TryGetValue(dstComp, out existing);

                if (existing != null)
                {
                    existing.PositionDelta = delta;
                    continue;
                }

                // Component moved but its content is unchanged — emit a
                // Modified diff with no PropertyDiffs so the row still appears.
                objectDiffs.Add(new ObjectDiff
                {
                    SrcObject = srcComp,
                    DstObject = dstComp,
                    DiffType = DiffType.Modified,
                    PositionDelta = delta
                });
            }
        }

        static List<LabeledComponent> BuildLabeledComponents(Component[] components)
        {
            List<LabeledComponent> result = new List<LabeledComponent>(components.Length);
            Dictionary<string, int> seen = new Dictionary<string, int>();

            foreach (Component c in components)
            {
                string typeName = c == null ? "<missing>" : c.GetType().Name;

                string label = seen.TryGetValue(typeName, out int n)
                    ? typeName + "#" + (n + 1)
                    : typeName;

                seen[typeName] = n + 1;

                result.Add(new LabeledComponent { Label = label, Component = c });
            }

            return result;
        }

        static List<LabeledComponent> FilterByCommon(
            List<LabeledComponent> labeled, HashSet<string> common)
        {
            List<LabeledComponent> result = new List<LabeledComponent>(labeled.Count);
            foreach (LabeledComponent lc in labeled)
            {
                if (common.Contains(lc.Label))
                    result.Add(lc);
            }
            return result;
        }

        static Dictionary<string, int> BuildPositionIndex(List<LabeledComponent> labeled)
        {
            Dictionary<string, int> result = new Dictionary<string, int>(labeled.Count);
            for (int i = 0; i < labeled.Count; i++)
                result[labeled[i].Label] = i;
            return result;
        }

        struct LabeledComponent
        {
            internal string Label;
            internal Component Component;
        }
    }
}
