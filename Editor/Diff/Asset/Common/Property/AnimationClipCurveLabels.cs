using System.Collections.Generic;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.Property
{
    // Replaces the generic "Curve" display name on AnimationClip curve leaves
    // with a label derived from the parent struct's `attribute` and `path`
    // fields, mirroring Unity's Animation window naming
    // (e.g. "Position.x", "Rotation.y (Spine/Head)").
    internal static class AnimationClipCurveLabels
    {
        internal static void TryDecorate(
            List<PropertyTreeNode> arrayElements, string arrayPath)
        {
            if (!IsCurveArray(arrayPath))
                return;

            string fallback = GetFallbackLabel(arrayPath);

            for (int i = 0; i < arrayElements.Count; i++)
            {
                PropertyTreeNode element = arrayElements[i];

                if (element.Kind != NodeKind.Object)
                    continue;

                PropertyTreeNode curveChild = null;
                string attribute = null;
                string targetPath = null;

                foreach (PropertyTreeNode child in element.Children)
                {
                    switch (child.Name)
                    {
                        case "curve": curveChild = child; break;
                        case "attribute": attribute = child.Value; break;
                        case "path": targetPath = child.Value; break;
                    }
                }

                if (curveChild == null || curveChild.Kind != NodeKind.Leaf)
                    continue;

                string label = BuildLabel(attribute, targetPath, fallback);

                arrayElements[i] = ReplaceChildDisplayName(element, curveChild, label);
            }
        }

        static bool IsCurveArray(string path)
        {
            switch (path)
            {
                case "m_PositionCurves":
                case "m_RotationCurves":
                case "m_ScaleCurves":
                case "m_EulerCurves":
                case "m_FloatCurves":
                case "m_PPtrCurves":
                case "m_EditorCurves":
                case "m_EulerEditorCurves":
                    return true;
                default:
                    return false;
            }
        }

        static string GetFallbackLabel(string arrayPath)
        {
            switch (arrayPath)
            {
                case "m_PositionCurves": return "Position";
                case "m_RotationCurves": return "Rotation";
                case "m_ScaleCurves": return "Scale";
                case "m_EulerCurves": return "Rotation (Euler)";
                case "m_EulerEditorCurves": return "Rotation (Euler)";
                default: return "Curve";
            }
        }

        static string BuildLabel(
            string attribute, string targetPath, string fallback)
        {
            string baseLabel = string.IsNullOrEmpty(attribute)
                ? fallback
                : StripAttributePrefix(attribute);

            if (string.IsNullOrEmpty(targetPath))
                return baseLabel;

            return baseLabel + " (" + targetPath + ")";
        }

        static string StripAttributePrefix(string attribute)
        {
            if (attribute.StartsWith("m_Local"))
                return attribute.Substring("m_Local".Length);

            if (attribute.StartsWith("m_"))
                return attribute.Substring(2);

            return attribute;
        }

        static PropertyTreeNode ReplaceChildDisplayName(
            PropertyTreeNode parent,
            PropertyTreeNode targetChild,
            string newDisplayName)
        {
            List<PropertyTreeNode> newChildren = new List<PropertyTreeNode>(
                parent.Children.Count);

            foreach (PropertyTreeNode child in parent.Children)
            {
                if (child == targetChild)
                {
                    newChildren.Add(PropertyTreeNode.CreateLeaf(
                        child.Name, child.Path, child.TypeTag, child.Value,
                        newDisplayName, child.Tag));
                    continue;
                }

                newChildren.Add(child);
            }

            return PropertyTreeNode.CreateObject(
                parent.Name, parent.Path, newChildren, parent.DisplayName, parent.Tag);
        }
    }
}
