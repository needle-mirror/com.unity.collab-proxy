using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEngine.Rendering;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Views.Swipe
{
    internal class SwipeImageView : VisualElement, IZoomableImageView
    {
        internal SwipeImageView()
        {
            CreateGUI();

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        internal void SetImages(Texture2D leftTexture, Texture2D rightTexture)
        {
            mLeftTexture = leftTexture;
            mRightTexture = rightTexture;

            UpdateComposedSize(leftTexture, rightTexture);

            mSwipePosition = 0.5f;

            mLeftImage.MarkDirtyRepaint();
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
            UnregisterCallback<PointerDownEvent>(OnPointerDown);
            UnregisterCallback<PointerUpEvent>(OnPointerUp);
            UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            UnregisterScrollCallbacks();
        }

        Vector2 IZoomableImageView.ImageSize
        {
            get { return mComposedSize; }
        }

        void IZoomableImageView.SetSize(float width, float height)
        {
            style.width = width;
            style.height = height;

            UpdateLayout(width, height);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            mScrollView = GetFirstAncestorOfType<ScrollView>();
            RegisterScrollCallbacks();
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            UnregisterScrollCallbacks();
        }

        void RegisterScrollCallbacks()
        {
            if (mScrollView == null)
                return;

            mScrollView.verticalScroller.valueChanged += OnScrollChanged;
            mScrollView.contentViewport.RegisterCallback<GeometryChangedEvent>(
                OnViewportGeometryChanged);
        }

        void UnregisterScrollCallbacks()
        {
            if (mScrollView == null)
                return;

            mScrollView.verticalScroller.valueChanged -= OnScrollChanged;
            mScrollView.contentViewport.UnregisterCallback<GeometryChangedEvent>(
                OnViewportGeometryChanged);
            mScrollView = null;
        }

        void OnScrollChanged(float value)
        {
            UpdateHandleVerticalPosition();
        }

        void OnViewportGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateHandleVerticalPosition();
        }

        void UpdateHandleVerticalPosition()
        {
            float containerHeight = resolvedStyle.height;
            float containerWidth = resolvedStyle.width;

            if (float.IsNaN(containerHeight) || containerHeight <= 0)
                return;
            if (float.IsNaN(containerWidth) || containerWidth <= 0)
                return;

            mSwipeHandle.visible =
                containerHeight > HANDLE_SIZE * 2 &&
                containerWidth > HANDLE_SIZE * 2;
            mSwipeHandle.style.top = GetHandleCenterY(containerHeight) - HANDLE_SIZE / 2f;
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

        void OnPointerDown(PointerDownEvent evt)
        {
            mIsDragging = true;
            this.CapturePointer(evt.pointerId);
            UpdateSwipeFromPointer(evt.localPosition);
            evt.StopPropagation();
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            mIsDragging = false;

            if (this.HasPointerCapture(evt.pointerId))
                this.ReleasePointer(evt.pointerId);
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (!mIsDragging)
                return;

            UpdateSwipeFromPointer(evt.localPosition);
        }

        void UpdateSwipeFromPointer(Vector2 localPosition)
        {
            float width = resolvedStyle.width;

            if (float.IsNaN(width) || width <= 0)
                return;

            mSwipePosition = Mathf.Clamp01(localPosition.x / width);
            UpdateLayout(width, resolvedStyle.height);
        }

        void UpdateLayout(float containerWidth, float containerHeight)
        {
            if (containerWidth <= 0 || containerHeight <= 0)
                return;

            float swipeX = containerWidth * mSwipePosition;

            mLeftClipContainer.style.width = swipeX;
            mRightClipContainer.style.left = swipeX;

            UpdateImagePosition(
                mLeftImage, mLeftTexture, 0, containerWidth, containerHeight);
            UpdateImagePosition(
                mRightImage, mRightTexture, swipeX, containerWidth, containerHeight);

            mSwipeLine.style.left = swipeX - SWIPE_LINE_HIT_WIDTH / 2f;

            mSwipeHandle.visible =
                containerHeight > HANDLE_SIZE * 2 &&
                containerWidth > HANDLE_SIZE * 2;
            mSwipeHandle.style.left = swipeX - HANDLE_SIZE / 2f;
            mSwipeHandle.style.top = GetHandleCenterY(containerHeight) - HANDLE_SIZE / 2f;
        }

        void UpdateImagePosition(
            IMGUIContainer image,
            Texture2D texture,
            float clipOffset,
            float containerWidth,
            float containerHeight)
        {
            if (texture == null || mComposedSize.x <= 0 || mComposedSize.y <= 0)
                return;

            float imageWidth = containerWidth * texture.width / mComposedSize.x;
            float imageHeight = containerHeight * texture.height / mComposedSize.y;

            image.style.left = (containerWidth - imageWidth) / 2f - clipOffset;
            image.style.top = (containerHeight - imageHeight) / 2f;
            image.style.width = imageWidth;
            image.style.height = imageHeight;
        }

        float GetHandleCenterY(float containerHeight)
        {
            if (mScrollView == null)
                return containerHeight / 2f;

            VisualElement viewport = mScrollView.contentViewport;
            float viewportHeight = viewport.resolvedStyle.height;

            if (float.IsNaN(viewportHeight) || viewportHeight <= 0)
                return containerHeight / 2f;

            if (containerHeight <= viewportHeight)
                return containerHeight / 2f;

            Vector2 viewportCenter = new Vector2(0, viewportHeight / 2f);
            Vector2 localCenter = viewport.ChangeCoordinatesTo(this, viewportCenter);

            return Mathf.Clamp(
                localCenter.y, HANDLE_SIZE / 2f,
                containerHeight - HANDLE_SIZE / 2f);
        }

        void CreateGUI()
        {
            style.overflow = Overflow.Visible;

            mLeftClipContainer = CreateClipContainer(isLeft: true);
            Add(mLeftClipContainer);

            mLeftImage = new IMGUIContainer(DrawLeftImage);
            mLeftImage.pickingMode = PickingMode.Ignore;
            mLeftImage.style.position = Position.Absolute;

            SetBorderColor(mLeftImage,
                UnityStyles.Colors.ImageDiff.LeftImageBorderColor);
            mLeftClipContainer.Add(mLeftImage);

            mRightClipContainer = CreateClipContainer(isLeft: false);
            Add(mRightClipContainer);

            mRightImage = new IMGUIContainer(DrawRightImage);
            mRightImage.pickingMode = PickingMode.Ignore;
            mRightImage.style.position = Position.Absolute;

            SetBorderColor(mRightImage,
                UnityStyles.Colors.ImageDiff.RightImageBorderColor);
            mRightClipContainer.Add(mRightImage);

            mSwipeLine = CreateSwipeLine();
            Add(mSwipeLine);

            mSwipeHandle = CreateSwipeHandle();
            Add(mSwipeHandle);
        }

        static IMGUIContainer CreateCheckerboard()
        {
            IMGUIContainer container = new IMGUIContainer();
            container.pickingMode = PickingMode.Ignore;
            container.style.position = Position.Absolute;
            container.style.top = 0;
            container.style.left = 0;
            container.style.right = 0;
            container.style.bottom = 0;
            return container;
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
            DrawTexture.WithColorMask(
                mRightImage.contentRect,
                mRightTexture,
                ScaleMode.ScaleToFit,
                mChannelMode);
        }


        static VisualElement CreateClipContainer(bool isLeft)
        {
            VisualElement container = new VisualElement();
            container.pickingMode = PickingMode.Ignore;
            container.style.position = Position.Absolute;
            container.style.top = 0;
            container.style.bottom = 0;
            container.style.overflow = Overflow.Hidden;

            if (isLeft)
                container.style.left = 0;
            else
                container.style.right = 0;

            return container;
        }

        static VisualElement CreateSwipeLine()
        {
            VisualElement line = new VisualElement();
            line.style.position = Position.Absolute;
            line.style.top = 0;
            line.style.bottom = 0;
            line.style.width = SWIPE_LINE_HIT_WIDTH;
            line.SetMouseCursor(MouseCursor.SplitResizeLeftRight);

            VisualElement lineVisual = new VisualElement();
            lineVisual.pickingMode = PickingMode.Ignore;
            lineVisual.style.position = Position.Absolute;
            lineVisual.style.top = 0;
            lineVisual.style.bottom = 0;
            lineVisual.style.width = SWIPE_LINE_VISUAL_WIDTH;
            lineVisual.style.left = (SWIPE_LINE_HIT_WIDTH - SWIPE_LINE_VISUAL_WIDTH) / 2f;
            lineVisual.style.backgroundColor =
                UnityStyles.Colors.ImageDiff.SwipeHandleBorderColor;
            line.Add(lineVisual);

            return line;
        }

        static VisualElement CreateSwipeHandle()
        {
            VisualElement handle = new VisualElement();
            handle.style.position = Position.Absolute;
            handle.style.width = HANDLE_SIZE;
            handle.style.height = HANDLE_SIZE;
            handle.style.borderTopLeftRadius = HANDLE_SIZE / 2f;
            handle.style.borderTopRightRadius = HANDLE_SIZE / 2f;
            handle.style.borderBottomLeftRadius = HANDLE_SIZE / 2f;
            handle.style.borderBottomRightRadius = HANDLE_SIZE / 2f;
            handle.style.backgroundColor =
                UnityStyles.Colors.ImageDiff.SwipeHandleBackgroundColor;
            handle.style.borderTopColor =
                UnityStyles.Colors.ImageDiff.SwipeHandleBorderColor;
            handle.style.borderBottomColor =
                UnityStyles.Colors.ImageDiff.SwipeHandleBorderColor;
            handle.style.borderLeftColor =
                UnityStyles.Colors.ImageDiff.SwipeHandleBorderColor;
            handle.style.borderRightColor =
                UnityStyles.Colors.ImageDiff.SwipeHandleBorderColor;
            handle.style.borderTopWidth = SWIPE_LINE_VISUAL_WIDTH;
            handle.style.borderBottomWidth = SWIPE_LINE_VISUAL_WIDTH;
            handle.style.borderLeftWidth = SWIPE_LINE_VISUAL_WIDTH;
            handle.style.borderRightWidth = SWIPE_LINE_VISUAL_WIDTH;
            handle.SetMouseCursor(MouseCursor.SplitResizeLeftRight);

            handle.Add(CreateHandleIcon(
                Images.GetArrowCaretLeftIcon(), isLeft: true));
            handle.Add(CreateHandleIcon(
                Images.GetArrowCaretRightIcon(), isLeft: false));

            return handle;
        }

        static Image CreateHandleIcon(Texture2D icon, bool isLeft)
        {
            Image image = new Image();
            image.pickingMode = PickingMode.Ignore;
            image.image = icon;
            image.scaleMode = ScaleMode.ScaleToFit;
            image.style.position = Position.Absolute;
            image.style.width = HANDLE_ICON_SIZE;
            image.style.height = HANDLE_ICON_SIZE;
            image.style.top = HANDLE_ICON_VERTICAL_MARGIN;
            image.tintColor = UnityStyles.Colors.ImageDiff.SwipeHandleBorderColor;

            if (isLeft)
                image.style.left = HANDLE_ICON_HORIZONTAL_MARGIN;
            else
                image.style.right = HANDLE_ICON_HORIZONTAL_MARGIN;

            return image;
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

        VisualElement mLeftClipContainer;
        VisualElement mRightClipContainer;
        IMGUIContainer mLeftImage;
        IMGUIContainer mRightImage;
        VisualElement mSwipeLine;
        VisualElement mSwipeHandle;
        Texture2D mLeftTexture;
        Texture2D mRightTexture;
        Texture2D mLeftFilteredTexture;
        Texture2D mRightFilteredTexture;
        ColorWriteMask mChannelMode = ColorWriteMask.All;
        ScrollView mScrollView;
        Vector2 mComposedSize;
        float mSwipePosition = 0.5f;
        bool mIsDragging;

        const float HANDLE_SIZE = 30f;
        const float HANDLE_ICON_SIZE = 14f;
        const float HANDLE_ICON_HORIZONTAL_MARGIN = 1f;
        const float HANDLE_ICON_VERTICAL_MARGIN = 6f;
        const float SWIPE_LINE_HIT_WIDTH = 16f;
        const float SWIPE_LINE_VISUAL_WIDTH = 2f;
    }
}
