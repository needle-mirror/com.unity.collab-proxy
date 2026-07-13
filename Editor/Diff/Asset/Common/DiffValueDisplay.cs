using Unity.PlasticSCM.Editor.Diff.Asset.Common.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Diff.Property;
using Unity.PlasticSCM.Editor.Diff.Asset.Content.Property;
using Unity.PlasticSCM.Editor.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Common
{
    internal class DiffValueDisplay : VisualElement
    {
        internal DiffValueDisplay()
        {
            CreateGUI();
        }

        internal void SetData(
            object tag,
            string fallbackText,
            bool hasContent,
            object counterpartTag)
        {
            HideActiveField();

            if (!hasContent)
                return;

            if (tag is LeafPropertyData leaf)
            {
                LeafPropertyData? counterpart = counterpartTag is LeafPropertyData cp
                    ? cp
                    : (LeafPropertyData?)null;
                ShowTypedField(leaf, counterpart);
                return;
            }

            ShowField(mTextField);
            mTextField.SetValueWithoutNotify(fallbackText ?? string.Empty);
        }

        void CreateGUI()
        {
            style.flexGrow = 1;
            style.overflow = Overflow.Visible;

            mFloatField = AddField(new FloatField { label = string.Empty });
            mIntField = AddField(new IntegerField { label = string.Empty });
            mToggleField = AddField(new Toggle { label = string.Empty });
            mTextField = AddField(new TextField { label = string.Empty });
            mColorField = AddField(new ColorField
            {
                label = string.Empty,
                showAlpha = true,
                hdr = false,
                showEyeDropper = false
            });
            mCurveField = AddField(new CurveField { label = string.Empty });
            mGradientField = AddField(new GradientField { label = string.Empty });
            mVector2Field = AddField(new Vector2Field { label = string.Empty });
            mVector3Field = AddField(new Vector3Field { label = string.Empty });
            mVector4Field = AddField(new Vector4Field { label = string.Empty });
            mVector2IntField = AddField(new Vector2IntField { label = string.Empty });
            mVector3IntField = AddField(new Vector3IntField { label = string.Empty });
            mRectField = AddField(new RectField { label = string.Empty });
            mRectIntField = AddField(new RectIntField { label = string.Empty });
            mBoundsField = AddField(new BoundsField { label = string.Empty });
            mBoundsIntField = AddField(new BoundsIntField { label = string.Empty });
            mHash128Field = AddField(new Hash128Field { label = string.Empty });
        }

        T AddField<T>(T field) where T : VisualElement
        {
            field.SetEnabled(false);
            field.style.opacity = 1f;
            field.style.display = DisplayStyle.None;
            field.style.flexGrow = 1;
            field.style.marginTop = 0;
            field.style.marginBottom = 0;
            field.style.marginLeft = 0;
            field.style.marginRight = 0;
            Add(field);

            field.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                StyleColor readonlyBg = new StyleColor(
                    UnityStyles.Colors.Diff.AssetDiff.ReadonlyFieldBackgroundColor);

                field.Query(className: INPUT_CLASS).ForEach(input =>
                    input.style.backgroundColor = readonlyBg);

                field.Query(className: TOGGLE_CHECKMARK_CLASS).ForEach(checkmark =>
                    checkmark.style.unityBackgroundImageTintColor = new StyleColor(
                        UnityStyles.Colors.Diff.AssetDiff.ReadonlyToggleTintColor));
            });

            return field;
        }

        void HideActiveField()
        {
            if (mActiveField == null)
                return;

            mActiveField.style.display = DisplayStyle.None;
            mActiveField = null;
        }

        void ShowField(VisualElement field)
        {
            field.style.display = DisplayStyle.Flex;
            mActiveField = field;
        }

        void ShowTypedField(LeafPropertyData data, LeafPropertyData? counterpart)
        {
            switch (data.PropertyType)
            {
                case SerializedPropertyType.Float:
                    ShowField(mFloatField);
                    mFloatField.SetValueWithoutNotify(data.FloatValue);
                    return;

                case SerializedPropertyType.Integer:
                    if (data.HasFormattedDisplay)
                    {
                        ShowField(mTextField);
                        mTextField.SetValueWithoutNotify(data.StringValue ?? string.Empty);
                        return;
                    }
                    ShowField(mIntField);
                    mIntField.SetValueWithoutNotify(data.IntValue);
                    return;

                case SerializedPropertyType.LayerMask:
                    if (data.HasFormattedDisplay)
                    {
                        ShowField(mTextField);
                        mTextField.SetValueWithoutNotify(data.StringValue ?? string.Empty);
                        return;
                    }
                    ShowField(mIntField);
                    mIntField.SetValueWithoutNotify(data.IntValue);
                    return;

                case SerializedPropertyType.Character:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.FixedBufferSize:
                    ShowField(mIntField);
                    mIntField.SetValueWithoutNotify(data.IntValue);
                    return;

                case SerializedPropertyType.Boolean:
                    ShowField(mToggleField);
                    mToggleField.SetValueWithoutNotify(data.BoolValue);
                    return;

                case SerializedPropertyType.String:
                    ShowField(mTextField);
                    mTextField.SetValueWithoutNotify(data.StringValue ?? string.Empty);
                    return;

                case SerializedPropertyType.Color:
                    ShowField(mColorField);
                    mColorField.SetValueWithoutNotify(data.ColorValue);
                    return;

                case SerializedPropertyType.AnimationCurve:
                    ShowField(mCurveField);
                    mCurveField.SetValueWithoutNotify(
                        data.AnimationCurveValue ?? new AnimationCurve());
                    return;

                case SerializedPropertyType.Gradient:
                    ShowField(mGradientField);
                    mGradientField.SetValueWithoutNotify(
                        data.GradientValue ?? new Gradient());
                    return;

                case SerializedPropertyType.Vector2:
                    ShowVector2(data.Vector2Value, counterpart);
                    return;

                case SerializedPropertyType.Vector3:
                    ShowVector3(data.Vector3Value, counterpart);
                    return;

                case SerializedPropertyType.Vector4:
                    ShowVector4(data.Vector4Value, counterpart);
                    return;

                case SerializedPropertyType.Quaternion:
                    ShowQuaternion(data.QuaternionValue, counterpart);
                    return;

                case SerializedPropertyType.Vector2Int:
                    ShowVector2Int(data.Vector2IntValue, counterpart);
                    return;

                case SerializedPropertyType.Vector3Int:
                    ShowVector3Int(data.Vector3IntValue, counterpart);
                    return;

                case SerializedPropertyType.Rect:
                    ShowRect(data.RectValue, counterpart);
                    return;

                case SerializedPropertyType.RectInt:
                    ShowRectInt(data.RectIntValue, counterpart);
                    return;

                case SerializedPropertyType.Bounds:
                    ShowBounds(data.BoundsValue, counterpart);
                    return;

                case SerializedPropertyType.BoundsInt:
                    ShowBoundsInt(data.BoundsIntValue, counterpart);
                    return;

                case SerializedPropertyType.Hash128:
                    ShowField(mHash128Field);
                    mHash128Field.SetValueWithoutNotify(data.Hash128Value);
                    return;

                default:
                    ShowField(mTextField);
                    mTextField.SetValueWithoutNotify(data.StringValue ?? string.Empty);
                    return;
            }
        }

        void ShowVector2(Vector2 value, LeafPropertyData? counterpart)
        {
            ShowField(mVector2Field);
            mVector2Field.SetValueWithoutNotify(value);

            float[] otherValues = TryGetVector2Components(counterpart);
            HighlightFloatComponents(
                mVector2Field, new[] { value.x, value.y }, otherValues);
        }

        void ShowVector3(Vector3 value, LeafPropertyData? counterpart)
        {
            ShowField(mVector3Field);
            mVector3Field.SetValueWithoutNotify(value);

            float[] otherValues = TryGetVector3Components(counterpart);
            HighlightFloatComponents(
                mVector3Field, new[] { value.x, value.y, value.z }, otherValues);
        }

        void ShowVector4(Vector4 value, LeafPropertyData? counterpart)
        {
            ShowField(mVector4Field);
            mVector4Field.SetValueWithoutNotify(value);

            float[] otherValues = TryGetVector4Components(counterpart);
            HighlightFloatComponents(
                mVector4Field,
                new[] { value.x, value.y, value.z, value.w },
                otherValues);
        }

        void ShowQuaternion(Quaternion value, LeafPropertyData? counterpart)
        {
            Vector3 euler = value.eulerAngles;
            ShowField(mVector3Field);
            mVector3Field.SetValueWithoutNotify(euler);

            float[] otherValues = null;
            if (counterpart.HasValue &&
                counterpart.Value.PropertyType == SerializedPropertyType.Quaternion)
            {
                Vector3 otherEuler = counterpart.Value.QuaternionValue.eulerAngles;
                otherValues = new[] { otherEuler.x, otherEuler.y, otherEuler.z };
            }

            HighlightFloatComponents(
                mVector3Field, new[] { euler.x, euler.y, euler.z }, otherValues);
        }

        void ShowVector2Int(Vector2Int value, LeafPropertyData? counterpart)
        {
            ShowField(mVector2IntField);
            mVector2IntField.SetValueWithoutNotify(value);

            int[] otherValues = TryGetVector2IntComponents(counterpart);
            HighlightIntComponents(
                mVector2IntField, new[] { value.x, value.y }, otherValues);
        }

        void ShowVector3Int(Vector3Int value, LeafPropertyData? counterpart)
        {
            ShowField(mVector3IntField);
            mVector3IntField.SetValueWithoutNotify(value);

            int[] otherValues = TryGetVector3IntComponents(counterpart);
            HighlightIntComponents(
                mVector3IntField,
                new[] { value.x, value.y, value.z },
                otherValues);
        }

        void ShowRect(Rect value, LeafPropertyData? counterpart)
        {
            ShowField(mRectField);
            mRectField.SetValueWithoutNotify(value);

            float[] otherValues = TryGetRectComponents(counterpart);
            HighlightFloatComponents(
                mRectField,
                new[] { value.x, value.y, value.width, value.height },
                otherValues);
        }

        void ShowRectInt(RectInt value, LeafPropertyData? counterpart)
        {
            ShowField(mRectIntField);
            mRectIntField.SetValueWithoutNotify(value);

            int[] otherValues = TryGetRectIntComponents(counterpart);
            HighlightIntComponents(
                mRectIntField,
                new[] { value.x, value.y, value.width, value.height },
                otherValues);
        }

        void ShowBounds(Bounds value, LeafPropertyData? counterpart)
        {
            ShowField(mBoundsField);
            mBoundsField.SetValueWithoutNotify(value);

            float[] otherValues = TryGetBoundsComponents(counterpart);
            HighlightFloatComponents(
                mBoundsField,
                new[]
                {
                    value.center.x, value.center.y, value.center.z,
                    value.extents.x, value.extents.y, value.extents.z
                },
                otherValues);
        }

        void ShowBoundsInt(BoundsInt value, LeafPropertyData? counterpart)
        {
            ShowField(mBoundsIntField);
            mBoundsIntField.SetValueWithoutNotify(value);

            int[] otherValues = TryGetBoundsIntComponents(counterpart);
            HighlightIntComponents(
                mBoundsIntField,
                new[]
                {
                    value.position.x, value.position.y, value.position.z,
                    value.size.x, value.size.y, value.size.z
                },
                otherValues);
        }

        static float[] TryGetVector2Components(LeafPropertyData? counterpart)
        {
            if (!counterpart.HasValue ||
                counterpart.Value.PropertyType != SerializedPropertyType.Vector2)
                return null;

            Vector2 v = counterpart.Value.Vector2Value;
            return new[] { v.x, v.y };
        }

        static float[] TryGetVector3Components(LeafPropertyData? counterpart)
        {
            if (!counterpart.HasValue ||
                counterpart.Value.PropertyType != SerializedPropertyType.Vector3)
                return null;

            Vector3 v = counterpart.Value.Vector3Value;
            return new[] { v.x, v.y, v.z };
        }

        static float[] TryGetVector4Components(LeafPropertyData? counterpart)
        {
            if (!counterpart.HasValue ||
                counterpart.Value.PropertyType != SerializedPropertyType.Vector4)
                return null;

            Vector4 v = counterpart.Value.Vector4Value;
            return new[] { v.x, v.y, v.z, v.w };
        }

        static int[] TryGetVector2IntComponents(LeafPropertyData? counterpart)
        {
            if (!counterpart.HasValue ||
                counterpart.Value.PropertyType != SerializedPropertyType.Vector2Int)
                return null;

            Vector2Int v = counterpart.Value.Vector2IntValue;
            return new[] { v.x, v.y };
        }

        static int[] TryGetVector3IntComponents(LeafPropertyData? counterpart)
        {
            if (!counterpart.HasValue ||
                counterpart.Value.PropertyType != SerializedPropertyType.Vector3Int)
                return null;

            Vector3Int v = counterpart.Value.Vector3IntValue;
            return new[] { v.x, v.y, v.z };
        }

        static float[] TryGetRectComponents(LeafPropertyData? counterpart)
        {
            if (!counterpart.HasValue ||
                counterpart.Value.PropertyType != SerializedPropertyType.Rect)
                return null;

            Rect r = counterpart.Value.RectValue;
            return new[] { r.x, r.y, r.width, r.height };
        }

        static int[] TryGetRectIntComponents(LeafPropertyData? counterpart)
        {
            if (!counterpart.HasValue ||
                counterpart.Value.PropertyType != SerializedPropertyType.RectInt)
                return null;

            RectInt r = counterpart.Value.RectIntValue;
            return new[] { r.x, r.y, r.width, r.height };
        }

        static float[] TryGetBoundsComponents(LeafPropertyData? counterpart)
        {
            if (!counterpart.HasValue ||
                counterpart.Value.PropertyType != SerializedPropertyType.Bounds)
                return null;

            Bounds b = counterpart.Value.BoundsValue;
            return new[]
            {
                b.center.x, b.center.y, b.center.z,
                b.extents.x, b.extents.y, b.extents.z
            };
        }

        static int[] TryGetBoundsIntComponents(LeafPropertyData? counterpart)
        {
            if (!counterpart.HasValue ||
                counterpart.Value.PropertyType != SerializedPropertyType.BoundsInt)
                return null;

            BoundsInt b = counterpart.Value.BoundsIntValue;
            return new[]
            {
                b.position.x, b.position.y, b.position.z,
                b.size.x, b.size.y, b.size.z
            };
        }

        static void HighlightFloatComponents(
            VisualElement vectorField,
            float[] values,
            float[] counterpartValues)
        {
            var floatFields = vectorField.Query<FloatField>().ToList();
            bool hasComparison = counterpartValues != null;

            for (int i = 0; i < floatFields.Count && i < values.Length; i++)
            {
                bool changed = hasComparison
                    && i < counterpartValues.Length
                    && !Mathf.Approximately(values[i], counterpartValues[i]);

                ApplyInputHighlight(floatFields[i], changed, hasComparison);
            }
        }

        static void HighlightIntComponents(
            VisualElement vectorField,
            int[] values,
            int[] counterpartValues)
        {
            var intFields = vectorField.Query<IntegerField>().ToList();
            bool hasComparison = counterpartValues != null;

            for (int i = 0; i < intFields.Count && i < values.Length; i++)
            {
                bool changed = hasComparison
                    && i < counterpartValues.Length
                    && values[i] != counterpartValues[i];

                ApplyInputHighlight(intFields[i], changed, hasComparison);
            }
        }

        static void ApplyInputHighlight(
            VisualElement field, bool highlight, bool hasComparison)
        {
            VisualElement input = field.Q(className: INPUT_CLASS);
            if (input == null)
                return;

            StyleColor color = highlight
                ? new StyleColor(UnityStyles.Colors.Diff.AssetDiff.VectorComponentHighlight)
                : new StyleColor(StyleKeyword.Null);

            input.style.borderTopColor = color;
            input.style.borderBottomColor = color;
            input.style.borderLeftColor = color;
            input.style.borderRightColor = color;

            input.style.opacity = (highlight || !hasComparison) ? 1f : 0.4f;
        }

        VisualElement mActiveField;

        FloatField mFloatField;
        IntegerField mIntField;
        Toggle mToggleField;
        TextField mTextField;
        ColorField mColorField;
        CurveField mCurveField;
        GradientField mGradientField;
        Vector2Field mVector2Field;
        Vector3Field mVector3Field;
        Vector4Field mVector4Field;
        Vector2IntField mVector2IntField;
        Vector3IntField mVector3IntField;
        RectField mRectField;
        RectIntField mRectIntField;
        BoundsField mBoundsField;
        BoundsIntField mBoundsIntField;
        Hash128Field mHash128Field;

        const string INPUT_CLASS = "unity-base-text-field__input";
        const string TOGGLE_CHECKMARK_CLASS = "unity-toggle__checkmark";
    }
}
