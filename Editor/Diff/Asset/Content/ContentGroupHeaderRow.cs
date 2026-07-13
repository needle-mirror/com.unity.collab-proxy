using Unity.PlasticSCM.Editor.UI;
using UnityEngine;
using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content
{
    internal class ContentGroupHeaderRow : VisualElement
    {
        internal ContentGroupHeaderRow()
        {
            CreateGUI();
        }

        internal void Bind(
            string label, bool isExpanded, string searchFilter, int indentLevel)
        {
            float padding = PropertyRow.BASE_PADDING_LEFT
                + indentLevel * PropertyRow.INDENT_PX;

            mPanel.style.paddingLeft = padding;
            mPanel.style.backgroundColor =
                UnityStyles.Colors.Diff.AssetDiff.PropertyRowBackground;

            mArrow.text = isExpanded
                ? HeaderRow.ARROW_DOWN : HeaderRow.ARROW_RIGHT;

            bool hasSearch = !string.IsNullOrEmpty(searchFilter);
            mLabel.enableRichText = hasSearch;
            mLabel.text = hasSearch
                ? SearchHighlight.Apply(label ?? string.Empty, searchFilter)
                : label ?? string.Empty;
        }

        void CreateGUI()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;

            mPanel = new VisualElement();
            mPanel.style.flexGrow = 1;
            mPanel.style.flexDirection = FlexDirection.Row;
            mPanel.style.alignItems = Align.Center;
            mPanel.style.paddingLeft = PropertyRow.BASE_PADDING_LEFT;
            mPanel.style.paddingRight = 4;
            mPanel.style.paddingTop = 1;
            mPanel.style.paddingBottom = 1;

            mArrow = new Label(HeaderRow.ARROW_RIGHT);
            mArrow.style.width = 16;
            mArrow.style.fontSize = 10;
            mArrow.style.unityTextAlign = TextAnchor.MiddleCenter;
            mArrow.style.color = UnityStyles.Colors.Diff.AssetDiff.ExpanderColor;
            mPanel.Add(mArrow);

            mLabel = new Label();
            mLabel.style.fontSize = 12;
            mLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            mLabel.style.flexShrink = 1;
            mLabel.style.overflow = Overflow.Hidden;
            mLabel.style.textOverflow = TextOverflow.Ellipsis;
            mLabel.style.color = UnityStyles.Colors.DefaultText;
            mPanel.Add(mLabel);

            Add(mPanel);
        }

        VisualElement mPanel;
        Label mArrow;
        Label mLabel;
    }
}
