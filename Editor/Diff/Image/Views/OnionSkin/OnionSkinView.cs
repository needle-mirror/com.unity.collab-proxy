using Unity.PlasticSCM.Editor.Diff.Texture.Toolbar;
using Unity.PlasticSCM.Editor.Diff.Texture.Views.ImageViewer;
using Unity.PlasticSCM.Editor.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Views.OnionSkin
{
    internal class OnionSkinView : VisualElement, IImageDiffView
    {
        internal OnionSkinView()
        {
            BuildComponents();
        }

        internal void SetImages(Texture2D leftTexture, Texture2D rightTexture)
        {
            mOnionSkinImageView.SetImages(leftTexture, rightTexture);
            mOpacitySlider.value = 0.5f;

            if (resolvedStyle.width > 0 && resolvedStyle.height > 0)
            {
                InitZoom();
                return;
            }

            InitZoomWhenLayoutReady();
        }

        internal void SetChannelMode(ColorWriteMask mode)
        {
            mOnionSkinImageView.SetChannelMode(mode);
        }

        internal void Dispose()
        {
            mOnionSkinImageView.Dispose();
            mImageContentView.Dispose();
            mZoomOptionsView.Dispose();
        }

        void IImageDiffView.ZoomIn()
        {
            mImageContentView.ZoomIn();
        }

        void IImageDiffView.ZoomOut()
        {
            mImageContentView.ZoomOut();
        }

        void IImageDiffView.ZoomOneToOne()
        {
            mImageContentView.ZoomOneToOne();
        }

        void IImageDiffView.ZoomToFit()
        {
            mImageContentView.ZoomToFit();
        }

        void InitZoom()
        {
            Vector2 frameSize = new Vector2(
                resolvedStyle.width, resolvedStyle.height);

            if (ImageDiffExtensions.IsImageBiggerThanFrame(
                    frameSize, mOnionSkinImageView.ImageSize))
            {
                mImageContentView.InitZoom(mImageContentView.GetZoomValueToFit());
                return;
            }

            mImageContentView.InitZoom(1f);
        }

        void InitZoomWhenLayoutReady()
        {
            void OnGeometryChanged(GeometryChangedEvent evt)
            {
                UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                InitZoom();
            }

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void BuildComponents()
        {
            style.flexGrow = 1;

            VisualElement sliderPanel = new VisualElement();
            sliderPanel.style.alignItems = Align.Center;
            sliderPanel.style.paddingBottom = 4;
            sliderPanel.style.borderBottomWidth = 1;
            sliderPanel.style.borderBottomColor = UnityStyles.Colors.BarBorder;

            mOpacitySlider = new Slider(0f, 1f);
            mOpacitySlider.value = 0.5f;
            mOpacitySlider.style.minWidth = 300;
            mOpacitySlider.RegisterValueChangedCallback(OnOpacityChanged);
            sliderPanel.Add(mOpacitySlider);

            Add(sliderPanel);

            mOnionSkinImageView = new OnionSkinImageView();
            mImageContentView = new ImageContentView(mOnionSkinImageView);
            mZoomOptionsView = new ZoomOptionsView(this);

            Add(mImageContentView);
            Add(mZoomOptionsView);
        }

        void OnOpacityChanged(ChangeEvent<float> evt)
        {
            mOnionSkinImageView.SetOpacity(evt.newValue);
        }

        OnionSkinImageView mOnionSkinImageView;
        ImageContentView mImageContentView;
        ZoomOptionsView mZoomOptionsView;
        Slider mOpacitySlider;
    }
}
