using System;

using Unity.PlasticSCM.Editor.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Views.OnionSkin
{
    internal class OnionSkinImageView : VisualElement, IZoomableImageView
    {
        internal OnionSkinImageView()
        {
            style.overflow = Overflow.Hidden;

            mLeftImage = new IMGUIContainer(DrawLeftImage);
            mLeftImage.style.position = Position.Absolute;
            SetBorderColor(mLeftImage,
                UnityStyles.Colors.ImageDiff.LeftImageBorderColor);
            Add(mLeftImage);

            mRightImage = new IMGUIContainer(DrawRightImage);
            mRightImage.style.position = Position.Absolute;
            mRightImage.style.opacity = 0.5f;
            SetBorderColor(mRightImage,
                UnityStyles.Colors.ImageDiff.RightImageBorderColor);
            Add(mRightImage);
        }

        public Vector2 ImageSize
        {
            get { return mComposedSize; }
        }

        public void SetSize(float width, float height)
        {
            style.width = width;
            style.height = height;

            UpdateImagePositions(width, height);
        }

        internal void SetImages(Texture2D leftTexture, Texture2D rightTexture)
        {
            mLeftTexture = leftTexture;
            mRightTexture = rightTexture;

            UpdateComposedSize(leftTexture, rightTexture);

            mLeftImage.MarkDirtyRepaint();
            mLeftImage.MarkDirtyRepaint();
        }

        internal void SetOpacity(float opacity)
        {
            mRightOpacity = opacity;
            mRightImage.MarkDirtyRepaint();
        }

        internal void SetChannelMode(ColorWriteMask mode)
        {
            if (mChannelMode == mode)
                return;

            mChannelMode = mode;
            mLeftImage.MarkDirtyRepaint();
            mRightImage.MarkDirtyRepaint();
        }

        internal void Dispose()
        {
            mLeftTexture = null;
            mRightTexture = null;
        }

        void UpdateComposedSize(Texture2D leftTexture, Texture2D rightTexture)
        {
            int leftW = leftTexture != null ? leftTexture.width : 0;
            int leftH = leftTexture != null ? leftTexture.height : 0;
            int rightW = rightTexture != null ? rightTexture.width : 0;
            int rightH = rightTexture != null ? rightTexture.height : 0;

            mComposedSize = new Vector2(
                Math.Max(leftW, rightW),
                Math.Max(leftH, rightH));
        }

        void UpdateImagePositions(float containerWidth, float containerHeight)
        {
            if (mComposedSize.x <= 0 || mComposedSize.y <= 0)
                return;

            UpdateImagePosition(
                mLeftImage, mLeftTexture, containerWidth, containerHeight);
            UpdateImagePosition(
                mRightImage, mRightTexture, containerWidth, containerHeight);
        }

        void UpdateImagePosition(
            IMGUIContainer image,
            Texture2D texture,
            float containerWidth,
            float containerHeight)
        {
            if (texture == null)
                return;

            float imageWidth = containerWidth * texture.width / mComposedSize.x;
            float imageHeight = containerHeight * texture.height / mComposedSize.y;

            image.style.left = (containerWidth - imageWidth) / 2f;
            image.style.top = (containerHeight - imageHeight) / 2f;
            image.style.width = imageWidth;
            image.style.height = imageHeight;
        }

        static void SetBorderColor(VisualElement element, Color color)
        {
            element.style.borderTopWidth = 1;
            element.style.borderBottomWidth = 1;
            element.style.borderLeftWidth = 1;
            element.style.borderRightWidth = 1;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
        }

        void DrawLeftImage()
        {
            DrawTexture.WithColorMask(
                mLeftImage.contentRect,
                mLeftTexture,
                ScaleMode.ScaleToFit,
                mChannelMode);
        }

        void DrawRightImage()
        {
            DrawTexture.DrawWithColorMaskAndOpacity(
                mRightImage.contentRect,
                mRightTexture,
                ScaleMode.ScaleToFit,
                mChannelMode,
                mRightOpacity);
        }

        float mRightOpacity = 0.5f;

        IMGUIContainer mLeftImage;
        IMGUIContainer mRightImage;
        Texture2D mLeftTexture;
        Texture2D mRightTexture;

        ColorWriteMask mChannelMode = ColorWriteMask.All;
        Vector2 mComposedSize;
    }
}
