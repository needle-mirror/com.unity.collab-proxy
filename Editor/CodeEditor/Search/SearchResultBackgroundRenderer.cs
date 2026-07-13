using System.Linq;
using Unity.CodeEditor.Document;
using Unity.CodeEditor.Rendering;
using UnityEngine;

namespace Unity.CodeEditor.Search
{
    internal class SearchResultBackgroundRenderer : IBackgroundRenderer
    {
        internal TextSegmentCollection<SearchResult> CurrentResults { get; } =
            new TextSegmentCollection<SearchResult>();

        public KnownLayer Layer => KnownLayer.Selection;

        internal Color MarkerColor { get; set; }

        internal SearchResultBackgroundRenderer(Color markerColor)
        {
            MarkerColor = markerColor;
        }

        public void OnGUI(TextView textView, Rect drawingRect)
        {
            if (CurrentResults == null || !textView.VisualLinesValid)
                return;

            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
                return;

            var firstLine = visualLines.First();
            var lastLine = visualLines.Last();
            if (firstLine.FirstDocumentLine.IsDeleted || lastLine.LastDocumentLine.IsDeleted)
                return;
            var viewStart = firstLine.FirstDocumentLine.Offset;
            var viewEnd = lastLine.LastDocumentLine.EndOffset;

            foreach (var result in CurrentResults.FindOverlappingSegments(viewStart, viewEnd - viewStart))
            {
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, result))
                {
                    GUI.color = MarkerColor;
                    GUI.DrawTexture(rect, Texture2D.whiteTexture);
                }
            }

            GUI.color = Color.white;
        }
    }
}
