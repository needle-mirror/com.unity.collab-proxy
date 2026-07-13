using Unity.PlasticSCM.Editor.Diff.Texture.Toolbar;
using Unity.PlasticSCM.Editor.Diff.Texture.Views.ImageViewer;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Views.Swipe
{
    internal class SwipeView : VisualElement, IImageDiffView
    {
        internal SwipeView()
        {
            BuildComponents();
        }

        internal void SetImages(Texture2D leftTexture, Texture2D rightTexture)
        {
            mSwipeImageView.SetImages(leftTexture, rightTexture);

            if (resolvedStyle.width > 0 && resolvedStyle.height > 0)
            {
                InitZoom();
                return;
            }

            InitZoomWhenLayoutReady();
        }

        internal void SetChannelMode(ColorWriteMask mode)
        {
            mSwipeImageView.SetChannelMode(mode);
        }

        internal void Dispose()
        {
            mSwipeImageView.Dispose();
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
                    frameSize, ((IZoomableImageView)mSwipeImageView).ImageSize))
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

            mSwipeImageView = new SwipeImageView();
            mImageContentView = new ImageContentView(mSwipeImageView);
            mZoomOptionsView = new ZoomOptionsView(this);

            Add(mImageContentView);
            Add(mZoomOptionsView);
        }

        SwipeImageView mSwipeImageView;
        ImageContentView mImageContentView;
        ZoomOptionsView mZoomOptionsView;
    }
}
