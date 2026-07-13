using System;
using System.Collections.Generic;
using Unity.CodeEditor.Document;

namespace Unity.CodeEditor.Search
{
    internal interface ISearchStrategy : IEquatable<ISearchStrategy>
    {
        IEnumerable<ISearchResult> FindAll(ITextSource document, int offset, int length);

        ISearchResult FindNext(ITextSource document, int offset, int length);
    }

    internal interface ISearchResult : ISegment
    {
        string ReplaceWith(string replacement);
    }

    internal enum SearchMode
    {
        Normal,
        RegEx
    }

    internal class SearchPatternException : Exception
    {
        internal SearchPatternException()
        {
        }

        internal SearchPatternException(string message) : base(message)
        {
        }

        internal SearchPatternException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
