using System;

using Unity.PlasticSCM.Editor.Diff.Texture;
using Unity.PlasticSCM.Editor.UI.UIElements;

using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Toolbar
{
    internal class ImageModeSwitcherView : VisualElement, IImageDiffToolbar
    {
        internal event Action<ImageDiffMode> ViewModeChanged;

        internal ImageModeSwitcherView()
        {
            CreateGUI();
        }

        internal void Dispose()
        {
            mOnionSkinToggle.UnregisterValueChangedCallback(OnModeToggleChanged);
            mSideBySideToggle.UnregisterValueChangedCallback(OnModeToggleChanged);
            mDifferencesToggle.UnregisterValueChangedCallback(OnModeToggleChanged);
            mSwipeToggle.UnregisterValueChangedCallback(OnModeToggleChanged);
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

            ImageDiffMode mode = GetMode(changedToggle);
            ViewModeChanged?.Invoke(mode);
        }

        void IImageDiffToolbar.EnableDifferencesMode()
        {
            mDifferencesToggle.SetEnabled(true);
        }

        void IImageDiffToolbar.DisableDifferencesMode()
        {
            mDifferencesToggle.SetEnabled(false);
        }

        void CreateGUI()
        {
            style.flexDirection = FlexDirection.Row;

            mOnionSkinToggle = CreateModeToggle("Onion skin");
            Add(mOnionSkinToggle);

            mSideBySideToggle = CreateModeToggle("Side by side");
            mSideBySideToggle.SetValueWithoutNotify(true);
            Add(mSideBySideToggle);

            mDifferencesToggle = CreateModeToggle("Differences");
            Add(mDifferencesToggle);

            mSwipeToggle = CreateModeToggle("Swipe");
            Add(mSwipeToggle);
        }

        ToolbarToggle CreateModeToggle(string label)
        {
            ToolbarToggle toggle = new ToolbarToggle();
            toggle.text = label;
            toggle.style.paddingTop = 0;
            toggle.RegisterValueChangedCallback(OnModeToggleChanged);
            return toggle;
        }

        void OnModeToggleChanged(ChangeEvent<bool> evt)
        {
            ToolbarToggle changedToggle = (ToolbarToggle)evt.target;
            HandleToggleChanged(changedToggle, evt.newValue);
        }

        void SetActiveToggle(ToolbarToggle activeToggle)
        {
            SetToggleValueIfNeeded(mOnionSkinToggle, mOnionSkinToggle == activeToggle);
            SetToggleValueIfNeeded(mSideBySideToggle, mSideBySideToggle == activeToggle);
            SetToggleValueIfNeeded(mDifferencesToggle, mDifferencesToggle == activeToggle);
            SetToggleValueIfNeeded(mSwipeToggle, mSwipeToggle == activeToggle);
        }

        static void SetToggleValueIfNeeded(ToolbarToggle toggle, bool value)
        {
            if (toggle.value != value)
                toggle.SetValueWithoutNotify(value);
        }

        ImageDiffMode GetMode(ToolbarToggle toggle)
        {
            if (toggle == mOnionSkinToggle)
                return ImageDiffMode.OnionSkin;

            if (toggle == mDifferencesToggle)
                return ImageDiffMode.Differences;

            if (toggle == mSwipeToggle)
                return ImageDiffMode.Swipe;

            return ImageDiffMode.SideBySide;
        }

        ToolbarToggle mOnionSkinToggle;
        ToolbarToggle mSideBySideToggle;
        ToolbarToggle mDifferencesToggle;
        ToolbarToggle mSwipeToggle;
    }
}
