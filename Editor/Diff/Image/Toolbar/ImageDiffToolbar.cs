using System;

using Unity.PlasticSCM.Editor.Diff.Texture;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Toolbar
{
    internal class ImageDiffToolbar : VisualElement
    {
        internal event Action<ImageDiffMode> ViewModeChanged;
        internal event Action<ColorWriteMask> ChannelModeChanged;

        internal IImageDiffToolbar ModeSwitcher
        {
            get { return mModeSwitcherView; }
        }

        internal ImageDiffToolbar()
        {
            CreateGUI();
        }

        internal void Dispose()
        {
            mModeSwitcherView.ViewModeChanged -= OnViewModeChanged;
            mModeSwitcherView.Dispose();
            mChannelOptionsView.ChannelModeChanged -= OnChannelModeChanged;
            mChannelOptionsView.Dispose();
        }

        void CreateGUI()
        {
            UnityEditor.UIElements.Toolbar toolbar =
                ControlBuilder.Toolbar.Create();

            mModeSwitcherView = new ImageModeSwitcherView();
            mModeSwitcherView.ViewModeChanged += OnViewModeChanged;
            toolbar.Add(mModeSwitcherView);

            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            mChannelOptionsView = new ChannelOptionsView();
            mChannelOptionsView.ChannelModeChanged += OnChannelModeChanged;
            toolbar.Add(mChannelOptionsView);

            Add(toolbar);
        }

        void OnViewModeChanged(ImageDiffMode mode)
        {
            ViewModeChanged?.Invoke(mode);
        }

        void OnChannelModeChanged(ColorWriteMask mode)
        {
            ChannelModeChanged?.Invoke(mode);
        }

        ImageModeSwitcherView mModeSwitcherView;
        ChannelOptionsView mChannelOptionsView;
    }
}
