using Unity.PlasticSCM.Editor.Diff.Texture.Toolbar;
using Unity.PlasticSCM.Editor.Diff.Texture.Views.ImageViewer;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Views.Differences
{
    internal class DifferencesView : VisualElement, IImageDiffView
    {
        internal DifferencesView()
        {
            BuildComponents();
        }

        internal void SetImage(UnityEngine.Texture preview)
        {
            mImageView.SetImage(preview);

            if (resolvedStyle.width > 0 && resolvedStyle.height > 0)
            {
                InitZoom(preview);
                return;
            }

            InitZoomWhenLayoutReady(preview);
        }

        internal void CleanImage()
        {
            mImageView.CleanImage();
        }

        internal void SetChannelMode(ColorWriteMask mode)
        {
            mImageView.SetChannelMode(mode);
        }

        internal void EnableZoomButtons()
        {
            mZoomOptionsView.EnableButtons();
        }

        internal void DisableZoomButtons()
        {
            mZoomOptionsView.DisableButtons();
        }

        internal void Dispose()
        {
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

        void InitZoomWhenLayoutReady(UnityEngine.Texture image)
        {

            void OnGeometryChanged(GeometryChangedEvent evt)
            {
                UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                InitZoom(image);
            }

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void InitZoom(UnityEngine.Texture image)
        {
            if (image == null)
                return;

            Vector2 frameSize = new Vector2(
                resolvedStyle.width, resolvedStyle.height);
            Vector2 imageSize = new Vector2(image.width, image.height);

            if (ImageDiffExtensions.IsImageBiggerThanFrame(frameSize, imageSize))
            {
                mImageContentView.InitZoom(mImageContentView.GetZoomValueToFit());
                return;
            }

            mImageContentView.InitZoom(1f);
        }

        void BuildComponents()
        {
            style.flexGrow = 1;

            mImageView = new ImageView();
            mImageContentView = new ImageContentView(mImageView);
            mZoomOptionsView = new ZoomOptionsView(this);

            Add(mImageContentView);
            Add(mZoomOptionsView);
        }

        ImageView mImageView;
        ImageContentView mImageContentView;
        ZoomOptionsView mZoomOptionsView;
    }
}
