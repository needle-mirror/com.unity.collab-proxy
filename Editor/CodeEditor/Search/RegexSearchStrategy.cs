using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.CodeEditor.Document;

namespace Unity.CodeEditor.Search
{
    internal class RegexSearchStrategy : ISearchStrategy
    {
        readonly Regex _searchPattern;
        readonly bool _matchWholeWords;

        internal RegexSearchStrategy(Regex searchPattern, bool matchWholeWords)
        {
            _searchPattern = searchPattern ?? throw new ArgumentNullException(nameof(searchPattern));
            _matchWholeWords = matchWholeWords;
        }

        public IEnumerable<ISearchResult> FindAll(ITextSource document, int offset, int length)
        {
            int endOffset = offset + length;
            foreach (Match result in _searchPattern.Matches(document.Text))
            {
                int resultEndOffset = result.Length + result.Index;
                if (offset > result.Index || endOffset < resultEndOffset)
                    continue;
                if (_matchWholeWords
                    && (!IsWordBorder(document, result.Index) || !IsWordBorder(document, resultEndOffset)))
                    continue;
                yield return new SearchResult
                {
                    StartOffset = result.Index,
                    Length = result.Length,
                    Data = result
                };
            }
        }

        static bool IsWordBorder(ITextSource document, int offset)
        {
            return TextUtilities.GetNextCaretPosition(
                document, offset - 1,
                LogicalDirection.Forward,
                CaretPositioningMode.WordBorder) == offset;
        }

        public ISearchResult FindNext(ITextSource document, int offset, int length)
        {
            return FindAll(document, offset, length).FirstOrDefault();
        }

        public bool Equals(ISearchStrategy other)
        {
            var strategy = other as RegexSearchStrategy;
            return strategy != null
                && strategy._searchPattern.ToString() == _searchPattern.ToString()
                && strategy._searchPattern.Options == _searchPattern.Options
                && strategy._searchPattern.RightToLeft == _searchPattern.RightToLeft;
        }
    }

    internal class SearchResult : TextSegment, ISearchResult
    {
        internal Match Data { get; set; }

        public string ReplaceWith(string replacement)
        {
            return Data.Result(replacement);
        }
    }
}
