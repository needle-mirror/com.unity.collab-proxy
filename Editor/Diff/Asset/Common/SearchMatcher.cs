using System;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common
{
    internal static class SearchMatcher
    {
        internal static bool Contains(string text, string search)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            return text.IndexOf(
                search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
