using Unity.PlasticSCM.Editor.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common
{
    internal static class HeaderRow
    {
        internal const float BASE_PADDING_LEFT = 4f;
        internal const float INDENT_PX = 16f;
        internal const string ARROW_RIGHT = "▶";
        internal const string ARROW_DOWN = "▼";

        internal static VisualElement BuildPanel(
            out VisualElement content,
            out Label arrow,
            out Image icon,
            out Label nameLabel,
            out Label annotationLabel)
        {
            VisualElement panel = new VisualElement();
            panel.style.flexGrow = 1;
            panel.style.flexBasis = 0;
            panel.style.flexDirection = FlexDirection.Row;

            content = new VisualElement();
            content.style.flexGrow = 1;
            content.style.flexDirection = FlexDirection.Row;
            content.style.alignItems = Align.Center;
            content.style.borderTopWidth = 1;
            content.style.borderBottomWidth = 1;
            panel.Add(content);

            arrow = new Label(ARROW_RIGHT);
            arrow.style.width = 16;
            arrow.style.fontSize = 10;
            arrow.style.unityTextAlign = TextAnchor.MiddleCenter;
            arrow.style.color = UnityStyles.Colors.Diff.AssetDiff.ExpanderColor;
            content.Add(arrow);

            icon = new Image();
            icon.style.width = 16;
            icon.style.height = 16;
            icon.style.marginRight = 4;
            content.Add(icon);

            nameLabel = new Label();
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.fontSize = 12;
            nameLabel.style.flexShrink = 1;
            nameLabel.style.overflow = Overflow.Hidden;
            nameLabel.style.textOverflow = TextOverflow.Ellipsis;
            content.Add(nameLabel);

            annotationLabel = new Label();
            annotationLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            annotationLabel.style.fontSize = 12;
            annotationLabel.style.marginLeft = 8;
            annotationLabel.style.flexShrink = 0;
            annotationLabel.style.color = UnityStyles.Colors.Diff.AssetDiff.AnnotationText;
            annotationLabel.style.display = DisplayStyle.None;
            content.Add(annotationLabel);

            return panel;
        }

        internal static void ConfigurePanel(
            Label arrow,
            Image icon,
            Label nameLabel,
            Label annotationLabel,
            VisualElement panel,
            bool hasContent,
            string arrowText,
            Color panelBg,
            UnityEngine.Texture objectIcon,
            string displayName,
            string searchFilter,
            string annotation = null,
            bool isExpandable = true)
        {
            panel.style.backgroundColor = panelBg;

            arrow.text = arrowText;
            arrow.style.display = (hasContent && isExpandable)
                ? DisplayStyle.Flex : DisplayStyle.None;

            icon.image = hasContent ? objectIcon : null;
            icon.style.display = hasContent
                ? DisplayStyle.Flex : DisplayStyle.None;

            bool hasSearch = !string.IsNullOrEmpty(searchFilter);

            nameLabel.enableRichText = hasSearch;
            nameLabel.text = hasSearch
                ? SearchHighlight.Apply(displayName, searchFilter)
                : displayName;

            nameLabel.style.display = hasContent
                ? DisplayStyle.Flex : DisplayStyle.None;
            nameLabel.style.color = hasContent
                ? UnityStyles.Colors.DefaultText
                : UnityStyles.Colors.DefaultTextDisabled;

            bool hasAnnotation = hasContent && !string.IsNullOrEmpty(annotation);
            annotationLabel.style.display = hasAnnotation
                ? DisplayStyle.Flex : DisplayStyle.None;
            if (hasAnnotation)
                annotationLabel.text = annotation;
        }

        internal static void ApplyContentBorder(
            VisualElement content,
            bool hasContent,
            float paddingLeft,
            bool isExpanded,
            bool suppressBottomBorder = false)
        {
            content.style.paddingLeft = paddingLeft;
            content.style.borderTopColor = hasContent
                ? UnityStyles.Colors.Diff.AssetDiff.HeaderBorder
                : Color.clear;
            content.style.borderBottomColor = hasContent && isExpanded && !suppressBottomBorder
                ? UnityStyles.Colors.Diff.AssetDiff.HeaderBorderLight
                : Color.clear;
        }

        internal static Color GetBackgroundColor(bool hasContent, bool bIsHovered)
        {
            if (!hasContent)
                return UnityStyles.Colors.Diff.AssetDiff.EmptyPanelBackground;

            return bIsHovered
                ? UnityStyles.Colors.Diff.AssetDiff.ObjectHeaderBackgroundHovered
                : UnityStyles.Colors.Diff.AssetDiff.ObjectHeaderBackground;
        }

        internal static void SetHovered(
            ref bool sideHovered,
            bool hasContent,
            VisualElement panel,
            bool bIsHovered)
        {
            if (!hasContent || sideHovered == bIsHovered)
                return;

            sideHovered = bIsHovered;
            panel.style.backgroundColor = GetBackgroundColor(hasContent, bIsHovered);
        }
    }
}
