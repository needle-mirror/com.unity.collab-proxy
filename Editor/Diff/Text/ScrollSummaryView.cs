using System;
using System.Collections.Generic;

using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;

using Unity.PlasticSCM.Editor.UI.UIElements;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class ScrollSummaryView : VisualElement
    {
        internal ScrollSummaryView(IDiffSummaryListener listener)
        {
            mListener = listener;

            style.flexShrink = 0;
            style.overflow = Overflow.Hidden;

            mImguiContainer = new IMGUIContainer(OnGUI);
            mImguiContainer.pickingMode = PickingMode.Ignore;
            mImguiContainer.focusable = false;
            mImguiContainer.style.position = Position.Absolute;
            mImguiContainer.style.left = 0;
            mImguiContainer.style.top = 0;
            mImguiContainer.style.right = 0;
            mImguiContainer.style.bottom = 0;
            Add(mImguiContainer);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        internal void UpdateDrawingInfo(
            List<DiffSummaryDraw> summaryDraws,
            int totalLines)
        {
            mSummaryDraws = summaryDraws;
            mTotalLines = totalLines;
            mImguiContainer.MarkDirtyRepaint();
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (mSummaryDraws == null)
                return;

            Rect drawingRectangle = GetDrawingRectangle();
            float scale = GetScale(drawingRectangle);

            mDiffRectangles.Clear();

            foreach (DiffSummaryDraw draw in mSummaryDraws)
            {
                float x = DIFF_DRAW_MARGIN;
                float y = drawingRectangle.y + (scale * draw.TopLine);
                float width = resolvedStyle.width - (2 * DIFF_DRAW_MARGIN);
                float height = scale * (draw.BottomLine - draw.TopLine);

                if (height <= 0)
                    continue;

                if (height > 0 && height < MIN_DRAW_HEIGHT)
                    height = MIN_DRAW_HEIGHT;

                Rect target = new Rect(x, y, width, height);

                Color targetColor = new Color(
                    draw.Color.R / 255f,
                    draw.Color.G / 255f,
                    draw.Color.B / 255f,
                    1f);

                EditorGUI.DrawRect(target, targetColor);

                mDiffRectangles[target] = draw;
            }

            UpdateCursorOverlays();
        }

        void UpdateCursorOverlays()
        {
            foreach (VisualElement overlay in mCursorOverlays)
                overlay.RemoveFromHierarchy();

            mCursorOverlays.Clear();

            foreach (Rect bounds in mDiffRectangles.Keys)
            {
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

        void OnPointerDown(PointerDownEvent e)
        {
            if (e.button != 0)
                return;

            try
            {
                Vector2 localPos = e.localPosition;

                DiffSummaryDraw diffDrawing = GetDrawInfoAt(localPos);

                int line = (diffDrawing != null)
                    ? diffDrawing.TopLine
                    : GetLineFromYCoord(GetDrawingRectangle(), localPos.y);

                if (line < 0) line = 0;
                if (line > mTotalLines) line = mTotalLines;

                mListener.Clicked(line);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        void OnPointerMove(PointerMoveEvent e)
        {
        }

        void OnPointerLeave(PointerLeaveEvent e)
        {
        }

        Rect GetDrawingRectangle()
        {
            float width = resolvedStyle.width;
            float height = resolvedStyle.height;

            if (width == 0 || height == 0)
                return default;

            float targetHeight = height -
                (2 * VSCROLL_BUTTON_HEIGHT) -
                HSCROLL_HEIGHT - MIN_DRAW_HEIGHT;

            return new Rect(
                0,
                VSCROLL_BUTTON_HEIGHT,
                width,
                targetHeight <= 0 ? 1 : targetHeight);
        }

        float GetScale(Rect drawingRectangle)
        {
            return drawingRectangle.height / mTotalLines;
        }

        int GetLineFromYCoord(Rect drawingRectangle, float y)
        {
            float scale = GetScale(drawingRectangle);
            return (int)((y - drawingRectangle.y) / scale);
        }

        DiffSummaryDraw GetDrawInfoAt(Vector2 point)
        {
            foreach (Rect rectangle in mDiffRectangles.Keys)
            {
                if (!rectangle.Contains(point))
                    continue;

                return mDiffRectangles[rectangle];
            }

            return null;
        }

        List<DiffSummaryDraw> mSummaryDraws;
        int mTotalLines = 1;
        readonly Dictionary<Rect, DiffSummaryDraw> mDiffRectangles =
            new Dictionary<Rect, DiffSummaryDraw>();
        readonly List<VisualElement> mCursorOverlays = new List<VisualElement>();

        readonly IDiffSummaryListener mListener;
        readonly IMGUIContainer mImguiContainer;

        const int MIN_DRAW_HEIGHT = 5;
        const int DIFF_DRAW_MARGIN = 2;

        const int VSCROLL_BUTTON_HEIGHT = 17;
        const int HSCROLL_HEIGHT = 18;
    }
}
