using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Views.ImageViewer
{
    internal class ImageView : VisualElement, IZoomableImageView
    {
        Vector2 IZoomableImageView.ImageSize
        {
            get { return ImageSize; }
        }

        void IZoomableImageView.SetSize(float width, float height)
        {
            SetSize(width, height);
        }

        internal Vector2 ImageSize
        {
            get
            {
                if (mTexture == null)
                    return Vector2.zero;

                return new Vector2(mTexture.width, mTexture.height);
            }
        }

        internal ImageView()
        {
            style.overflow = Overflow.Hidden;

            mImageElement = new IMGUIContainer(DrawImage);
            mImageElement.name = "image";
            mImageElement.style.position = Position.Absolute;
            mImageElement.style.top = 0;
            mImageElement.style.left = 0;
            mImageElement.style.right = 0;
            mImageElement.style.bottom = 0;
            Add(mImageElement);

            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;

        }

        internal void SetBorderColor(Color color)
        {
            style.borderTopColor = color;
            style.borderBottomColor = color;
            style.borderLeftColor = color;
            style.borderRightColor = color;
        }

        internal void SetImage(UnityEngine.Texture texture)
        {
            mTexture = texture;
            mImageElement.MarkDirtyRepaint();
        }

        internal void CleanImage()
        {
            mTexture = null;
        }

        internal void SetChannelMode(ColorWriteMask mode)
        {
            if (mChannelMode == mode)
                return;

            mChannelMode = mode;
            mImageElement.MarkDirtyRepaint();
        }

        internal void SetSize(float width, float height)
        {
            style.width = width;
            style.height = height;
        }

        void DrawImage()
        {
            DrawTexture.WithColorMask(
                mImageElement.contentRect,
                mTexture,
                ScaleMode.ScaleToFit,
                mChannelMode);
        }

        IMGUIContainer mImageElement;
        UnityEngine.Texture mTexture;
        ColorWriteMask mChannelMode = ColorWriteMask.All;
        Texture2D mFilteredTexture;
    }
}
