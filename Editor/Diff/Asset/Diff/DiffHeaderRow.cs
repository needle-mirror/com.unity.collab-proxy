using PlasticGui;
using UnityEngine;
using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    internal class DiffHeaderRow : VisualElement
    {
        internal DiffHeaderRow()
        {
            CreateGUI();
        }

        internal void Bind(
            ObjectDiff objDiff,
            bool isExpanded,
            string searchFilter,
            int indentLevel)
        {
            // Container GameObjects (no own changes, only nested component
            // changes) are stored as Unchanged so they don't inflate status
            // counts, but visually they should display like Modified.
            DiffType displayType = IsContainer(objDiff)
                ? DiffType.Modified
                : objDiff.DiffType;

            mHasSrc = displayType != DiffType.Added;
            mHasDst = displayType != DiffType.Removed;
            mbIsLeftHovered = false;
            mbIsRightHovered = false;

            float padding = HeaderRow.BASE_PADDING_LEFT
                + indentLevel * HeaderRow.INDENT_PX;
            Color gutterColor = DiffGutter.GetColor(displayType);

            DiffGutter.SetColor(mLeftPanel, mHasSrc ? gutterColor : Color.clear);
            DiffGutter.SetColor(mRightPanel, mHasDst ? gutterColor : Color.clear);

            bool suppressBottomBorder = objDiff.IsGameObject();
            HeaderRow.ApplyContentBorder(mLeftContent, mHasSrc, padding, isExpanded, suppressBottomBorder);
            HeaderRow.ApplyContentBorder(mRightContent, mHasDst, padding, isExpanded, suppressBottomBorder);

            string arrowText = isExpanded
                ? HeaderRow.ARROW_DOWN : HeaderRow.ARROW_RIGHT;
            UnityEngine.Texture icon = ObjectIconResolver.GetIcon(objDiff);

            string leftAnnotation = BuildLeftAnnotation(objDiff);
            string rightAnnotation = BuildRightAnnotation(objDiff);
            bool isExpandable =
                (objDiff.PropertyDiffTree != null
                    && objDiff.PropertyDiffTree.Children.Count > 0)
                || (objDiff.ComponentDiffs != null && objDiff.ComponentDiffs.Count > 0);

            HeaderRow.ConfigurePanel(
                mLeftArrow, mLeftIcon, mLeftLabel, mLeftAnnotation, mLeftPanel,
                mHasSrc, arrowText,
                HeaderRow.GetBackgroundColor(mHasSrc, mbIsLeftHovered), icon,
                mHasSrc ? objDiff.GetSrcDisplayName() : string.Empty,
                searchFilter,
                leftAnnotation,
                isExpandable);

            HeaderRow.ConfigurePanel(
                mRightArrow, mRightIcon, mRightLabel, mRightAnnotation, mRightPanel,
                mHasDst, arrowText,
                HeaderRow.GetBackgroundColor(mHasDst, mbIsRightHovered), icon,
                mHasDst ? objDiff.GetDstDisplayName() : string.Empty,
                searchFilter,
                rightAnnotation,
                isExpandable);

            ApplyDataLossTooltip(objDiff, mLeftAnnotation, mRightAnnotation);
        }

        // The annotation Label is hardcoded italic in HeaderRow.BuildPanel
        // — fine for "moved up 1 position" but visually off for the data-
        // loss glyph. Toggle the font style + add the tooltip in one pass
        // so the two stay in sync.
        static void ApplyDataLossTooltip(
            ObjectDiff objDiff, Label leftAnnotation, Label rightAnnotation)
        {
            DataLossKind kind = GetEffectiveDataLoss(objDiff);
            bool showingDataLoss = kind != DataLossKind.None;

            string tooltip = showingDataLoss
                ? DataLossDescriptions.GetRowTooltip(kind)
                : string.Empty;
            FontStyle fontStyle = showingDataLoss
                ? FontStyle.Normal
                : FontStyle.Italic;

            leftAnnotation.tooltip = tooltip;
            leftAnnotation.style.unityFontStyleAndWeight = fontStyle;

            rightAnnotation.tooltip = tooltip;
            rightAnnotation.style.unityFontStyleAndWeight = fontStyle;
        }

        // Reparenting and sibling reorder are mutually exclusive on the same
        // GameObject — see ParentChangeDiffs / SiblingReorderDiffs.
        // Reparent annotation is per-side (prior location on src, new location
        // on dst); sibling reorder uses the same delta-based annotation on both.
        //
        // Data-loss takes precedence over reorder/reparent text — the
        // user needs to know the diff is incomplete before they reason
        // about position changes. The two rarely co-occur in practice
        // (broken objects don't typically get reordered alongside their
        // breakage). The annotation slot then carries a short warning
        // glyph and the full description goes on the Label's tooltip
        // (set in Bind after ConfigurePanel runs).
        static string BuildLeftAnnotation(ObjectDiff objDiff)
        {
            if (GetEffectiveDataLoss(objDiff) != DataLossKind.None)
                return DATA_LOSS_GLYPH;

            if (objDiff.HasParentChange())
                return BuildParentLocationAnnotation(objDiff.SrcParent, isDst: false);
            return BuildPositionAnnotation(objDiff.PositionDelta);
        }

        static string BuildRightAnnotation(ObjectDiff objDiff)
        {
            if (GetEffectiveDataLoss(objDiff) != DataLossKind.None)
                return DATA_LOSS_GLYPH;

            if (objDiff.HasParentChange())
                return BuildParentLocationAnnotation(objDiff.DstParent, isDst: true);
            return BuildPositionAnnotation(objDiff.PositionDelta);
        }

        // Returns this row's own DataLoss when set, otherwise the worst
        // DataLoss across nested ComponentDiffs. This lets a collapsed
        // GameObject container surface the warning when any nested
        // component is broken — the user shouldn't have to expand every
        // row to discover where data was dropped.
        static DataLossKind GetEffectiveDataLoss(ObjectDiff objDiff)
        {
            if (objDiff.DataLoss != DataLossKind.None)
                return objDiff.DataLoss;

            if (objDiff.ComponentDiffs == null)
                return DataLossKind.None;

            DataLossKind worst = DataLossKind.None;
            foreach (ObjectDiff comp in objDiff.ComponentDiffs)
            {
                if ((int)comp.DataLoss > (int)worst)
                    worst = comp.DataLoss;
            }
            return worst;
        }

        static string BuildParentLocationAnnotation(
            UnityEngine.GameObject parent, bool isDst)
        {
            string parentLabel = parent == null
                ? PlasticLocalization.Name.DiffSceneRoot.GetString()
                : "'" + parent.name + "'";
            return isDst
                ? PlasticLocalization.Name.DiffMovedToLocation.GetString(parentLabel)
                : PlasticLocalization.Name.DiffInLocation.GetString(parentLabel);
        }

        static bool IsContainer(ObjectDiff objDiff)
        {
            return objDiff.DiffType == DiffType.Unchanged
                && objDiff.ComponentDiffs != null
                && objDiff.ComponentDiffs.Count > 0;
        }

        static string BuildPositionAnnotation(int delta)
        {
            if (delta == 0)
                return null;

            int abs = delta < 0 ? -delta : delta;

            if (delta < 0)
            {
                return abs == 1
                    ? PlasticLocalization.Name.DiffMovedUpOnePosition.GetString(abs)
                    : PlasticLocalization.Name.DiffMovedUpPositions.GetString(abs);
            }

            return abs == 1
                ? PlasticLocalization.Name.DiffMovedDownOnePosition.GetString(abs)
                : PlasticLocalization.Name.DiffMovedDownPositions.GetString(abs);
        }

        void CreateGUI()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;

            mLeftPanel = HeaderRow.BuildPanel(
                out mLeftContent, out mLeftArrow, out mLeftIcon,
                out mLeftLabel, out mLeftAnnotation);
            mLeftPanel.RegisterCallback<MouseEnterEvent>(OnLeftPanelMouseEnter);
            mLeftPanel.RegisterCallback<MouseLeaveEvent>(OnLeftPanelMouseLeave);
            Add(mLeftPanel);

            mRightPanel = HeaderRow.BuildPanel(
                out mRightContent, out mRightArrow, out mRightIcon,
                out mRightLabel, out mRightAnnotation);
            mRightPanel.RegisterCallback<MouseEnterEvent>(OnRightPanelMouseEnter);
            mRightPanel.RegisterCallback<MouseLeaveEvent>(OnRightPanelMouseLeave);
            Add(mRightPanel);
        }

        void OnLeftPanelMouseEnter(MouseEnterEvent evt)
        {
            HeaderRow.SetHovered(
                ref mbIsLeftHovered, mHasSrc, mLeftPanel, bIsHovered: true);
        }

        void OnLeftPanelMouseLeave(MouseLeaveEvent evt)
        {
            HeaderRow.SetHovered(
                ref mbIsLeftHovered, mHasSrc, mLeftPanel, bIsHovered: false);
        }

        void OnRightPanelMouseEnter(MouseEnterEvent evt)
        {
            HeaderRow.SetHovered(
                ref mbIsRightHovered, mHasDst, mRightPanel, bIsHovered: true);
        }

        void OnRightPanelMouseLeave(MouseLeaveEvent evt)
        {
            HeaderRow.SetHovered(
                ref mbIsRightHovered, mHasDst, mRightPanel, bIsHovered: false);
        }

        VisualElement mLeftPanel;
        VisualElement mLeftContent;
        Label mLeftArrow;
        Image mLeftIcon;
        Label mLeftLabel;
        Label mLeftAnnotation;

        VisualElement mRightPanel;
        VisualElement mRightContent;
        Label mRightArrow;
        Image mRightIcon;
        Label mRightLabel;
        Label mRightAnnotation;

        bool mHasSrc;
        bool mHasDst;
        bool mbIsLeftHovered;
        bool mbIsRightHovered;

        const string DATA_LOSS_GLYPH = "⚠";
    }
}
