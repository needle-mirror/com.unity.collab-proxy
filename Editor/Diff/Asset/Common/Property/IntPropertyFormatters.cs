using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common.Property
{
    internal static class IntPropertyFormatters
    {
        internal static bool TryFormat(SerializedProperty prop, out string display)
        {
            display = null;

            if (prop.propertyType == SerializedPropertyType.LayerMask)
            {
                display = FormatLayerMaskBits(prop.intValue);
                return true;
            }

            if (prop.propertyType != SerializedPropertyType.Integer)
                return false;

            UnityEngine.Object target = prop.serializedObject.targetObject;
            int value = prop.intValue;

            if (TryFormatFromRegistry(target, prop.propertyPath, value, out display))
                return true;

            if (TryFormatFromReflectedEnum(target, prop.propertyPath, value, out display))
                return true;

            return TryFormatFromPathFallback(prop.propertyPath, value, out display);
        }

        internal static bool TryFormatPath(string path, int value, out string display)
        {
            return TryFormatFromPathFallback(path, value, out display);
        }

        internal static string FormatEnum(Type enumType, int value)
        {
            bool bIsFlags = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);

            if (bIsFlags)
            {
                if (value == 0)
                    return NOTHING_LABEL;

                return NicifyFlagsList(Enum.ToObject(enumType, value).ToString());
            }

            string name = Enum.GetName(enumType, value);
            return name == null ? value.ToString() : ObjectNames.NicifyVariableName(name);
        }

        static string NicifyFlagsList(string commaJoined)
        {
            if (string.IsNullOrEmpty(commaJoined))
                return commaJoined;

            string[] parts = commaJoined.Split(',');
            for (int i = 0; i < parts.Length; i++)
                parts[i] = ObjectNames.NicifyVariableName(parts[i].Trim());

            return string.Join(", ", parts);
        }

        static bool TryFormatFromRegistry(
            UnityEngine.Object target, string path, int value, out string display)
        {
            display = null;

            if (target == null)
                return false;

            Type type = target.GetType();
            for (Type t = type; t != null; t = t.BaseType)
            {
                Func<int, string> formatter;
                if (mRegistry.TryGetValue(BuildKey(t, path), out formatter))
                {
                    display = formatter(value);
                    return display != null;
                }
            }

            return false;
        }

        static bool TryFormatFromPathFallback(string path, int value, out string display)
        {
            display = null;

            Func<int, string> fallback;
            if (mFallbackByPath.TryGetValue(path, out fallback))
            {
                display = fallback(value);
                return display != null;
            }

            return false;
        }

        static bool TryFormatFromReflectedEnum(
            UnityEngine.Object target, string path, int value, out string display)
        {
            display = null;

            if (target == null)
                return false;

            FieldInfo field = ResolveFieldByPath(target.GetType(), path);
            if (field == null || !field.FieldType.IsEnum)
                return false;

            display = FormatEnum(field.FieldType, value);
            return true;
        }

        static FieldInfo ResolveFieldByPath(Type type, string path)
        {
            const BindingFlags FLAGS =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Type current = type;
            FieldInfo field = null;

            string[] segments = path.Split('.');
            for (int i = 0; i < segments.Length; i++)
            {
                if (current == null)
                    return null;

                string segment = segments[i];

                if (segment == "Array" || segment.StartsWith("data["))
                    return null;

                field = current.GetField(segment, FLAGS);
                if (field == null)
                    return null;

                current = field.FieldType;
            }

            return field;
        }

        static string FormatStaticEditorFlags(int value)
        {
            if (value == 0)
                return NOTHING_LABEL;

            if (value == int.MaxValue)
                return EVERYTHING_LABEL;

            string raw = ((StaticEditorFlags)value).ToString()
                .Replace(
                    "LightmapStatic", // obsolete -> use ContributeGI
                    nameof(StaticEditorFlags.ContributeGI));

            return NicifyFlagsList(raw);
        }

        static string FormatLayer(int value)
        {
            string name = LayerMask.LayerToName(value);
            return string.IsNullOrEmpty(name) ? value.ToString() : name;
        }

        static string FormatTargetDisplay(int value)
        {
            return string.Format("Display {0}", value + 1);
        }

        static string FormatLayerMaskBits(int value)
        {
            if (value == 0)
                return "Nothing";

            uint bits = unchecked((uint)value);
            if (bits == uint.MaxValue)
                return "Everything";

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                if ((bits & (1u << i)) == 0)
                    continue;

                string layerName = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(layerName))
                    continue;

                if (builder.Length > 0)
                    builder.Append(", ");

                builder.Append(layerName);
            }

            if (builder.Length == 0)
                return string.Format("0x{0:X8}", bits);

            return builder.ToString();
        }

        static string BuildKey(Type type, string path)
        {
            return string.Concat(type.FullName, "|", path);
        }

        static Dictionary<string, Func<int, string>> BuildRegistry()
        {
            Dictionary<string, Func<int, string>> registry =
                new Dictionary<string, Func<int, string>>();

            Add(registry, typeof(GameObject), "m_StaticEditorFlags", FormatStaticEditorFlags);
            Add(registry, typeof(GameObject), "m_Layer", FormatLayer);

            Add(registry, typeof(Camera), "m_FOVAxisMode",
                value => FormatEnum(typeof(Camera.FieldOfViewAxis), value));
            Add(registry, typeof(Camera), "m_GateFitMode",
                value => FormatEnum(typeof(Camera.GateFitMode), value));
            Add(registry, typeof(Camera), "m_RenderingPath",
                value => FormatEnum(typeof(RenderingPath), value));
            Add(registry, typeof(Camera), "m_TargetEye",
                value => FormatEnum(typeof(StereoTargetEyeMask), value));
            Add(registry, typeof(Camera), "m_ClearFlags",
                value => FormatEnum(typeof(CameraClearFlags), value));
            Add(registry, typeof(Camera), "m_TargetDisplay", FormatTargetDisplay);

            Add(registry, typeof(Light), "m_Type",
                value => FormatEnum(typeof(LightType), value));
            Add(registry, typeof(Light), "m_Lightmapping",
                value => FormatEnum(typeof(LightmapBakeType), value));
            Add(registry, typeof(Light), "m_Shadows.m_Type",
                value => FormatEnum(typeof(LightShadows), value));

            Add(registry, typeof(Renderer), "m_LightProbeUsage",
                value => FormatEnum(typeof(LightProbeUsage), value));
            Add(registry, typeof(Renderer), "m_ReflectionProbeUsage",
                value => FormatEnum(typeof(ReflectionProbeUsage), value));

            Add(registry, typeof(ReflectionProbe), "m_Mode",
                value => FormatEnum(typeof(ReflectionProbeMode), value));
            Add(registry, typeof(ReflectionProbe), "m_ClearFlags",
                value => FormatEnum(typeof(CameraClearFlags), value));

            Add(registry, typeof(Rigidbody), "m_Constraints",
                value => FormatEnum(typeof(RigidbodyConstraints), value));
            Add(registry, typeof(Rigidbody2D), "m_Constraints",
                value => FormatEnum(typeof(RigidbodyConstraints2D), value));

            return registry;
        }

        static Dictionary<string, Func<int, string>> BuildFallbackByPath()
        {
           // Path-keyed entries used when sRegistry has no match, or when the caller
           // has no target type at all (meta-file YAML parsing). Reach for sRegistry
           // first; only add here when one of the cases below applies.

            Dictionary<string, Func<int, string>> fallback =
                new Dictionary<string, Func<int, string>>();

            // LayerMask struct fields appear on many unrelated components
            // (Camera, Rigidbody, every Collider variant...). Path-keyed avoids
            // listing one entry per component type.
            fallback["m_CullingMask.m_Bits"] = FormatLayerMaskBits;
            fallback["m_IncludeLayers.m_Bits"] = FormatLayerMaskBits;
            fallback["m_ExcludeLayers.m_Bits"] = FormatLayerMaskBits;

            // Meta-file YAML has no SerializedObject, so MetaPropertyTreeBuilder
            // calls TryFormatPath without a target type. The full path includes
            // the document type prefix produced by the YAML parser.
            fallback["AudioImporter.defaultSettings.loadType"] =
                value => FormatEnum(typeof(AudioClipLoadType), value);
            fallback["AudioImporter.defaultSettings.compressionFormat"] =
                value => FormatEnum(typeof(AudioCompressionFormat), value);
            fallback["AudioImporter.defaultSettings.sampleRateSetting"] =
                value => FormatEnum(typeof(AudioSampleRateSetting), value);

            return fallback;
        }

        static void Add(
            Dictionary<string, Func<int, string>> registry,
            Type type,
            string path,
            Func<int, string> formatter)
        {
            registry[BuildKey(type, path)] = formatter;
        }

        const string NOTHING_LABEL = "Nothing";
        const string EVERYTHING_LABEL = "Everything";

        static readonly Dictionary<string, Func<int, string>> mRegistry = BuildRegistry();
        static readonly Dictionary<string, Func<int, string>> mFallbackByPath =
            BuildFallbackByPath();
    }
}
