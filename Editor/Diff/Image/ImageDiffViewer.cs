using System;

using ImagesGui.ImageDiff;

using Unity.PlasticSCM.Editor.Diff.Texture.Views;
using Unity.PlasticSCM.Editor.Diff.Texture.Views.Differences;
using Unity.PlasticSCM.Editor.Diff.Texture.Views.OnionSkin;
using Unity.PlasticSCM.Editor.Diff.Texture.Views.SideBySide;
using Unity.PlasticSCM.Editor.Diff.Texture.Views.Swipe;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture
{
    internal class ImageDiffViewer : VisualElement
    {
        internal ImageDiffViewer(IImageDiffToolbar toolbar)
        {
            mToolbar = toolbar;

            style.flexGrow = 1;
        }

        internal void Dispose()
        {
            DestroyTextures();
            DestroyDiffTexture();

            if (mOnionSkinView != null)
                mOnionSkinView.Dispose();

            if (mSideBySideView != null)
                mSideBySideView.Dispose();

            if (mDifferencesView != null)
                mDifferencesView.Dispose();

            if (mSwipeView != null)
                mSwipeView.Dispose();
        }

        internal void SetInfo(
            string leftFile, string rightFile,
            string leftSymbolicName, string rightSymbolicName)
        {
            DestroyTextures();
            DestroyDiffTexture();

            mLeftTexture = TextureLoader.LoadFromFile(leftFile);
            mRightTexture = TextureLoader.LoadFromFile(rightFile);

            mToolbar.EnableDifferencesMode();

            UpdateCurrentView();
        }

        internal void SetViewMode(ImageDiffMode mode)
        {
            if (mCurrentMode == mode)
                return;

            mCurrentMode = mode;
            UpdateCurrentView();
        }

        internal void SetChannelMode(ColorWriteMask mode)
        {
            mChannelMode = mode;

            if (mDifferencesView != null)
                mDifferencesView.SetChannelMode(mode);

            if (mSideBySideView != null)
                mSideBySideView.SetChannelMode(mode);

            if (mOnionSkinView != null)
                mOnionSkinView.SetChannelMode(mode);

            if (mSwipeView != null)
                mSwipeView.SetChannelMode(mode);
        }

        internal ImageDiffMode CurrentViewMode { get { return mCurrentMode; } }

        internal ColorWriteMask CurrentChannelMode { get { return mChannelMode; } }

        void UpdateCurrentView()
        {
            if (mLeftTexture == null && mRightTexture == null)
                return;

            VisualElement view = GetOrCreateView(mCurrentMode);

            ReplaceView(view);
        }

        VisualElement GetOrCreateView(ImageDiffMode mode)
        {
            switch (mode)
            {
                case ImageDiffMode.OnionSkin:
                    return GetOrCreateOnionSkinView();
                case ImageDiffMode.Differences:
                    return GetOrCreateDifferencesView();
                case ImageDiffMode.Swipe:
                    return GetOrCreateSwipeView();
                case ImageDiffMode.SideBySide:
                default:
                    return GetOrCreateSideBySideView();
            }
        }

        VisualElement GetOrCreateOnionSkinView()
        {
            if (mOnionSkinView == null)
                mOnionSkinView = new OnionSkinView();

            mOnionSkinView.SetImages(mLeftTexture, mRightTexture);
            mOnionSkinView.SetChannelMode(mChannelMode);
            return mOnionSkinView;
        }

        VisualElement GetOrCreateSideBySideView()
        {
            if (mSideBySideView == null)
                mSideBySideView = new SideBySideView();

            mSideBySideView.SetImages(mLeftTexture, mRightTexture);
            mSideBySideView.SetChannelMode(mChannelMode);
            return mSideBySideView;
        }

        VisualElement GetOrCreateDifferencesView()
        {
            if (mDifferencesView == null)
                mDifferencesView = new DifferencesView();

            if (mDiffTexture == null)
                mDiffTexture = ComputePixelDiff(mLeftTexture, mRightTexture);

            mDifferencesView.SetImage(mDiffTexture);
            mDifferencesView.EnableZoomButtons();
            mDifferencesView.SetChannelMode(mChannelMode);
            return mDifferencesView;
        }

        VisualElement GetOrCreateSwipeView()
        {
            if (mSwipeView == null)
                mSwipeView = new SwipeView();

            mSwipeView.SetImages(mLeftTexture, mRightTexture);
            mSwipeView.SetChannelMode(mChannelMode);
            return mSwipeView;
        }

        static unsafe Texture2D ComputePixelDiff(
            Texture2D leftTexture, Texture2D rightTexture)
        {
            ImagePixelDiff.GetDiffImageSize(
                leftTexture != null ? leftTexture.width : 0,
                leftTexture != null ? leftTexture.height : 0,
                rightTexture != null ? rightTexture.width : 0,
                rightTexture != null ? rightTexture.height : 0,
                out int diffWidth, out int diffHeight);

            if (diffWidth <= 0 || diffHeight <= 0)
                return null;

            Color32[] leftPixels = leftTexture != null
                ? leftTexture.GetPixels32()
                : new Color32[0];
            Color32[] rightPixels = rightTexture != null
                ? rightTexture.GetPixels32()
                : new Color32[0];
            Color32[] diffPixels = new Color32[diffWidth * diffHeight];

            int bytesPerPixel = 4;
            int leftStride = leftTexture != null
                ? leftTexture.width * bytesPerPixel : 0;
            int rightStride = rightTexture != null
                ? rightTexture.width * bytesPerPixel : 0;
            int diffStride = diffWidth * bytesPerPixel;

            fixed (Color32* pLeft = leftPixels)
            fixed (Color32* pRight = rightPixels)
            fixed (Color32* pDiff = diffPixels)
            {
                ImagePixelDiff.PixelDiff(
                    (byte*)pLeft,
                    leftTexture != null ? leftTexture.width : 0,
                    leftTexture != null ? leftTexture.height : 0,
                    leftStride,
                    (byte*)pRight,
                    rightTexture != null ? rightTexture.width : 0,
                    rightTexture != null ? rightTexture.height : 0,
                    rightStride,
                    (byte*)pDiff,
                    diffWidth, diffHeight, diffStride,
                    bytesPerPixel);
            }

            Texture2D diffTexture = new Texture2D(
                diffWidth, diffHeight, TextureFormat.RGBA32, false);
            diffTexture.filterMode = FilterMode.Bilinear;
            diffTexture.hideFlags = HideFlags.HideAndDontSave;
            diffTexture.SetPixels32(diffPixels);
            diffTexture.Apply();

            return diffTexture;
        }

        void ReplaceView(VisualElement view)
        {
            if (mCurrentView == view)
                return;

            if (mCurrentView != null)
                Remove(mCurrentView);

            Add(view);
            mCurrentView = view;
        }

        void DestroyTextures()
        {
            if (mLeftTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(mLeftTexture);
                mLeftTexture = null;
            }

            if (mRightTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(mRightTexture);
                mRightTexture = null;
            }
        }

        void DestroyDiffTexture()
        {
            if (mDiffTexture == null)
                return;

            UnityEngine.Object.DestroyImmediate(mDiffTexture);
            mDiffTexture = null;
        }

        VisualElement mCurrentView;
        ImageDiffMode mCurrentMode = ImageDiffMode.SideBySide;
        ColorWriteMask mChannelMode = ColorWriteMask.All;

        Texture2D mLeftTexture;
        Texture2D mRightTexture;
        Texture2D mDiffTexture;

        OnionSkinView mOnionSkinView;
        SideBySideView mSideBySideView;
        DifferencesView mDifferencesView;
        SwipeView mSwipeView;

        readonly IImageDiffToolbar mToolbar;
    }
}
