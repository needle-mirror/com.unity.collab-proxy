using System;

using Codice.CM.Client.Differences.Graphic;

using Unity.CodeEditor.Rendering;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using XDiffGui;
using XDiffGui.Drawing;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class DiffSplitter : VisualElement
    {
        internal DiffSplitter(
            Unity.CodeEditor.TextEditor srcEditor,
            Unity.CodeEditor.TextEditor dstEditor,
            VisualElement leftPanel,
            VisualElement rightPanel)
        {
            mLeftTextView = srcEditor;
            mRightTextView = dstEditor;
            mLeftPanel = leftPanel;
            mRightPanel = rightPanel;

            style.backgroundColor = UnityStyles.Colors.Diff.DiffSplitterBackgroundColor;
            this.SetMouseCursor(MouseCursor.SplitResizeLeftRight);

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
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        internal void Dispose()
        {
            UnregisterCallback<PointerDownEvent>(OnPointerDown);
            UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        internal DiffSplitterDrawingInfo DrawingInfo
        {
            get { return mDiffDrawingInfo; }
        }

        internal void SetDrawingInfo(DiffSplitterDrawingInfo diffDrawingInfo)
        {
            mDiffDrawingInfo = diffDrawingInfo;
            Redraw();
        }

        internal void Redraw()
        {
            mImguiContainer.MarkDirtyRepaint();
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            try
            {
                if (mLeftTextView == null || mRightTextView == null)
                    return;

                if (mDiffDrawingInfo == null)
                    return;

                GUI.BeginClip(new Rect(0, 0, resolvedStyle.width, resolvedStyle.height));

                for (int i = 0; i < mDiffDrawingInfo.Left.Count; i++)
                {
                    ColorTextRegion leftRegion = mDiffDrawingInfo.Left[i];
                    ColorTextRegion rightRegion = mDiffDrawingInfo.Right[i];

                    if (DiffSplitterDrawingInfo.AreMovedRegions(
                        mDiffDrawingInfo, leftRegion, rightRegion))
                        continue;

                    DrawRegion(
                        leftRegion, rightRegion,
                        ColorConfiguration.Value.GetSplitterFillColor(
                            leftRegion, mDiffDrawingInfo.IsMergeSplitter));
                }

                if (mDiffDrawingInfo.MovedRegions != null)
                {
                    for (int i = 0; i < mDiffDrawingInfo.MovedRegions.Left.Count; i++)
                    {
                        DrawRegion(
                            mDiffDrawingInfo.MovedRegions.Left[i],
                            mDiffDrawingInfo.MovedRegions.Right[i],
                            ColorConfiguration.Value.DiffLinesMoveColor);
                    }
                }

                GUI.EndClip();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        void DrawRegion(
            ColorTextRegion leftRegion,
            TextRegion rightRegion,
            ArgbColor fillColor)
        {
            if (!IsDifferenceRegionVisible(leftRegion, rightRegion))
                return;

            if (mLeftTextView.TextArea.TextView.VisualLines.Count == 0 ||
                mRightTextView.TextArea.TextView.VisualLines.Count == 0)
                return;

            bool isCurrentRegion = mDiffDrawingInfo.CurrentLines != null &&
                mDiffDrawingInfo.CurrentLines.IsCurrentRegion(leftRegion);

            float width = resolvedStyle.width;

            float topLeftY = GetStartPosition(leftRegion.InitialLine, mLeftTextView);
            float bottomLeftY = GetEndPosition(leftRegion.EndLine - 1, mLeftTextView);
            float topRightY = GetStartPosition(rightRegion.InitialLine, mRightTextView);
            float bottomRightY = GetEndPosition(rightRegion.EndLine - 1, mRightTextView);

            Vector3 topLeft = new Vector3(0, topLeftY, 0);
            Vector3 bottomLeft = new Vector3(0,
                topLeftY == bottomLeftY ? topLeftY : bottomLeftY, 0);
            Vector3 topRight = new Vector3(width, topRightY, 0);
            Vector3 bottomRight = new Vector3(width,
                topRightY == bottomRightY ? topRightY : bottomRightY, 0);

            // CalculateTorsionPoints outputs (nearEnd, nearStart) tangents
            // for a left-to-right bezier curve.
            CalculateTorsionPoints(width, topLeft, topRight,
                out Vector3 topNearEnd, out Vector3 topNearStart);
            CalculateTorsionPoints(width, bottomLeft, bottomRight,
                out Vector3 botNearEnd, out Vector3 botNearStart);

            bool topIsStraight = Mathf.Approximately(topLeft.y, topRight.y);
            bool bottomIsStraight = Mathf.Approximately(bottomLeft.y, bottomRight.y);

            // 1) Fill first. Floor/Ceil on bezier edges guarantees the fill
            //    always reaches the border center line, so the 2px bezier
            //    border (1px each side of center) fully covers the seam.
            DrawFill(
                ToColor(fillColor), width,
                topLeft, topRight, topNearStart, topNearEnd, topIsStraight,
                bottomLeft, bottomRight, botNearStart, botNearEnd, bottomIsStraight);

            // 2) Borders on top of fill.
            //    For non-bordered regions we still draw a fill-colored border
            //    to anti-alias the bezier edges, but we position the bottom
            //    edge so it ends AT the fill boundary instead of extending below.
            bool isUnselected = AdornmentStyle.IsUnselectedRegion(
                leftRegion, isCurrentRegion);
            bool showBorder = AdornmentStyle.ShowRegionBorderLine(isUnselected);

            Color borderColor = showBorder
                ? ToColor(AdornmentStyle.GetColorLine(leftRegion, isCurrentRegion))
                : ToColor(fillColor);

            float straightThickness = showBorder
                ? (float)AdornmentStyle.GetBorderThickness(isCurrentRegion)
                : FILL_BORDER_THICKNESS;

            // Non-bordered straight edges don't need a border at all -- the
            // fill is already pixel-perfect. Drawing a fill-colored rect on
            // top would double-blend the alpha, producing a visible darker band.
            // We only draw the fill-colored border on bezier edges (for anti-aliasing).
            //
            // For bordered bezier borders, shift the path by straightThickness/2
            // so the bezier center aligns with the center of the straight rect
            // borders drawn by TextEditorDrawing (which start at Y and extend
            // downward by straightThickness).
            float bezierYOffset = showBorder ? straightThickness / 2f : 0f;

            if (showBorder || !topIsStraight)
            {
                DrawEdgeBorder(
                    borderColor, straightThickness, width,
                    topLeft, topRight, topNearStart, topNearEnd,
                    topIsStraight, adjustBottomEdge: false,
                    bezierYOffset: bezierYOffset);
            }

            if (showBorder || !bottomIsStraight)
            {
                DrawEdgeBorder(
                    borderColor, straightThickness, width,
                    bottomLeft, bottomRight, botNearStart, botNearEnd,
                    bottomIsStraight, adjustBottomEdge: !showBorder,
                    bezierYOffset: bezierYOffset);
            }
        }

        static void DrawEdgeBorder(
            Color color, float straightThickness, float width,
            Vector3 left, Vector3 right,
            Vector3 nearStart, Vector3 nearEnd,
            bool isStraight, bool adjustBottomEdge,
            float bezierYOffset)
        {
            if (isStraight)
            {
                // adjustBottomEdge: pull the rect up so it ends at left.y
                // instead of extending below (used for non-bordered regions).
                float y = adjustBottomEdge
                    ? left.y - straightThickness
                    : left.y;

                EditorGUI.DrawRect(
                    new Rect(0, y, width, straightThickness), color);
            }
            else
            {
                // Shift the bezier path so its center aligns with the center
                // of the rect borders drawn by TextEditorDrawing.
                left.y += bezierYOffset;
                right.y += bezierYOffset;
                nearStart.y += bezierYOffset;
                nearEnd.y += bezierYOffset;

                Handles.DrawBezier(
                    left, right,
                    nearStart, nearEnd,
                    color, null, BEZIER_BORDER_THICKNESS);
            }
        }

        static void DrawFill(
            Color fillColor, float width,
            Vector3 topLeft, Vector3 topRight,
            Vector3 topNearStart, Vector3 topNearEnd, bool topIsStraight,
            Vector3 bottomLeft, Vector3 bottomRight,
            Vector3 botNearStart, Vector3 botNearEnd, bool bottomIsStraight)
        {
            if (topIsStraight && bottomIsStraight)
            {
                float top = topLeft.y;
                float bottom = bottomLeft.y;
                if (bottom > top)
                    EditorGUI.DrawRect(new Rect(0, top, width, bottom - top), fillColor);
                return;
            }

            Vector3[] topSamples = topIsStraight
                ? null
                : Handles.MakeBezierPoints(
                    topLeft, topRight, topNearStart, topNearEnd,
                    BEZIER_SAMPLES + 1);
            Vector3[] bottomSamples = bottomIsStraight
                ? null
                : Handles.MakeBezierPoints(
                    bottomLeft, bottomRight, botNearStart, botNearEnd,
                    BEZIER_SAMPLES + 1);

            int columnCount = Mathf.Max(1, Mathf.CeilToInt(width));

            for (int i = 0; i < columnCount; i++)
            {
                float x = i;

                float topY = topIsStraight
                    ? topLeft.y
                    : Mathf.Floor(InterpolateYAtX(topSamples, x));
                float bottomY = bottomIsStraight
                    ? bottomLeft.y
                    : Mathf.Ceil(InterpolateYAtX(bottomSamples, x));

                if (bottomY > topY)
                {
                    EditorGUI.DrawRect(
                        new Rect(x, topY, 1, bottomY - topY), fillColor);
                }
            }
        }

        static void CalculateTorsionPoints(
            float boundsWidth,
            Vector3 leftPoint, Vector3 rightPoint,
            out Vector3 torsionLeftPoint, out Vector3 torsionRightPoint)
        {
            float differenceHeight = leftPoint.y - rightPoint.y;
            float differenceWidth;

            if (differenceHeight != 0)
            {
                differenceWidth = Mathf.Max(
                    Mathf.Min(
                        boundsWidth / 2f + (boundsWidth / 10f *
                            (boundsWidth / Mathf.Abs(differenceHeight))),
                        boundsWidth * 0.95f),
                    boundsWidth * 0.7f);
            }
            else
            {
                differenceWidth = boundsWidth * 0.35f;
            }

            differenceHeight = differenceHeight *
                Mathf.Min(Mathf.Abs(differenceHeight) / boundsWidth, 0.3f);

            torsionLeftPoint = new Vector3(
                rightPoint.x - differenceWidth,
                rightPoint.y + differenceHeight, 0);
            torsionRightPoint = new Vector3(
                leftPoint.x + differenceWidth,
                leftPoint.y - differenceHeight, 0);
        }

        bool IsDifferenceRegionVisible(TextRegion leftRegion, TextRegion rightRegion)
        {
            TextView leftTextView = mLeftTextView.TextArea.TextView;
            TextView rightTextView = mRightTextView.TextArea.TextView;

            if (!leftTextView.VisualLinesValid || !rightTextView.VisualLinesValid)
                return false;

            if (leftTextView.VisualLines.Count == 0 || rightTextView.VisualLines.Count == 0)
                return false;

            int leftFirstVisibleLine =
                leftTextView.VisualLines[0].FirstDocumentLine.LineNumber;
            int leftLastVisibleLine =
                leftTextView.VisualLines[leftTextView.VisualLines.Count - 1]
                    .LastDocumentLine.LineNumber;
            int rightFirstVisibleLine =
                rightTextView.VisualLines[0].FirstDocumentLine.LineNumber;
            int rightLastVisibleLine =
                rightTextView.VisualLines[rightTextView.VisualLines.Count - 1]
                    .LastDocumentLine.LineNumber;

            return TextRegionChecker.IsDifferenceRegionVisible(
                leftRegion, rightRegion,
                leftFirstVisibleLine - 1, leftLastVisibleLine - 1,
                rightFirstVisibleLine - 1, rightLastVisibleLine - 1);
        }

        float GetStartPosition(int line, Unity.CodeEditor.TextEditor editor)
        {
            if (line < 0)
                return 0;

            if (line >= editor.Document.LineCount)
                return GetEndPosition(editor.Document.LineCount - 1, editor);

            VisualLine visualLine = editor.TextArea.TextView.GetOrConstructVisualLine(
                editor.Document.GetLineByNumber(line + 1));
            return visualLine.VisualTop - (float)editor.TextArea.TextView.ScrollOffset.y;
        }

        float GetEndPosition(int line, Unity.CodeEditor.TextEditor editor)
        {
            if (line < 0)
                return 0;

            if (line >= editor.Document.LineCount)
                line = editor.Document.LineCount - 1;

            VisualLine visualLine = editor.TextArea.TextView.GetOrConstructVisualLine(
                editor.Document.GetLineByNumber(line + 1));
            return visualLine.VisualTop + visualLine.Height
                - (float)editor.TextArea.TextView.ScrollOffset.y;
        }

        static float InterpolateYAtX(Vector3[] samples, float x)
        {
            if (x <= samples[0].x)
                return samples[0].y;

            for (int i = 0; i < samples.Length - 1; i++)
            {
                if (x <= samples[i + 1].x)
                {
                    float dx = samples[i + 1].x - samples[i].x;
                    if (dx < 0.0001f)
                        return samples[i].y;

                    float t = (x - samples[i].x) / dx;
                    return Mathf.Lerp(samples[i].y, samples[i + 1].y, t);
                }
            }

            return samples[samples.Length - 1].y;
        }

        static Color ToColor(ArgbColor color)
        {
            return new Color(
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
                return;

            mIsDragging = true;
            mDragStartX = evt.position.x;
            mStartLeftWidth = mLeftPanel.resolvedStyle.width;
            mStartRightWidth = mRightPanel.resolvedStyle.width;

            this.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (!mIsDragging)
                return;

            float deltaX = evt.position.x - mDragStartX;

            float newLeftWidth = mStartLeftWidth + deltaX;
            float newRightWidth = mStartRightWidth - deltaX;

            if (newLeftWidth < MIN_PANEL_WIDTH)
            {
                newLeftWidth = MIN_PANEL_WIDTH;
                newRightWidth = mStartLeftWidth + mStartRightWidth - MIN_PANEL_WIDTH;
            }
            else if (newRightWidth < MIN_PANEL_WIDTH)
            {
                newRightWidth = MIN_PANEL_WIDTH;
                newLeftWidth = mStartLeftWidth + mStartRightWidth - MIN_PANEL_WIDTH;
            }

            mLeftPanel.style.flexGrow = newLeftWidth;
            mRightPanel.style.flexGrow = newRightWidth;

            evt.StopPropagation();
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (!mIsDragging)
                return;

            mIsDragging = false;
            this.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }

        DiffSplitterDrawingInfo mDiffDrawingInfo;
        readonly Unity.CodeEditor.TextEditor mLeftTextView;
        readonly Unity.CodeEditor.TextEditor mRightTextView;
        readonly VisualElement mLeftPanel;
        readonly VisualElement mRightPanel;
        readonly IMGUIContainer mImguiContainer;

        bool mIsDragging;
        float mDragStartX;
        float mStartLeftWidth;
        float mStartRightWidth;

        const int BEZIER_SAMPLES = 50;
        const float BEZIER_BORDER_THICKNESS = 2f;
        const float FILL_BORDER_THICKNESS = 2f;
        const float MIN_PANEL_WIDTH = 100f;
    }
}
