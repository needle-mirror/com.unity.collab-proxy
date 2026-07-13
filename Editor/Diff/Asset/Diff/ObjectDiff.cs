using System.Collections.Generic;

using PlasticGui;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    internal enum DiffType
    {
        Unchanged,
        Added,
        Removed,
        Modified
    }

    internal class ObjectDiff
    {
        internal UnityEngine.Object SrcObject;
        internal UnityEngine.Object DstObject;
        internal DiffType DiffType;
        internal DataLossKind DataLoss;
        internal PropertyDiffNode PropertyDiffTree;
        internal List<ObjectDiff> ComponentDiffs;

        // Component reorder annotation. 0 = no position change.
        // Positive = moved down N positions, negative = moved up N positions.
        internal int PositionDelta;

        // Reparent annotation. Both null = no parent change. Either non-null
        // means the GameObject was reparented; null on a side means that side
        // had it at the scene root (or as the prefab's top-level).
        internal UnityEngine.GameObject SrcParent;
        internal UnityEngine.GameObject DstParent;

        // Populated when this row represents a section of a .meta file —
        // the originating .meta filename (MetaFileName) and the nicified
        // top-level YAML key (MetaSectionName, e.g. "Audio Importer"),
        // analogous to a Component header in UnityObject diffs. MetaFileName
        // is the canonical "this is a meta row" flag (see IsMetaFileObject);
        // Src/DstObject are null on meta rows.
        internal string MetaFileName;
        internal string MetaSectionName;

        // Gated on MetaFileName rather than Src/DstObject being null, so we
        // don't misclassify a UnityObject row whose underlying native object
        // has been destroyed (Unity's overloaded == reports it null) as a
        // meta row.
        internal bool IsMetaFileObject
        {
            get { return !string.IsNullOrEmpty(MetaFileName); }
        }

        internal bool HasParentChange()
        {
            return SrcParent != null || DstParent != null;
        }

        internal int GetObjectId()
        {
            if (IsMetaFileObject)
                return (MetaFileName + ":" + MetaSectionName).GetHashCode();
            UnityEngine.Object obj = DstObject ?? SrcObject;
            return obj != null ? obj.GetObjectId() : 0;
        }

        internal string GetSrcDisplayName()
        {
            if (IsMetaFileObject)
                return GetMetaDisplayName();
            return BuildDisplayName(SrcObject ?? DstObject);
        }

        internal string GetDstDisplayName()
        {
            if (IsMetaFileObject)
                return GetMetaDisplayName();
            return BuildDisplayName(DstObject ?? SrcObject);
        }

        string GetMetaDisplayName()
        {
            return string.IsNullOrEmpty(MetaSectionName)
                ? MetaFileName
                : MetaSectionName;
        }

        // Components share their GameObject's name, so showing it would
        // duplicate the parent header. For everything else the icon already
        // conveys the type — show the object's own name.
        static string BuildDisplayName(UnityEngine.Object obj)
        {
            if (obj == null)
                return PlasticLocalization.Name.DiffUnknownObject.GetString();
            if (obj is UnityEngine.Component)
                return obj.GetType().Name;
            return string.IsNullOrEmpty(obj.name) ? obj.GetType().Name : obj.name;
        }

        internal string GetTypeName()
        {
            UnityEngine.Object obj = SrcObject ?? DstObject;
            return obj == null ? null : obj.GetType().Name;
        }

        internal string GetObjectName()
        {
            UnityEngine.Object obj = SrcObject ?? DstObject;
            return obj == null ? null : obj.name;
        }

        internal bool IsGameObject()
        {
            return (SrcObject ?? DstObject) is UnityEngine.GameObject;
        }

        internal UnityEngine.GameObject GetGameObject()
        {
            UnityEngine.Object obj = SrcObject ?? DstObject;
            if (obj is UnityEngine.GameObject go)
                return go;
            if (obj is UnityEngine.Component c)
                return c.gameObject;
            return null;
        }
    }
}
