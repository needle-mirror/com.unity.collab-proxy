using UnityEditor;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.Property
{
    internal static class GroupHeaderLabels
    {
        internal static string ForArray(string displayName, string path)
        {
            if (string.IsNullOrEmpty(displayName) || displayName.StartsWith("m_"))
                return ObjectNames.NicifyVariableName(SafeLastSegment(path));
            return displayName;
        }

        internal static string ForArrayElement(string path)
        {
            return ELEMENT_PREFIX + (ExtractTrailingElementIndex(path) + 1);
        }

        static string SafeLastSegment(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            int lastDot = path.LastIndexOf('.');
            return lastDot < 0 ? path : path.Substring(lastDot + 1);
        }

        static int ExtractTrailingElementIndex(string path)
        {
            if (string.IsNullOrEmpty(path))
                return 0;

            int closeBracket = path.LastIndexOf(']');
            if (closeBracket <= 0)
                return 0;

            int openBracket = path.LastIndexOf('[', closeBracket - 1);
            if (openBracket < 0)
                return 0;

            string indexText = path.Substring(
                openBracket + 1, closeBracket - openBracket - 1);
            return int.TryParse(indexText, out int idx) ? idx : 0;
        }

        const string ELEMENT_PREFIX = "Element ";
    }
}
