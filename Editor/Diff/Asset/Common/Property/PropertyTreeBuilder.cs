using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.Property
{
    internal static class PropertyTreeBuilder
    {
        internal static PropertyTreeNode Build(SerializedObject so)
        {
            List<PropertyTreeNode> children = new List<PropertyTreeNode>();
            SerializedProperty prop = so.GetIterator();

            if (!prop.Next(true))
                return PropertyTreeNode.CreateRoot(children);

            do
            {
                if (IgnoredProperties.IsIgnored(prop.propertyPath))
                    continue;

                children.Add(BuildNode(prop.Copy(), isArrayElement: false));
            }
            while (prop.Next(false));

            return PropertyTreeNode.CreateRoot(children);
        }

        static PropertyTreeNode BuildNode(
            SerializedProperty prop,
            bool isArrayElement)
        {
            string name = ExtractLocalName(prop.propertyPath);
            string path = prop.propertyPath;
            string displayName = BuildDisplayName(prop);

            if (IsLeafProperty(prop))
            {
                LeafPropertyData leafData = BuildLeafData(prop);

                return PropertyTreeNode.CreateLeaf(
                    name, path, leafData.PropertyType.ToString(),
                    leafData.StringValue, displayName, leafData);
            }

            if (isArrayElement)
                displayName = GroupHeaderLabels.ForArrayElement(path);
            else if (prop.isArray)
                displayName = GroupHeaderLabels.ForArray(displayName, path);

            if (prop.isArray)
                return BuildArrayNode(prop, name, path, displayName);

            return BuildObjectNode(prop, name, path, displayName);
        }

        static LeafPropertyData BuildLeafData(SerializedProperty prop)
        {
            if (prop.propertyPath == "m_LocalRotation"
                && prop.propertyType == SerializedPropertyType.Quaternion
                && prop.serializedObject.targetObject is Transform transform)
            {
                Vector3 inspectorEuler = TransformUtils.GetInspectorRotation(transform);

                return new LeafPropertyData
                {
                    PropertyType = SerializedPropertyType.Vector3,
                    Vector3Value = inspectorEuler,
                    StringValue = inspectorEuler.ToString()
                };
            }

            return LeafPropertyData.FromProperty(prop);
        }

        static PropertyTreeNode BuildArrayNode(
            SerializedProperty prop,
            string name,
            string path,
            string displayName)
        {
            List<PropertyTreeNode> children = new List<PropertyTreeNode>();

            for (int i = 0; i < prop.arraySize; i++)
            {
                SerializedProperty element = prop.GetArrayElementAtIndex(i);

                if (IgnoredProperties.IsIgnored(element.propertyPath))
                    continue;

                children.Add(BuildNode(element.Copy(), isArrayElement: true));
            }

            AnimationClipCurveLabels.TryDecorate(children, path);

            if (IsKeyedArray(prop))
                return BuildKeyedArrayAsObject(children, name, path, displayName);

            return PropertyTreeNode.CreateArray(name, path, children, displayName);
        }

        static PropertyTreeNode BuildKeyedArrayAsObject(
            List<PropertyTreeNode> rawElements,
            string name,
            string path,
            string displayName)
        {
            List<PropertyTreeNode> namedChildren = new List<PropertyTreeNode>();

            foreach (PropertyTreeNode element in rawElements)
            {
                string key = null;
                PropertyTreeNode valueNode = null;

                foreach (PropertyTreeNode child in element.Children)
                {
                    if (child.Name == "first" && child.Kind == NodeKind.Leaf)
                        key = child.Value;
                    else if (child.Name == "second")
                        valueNode = child;
                }

                if (key != null && valueNode != null)
                {
                    string childPath = path + "." + key;
                    string childDisplayName = ObjectNames.NicifyVariableName(
                        key.TrimStart('_'));

                    if (valueNode.Kind == NodeKind.Leaf)
                    {
                        namedChildren.Add(PropertyTreeNode.CreateLeaf(
                            key, childPath, valueNode.TypeTag,
                            valueNode.Value, childDisplayName, valueNode.Tag));
                    }
                    else
                    {
                        List<PropertyTreeNode> rewrappedChildren = new List<PropertyTreeNode>(
                            valueNode.Children);

                        namedChildren.Add(PropertyTreeNode.CreateObject(
                            key, childPath, rewrappedChildren, childDisplayName));
                    }
                }
                else
                {
                    namedChildren.Add(element);
                }
            }

            return PropertyTreeNode.CreateObject(name, path, namedChildren, displayName);
        }

        static PropertyTreeNode BuildObjectNode(
            SerializedProperty prop,
            string name,
            string path,
            string displayName)
        {
            List<PropertyTreeNode> children = new List<PropertyTreeNode>();
            int parentDepth = prop.depth;
            SerializedProperty iter = prop.Copy();

            if (!iter.Next(true) || iter.depth <= parentDepth)
                return PropertyTreeNode.CreateObject(name, path, children, displayName);

            do
            {
                if (!IgnoredProperties.IsIgnored(iter.propertyPath))
                    children.Add(BuildNode(iter.Copy(), isArrayElement: false));
            }
            while (iter.Next(false) && iter.depth > parentDepth);

            return PropertyTreeNode.CreateObject(name, path, children, displayName);
        }

        static bool IsKeyedArray(SerializedProperty arrayProp)
        {
            if (arrayProp.arraySize == 0)
                return false;

            SerializedProperty firstElement = arrayProp.GetArrayElementAtIndex(0);

            if (firstElement.propertyType != SerializedPropertyType.Generic)
                return false;

            int parentDepth = firstElement.depth;
            SerializedProperty iter = firstElement.Copy();

            if (!iter.Next(true) || iter.depth <= parentDepth)
                return false;

            bool hasFirst = false;
            bool hasSecond = false;

            do
            {
                string localName = ExtractLocalName(iter.propertyPath);

                if (localName == "first" && iter.propertyType == SerializedPropertyType.String)
                    hasFirst = true;
                else if (localName == "second")
                    hasSecond = true;
            }
            while (iter.Next(false) && iter.depth > parentDepth);

            return hasFirst && hasSecond;
        }

        static bool IsLeafProperty(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.AnimationCurve:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.BoundsInt:
                case SerializedPropertyType.Hash128:
                    return true;

                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.FixedBufferSize:
                    return false;

                default:
                    return !prop.hasChildren;
            }
        }

        static string BuildDisplayName(SerializedProperty prop)
        {
            string path = prop.propertyPath;

            if (path.StartsWith("m_LocalPosition"))
                return prop.displayName.Replace("Local ", "");
            if (path.StartsWith("m_LocalRotation"))
                return prop.displayName.Replace("Local ", "");
            if (path.StartsWith("m_LocalScale"))
                return prop.displayName.Replace("Local ", "");

            return prop.displayName;
        }

        static string ExtractLocalName(string propertyPath)
        {
            int lastDot = propertyPath.LastIndexOf('.');

            if (lastDot < 0)
                return propertyPath;

            return propertyPath.Substring(lastDot + 1);
        }
    }
}
