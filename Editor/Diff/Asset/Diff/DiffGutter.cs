using Unity.PlasticSCM.Editor.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    internal static class DiffGutter
    {
        internal static Color GetColor(DiffType diffType)
        {
            switch (diffType)
            {
                case DiffType.Added:
                    return UnityStyles.Colors.Diff.AssetDiff.GutterAdded;
                case DiffType.Removed:
                    return UnityStyles.Colors.Diff.AssetDiff.GutterRemoved;
                case DiffType.Modified:
                    return UnityStyles.Colors.Diff.AssetDiff.GutterModified;
                default:
                    return Color.clear;
            }
        }

        internal static void Apply(
            VisualElement panel, Color color, float paddingLeft)
        {
            SetColor(panel, color);
            panel.style.paddingLeft = paddingLeft;
        }

        internal static void SetColor(VisualElement panel, Color color)
        {
            panel.style.borderLeftWidth = GUTTER_WIDTH;
            panel.style.borderLeftColor = color;
        }

        const float GUTTER_WIDTH = 3f;
    }
}
