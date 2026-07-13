using UnityEditor;
using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common
{
    internal static class PropertyRow
    {
        internal const float BASE_PADDING_LEFT = 20f;
        internal const float INDENT_PX = 16f;
        internal const float LABEL_WIDTH = 180f;
        internal const float LABEL_MIN_WIDTH = 60f;

        internal static bool IsTallProperty(object tag)
        {
            if (!(tag is LeafPropertyData leaf))
                return false;

            switch (leaf.PropertyType)
            {
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.BoundsInt:
                    return true;
                default:
                    return false;
            }
        }

        internal static VisualElement BuildPanel(out Label nameLabel)
        {
            VisualElement panel = new VisualElement();
            panel.style.flexGrow = 1;
            panel.style.flexBasis = 0;
            panel.style.flexDirection = FlexDirection.Row;
            panel.style.alignItems = Align.FlexStart;
            panel.style.paddingLeft = BASE_PADDING_LEFT;
            panel.style.paddingRight = 4;
            panel.style.paddingTop = 3;
            panel.style.paddingBottom = 1;

            nameLabel = new Label();
            nameLabel.style.width = LABEL_WIDTH;
            nameLabel.style.minWidth = LABEL_MIN_WIDTH;
            nameLabel.style.fontSize = 12;
            nameLabel.style.overflow = Overflow.Hidden;
            nameLabel.style.textOverflow = TextOverflow.Ellipsis;
            nameLabel.style.color = UnityStyles.Colors.DefaultText;
            panel.Add(nameLabel);

            return panel;
        }

        internal static void ConfigureNameLabel(
            Label label,
            bool hasContent,
            bool hasSearch,
            string displayLabel,
            string tooltipPath)
        {
            label.enableRichText = hasSearch;
            label.text = hasContent ? displayLabel : string.Empty;
            label.tooltip = hasContent ? tooltipPath : string.Empty;
            label.style.display = hasContent
                ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
