using System.Collections.Generic;

using Codice.CM.Client.Differences.Graphic;
using Unity.CodeEditor.Document;
using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal static class DiffBarActions
    {
        internal static void DeleteDifference(
            DiffAction action,
            TextDocument rightTextDocument,
            List<ColorTextRegion> rightTextRegions)
        {
            DeleteTextRegion(rightTextDocument, action.RightRegion);

            action.RightRegion.Visible = false;

            int numRemovedLines = action.RightRegion.EndLine - action.RightRegion.InitialLine;

            TextRegionModifier.UpdateBelowTextRegions(
                rightTextRegions, action.RightRegion.InitialLine, -numRemovedLines);
        }

        internal static void RestoreDifference(
            DiffAction action,
            TextDocument leftTextDocument,
            TextDocument rightTextDocument,
            List<ColorTextRegion> rightTextRegions)
        {
            DeleteTextRegion(rightTextDocument, action.RightRegion);

            InsertTextRegion(leftTextDocument, rightTextDocument, action);

            action.LeftRegion.Visible = false;
            action.RightRegion.Visible = false;

            int rightAddedLines = action.LeftRegion.LineCount - action.RightRegion.LineCount;

            TextRegionModifier.UpdateBelowTextRegions(
                rightTextRegions, action.RightRegion.InitialLine, rightAddedLines);
        }

        static void DeleteTextRegion(
            TextDocument rightTextDocument,
            ColorTextRegion region)
        {
            int deletedLines = region.EndLine - region.InitialLine;

            if (deletedLines == 0)
                return;

            DocumentLine startLine = rightTextDocument.Lines[region.InitialLine];
            DocumentLine endLine = rightTextDocument.Lines[region.EndLine - 1];

            int startOffset = startLine.Offset;
            int endOffset = endLine.EndOffset + endLine.DelimiterLength;

            rightTextDocument.Remove(startOffset, endOffset - startOffset);
        }

        static void InsertTextRegion(
            TextDocument leftTextDocument,
            TextDocument rightTextDocument,
            DiffAction action)
        {
            string text = GetLeftText(leftTextDocument, action.LeftRegion);

            int offset;

            if (action.RightRegion.InitialLine >= rightTextDocument.Lines.Count)
            {
                int index = rightTextDocument.Lines.Count - 1;

                string newLine = TextUtilities.GetNewLineFromDocument(
                    rightTextDocument, index + 1);

                DocumentLine line = rightTextDocument.Lines[index];

                text = string.Concat(newLine, text);
                offset = line.EndOffset + line.DelimiterLength;
            }
            else
            {
                offset = rightTextDocument.Lines[action.RightRegion.InitialLine].Offset;
            }

            rightTextDocument.Insert(offset, text);
        }

        static string GetLeftText(TextDocument leftTextDocument, TextRegion leftRegion)
        {
            DocumentLine iniLine = leftTextDocument.Lines[leftRegion.InitialLine];
            DocumentLine endLine = leftTextDocument.Lines[leftRegion.EndLine - 1];

            return leftTextDocument.GetText(
                iniLine.Offset, endLine.Offset + endLine.TotalLength - iniLine.Offset);
        }
    }
}
