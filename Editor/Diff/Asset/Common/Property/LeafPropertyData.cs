using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.Property
{
    internal struct LeafPropertyData
    {
        internal SerializedPropertyType PropertyType;
        internal string StringValue;
        internal float FloatValue;
        internal int IntValue;
        internal bool BoolValue;
        internal Vector2 Vector2Value;
        internal Vector3 Vector3Value;
        internal Vector4 Vector4Value;
        internal Vector2Int Vector2IntValue;
        internal Vector3Int Vector3IntValue;
        internal Color ColorValue;
        internal Quaternion QuaternionValue;
        internal Rect RectValue;
        internal Bounds BoundsValue;
        internal AnimationCurve AnimationCurveValue;
        internal Gradient GradientValue;
        internal RectInt RectIntValue;
        internal BoundsInt BoundsIntValue;
        internal Hash128 Hash128Value;
        internal bool HasFormattedDisplay;

        internal static LeafPropertyData FromProperty(SerializedProperty prop)
        {
            LeafPropertyData data = new LeafPropertyData
            {
                PropertyType = prop.propertyType,
                StringValue = GetStringValue(prop)
            };

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Float:
                    data.FloatValue = prop.floatValue; break;
                case SerializedPropertyType.Integer:
                    data.IntValue = prop.intValue;
                    string formattedInt;
                    if (IntPropertyFormatters.TryFormat(prop, out formattedInt))
                    {
                        data.StringValue = formattedInt;
                        data.HasFormattedDisplay = true;
                    }
                    break;
                case SerializedPropertyType.LayerMask:
                    data.IntValue = prop.intValue;
                    string formattedLayerMask;
                    if (IntPropertyFormatters.TryFormat(prop, out formattedLayerMask))
                    {
                        data.StringValue = formattedLayerMask;
                        data.HasFormattedDisplay = true;
                    }
                    break;
                case SerializedPropertyType.Character:
                    data.IntValue = prop.intValue; break;
                case SerializedPropertyType.Boolean:
                    data.BoolValue = prop.boolValue; break;
                case SerializedPropertyType.Vector2:
                    data.Vector2Value = prop.vector2Value; break;
                case SerializedPropertyType.Vector3:
                    data.Vector3Value = prop.vector3Value; break;
                case SerializedPropertyType.Vector4:
                    data.Vector4Value = prop.vector4Value; break;
                case SerializedPropertyType.Vector2Int:
                    data.Vector2IntValue = prop.vector2IntValue; break;
                case SerializedPropertyType.Vector3Int:
                    data.Vector3IntValue = prop.vector3IntValue; break;
                case SerializedPropertyType.Color:
                    data.ColorValue = prop.colorValue; break;
                case SerializedPropertyType.Quaternion:
                    data.QuaternionValue = prop.quaternionValue; break;
                case SerializedPropertyType.Rect:
                    data.RectValue = prop.rectValue; break;
                case SerializedPropertyType.Bounds:
                    data.BoundsValue = prop.boundsValue; break;
                case SerializedPropertyType.String:
                    data.StringValue = prop.stringValue; break;
                case SerializedPropertyType.Enum:
                    data.IntValue = prop.enumValueIndex;
                    data.StringValue = prop.enumDisplayNames.Length > prop.enumValueIndex
                                       && prop.enumValueIndex >= 0
                        ? prop.enumDisplayNames[prop.enumValueIndex]
                        : prop.enumValueIndex.ToString();
                    break;
                case SerializedPropertyType.AnimationCurve:
                    data.AnimationCurveValue = prop.animationCurveValue; break;
                case SerializedPropertyType.Gradient:
                    data.GradientValue = prop.gradientValue; break;
                case SerializedPropertyType.RectInt:
                    data.RectIntValue = prop.rectIntValue; break;
                case SerializedPropertyType.BoundsInt:
                    data.BoundsIntValue = prop.boundsIntValue; break;
                case SerializedPropertyType.Hash128:
                    data.Hash128Value = prop.hash128Value; break;
            }

            return data;
        }

        static string GetStringValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    string formattedInt;
                    if (IntPropertyFormatters.TryFormat(prop, out formattedInt))
                        return formattedInt;
                    return prop.intValue.ToString();
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.FixedBufferSize:
                    return prop.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return prop.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return prop.floatValue.ToString("G");
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Color:
                    return prop.colorValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    UnityEngine.Object obj = prop.objectReferenceValue;
                    return obj == null
                        ? "(None)"
                        : $"{obj.GetType().Name}: {obj.name}";
                case SerializedPropertyType.LayerMask:
                    string formattedLayerMask;
                    if (IntPropertyFormatters.TryFormat(prop, out formattedLayerMask))
                        return formattedLayerMask;
                    return prop.intValue.ToString();
                case SerializedPropertyType.Enum:
                    return prop.enumDisplayNames.Length > prop.enumValueIndex
                           && prop.enumValueIndex >= 0
                        ? prop.enumDisplayNames[prop.enumValueIndex]
                        : prop.enumValueIndex.ToString();
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value.ToString();
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value.ToString();
                case SerializedPropertyType.Rect:
                    return prop.rectValue.ToString();
                case SerializedPropertyType.Character:
                    return ((char)prop.intValue).ToString();
                case SerializedPropertyType.AnimationCurve:
                    AnimationCurve curve = prop.animationCurveValue;
                    return curve != null
                        ? $"AnimationCurve ({curve.length} keys)"
                        : "AnimationCurve (empty)";
                case SerializedPropertyType.Gradient:
                    Gradient gradient = prop.gradientValue;
                    return gradient != null
                        ? $"Gradient ({gradient.colorKeys.Length} color keys, " +
                          $"{gradient.alphaKeys.Length} alpha keys)"
                        : "Gradient (empty)";
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue.ToString();
                case SerializedPropertyType.Quaternion:
                    return prop.quaternionValue.eulerAngles.ToString();
                case SerializedPropertyType.Vector2Int:
                    return prop.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3Int:
                    return prop.vector3IntValue.ToString();
                case SerializedPropertyType.RectInt:
                    return prop.rectIntValue.ToString();
                case SerializedPropertyType.BoundsInt:
                    return prop.boundsIntValue.ToString();
                case SerializedPropertyType.Hash128:
                    return prop.hash128Value.ToString();
                default:
                    return $"({prop.propertyType})";
            }
        }
    }
}
