using System.Collections.Generic;

using PlasticGui;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content
{
    internal class ObjectContent
    {
        internal UnityEngine.Object Object;
        internal PropertyTreeNode PropertyTree;
        internal List<ObjectContent> ComponentContents;

        // Populated when this row represents a section of a .meta file —
        // the originating .meta filename (MetaFileName) and the nicified
        // top-level YAML key (MetaSectionName, e.g. "Audio Importer"),
        // analogous to a Component header in UnityObject diffs. MetaFileName
        // is the canonical "this is a meta row" flag (see IsMetaFileObject);
        // Object is null on meta rows.
        internal string MetaFileName;
        internal string MetaSectionName;

        // Gated on MetaFileName rather than Object being null, so we don't
        // misclassify a UnityObject row whose underlying native object has
        // been destroyed (Unity's overloaded == reports it null) as a meta
        // row.
        internal bool IsMetaFileObject
        {
            get { return !string.IsNullOrEmpty(MetaFileName); }
        }

        internal int GetObjectId()
        {
            if (IsMetaFileObject)
                return (MetaFileName + ":" + MetaSectionName).GetHashCode();
            return Object != null ? Object.GetObjectId() : 0;
        }

        internal string GetDisplayName()
        {
            if (IsMetaFileObject)
                return GetMetaDisplayName();
            if (Object == null)
                return PlasticLocalization.Name.DiffUnknownObject.GetString();
            // Components share their GameObject's name, so showing it would
            // duplicate the parent header. For everything else the icon
            // already conveys the type — show the object's own name.
            if (Object is UnityEngine.Component)
                return Object.GetType().Name;
            return string.IsNullOrEmpty(Object.name) ? Object.GetType().Name : Object.name;
        }

        internal string GetTypeName()
        {
            return Object == null ? null : Object.GetType().Name;
        }

        internal string GetObjectName()
        {
            return Object == null ? null : Object.name;
        }

        internal bool IsGameObject()
        {
            return Object is UnityEngine.GameObject;
        }

        internal UnityEngine.GameObject GetGameObject()
        {
            if (Object is UnityEngine.GameObject go)
                return go;
            if (Object is UnityEngine.Component c)
                return c.gameObject;
            return null;
        }

        string GetMetaDisplayName()
        {
            return string.IsNullOrEmpty(MetaSectionName)
                ? MetaFileName
                : MetaSectionName;
        }
    }
}
