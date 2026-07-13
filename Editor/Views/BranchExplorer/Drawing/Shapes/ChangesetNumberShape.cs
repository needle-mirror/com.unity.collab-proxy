using System.Globalization;

using Codice.Client.BaseCommands.BranchExplorer;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.Views.BranchExplorer.Drawing.Shapes.Colors;
using Unity.PlasticSCM.Editor.Views.BranchExplorer.Virtualization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Views.BranchExplorer.Drawing.Shapes
{
    internal class ChangesetNumberShape : BrExShape
    {
        internal ChangesetDrawInfo ChangesetDraw
        {
            get { return VirtualShape.DrawInfo as ChangesetDrawInfo; }
        }

        internal static string BuildName(long changesetId)
        {
            return "changeset-number-" + changesetId;
        }

        internal ChangesetNumberShape(VirtualShape virtualShape) : base(virtualShape)
        {
            name = BuildName(GetChangesetId(ChangesetDraw));

            mContainer = new VisualElement();
            mContainer.style.position = Position.Absolute;
            mContainer.style.left = 0;
            mContainer.style.top = 0;
            mContainer.style.width = Length.Percent(100);
            mContainer.style.height = Length.Percent(100);
            mContainer.style.overflow = Overflow.Hidden;
            mContainer.style.borderTopLeftRadius = CornerRadius;
            mContainer.style.borderTopRightRadius = CornerRadius;
            mContainer.style.borderBottomLeftRadius = CornerRadius;
            mContainer.style.borderBottomRightRadius = CornerRadius;

            Color borderColor = BrExColors.Label.GetCaptionBorderColor();

            mContainer.style.borderTopColor = borderColor;
            mContainer.style.borderBottomColor = borderColor;
            mContainer.style.borderLeftColor = borderColor;
            mContainer.style.borderRightColor = borderColor;

            mContainer.style.borderTopWidth = ExpandedBorderWidth;
            mContainer.style.borderBottomWidth = ExpandedBorderWidth;
            mContainer.style.borderLeftWidth = ExpandedBorderWidth;
            mContainer.style.borderRightWidth = ExpandedBorderWidth;

            mContainer.style.paddingLeft = ExpandedPadding;
            mContainer.style.paddingRight = ExpandedPadding;
            mContainer.style.paddingTop = ExpandedPadding;
            mContainer.style.paddingBottom = ExpandedPadding;

            mContainer.style.backgroundColor =
                UnityStyles.Colors.BranchExplorer.ControlBackgroundColor;

            mNumberLabel = new Label();
            mNumberLabel.style.fontSize = DefaultFontSize;
            mNumberLabel.style.color = UnityStyles.Colors.Label;
            mNumberLabel.style.whiteSpace = WhiteSpace.NoWrap;
            mNumberLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            mNumberLabel.style.marginLeft = 0;
            mNumberLabel.style.marginRight = 0;
            mNumberLabel.style.marginTop = 0;
            mNumberLabel.style.marginBottom = 0;
            mNumberLabel.pickingMode = PickingMode.Ignore;

            mContainer.Add(mNumberLabel);
            Add(mContainer);

            pickingMode = PickingMode.Position;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);

            Redraw();
        }

        internal static ChangesetNumberShape FindForChangeset(
            VirtualCanvas canvas, ChangesetDrawInfo changesetDraw)
        {
            if (canvas == null)
                return null;

            foreach (BrExShape shape in canvas.GetShapes())
            {
                if (shape is ChangesetNumberShape numberShape &&
                    numberShape.ChangesetDraw == changesetDraw)
                    return numberShape;
            }

            return null;
        }

        internal static Vector2 MeasureNumberBoxSize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Vector2.zero;

            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fontSize = DefaultFontSize;
            Vector2 textSize = style.CalcSize(new GUIContent(text));
            float edge = (ExpandedPadding + ExpandedBorderWidth) * 2f;
            return new Vector2(textSize.x + edge, textSize.y + edge);
        }

        static long GetChangesetId(ChangesetDrawInfo changesetDraw)
        {
            BrExChangeset changeset = changesetDraw.Tag as BrExChangeset;
            return changeset != null ? changeset.Id : 0;
        }

        internal static string GetNumberText(ChangesetDrawInfo drawInfo)
        {
            if (drawInfo == null)
                return null;

            BrExChangeset changeset = drawInfo.Tag as BrExChangeset;
            if (changeset == null || changeset.IsCheckoutChangeset)
                return null;

            return changeset.Id.ToString(CultureInfo.InvariantCulture);
        }

        internal void Expand()
        {
            CancelScheduledCollapse();

            if (mIsExpanded)
                return;

            if (!IsVisibleForZoomLevel())
                return;

            mIsExpanded = true;

            UpdateVisibilityForZoom();
        }

        internal void ScheduleCollapse()
        {
            CancelScheduledCollapse();
            mScheduledCollapse = schedule.Execute(Collapse).StartingIn(CollapseDelayMs);
        }

        internal void CancelScheduledCollapse()
        {
            if (mScheduledCollapse == null)
                return;

            mScheduledCollapse.Pause();
            mScheduledCollapse = null;
        }

        internal override void Dispose()
        {
            CancelScheduledCollapse();
            UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            base.Dispose();
        }

        internal override void Redraw()
        {
            if (!DisplayOptions.DisplayChangesetNumbers)
            {
                mIsExpanded = false;
                CancelScheduledCollapse();
            }

            UpdateNumberText();
            UpdateVisibilityForZoom();
            base.Redraw();
        }

        protected override void GenerateVisualContent(Painter2D painter)
        {
        }

        internal override bool HasContextMenu()
        {
            return false;
        }

        internal override void OnScrollChanged()
        {
            UpdateVisibilityForZoom();
        }

        protected internal override VirtualShape GetVirtualShapeForSelection()
        {
            VirtualCanvas canvas = GetFirstAncestorOfType<VirtualCanvas>();
            if (canvas == null)
                return null;

            foreach (BrExShape brExShape in canvas.GetShapes())
            {
                if (brExShape is ChangesetShape changesetShape &&
                    changesetShape.ChangesetDraw == ChangesetDraw)
                    return brExShape.VirtualShape;
            }

            return null;
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            UpdateVisibilityForZoom();
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            if (evt.pressedButtons != 0)
                return;

            Expand();
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (mIsExpanded && evt.pressedButtons != 0)
                return;

            ScheduleCollapse();
        }

        void Collapse()
        {
            mScheduledCollapse = null;

            if (!mIsExpanded)
                return;

            mIsExpanded = false;
            UpdateVisibilityForZoom();
        }

        void UpdateNumberText()
        {
            string text = GetNumberText(ChangesetDraw);
            mNumberLabel.text = text ?? string.Empty;
        }

        void UpdateVisibilityForZoom()
        {
            bool zoomOk = IsVisibleForZoomLevel();

            if (!zoomOk && mIsExpanded)
            {
                mIsExpanded = false;
                CancelScheduledCollapse();
            }

            visible = zoomOk && DisplayOptions.DisplayChangesetNumbers;
            SyncContainerVisibility();
            MarkDirtyRepaint();
        }

        void SyncContainerVisibility()
        {
            string text = GetNumberText(ChangesetDraw);
            bool showCaption =
                visible &&
                mIsExpanded &&
                !string.IsNullOrEmpty(text);

            mContainer.style.display = showCaption ? DisplayStyle.Flex : DisplayStyle.None;
        }

        bool IsVisibleForZoomLevel()
        {
            VirtualCanvas canvas = GetFirstAncestorOfType<VirtualCanvas>();
            return canvas != null && canvas.ZoomLevel >= BrExShape.MinZoomToShowText;
        }

        readonly VisualElement mContainer;
        readonly Label mNumberLabel;

        IVisualElementScheduledItem mScheduledCollapse;
        bool mIsExpanded;

        const float ExpandedPadding = 4;
        const float ExpandedBorderWidth = 1;
        const float CornerRadius = 4;
        const int DefaultFontSize = 10;
        const long CollapseDelayMs = 100;
    }
}
