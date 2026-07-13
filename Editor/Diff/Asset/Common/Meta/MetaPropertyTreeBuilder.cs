using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.Meta
{
    internal static class MetaPropertyTreeBuilder
    {
        internal static PropertyTreeNode Build(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                return PropertyTreeNode.CreateRoot(new List<PropertyTreeNode>());

            string[] lines = Encoding.UTF8.GetString(fileBytes).Split('\n');
            int lineIdx = 0;
            List<PropertyTreeNode> children = ParseMappingEntries(lines, ref lineIdx, -1, string.Empty);
            return PropertyTreeNode.CreateRoot(children);
        }

        #region Block Parsing

        static List<PropertyTreeNode> ParseMappingEntries(
            string[] lines, ref int lineIdx, int parentIndent, string parentPath)
        {
            List<PropertyTreeNode> children = new List<PropertyTreeNode>();

            while (lineIdx < lines.Length)
            {
                string line = TrimLineEnd(lines[lineIdx]);

                if (string.IsNullOrWhiteSpace(line))
                {
                    lineIdx++;
                    continue;
                }

                int indent = CountLeadingSpaces(line);
                string content = line.Substring(indent);

                if (IsDirectiveLine(content))
                {
                    lineIdx++;
                    continue;
                }

                if (indent <= parentIndent)
                    break;

                // Sequence items are handled by ParseSequenceItems, not here
                if (content.StartsWith("- ", StringComparison.Ordinal))
                    break;

                if (!TrySplitKeyValue(content, out string key, out string value))
                {
                    lineIdx++;
                    continue;
                }

                lineIdx++;

                string path = AppendPath(parentPath, key);
                string displayName = ObjectNames.NicifyVariableName(key);
                PropertyTreeNode node = BuildValueNode(
                    lines, ref lineIdx, indent, key, path, displayName, value);

                children.Add(node);
            }

            return children;
        }

        static PropertyTreeNode BuildValueNode(
            string[] lines, ref int lineIdx, int keyIndent,
            string name, string path, string displayName, string value)
        {
            if (string.IsNullOrEmpty(value))
                return BuildBlockValueNode(
                    lines, ref lineIdx, keyIndent, name, path, displayName);

            if (value == "{}")
                return PropertyTreeNode.CreateObject(
                    name, path, new List<PropertyTreeNode>(), displayName);

            if (value == "[]")
                return PropertyTreeNode.CreateArray(
                    name, path, new List<PropertyTreeNode>(), displayName);

            if (value.StartsWith("{", StringComparison.Ordinal) &&
                value.EndsWith("}", StringComparison.Ordinal))
                return ParseInlineMap(name, path, displayName, value);

            if (value.StartsWith("[", StringComparison.Ordinal) &&
                value.EndsWith("]", StringComparison.Ordinal))
                return ParseInlineSequence(name, path, displayName, value);

            return CreateTypedLeaf(name, path, displayName, value);
        }

        static PropertyTreeNode BuildBlockValueNode(
            string[] lines, ref int lineIdx, int keyIndent,
            string name, string path, string displayName)
        {
            SkipBlankAndDirectiveLines(lines, ref lineIdx);

            if (lineIdx >= lines.Length)
                return CreateTypedLeaf(name, path, displayName, string.Empty);

            string nextLine = TrimLineEnd(lines[lineIdx]);
            int nextIndent = CountLeadingSpaces(nextLine);
            string nextContent = nextLine.Substring(nextIndent);

            // Sequences can appear at the same indent as the key (Unity's standard format)
            // or indented further
            bool isSequence = nextContent.StartsWith("- ", StringComparison.Ordinal)
                              && nextIndent >= keyIndent;

            if (isSequence)
            {
                List<PropertyTreeNode> items = ParseSequenceItems(
                    lines, ref lineIdx, nextIndent, path);
                return PropertyTreeNode.CreateArray(name, path, items, displayName);
            }

            if (nextIndent > keyIndent)
            {
                List<PropertyTreeNode> children = ParseMappingEntries(
                    lines, ref lineIdx, keyIndent, path);
                return PropertyTreeNode.CreateObject(name, path, children, displayName);
            }

            return CreateTypedLeaf(name, path, displayName, string.Empty);
        }

        static List<PropertyTreeNode> ParseSequenceItems(
            string[] lines, ref int lineIdx, int dashIndent, string parentPath)
        {
            List<PropertyTreeNode> items = new List<PropertyTreeNode>();
            int itemIndex = 0;

            while (lineIdx < lines.Length)
            {
                string line = TrimLineEnd(lines[lineIdx]);

                if (string.IsNullOrWhiteSpace(line))
                {
                    lineIdx++;
                    continue;
                }

                int indent = CountLeadingSpaces(line);

                if (indent < dashIndent)
                    break;

                string content = line.Substring(indent);

                if (IsDirectiveLine(content))
                {
                    lineIdx++;
                    continue;
                }

                if (indent != dashIndent ||
                    !content.StartsWith("- ", StringComparison.Ordinal))
                    break;

                lineIdx++;

                string itemPath = $"{parentPath}[{itemIndex}]";
                string itemName = $"[{itemIndex}]";
                string itemContent = content.Substring(2).Trim();
                int contentIndent = indent + 2;

                PropertyTreeNode itemNode;

                if (string.IsNullOrEmpty(itemContent))
                {
                    List<PropertyTreeNode> children = ParseMappingEntries(
                        lines, ref lineIdx, indent, itemPath);
                    string label = FindNameLabel(children);
                    itemNode = PropertyTreeNode.CreateObject(
                        itemName, itemPath, children, label);
                }
                else if (TrySplitKeyValue(
                    itemContent, out string key, out string value))
                {
                    List<PropertyTreeNode> itemChildren = new List<PropertyTreeNode>();

                    string firstPath = AppendPath(itemPath, key);
                    string firstDisplay = ObjectNames.NicifyVariableName(key);
                    PropertyTreeNode firstChild = BuildValueNode(
                        lines, ref lineIdx, contentIndent,
                        key, firstPath, firstDisplay, value);
                    itemChildren.Add(firstChild);

                    List<PropertyTreeNode> siblings = ParseMappingEntries(
                        lines, ref lineIdx, indent, itemPath);
                    itemChildren.AddRange(siblings);

                    string label = FindNameLabel(itemChildren);
                    itemNode = PropertyTreeNode.CreateObject(
                        itemName, itemPath, itemChildren, label);
                }
                else
                {
                    LeafPropertyData leafData = InferScalarType(
                        string.Empty, itemPath, itemContent);
                    itemNode = PropertyTreeNode.CreateLeaf(
                        itemName, itemPath, leafData.PropertyType.ToString(),
                        leafData.StringValue, null, leafData);
                }

                items.Add(itemNode);
                itemIndex++;
            }

            return items;
        }

        static string FindNameLabel(List<PropertyTreeNode> children)
        {
            foreach (PropertyTreeNode child in children)
            {
                if (child.Kind != NodeKind.Leaf)
                    continue;

                bool isLabelKey =
                    string.Equals(child.Name, "name", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(child.Name, "buildTarget", StringComparison.OrdinalIgnoreCase);

                if (!isLabelKey)
                    continue;

                string value = child.Value?.Trim();
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            return null;
        }

        #endregion

        #region Inline Parsing

        static PropertyTreeNode ParseInlineMap(
            string name, string path, string displayName, string text)
        {
            string inner = text.Substring(1, text.Length - 2).Trim();

            if (string.IsNullOrEmpty(inner))
                return PropertyTreeNode.CreateObject(
                    name, path, new List<PropertyTreeNode>(), displayName);

            if (TryBuildInlineVector(
                name, path, displayName, inner, out PropertyTreeNode vectorNode))
                return vectorNode;

            if (TryBuildInlineRect(
                name, path, displayName, inner, out PropertyTreeNode rectNode))
                return rectNode;

            List<PropertyTreeNode> children = new List<PropertyTreeNode>();

            foreach (string part in SplitInlineElements(inner))
            {
                if (!TrySplitKeyValue(part, out string key, out string value))
                    continue;

                string childPath = AppendPath(path, key);
                string childDisplay = ObjectNames.NicifyVariableName(key);
                children.Add(CreateTypedLeaf(key, childPath, childDisplay, value));
            }

            return PropertyTreeNode.CreateObject(name, path, children, displayName);
        }

        static bool TryBuildInlineVector(
            string name, string path, string displayName,
            string inner, out PropertyTreeNode node)
        {
            node = null;
            Dictionary<string, float> components = new Dictionary<string, float>(
                StringComparer.OrdinalIgnoreCase);

            foreach (string part in SplitInlineElements(inner))
            {
                if (!TrySplitKeyValue(part, out string key, out string val))
                    return false;

                if (!VECTOR_COMPONENT_KEYS.Contains(key.ToLowerInvariant()))
                    return false;

                if (!float.TryParse(
                    UnquoteValue(val), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float parsed))
                    return false;

                components[key.ToLowerInvariant()] = parsed;
            }

            LeafPropertyData leafData;

            if (components.Count == 2 &&
                components.ContainsKey("x") &&
                components.ContainsKey("y"))
            {
                Vector2 vec = new Vector2(components["x"], components["y"]);
                leafData = new LeafPropertyData
                {
                    PropertyType = SerializedPropertyType.Vector2,
                    StringValue = vec.ToString(),
                    Vector2Value = vec
                };
            }
            else if (components.Count == 3 &&
                     components.ContainsKey("x") &&
                     components.ContainsKey("y") &&
                     components.ContainsKey("z"))
            {
                Vector3 vec = new Vector3(
                    components["x"], components["y"], components["z"]);
                leafData = new LeafPropertyData
                {
                    PropertyType = SerializedPropertyType.Vector3,
                    StringValue = vec.ToString(),
                    Vector3Value = vec
                };
            }
            else if (components.Count == 4 &&
                     components.ContainsKey("x") &&
                     components.ContainsKey("y") &&
                     components.ContainsKey("z") &&
                     components.ContainsKey("w"))
            {
                Vector4 vec = new Vector4(
                    components["x"], components["y"],
                    components["z"], components["w"]);
                leafData = new LeafPropertyData
                {
                    PropertyType = SerializedPropertyType.Vector4,
                    StringValue = vec.ToString(),
                    Vector4Value = vec
                };
            }
            else
            {
                return false;
            }

            node = PropertyTreeNode.CreateLeaf(
                name, path, leafData.PropertyType.ToString(),
                leafData.StringValue, displayName, leafData);
            return true;
        }

        static bool TryBuildInlineRect(
            string name, string path, string displayName,
            string inner, out PropertyTreeNode node)
        {
            node = null;
            Dictionary<string, float> components = new Dictionary<string, float>(
                StringComparer.OrdinalIgnoreCase);

            foreach (string part in SplitInlineElements(inner))
            {
                if (!TrySplitKeyValue(part, out string key, out string val))
                    return false;

                string lowerKey = key.ToLowerInvariant();
                if (!RECT_COMPONENT_KEYS.Contains(lowerKey))
                    return false;

                if (!float.TryParse(
                    UnquoteValue(val), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float parsed))
                    return false;

                components[lowerKey] = parsed;
            }

            if (components.Count != 4 ||
                !components.ContainsKey("x") ||
                !components.ContainsKey("y") ||
                !components.ContainsKey("width") ||
                !components.ContainsKey("height"))
                return false;

            Rect rect = new Rect(
                components["x"], components["y"],
                components["width"], components["height"]);

            LeafPropertyData leafData = new LeafPropertyData
            {
                PropertyType = SerializedPropertyType.Rect,
                StringValue = rect.ToString(),
                RectValue = rect
            };

            node = PropertyTreeNode.CreateLeaf(
                name, path, leafData.PropertyType.ToString(),
                leafData.StringValue, displayName, leafData);
            return true;
        }

        static PropertyTreeNode ParseInlineSequence(
            string name, string path, string displayName, string text)
        {
            string inner = text.Substring(1, text.Length - 2).Trim();

            if (string.IsNullOrEmpty(inner))
                return PropertyTreeNode.CreateArray(
                    name, path, new List<PropertyTreeNode>(), displayName);

            List<PropertyTreeNode> children = new List<PropertyTreeNode>();
            int index = 0;

            foreach (string element in SplitInlineElements(inner))
            {
                string itemPath = $"{path}[{index}]";
                string itemName = $"[{index}]";
                children.Add(CreateTypedLeaf(itemName, itemPath, null, element));
                index++;
            }

            return PropertyTreeNode.CreateArray(name, path, children, displayName);
        }

        #endregion

        #region Type Inference

        static PropertyTreeNode CreateTypedLeaf(
            string name, string path, string displayName, string rawValue)
        {
            LeafPropertyData leafData = InferScalarType(name, path, rawValue);
            return PropertyTreeNode.CreateLeaf(
                name, path, leafData.PropertyType.ToString(),
                leafData.StringValue, displayName, leafData);
        }

        static LeafPropertyData InferScalarType(string key, string path, string rawValue)
        {
            string raw = (rawValue ?? string.Empty).Trim();
            bool wasQuoted = IsQuotedValue(raw);
            string display = UnquoteValue(raw);

            if (wasQuoted)
                return MakeStringLeaf(display);

            if (IsBooleanNumericValue(key, raw, out bool boolFromNumeric))
            {
                return new LeafPropertyData
                {
                    PropertyType = SerializedPropertyType.Boolean,
                    BoolValue = boolFromNumeric,
                    StringValue = boolFromNumeric.ToString()
                };
            }

            if (TryParseBooleanLiteral(raw, out bool boolValue))
            {
                return new LeafPropertyData
                {
                    PropertyType = SerializedPropertyType.Boolean,
                    BoolValue = boolValue,
                    StringValue = boolValue.ToString()
                };
            }

            if (int.TryParse(raw, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out int intValue))
            {
                LeafPropertyData intLeaf = new LeafPropertyData
                {
                    PropertyType = SerializedPropertyType.Integer,
                    IntValue = intValue,
                    StringValue = intValue.ToString()
                };

                if (!string.IsNullOrEmpty(path) &&
                    IntPropertyFormatters.TryFormatPath(path, intValue, out string formatted))
                {
                    intLeaf.StringValue = formatted;
                    intLeaf.HasFormattedDisplay = true;
                }

                return intLeaf;
            }

            if (LooksLikeFloat(raw) &&
                float.TryParse(raw, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out float floatValue))
            {
                return new LeafPropertyData
                {
                    PropertyType = SerializedPropertyType.Float,
                    FloatValue = floatValue,
                    StringValue = floatValue.ToString("G")
                };
            }

            return MakeStringLeaf(display);
        }

        static LeafPropertyData MakeStringLeaf(string value)
        {
            return new LeafPropertyData
            {
                PropertyType = SerializedPropertyType.String,
                StringValue = value
            };
        }

        static bool IsBooleanNumericValue(
            string key, string value, out bool result)
        {
            result = false;

            if (value != "0" && value != "1")
                return false;

            if (string.IsNullOrEmpty(key))
                return false;

            string leafKey = ExtractLeafName(key);

            if (BOOLEAN_HINT_KEYS.Contains(leafKey) || LooksLikeBooleanKey(leafKey))
            {
                result = value == "1";
                return true;
            }

            return false;
        }

        static bool LooksLikeBooleanKey(string key)
        {
            return StartsWithAny(key,
                "is", "has", "use", "can", "allow",
                "enable", "disable", "force", "preload",
                "normalize", "loop");
        }

        static bool StartsWithAny(string text, params string[] prefixes)
        {
            foreach (string prefix in prefixes)
            {
                if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        static bool TryParseBooleanLiteral(string value, out bool result)
        {
            if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }

            if (value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }

            result = false;
            return false;
        }

        static bool LooksLikeFloat(string value)
        {
            return value.IndexOf('.') >= 0 ||
                   value.IndexOf('e') >= 0 ||
                   value.IndexOf('E') >= 0;
        }

        #endregion

        #region String Parsing Utilities

        static IEnumerable<string> SplitInlineElements(string text)
        {
            List<string> result = new List<string>();
            int start = 0;
            int curlyDepth = 0;
            int squareDepth = 0;
            char quote = '\0';

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (quote != '\0')
                {
                    if (c == quote && (i == 0 || text[i - 1] != '\\'))
                        quote = '\0';
                    continue;
                }

                if (c == '"' || c == '\'') { quote = c; continue; }
                if (c == '{') { curlyDepth++; continue; }
                if (c == '}') { curlyDepth--; continue; }
                if (c == '[') { squareDepth++; continue; }
                if (c == ']') { squareDepth--; continue; }

                if (c == ',' && curlyDepth == 0 && squareDepth == 0)
                {
                    result.Add(text.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }

            if (start < text.Length)
                result.Add(text.Substring(start).Trim());

            return result;
        }

        static bool TrySplitKeyValue(
            string text, out string key, out string value)
        {
            key = null;
            value = null;

            int curlyDepth = 0;
            int squareDepth = 0;
            char quote = '\0';

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (quote != '\0')
                {
                    if (c == quote && (i == 0 || text[i - 1] != '\\'))
                        quote = '\0';
                    continue;
                }

                if (c == '"' || c == '\'') { quote = c; continue; }
                if (c == '{') { curlyDepth++; continue; }
                if (c == '}') { curlyDepth--; continue; }
                if (c == '[') { squareDepth++; continue; }
                if (c == ']') { squareDepth--; continue; }

                if (c == ':' && curlyDepth == 0 && squareDepth == 0)
                {
                    key = text.Substring(0, i).Trim();
                    value = text.Substring(i + 1).Trim();
                    return !string.IsNullOrEmpty(key);
                }
            }

            return false;
        }

        static bool IsQuotedValue(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2)
                return false;

            return (value[0] == '"' && value[value.Length - 1] == '"') ||
                   (value[0] == '\'' && value[value.Length - 1] == '\'');
        }

        static string UnquoteValue(string value)
        {
            return IsQuotedValue(value)
                ? value.Substring(1, value.Length - 2)
                : value;
        }

        static int CountLeadingSpaces(string line)
        {
            int count = 0;
            while (count < line.Length && line[count] == ' ')
                count++;
            return count;
        }

        static bool IsDirectiveLine(string content)
        {
            return content.StartsWith("%", StringComparison.Ordinal) ||
                   content.StartsWith("---", StringComparison.Ordinal) ||
                   content.StartsWith("...", StringComparison.Ordinal);
        }

        static string AppendPath(string basePath, string key)
        {
            return string.IsNullOrEmpty(basePath) ? key : $"{basePath}.{key}";
        }

        static string ExtractLeafName(string key)
        {
            int lastDot = key.LastIndexOf('.');
            string segment = lastDot >= 0 ? key.Substring(lastDot + 1) : key;

            int bracket = segment.IndexOf('[');
            if (bracket >= 0)
                segment = segment.Substring(0, bracket);

            return segment;
        }

        static string TrimLineEnd(string line)
        {
            return line.TrimEnd('\r');
        }

        static void SkipBlankAndDirectiveLines(string[] lines, ref int lineIdx)
        {
            while (lineIdx < lines.Length)
            {
                string line = TrimLineEnd(lines[lineIdx]);

                if (string.IsNullOrWhiteSpace(line))
                {
                    lineIdx++;
                    continue;
                }

                int indent = CountLeadingSpaces(line);
                string content = line.Substring(indent);

                if (IsDirectiveLine(content))
                {
                    lineIdx++;
                    continue;
                }

                break;
            }
        }

        #endregion

        #region Constants

        static readonly HashSet<string> BOOLEAN_HINT_KEYS =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "3D",
                "ambisonic",
                "alphaIsTransparency",
                "convertToNormalmap",
                "crunchedCompression",
                "forceToMono",
                "generateCubemap",
                "ignorePngGamma",
                "isReadable",
                "loadInBackground",
                "loopable",
                "mipMapsPreserveCoverage",
                "normalize",
                "preloadAudioData",
                "streamingMipmaps",
                "sRGBTexture"
            };

        static readonly HashSet<string> VECTOR_COMPONENT_KEYS =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "x", "y", "z", "w" };

        static readonly HashSet<string> RECT_COMPONENT_KEYS =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "x", "y", "width", "height" };

        #endregion
    }
}
