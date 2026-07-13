using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Unity.CodeEditor.Search
{
    internal static class SearchStrategyFactory
    {
        internal static ISearchStrategy Create(
            string searchPattern, bool ignoreCase, bool matchWholeWords, SearchMode mode)
        {
            if (searchPattern == null)
                throw new ArgumentNullException(nameof(searchPattern));

            var options = RegexOptions.Multiline;
            if (ignoreCase)
                options |= RegexOptions.IgnoreCase;

            if (mode == SearchMode.Normal)
                searchPattern = Regex.Escape(searchPattern);

            try
            {
                var pattern = new Regex(searchPattern, options);
                return new RegexSearchStrategy(pattern, matchWholeWords);
            }
            catch (ArgumentException ex)
            {
                throw new SearchPatternException(ex.Message, ex);
            }
        }
    }
}
