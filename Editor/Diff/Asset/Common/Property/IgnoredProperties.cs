namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.Property
{
    internal static class IgnoredProperties
    {
        internal static bool IsIgnored(string propertyPath)
        {
            // editor-only flags on every UnityEngine.Object
            if (propertyPath == "m_ObjectHideFlags")
                return true;

            if (propertyPath == "m_EditorHideFlags")
                return true;

            // internal class-mapping field on MonoBehaviours
            if (propertyPath == "m_EditorClassIdentifier")
                return true;

            // reference to the script asset itself
            if (propertyPath == "m_Script" || propertyPath.StartsWith("m_Script."))
                return true;

            // opaque serialized-reference IDs inside ObjectReference fields
            if (propertyPath.Contains(".m_FileID") || propertyPath.Contains(".m_PathID"))
                return true;

            // prefab system bookkeeping (includes legacy pre-2018.3 names)
            if (propertyPath == "m_CorrespondingSourceObject" ||
                propertyPath == "m_PrefabInstance" ||
                propertyPath == "m_PrefabAsset" ||
                propertyPath == "m_PrefabParentObject" ||
                propertyPath == "m_PrefabInternal")
                return true;

            // GameObject's component list — components are diffed individually
            if (propertyPath == "m_Component" || propertyPath.StartsWith("m_Component."))
                return true;

            // back-reference from Component to its owning GameObject
            if (propertyPath == "m_GameObject" || propertyPath.StartsWith("m_GameObject."))
                return true;

            // scene/prefab root list — structural, not property data
            if (propertyPath == "m_Roots" || propertyPath.StartsWith("m_Roots."))
                return true;

            // Transform hierarchy (parent/children/sibling order)
            if (propertyPath == "m_Children" || propertyPath.StartsWith("m_Children."))
                return true;
            if (propertyPath == "m_Father" || propertyPath.StartsWith("m_Father."))
                return true;
            if (propertyPath == "m_RootOrder")
                return true;

            // redundant editor hint — the real rotation is m_LocalRotation
            if (propertyPath.StartsWith("m_LocalEulerAnglesHint"))
                return true;

            // editor-only shadow data for animation curves
            if (propertyPath == "m_EditorCurves" || propertyPath.StartsWith("m_EditorCurves."))
                return true;
            if (propertyPath == "m_EulerEditorCurves" || propertyPath.StartsWith("m_EulerEditorCurves."))
                return true;

            return false;
        }
    }
}
