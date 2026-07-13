// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Diagnostics.CodeAnalysis;
using Unity.CodeEditor.Document;
using Unity.CodeEditor.Text;
using Document_LogicalDirection = Unity.CodeEditor.Document.LogicalDirection;
using LogicalDirection = Unity.CodeEditor.Document.LogicalDirection;

namespace Unity.CodeEditor.Rendering
{
    // This class is internal because it does not need to be accessed by the user - it can be configured using TextEditorOptions.

    /// <summary>
    /// Element generator that displays · for spaces and » for tabs and a box for control characters.
    /// </summary>
    /// <remarks>
    /// This element generator is present in every TextView by default; the enabled features can be configured using the
    /// <see cref="TextEditorOptions"/>.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Whitespace")]
    internal sealed class SingleCharacterElementGenerator : VisualLineElementGenerator, IBuiltinElementGenerator
    {
        /// <summary>
        /// Gets/Sets whether to show · for spaces.
        /// </summary>
        internal bool ShowSpaces { get; set; }

        /// <summary>
        /// Gets/Sets whether to show » for tabs.
        /// </summary>
        internal bool ShowTabs { get; set; }

        // precomputed offsets of spaces/tabs for the current line
        // built once in StartGeneration, then GetFirstInterestedOffset
        // just walks the cursor — O(1) per call instead of scanning text.
        private int[] _interestingOffsets;
        private int _interestingOffsetCount;
        private int _interestingOffsetCursor;

        /// <summary>
        /// Creates a new SingleCharacterElementGenerator instance.
        /// </summary>
        public SingleCharacterElementGenerator()
        {
            ShowSpaces = true;
            ShowTabs = true;
        }

        void IBuiltinElementGenerator.FetchOptions(TextEditorOptions options)
        {
            ShowSpaces = options.ShowSpaces;
            ShowTabs = options.ShowTabs;
        }

        internal override void StartGeneration(ITextRunConstructionContext context)
        {
            base.StartGeneration(context);

            var firstLine = context.VisualLine.FirstDocumentLine;
            var lineStart = firstLine.Offset;
            var relevantText = context.GetText(lineStart, firstLine.EndOffset - lineStart);

            // reuse array if large enough, otherwise allocate
            if (_interestingOffsets == null || _interestingOffsets.Length < relevantText.Count)
                _interestingOffsets = new int[relevantText.Count];

            _interestingOffsetCount = 0;
            for (var i = 0; i < relevantText.Count; i++)
            {
                var c = relevantText.Text[relevantText.Offset + i];
                if ((ShowSpaces && c == ' ') || c == '\t')
                {
                    _interestingOffsets[_interestingOffsetCount++] = lineStart + i;
                }
            }
            _interestingOffsetCursor = 0;
        }

        internal override int GetFirstInterestedOffset(int startOffset)
        {
            // advance cursor past offsets before startOffset
            while (_interestingOffsetCursor < _interestingOffsetCount
                   && _interestingOffsets[_interestingOffsetCursor] < startOffset)
            {
                _interestingOffsetCursor++;
            }

            if (_interestingOffsetCursor < _interestingOffsetCount)
            {
                return _interestingOffsets[_interestingOffsetCursor];
            }

            return -1;
        }

        internal override VisualLineElement ConstructElement(int offset)
        {
            var c = CurrentContext.Document.GetCharAt(offset);

            if (ShowSpaces && c == ' ')
            {
                // put consecutive spaces to batch into a single element
                int spaceCount = 1;
                var endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
                while (offset + spaceCount < endOffset
                       && CurrentContext.Document.GetCharAt(offset + spaceCount) == ' ')
                {
                    spaceCount++;
                }

                var runProperties = CurrentContext.TextParagraphProperties.Clone();
                runProperties.SetForegroundColor(CurrentContext.TextView.NonPrintableCharacterColor);
                return new SpaceTextElement(
                    CurrentContext.TextView.Options.ShowSpacesGlyph,
                    spaceCount,
                    runProperties);
            }
            else if (c == '\t')
            {
                var runProperties = CurrentContext.TextParagraphProperties.Clone();
                runProperties.SetForegroundColor(CurrentContext.TextView.NonPrintableCharacterColor);
                return new TabTextElement(
                    CurrentContext.TextView.Options.ShowTabsGlyph,
                    CurrentContext.TextView.Options.IndentationSize,
                    ShowTabs,
                    runProperties);
            }

            return null;
        }

        private sealed class SpaceTextElement : VisualLineElement
        {
            string _spaceString;
            TextParagraphProperties _spaceParagraphProperties;

            internal SpaceTextElement(string spaceGlyph, int spaceCount, TextParagraphProperties properties)
                : base(spaceGlyph.Length * spaceCount, spaceCount)
            {
                if (spaceCount == 1)
                    _spaceString = spaceGlyph;
                else if (spaceGlyph.Length == 1)
                    _spaceString = new string(spaceGlyph[0], spaceCount);
                else
                {
                    var sb = new System.Text.StringBuilder(spaceGlyph.Length * spaceCount);
                    for (int i = 0; i < spaceCount; i++)
                        sb.Append(spaceGlyph);
                    _spaceString = sb.ToString();
                }
                _spaceParagraphProperties = properties;
            }

            internal override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
            {
                return new StringTextRun(_spaceString, _spaceParagraphProperties);
            }

            internal override int GetNextCaretPosition(int visualColumn, Document_LogicalDirection direction,
                CaretPositioningMode mode)
            {
                if (mode == CaretPositioningMode.Normal || mode == CaretPositioningMode.EveryCodepoint)
                    return base.GetNextCaretPosition(visualColumn, direction, mode);
                else
                    return -1;
            }

            internal override bool IsWhitespace(int visualColumn)
            {
                return true;
            }
        }

        private sealed class TabTextElement : VisualLineElement
        {
            string _tabString;
            TextParagraphProperties _tabParagraphProperties;

            internal TabTextElement(string tabGlyph, int tabSize, bool showTabs, TextParagraphProperties properties) : base(tabSize, 1)
            {
                _tabString = GenerateTabText(tabSize, tabGlyph, showTabs);
                _tabParagraphProperties = properties;
            }

            internal override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
            {
                return new TabTextRun(_tabString, _tabParagraphProperties);
            }

            internal override int GetNextCaretPosition(int visualColumn, Document_LogicalDirection direction,
                CaretPositioningMode mode)
            {
                if (mode == CaretPositioningMode.Normal || mode == CaretPositioningMode.EveryCodepoint)
                    return base.GetNextCaretPosition(visualColumn, direction, mode);
                else
                    return -1;
            }

            internal override bool IsWhitespace(int visualColumn)
            {
                return true;
            }

            static string GenerateTabText(int tabSize, string tabGlyph, bool showTabs)
            {
                if (showTabs)
                {
                    return string.Concat(tabGlyph,
                        new string(' ', tabSize - tabGlyph.Length));
                }

                return new string(' ', tabSize);
            }
        }
    }
}
