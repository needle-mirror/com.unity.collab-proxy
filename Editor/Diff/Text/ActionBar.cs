using System;
using System.Collections.Generic;

using Codice.CM.Client.Differences.Graphic;

using Unity.CodeEditor;
using Unity.CodeEditor.Rendering;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UIElements;

using XDiffGui;
using XDiffGui.Drawing;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal abstract class ActionBar : VisualElement
    {
        internal ActionBar(
            Unity.CodeEditor.TextEditor textEditor,
            IActionBarClickListener listener)
        {
            mTextEditor = textEditor;
            mClickListener = listener;

            style.flexShrink = 0;
            style.width = WIDTH;
            style.overflow = Overflow.Hidden;
            style.backgroundColor = TextEditorColors.Background;

            mImguiContainer = new IMGUIContainer(OnGUI);
            mImguiContainer.pickingMode = PickingMode.Ignore;
            mImguiContainer.focusable = false;
            mImguiContainer.style.position = Position.Absolute;
            mImguiContainer.style.left = 0;
            mImguiContainer.style.top = 0;
            mImguiContainer.style.right = 0;
            mImguiContainer.style.bottom = 0;
            Add(mImguiContainer);

            mTextEditor.TextArea.TextView.VisualLinesChanged += OnVisualLinesChanged;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        internal void Dispose()
        {
            mTextEditor.TextArea.TextView.VisualLinesChanged -= OnVisualLinesChanged;

            UnregisterCallback<PointerDownEvent>(OnPointerDown);
            UnregisterCallback<PointerUpEvent>(OnPointerUp);
            UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);

            foreach (VisualElement overlay in mCursorOverlays)
                overlay.RemoveFromHierarchy();

            mCursorOverlays.Clear();
        }

        internal List<DiffAction> DiffActions
        {
            get { return mDiffActions; }
            set
            {
                mDiffActions = value;
                UpdateActions();
            }
        }

        internal void UpdateActions()
        {
            if (mDiffActions == null)
                return;

            if (resolvedStyle.width == 0)
                return;

            mDiffAreas.Clear();

            TextView textView = mTextEditor.TextArea.TextView;
            if (!textView.VisualLinesValid)
                return;

            IReadOnlyList<VisualLine> visibleLines = textView.VisualLines;
            if (visibleLines.Count == 0)
                return;

            int firstVisibleLine = visibleLines[0].FirstDocumentLine.LineNumber;
            int lastVisibleLine = visibleLines[visibleLines.Count - 1]
                .FirstDocumentLine.LineNumber;

            foreach (DiffAction action in mDiffActions)
            {
                ColorTextRegion region = GetTextRegion(action);

                if (!region.Visible)
                    continue;

                if (!TextRegionChecker.IsRegionVisible(
                    region, firstVisibleLine - 1, lastVisibleLine - 1))
                    continue;

                float topPosition = GetTopPosition(region.InitialLine + 1);
                float bottomPosition = GetBottomPosition(region.EndLine);

                float y = topPosition;
                float width = resolvedStyle.width;
                float height = bottomPosition - y;

                Rect rectangle = new Rect(
                    GetButtonX(),
                    y - textView.VerticalOffset + 1.5f,
                    width,
                    Mathf.Max(0, height - 1));

                ActionBarArea area = new ActionBarArea();
                area.DiffIndex = action.DiffIndex;
                area.DiffAction = action;
                area.Bounds = rectangle;

                if (action.ShowAction)
                    area.Button = GetDifferenceButton(rectangle);

                mDiffAreas.Add(area);
            }

            mImguiContainer.MarkDirtyRepaint();
            UpdateCursorOverlays();
        }

        void UpdateCursorOverlays()
        {
            foreach (VisualElement overlay in mCursorOverlays)
                overlay.RemoveFromHierarchy();

            mCursorOverlays.Clear();

            foreach (ActionBarArea area in mDiffAreas)
            {
                if (area.Button == null)
                    continue;

                Rect bounds = area.Button.Bounds;

                VisualElement overlay = new VisualElement();
                overlay.style.position = Position.Absolute;
                overlay.style.left = bounds.x;
                overlay.style.top = bounds.y;
                overlay.style.width = bounds.width;
                overlay.style.height = bounds.height;
                overlay.focusable = false;
                overlay.SetMouseCursor(MouseCursor.Link);

                Add(overlay);
                mCursorOverlays.Add(overlay);
            }
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            TextBoxDrawingInfo textBoxDrawingInfo = mTextEditor.Tag as TextBoxDrawingInfo;
            if (textBoxDrawingInfo != null)
            {
                TextEditorDrawing.DrawTextRegions(
                    resolvedStyle.width,
                    textBoxDrawingInfo,
                    mTextEditor);
            }

            Handles.BeginGUI();
            PaintDifferenceButtons();
            PaintSeparatorLine();
            Handles.EndGUI();
        }

        protected abstract ColorTextRegion GetTextRegion(DiffAction action);
        protected abstract Vector2 GetLineStartPoint();
        protected abstract Vector2 GetLineEndPoint();
        protected abstract AreaButton GetDifferenceButton(Rect rectangle);
        protected abstract float GetButtonX();

        void OnVisualLinesChanged(object sender, EventArgs e)
        {
            UpdateActions();
        }

        void OnPointerDown(PointerDownEvent e)
        {
            if (e.button != 0)
                return;

            UnClickAllButtons();

            Vector2 localPos = e.localPosition;
            ActionBarArea clickedArea = GetButtonAreaAt(localPos);

            if (clickedArea != null && clickedArea.Button != null)
            {
                clickedArea.Button.IsClicked = true;
                mClickedButtonArea = clickedArea;
                this.CapturePointer(e.pointerId);
                e.StopPropagation();
            }

            mImguiContainer.MarkDirtyRepaint();
        }

        void OnPointerUp(PointerUpEvent e)
        {
            if (this.HasPointerCapture(e.pointerId))
                this.ReleasePointer(e.pointerId);

            UnClickAllButtons();

            Vector2 localPos = e.localPosition;
            ActionBarArea area = GetButtonAreaAt(localPos);

            ActionBarArea clickedArea = mClickedButtonArea;
            mClickedButtonArea = null;

            mImguiContainer.MarkDirtyRepaint();

            if (area == null || area != clickedArea)
                return;

            e.StopPropagation();
            OnAreaClick(area);
        }

        void OnPointerMove(PointerMoveEvent e)
        {
            UnHoverAllButtons();

            Vector2 localPos = e.localPosition;
            ActionBarArea hoverArea = GetButtonAreaAt(localPos);

            if (hoverArea != null && hoverArea.Button != null)
                hoverArea.Button.IsHovered = true;

            mImguiContainer.MarkDirtyRepaint();
        }

        void OnPointerLeave(PointerLeaveEvent e)
        {
            UnHoverAllButtons();

            if (mClickedButtonArea == null)
                UnClickAllButtons();

            mImguiContainer.MarkDirtyRepaint();
        }

        protected Rect GetAreaButtonRectangle(Rect diffRegionRectangle)
        {
            return new Rect(
                diffRegionRectangle.x +
                    (diffRegionRectangle.width - BUTTON_SIZE) / 2,
                diffRegionRectangle.y + BUTTON_MARGIN_TOP,
                BUTTON_SIZE,
                BUTTON_SIZE);
        }

        void OnAreaClick(ActionBarArea area)
        {
            if (area == null) return;
            if (area.Button == null) return;

            if (mClickListener == null)
                return;

            mClickListener.OnButtonClick(area.DiffAction, area.Button.ButtonAction);
        }

        void UnClickAllButtons()
        {
            foreach (ActionBarArea area in mDiffAreas)
            {
                if (area.Button == null)
                    continue;

                area.Button.IsClicked = false;
            }
        }

        void UnHoverAllButtons()
        {
            foreach (ActionBarArea area in mDiffAreas)
            {
                if (area.Button == null)
                    continue;

                area.Button.IsHovered = false;
            }
        }

        void PaintDifferenceButtons()
        {
            foreach (ActionBarArea area in mDiffAreas)
            {
                if (area.Button == null)
                    continue;

                area.Button.Paint();
            }
        }

        void PaintSeparatorLine()
        {
            Vector2 start = GetLineStartPoint();
            Vector2 end = GetLineEndPoint();

            float x = Mathf.Min(start.x, end.x);
            float y = Mathf.Min(start.y, end.y);
            float width = Mathf.Max(1, Mathf.Abs(end.x - start.x));
            float height = Mathf.Max(1, Mathf.Abs(end.y - start.y));

            EditorGUI.DrawRect(
                new Rect(x, y, width, height),
                UnityStyles.Colors.BarBorder);
        }

        ActionBarArea GetButtonAreaAt(Vector2 point)
        {
            foreach (ActionBarArea area in mDiffAreas)
            {
                if (area.Button == null)
                    continue;

                if (area.Button.Bounds.Contains(point))
                    return area;
            }

            return null;
        }

        float GetTopPosition(int line)
        {
            VisualLine visualLine = mTextEditor.TextArea.TextView.GetVisualLine(line);
            if (visualLine == null)
                return 0;

            return visualLine.VisualTop;
        }

        float GetBottomPosition(int line)
        {
            VisualLine visualLine = mTextEditor.TextArea.TextView.GetVisualLine(line);
            if (visualLine == null)
                return mTextEditor.TextArea.TextView.Bounds.height +
                    mTextEditor.TextArea.TextView.VerticalOffset;

            return visualLine.VisualTop + visualLine.Height;
        }

        protected List<ActionBarArea> mDiffAreas = new List<ActionBarArea>();

        ActionBarArea mClickedButtonArea;
        List<DiffAction> mDiffActions;
        readonly List<VisualElement> mCursorOverlays = new List<VisualElement>();

        readonly Unity.CodeEditor.TextEditor mTextEditor;
        readonly IActionBarClickListener mClickListener;
        readonly IMGUIContainer mImguiContainer;

        const float WIDTH = 16;
        const int BUTTON_SIZE = 11;
        const int BUTTON_MARGIN_TOP = 2;
    }

    internal class ActionBarArea
    {
        internal DiffAction DiffAction { get; set; }
        internal AreaButton Button { get; set; }
        internal int DiffIndex { get; set; }
        internal Rect Bounds { get; set; }
    }

    internal class AreaButton
    {
        internal DiffButtonActions ButtonAction { get; set; }
        internal Rect Bounds { get; set; }
        internal bool IsHovered { get; set; }
        internal bool IsClicked { get; set; }

        internal AreaButton(DiffButtonActions action)
        {
            ButtonAction = action;
        }

        internal void Paint()
        {
            Color color;

            if (IsClicked)
                color = UnityStyles.Colors.Diff.ActionBar.ButtonClicked;
            else if (IsHovered)
                color = UnityStyles.Colors.Diff.ActionBar.ButtonHovered;
            else
                color = UnityStyles.Colors.Diff.ActionBar.ButtonNormal;

            float cx = Mathf.Round(Bounds.x + Bounds.width / 2f);
            float cy = Mathf.Round(Bounds.y + Bounds.height / 2f);

            if (IsClicked)
            {
                cx += 1;
                cy += 1;
            }

            Color prevColor = Handles.color;
            Handles.color = color;

            if (ButtonAction == DiffButtonActions.Delete)
                DrawCross(cx, cy);
            else if (ButtonAction == DiffButtonActions.Restore)
                DrawDoubleChevron(cx, cy);

            Handles.color = prevColor;
        }

        static void DrawCross(float cx, float cy)
        {
            DrawLine(cx - 3, cy - 3, cx + 3, cy + 3);
            DrawLine(cx + 3, cy - 3, cx - 3, cy + 3);
        }

        static void DrawDoubleChevron(float cx, float cy)
        {
            DrawChevron(cx - 4, cx - 1, cy, 3);
            DrawChevron(cx, cx + 3, cy, 3);
        }

        static void DrawLine(float x1, float y1, float x2, float y2)
        {
            Handles.DrawAAPolyLine(THICKNESS,
                new Vector3(x1, y1, 0),
                new Vector3(x2, y2, 0));
        }

        static void DrawChevron(float left, float tip, float cy, float half)
        {
            Handles.DrawAAPolyLine(THICKNESS,
                new Vector3(left, cy - half, 0),
                new Vector3(tip, cy, 0),
                new Vector3(left, cy + half, 0));
        }

        const float THICKNESS = 2f;
    }
}
