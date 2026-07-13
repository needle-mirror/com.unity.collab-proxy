using System;
using CodiceApp;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Toolbar
{
    internal class ChannelOptionsView : VisualElement
    {
        internal event Action<ColorWriteMask> ChannelModeChanged;

        internal ChannelOptionsView()
        {
            CreateGUI();
        }

        internal void Dispose()
        {
            mRgbToggle.UnregisterValueChangedCallback(OnChannelToggleChanged);
            mRedToggle.UnregisterValueChangedCallback(OnChannelToggleChanged);
            mGreenToggle.UnregisterValueChangedCallback(OnChannelToggleChanged);
            mBlueToggle.UnregisterValueChangedCallback(OnChannelToggleChanged);
            mAlphaToggle.UnregisterValueChangedCallback(OnChannelToggleChanged);
        }

        // internal for testing
        internal void HandleToggleChanged(
            ToolbarToggle changedToggle, bool newValue)
        {
            if (!newValue)
            {
                changedToggle.SetValueWithoutNotify(true);
                return;
            }

            SetActiveToggle(changedToggle);

            ColorWriteMask mode = GetChannelMode(changedToggle);
            ChannelModeChanged?.Invoke(mode);
        }

        void CreateGUI()
        {
            style.flexDirection = FlexDirection.Row;

            mRgbToggle = CreateChannelToggleLeft(
                Images.GetChannelRGBAAllIcon(),
                PlasticLocalization.Name.ImageChannelShowRGB.GetString());
            mRgbToggle.SetValueWithoutNotify(true);
            Add(mRgbToggle);

            mRedToggle = CreateChannelToggleLeft(
                Images.GetChannelRedIcon(),
                PlasticLocalization.Name.ImageChannelShowRed.GetString());
            Add(mRedToggle);

            mGreenToggle = CreateChannelToggleLeft(
                Images.GetChannelGreenIcon(),
                PlasticLocalization.Name.ImageChannelShowGreen.GetString());
            Add(mGreenToggle);

            mBlueToggle = CreateChannelToggleLeft(
                Images.GetChannelBlueIcon(),
                PlasticLocalization.Name.ImageChannelShowBlue.GetString());
            Add(mBlueToggle);

            mAlphaToggle = CreateChannelToggle(
                Images.GetChannelAlphaIcon(),
                PlasticLocalization.Name.ImageChannelShowAlpha.GetString());
            Add(mAlphaToggle);
        }

        ToolbarToggle CreateChannelToggleLeft(
            UnityEngine.Texture2D icon, string tooltip)
        {
            ToolbarToggle toggle = ControlBuilder.Toolbar.CreateImageToggleLeft(
                icon, tooltip);
            toggle.style.paddingTop = 0;
            toggle.RegisterValueChangedCallback(OnChannelToggleChanged);
            return toggle;
        }

        ToolbarToggle CreateChannelToggle(
            UnityEngine.Texture2D icon, string tooltip)
        {
            ToolbarToggle toggle = ControlBuilder.Toolbar.CreateImageToggle(
                icon, tooltip);
            toggle.style.paddingTop = 0;
            toggle.RegisterValueChangedCallback(OnChannelToggleChanged);
            return toggle;
        }

        void OnChannelToggleChanged(ChangeEvent<bool> evt)
        {
            ToolbarToggle changedToggle = (ToolbarToggle)evt.target;
            HandleToggleChanged(changedToggle, evt.newValue);
        }

        void SetActiveToggle(ToolbarToggle activeToggle)
        {
            SetToggleValueIfNeeded(mRgbToggle, mRgbToggle == activeToggle);
            SetToggleValueIfNeeded(mRedToggle, mRedToggle == activeToggle);
            SetToggleValueIfNeeded(mGreenToggle, mGreenToggle == activeToggle);
            SetToggleValueIfNeeded(mBlueToggle, mBlueToggle == activeToggle);
            SetToggleValueIfNeeded(mAlphaToggle, mAlphaToggle == activeToggle);
        }

        static void SetToggleValueIfNeeded(ToolbarToggle toggle, bool value)
        {
            if (toggle.value != value)
                toggle.SetValueWithoutNotify(value);
        }

        ColorWriteMask GetChannelMode(ToolbarToggle toggle)
        {
            if (toggle == mRedToggle)
                return ColorWriteMask.Red;

            if (toggle == mGreenToggle)
                return ColorWriteMask.Green;

            if (toggle == mBlueToggle)
                return ColorWriteMask.Blue;

            if (toggle == mAlphaToggle)
                return ColorWriteMask.Alpha;

            return ColorWriteMask.All;
        }

        ToolbarToggle mRgbToggle;
        ToolbarToggle mRedToggle;
        ToolbarToggle mGreenToggle;
        ToolbarToggle mBlueToggle;
        ToolbarToggle mAlphaToggle;
    }
}
