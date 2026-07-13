using Unity.PlasticSCM.Editor.UI;
using UnityEngine;
using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    internal class GroupHeaderRow : VisualElement
    {
        internal GroupHeaderRow()
        {
            CreateGUI();
        }

        internal void Bind(
            string label,
            DiffType groupDiffType,
            bool isExpanded,
            string searchFilter,
            int indentLevel)
        {
            bool hasSrc = groupDiffType != DiffType.Added;
            bool hasDst = groupDiffType != DiffType.Removed;

            float padding = PropertyRow.BASE_PADDING_LEFT
                + indentLevel * PropertyRow.INDENT_PX;
            Color gutterColor = DiffGutter.GetColor(groupDiffType);

            DiffGutter.Apply(mLeftPanel, hasSrc ? gutterColor : Color.clear, padding);
            DiffGutter.Apply(mRightPanel, hasDst ? gutterColor : Color.clear, padding);

            ApplyPanelBackground(mLeftPanel, groupDiffType, isLeftSide: true);
            ApplyPanelBackground(mRightPanel, groupDiffType, isLeftSide: false);

            string arrowText = isExpanded
                ? HeaderRow.ARROW_DOWN : HeaderRow.ARROW_RIGHT;

            bool hasSearch = !string.IsNullOrEmpty(searchFilter);
            string displayLabel = hasSearch
                ? SearchHighlight.Apply(label, searchFilter)
                : label;

            ConfigureSide(
                mLeftArrow, mLeftLabel, hasSrc, arrowText, displayLabel, hasSearch);
            ConfigureSide(
                mRightArrow, mRightLabel, hasDst, arrowText, displayLabel, hasSearch);
        }

        void CreateGUI()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;

            mLeftPanel = BuildPanel(out mLeftArrow, out mLeftLabel);
            Add(mLeftPanel);

            mRightPanel = BuildPanel(out mRightArrow, out mRightLabel);
            Add(mRightPanel);
        }

        static VisualElement BuildPanel(out Label arrow, out Label label)
        {
            VisualElement panel = new VisualElement();
            panel.style.flexGrow = 1;
            panel.style.flexBasis = 0;
            panel.style.flexDirection = FlexDirection.Row;
            panel.style.alignItems = Align.Center;
            panel.style.paddingLeft = PropertyRow.BASE_PADDING_LEFT;
            panel.style.paddingRight = 4;
            panel.style.paddingTop = 1;
            panel.style.paddingBottom = 1;

            arrow = new Label(HeaderRow.ARROW_RIGHT);
            arrow.style.width = 16;
            arrow.style.fontSize = 10;
            arrow.style.unityTextAlign = TextAnchor.MiddleCenter;
            arrow.style.color = UnityStyles.Colors.Diff.AssetDiff.ExpanderColor;
            panel.Add(arrow);

            label = new Label();
            label.style.fontSize = 12;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.flexShrink = 1;
            label.style.overflow = Overflow.Hidden;
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.color = UnityStyles.Colors.DefaultText;
            panel.Add(label);

            return panel;
        }

        static void ConfigureSide(
            Label arrow, Label label, bool hasContent,
            string arrowText, string text, bool enableRichText)
        {
            arrow.text = arrowText;
            arrow.style.display = hasContent
                ? DisplayStyle.Flex : DisplayStyle.None;

            label.enableRichText = enableRichText;
            label.text = hasContent ? text : string.Empty;
            label.style.display = hasContent
                ? DisplayStyle.Flex : DisplayStyle.None;
        }

        static void ApplyPanelBackground(
            VisualElement panel,
            DiffType diffType,
            bool isLeftSide)
        {
            bool isEmpty =
                (diffType == DiffType.Added && isLeftSide) ||
                (diffType == DiffType.Removed && !isLeftSide);

            panel.style.backgroundColor = isEmpty
                ? UnityStyles.Colors.Diff.AssetDiff.EmptyPanelBackground
                : UnityStyles.Colors.Diff.AssetDiff.PropertyRowBackground;
        }

        VisualElement mLeftPanel;
        Label mLeftArrow;
        Label mLeftLabel;

        VisualElement mRightPanel;
        Label mRightArrow;
        Label mRightLabel;
    }
}
