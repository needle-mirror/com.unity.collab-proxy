using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;
using Unity.PlasticSCM.Editor.UI;
using UnityEngine;
using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    internal class DiffPropertyRow : VisualElement
    {
        internal DiffPropertyRow()
        {
            CreateGUI();
        }

        internal void BindSpacer(PropertyDiff propDiff, int indentLevel)
        {
            DiffType diffType = propDiff.DiffType;
            bool hasSrc = diffType != DiffType.Added;
            bool hasDst = diffType != DiffType.Removed;

            float padding = PropertyRow.BASE_PADDING_LEFT
                + indentLevel * PropertyRow.INDENT_PX;
            Color gutterColor = DiffGutter.GetColor(diffType);

            DiffGutter.Apply(mLeftPanel, hasSrc ? gutterColor : Color.clear, padding);
            DiffGutter.Apply(mRightPanel, hasDst ? gutterColor : Color.clear, padding);

            mLeftPanel.style.backgroundColor = Color.clear;
            mRightPanel.style.backgroundColor = Color.clear;

            mLeftLabel.style.display = DisplayStyle.None;
            mRightLabel.style.display = DisplayStyle.None;

            mLeftValue.style.display = DisplayStyle.None;
            mRightValue.style.display = DisplayStyle.None;

            pickingMode = PickingMode.Ignore;
            mLeftPanel.pickingMode = PickingMode.Ignore;
            mRightPanel.pickingMode = PickingMode.Ignore;
        }

        internal void Bind(
            PropertyDiff propDiff,
            string searchFilter,
            int indentLevel)
        {
            DiffType diffType = propDiff.DiffType;
            bool hasSrc = diffType != DiffType.Added;
            bool hasDst = diffType != DiffType.Removed;

            float padding = PropertyRow.BASE_PADDING_LEFT
                + indentLevel * PropertyRow.INDENT_PX;
            Color gutterColor = DiffGutter.GetColor(diffType);

            DiffGutter.Apply(mLeftPanel, hasSrc ? gutterColor : Color.clear, padding);
            DiffGutter.Apply(mRightPanel, hasDst ? gutterColor : Color.clear, padding);

            ApplyPanelBackground(mLeftPanel, diffType, isLeftSide: true);
            ApplyPanelBackground(mRightPanel, diffType, isLeftSide: false);

            pickingMode = PickingMode.Position;
            mLeftPanel.pickingMode = PickingMode.Position;
            mRightPanel.pickingMode = PickingMode.Position;

            string label = propDiff.DisplayName ?? propDiff.Path;
            bool hasSearch = !string.IsNullOrEmpty(searchFilter);
            string displayLabel = hasSearch
                ? SearchHighlight.Apply(label, searchFilter)
                : label;

            PropertyRow.ConfigureNameLabel(
                mLeftLabel, hasSrc, hasSearch, displayLabel, propDiff.Path);
            PropertyRow.ConfigureNameLabel(
                mRightLabel, hasDst, hasSearch, displayLabel, propDiff.Path);

            mLeftValue.style.display = hasSrc
                ? DisplayStyle.Flex : DisplayStyle.None;
            mRightValue.style.display = hasDst
                ? DisplayStyle.Flex : DisplayStyle.None;

            mLeftValue.SetData(
                propDiff.SrcTag, propDiff.SrcValue, hasSrc,
                hasDst ? propDiff.DstTag : null);
            mRightValue.SetData(
                propDiff.DstTag, propDiff.DstValue, hasDst,
                hasSrc ? propDiff.SrcTag : null);
        }

        void CreateGUI()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;

            mLeftPanel = BuildPanel(out mLeftLabel, out mLeftValue);
            Add(mLeftPanel);

            mRightPanel = BuildPanel(out mRightLabel, out mRightValue);
            Add(mRightPanel);
        }

        static VisualElement BuildPanel(
            out Label nameLabel,
            out DiffValueDisplay valueDisplay)
        {
            VisualElement panel = PropertyRow.BuildPanel(out nameLabel);

            valueDisplay = new DiffValueDisplay();
            panel.Add(valueDisplay);

            return panel;
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
        Label mLeftLabel;
        DiffValueDisplay mLeftValue;

        VisualElement mRightPanel;
        Label mRightLabel;
        DiffValueDisplay mRightValue;
    }
}
