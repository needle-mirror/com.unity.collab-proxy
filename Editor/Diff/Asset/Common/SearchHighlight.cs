using System;
using System.Text;
using Unity.PlasticSCM.Editor.UI;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common
{
    internal static class SearchHighlight
    {
        internal static string Apply(string text, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(text))
                return text;

            int firstIdx = text.IndexOf(
                searchTerm, StringComparison.OrdinalIgnoreCase);
            if (firstIdx < 0)
                return text;

            string openTag = GetOpenTag();
            int searchLen = searchTerm.Length;

            StringBuilder sb = new StringBuilder(
                text.Length + openTag.Length + CLOSE_TAG.Length);

            int startIdx = 0;
            while (startIdx < text.Length)
            {
                int idx = text.IndexOf(
                    searchTerm, startIdx, StringComparison.OrdinalIgnoreCase);

                if (idx < 0)
                {
                    sb.Append(text, startIdx, text.Length - startIdx);
                    break;
                }

                sb.Append(text, startIdx, idx - startIdx);
                sb.Append(openTag);
                sb.Append(text, idx, searchLen);
                sb.Append(CLOSE_TAG);
                startIdx = idx + searchLen;
            }

            return sb.ToString();
        }

        static string GetOpenTag()
        {
            string bgHex = ColorUtility.ToHtmlStringRGBA(
                UnityStyles.Colors.Diff.AssetDiff.SearchHighlightBackground);
            return $"<mark=#{bgHex}>";
        }

        const string CLOSE_TAG = "</mark>";
    }
}
