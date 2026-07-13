using System.Collections.Generic;

using Codice.CM.Client.Differences.Graphic;
using Unity.CodeEditor.Rendering;
using UnityEditor;
using UnityEngine;
using XDiffGui.Drawing;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal static class TextEditorDrawing
    {
        internal static void DrawTextRegions(
            double controlWidth,
            TextBoxDrawingInfo textBoxDrawingInfo,
            Unity.CodeEditor.TextEditor textEditor,
            bool drawInsideLineRegions = false)
        {
            if (!textEditor.TextArea.TextView.VisualLinesValid)
                return;

            Dictionary<int, List<ColorInsideLineTextRegion>> insideLineRegions =
                drawInsideLineRegions
                    ? textBoxDrawingInfo.GetInsideLineRegions()
                    : null;

            foreach (VisualLine visualLine in textEditor.TextArea.TextView.VisualLines)
            {
                if (visualLine.FirstDocumentLine.IsDeleted)
                    continue;

                int currentLineIndex = visualLine.FirstDocumentLine.LineNumber - 1;

                ColorTextRegion lineRegion = textBoxDrawingInfo.GetTextRegion(currentLineIndex);

                if (lineRegion == null)
                    continue;

                float rectTop = visualLine.VisualTop - textEditor.TextArea.TextView.ScrollOffset.y;
                float rectBottom = rectTop + visualLine.Height;

                EditorGUI.DrawRect(
                    new Rect(
                        0,
                        rectTop,
                        (float)controlWidth,
                        visualLine.Height),
                    ToColor(lineRegion.Color));

                if (drawInsideLineRegions &&
                    insideLineRegions.TryGetValue(
                        currentLineIndex, out List<ColorInsideLineTextRegion> insideLineRegionList))
                {
                    DrawInsideLineRegionsForVisualLine(
                        visualLine,
                        textEditor.TextArea.TextView,
                        insideLineRegionList);
                }

                DrawCurrentDifference(
                    textEditor.TextArea.TextView,
                    textBoxDrawingInfo,
                    lineRegion,
                    currentLineIndex,
                    rectTop,
                    rectBottom);
            }
        }

        static void DrawInsideLineRegionsForVisualLine(
            VisualLine visualLine,
            TextView textView,
            List<ColorInsideLineTextRegion> regions)
        {
            foreach (ColorInsideLineTextRegion region in regions)
            {
                Vector2 initialPosition = visualLine.GetVisualPosition(
                    visualLine.GetVisualColumn(region.InitialColumn), VisualYPosition.TextTop);
                Vector2 endPosition = visualLine.GetVisualPosition(
                    visualLine.GetVisualColumn(region.EndColumn + 1), VisualYPosition.TextTop);

                Rect insideLineRegionRect = new Rect(
                    initialPosition.x - textView.ScrollOffset.x,
                    initialPosition.y - textView.ScrollOffset.y,
                    endPosition.x - initialPosition.x,
                    visualLine.Height);

                EditorGUI.DrawRect(insideLineRegionRect, ToColor(region.Color));
            }
        }

        static void DrawCurrentDifference(
            TextView textView,
            TextBoxDrawingInfo drawingInfo,
            ColorTextRegion region,
            int currentLine,
            float rectTop,
            float rectBottom)
        {
            bool isCurrentRegion = drawingInfo.CurrentLines != null &&
                drawingInfo.CurrentLines.IsCurrentRegion(region);

            bool isUnselectedRegion = AdornmentStyle.IsUnselectedRegion(region, isCurrentRegion);

            if (AdornmentStyle.HasTopLine(currentLine, region, drawingInfo))
            {
                bool isCurrentTopLine = drawingInfo.CurrentLines != null &&
                    drawingInfo.CurrentLines.IsCurrentTopLine(region.InitialLine);

                DrawRegionBorderLine(
                    textView,
                    region,
                    isUnselectedRegion,
                    isCurrentTopLine,
                    rectTop);
            }

            if (region.InitialLine == region.EndLine)
                return;

            if (AdornmentStyle.HasBottomLine(currentLine, region, drawingInfo))
            {
                bool isCurrentBottomLine = drawingInfo.CurrentLines != null &&
                    drawingInfo.CurrentLines.IsCurrentBottomLine(region.EndLine);

                DrawRegionBorderLine(
                    textView,
                    region,
                    isUnselectedRegion,
                    isCurrentBottomLine,
                    rectBottom);
            }
        }

        static void DrawRegionBorderLine(
            TextView textView,
            ColorTextRegion region,
            bool isUnselectedRegion,
            bool isCurrentLine,
            float y)
        {
            if (!AdornmentStyle.ShowRegionBorderLine(isUnselectedRegion) && !isCurrentLine)
                return;

            ArgbColor colorLine = AdornmentStyle.GetColorLine(region, isCurrentLine);
            float borderThickness = (float)AdornmentStyle.GetBorderThickness(isCurrentLine);

            float startX = -textView.ScrollOffset.x;
            float lineWidth = textView.Bounds.width + textView.ScrollOffset.x;

            EditorGUI.DrawRect(
                new Rect(startX, y, lineWidth, borderThickness),
                ToColor(colorLine));
        }

        static Color ToColor(ArgbColor color)
        {
            return new Color(
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f);
        }
    }
}
