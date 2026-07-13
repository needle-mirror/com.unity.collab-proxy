using System;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Views.ImageViewer
{
    internal class ImageContentView : VisualElement
    {
        internal const float ZOOM_ANIMATION_DURATION_MS = 300f;
        internal const long ANIMATION_FRAME_INTERVAL_MS = 16;

        internal ScrollView ScrollView { get { return mScrollView; } }
        internal float ZoomLevel { get { return mZoomLevel; } }

        internal bool DisableAnimationsForTesting { get; set; }

        internal ImageContentView(IZoomableImageView imageView)
        {
            mImageView = imageView;

            BuildComponents();
        }

        internal void Dispose()
        {
            mScrollView.UnregisterCallback<GeometryChangedEvent>(OnContentViewportGeometryChanged);
            mScrollView.UnregisterCallback<WheelEvent>(OnWheelEvent, TrickleDown.TrickleDown);
            mScrollView.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            mScrollView.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            mScrollView.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            mScrollView.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        internal void HideScrollbars()
        {
            mHideScrollbars = true;
            mScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            mScrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        }

        internal void ApplyZoom(float newZoom)
        {
            CancelZoomAnimation();

            mZoomLevel = newZoom;
            SetupImageSize(newZoom);
            mIsInitialized = true;
        }

        internal void ApplyZoomWithScrollAnchor(
            float newZoom, Vector2 mouseInViewport)
        {
            CancelZoomAnimation();

            float oldZoom = mZoomLevel;
            Vector2 mouseInContent = mScrollView.scrollOffset + mouseInViewport;
            float ratio = newZoom / oldZoom;

            mZoomLevel = newZoom;
            SetupImageSize(newZoom);
            mIsInitialized = true;

            Vector2 newScrollOffset = mouseInContent * ratio - mouseInViewport;
            mScrollView.scrollOffset = newScrollOffset;
        }

        internal void InitZoom(float zoomLevel)
        {
            CancelZoomAnimation();

            mZoomLevel = zoomLevel;
            SetupImageSize(zoomLevel);
            mIsInitialized = true;
        }

        internal void ZoomIn()
        {
            AnimateZoomTo(mZoomLevel + ZOOM_STEP);
        }

        internal void ZoomOut()
        {
            AnimateZoomTo(Math.Max(mZoomLevel - ZOOM_STEP, MIN_ZOOM_LEVEL));
        }

        internal void ZoomOneToOne()
        {
            AnimateZoomTo(1f);
        }

        internal void ZoomToFit()
        {
            AnimateZoomTo(Math.Max(GetZoomValueToFit(), MIN_ZOOM_LEVEL));
        }

        internal float GetZoomValueToFit()
        {
            float margin = 4f;

            float availableWidth = resolvedStyle.width - margin;
            float availableHeight = resolvedStyle.height - margin;

            if (float.IsNaN(availableWidth) || float.IsNaN(availableHeight)
                || availableWidth <= 0 || availableHeight <= 0)
                return 1f;

            return ImageDiffExtensions.CalculateZoomToFit(
                mImageView.ImageSize.x, mImageView.ImageSize.y,
                availableWidth, availableHeight);
        }

        void AnimateZoomTo(float targetZoom)
        {
            if (!mIsInitialized || DisableAnimationsForTesting)
            {
                mZoomLevel = targetZoom;
                SetupImageSize(targetZoom);
                return;
            }

            CancelZoomAnimation();

            float startZoom = mZoomLevel;

            if (Mathf.Approximately(startZoom, targetZoom))
                return;

            Stopwatch stopwatch = Stopwatch.StartNew();

            mZoomAnimation = schedule.Execute(() =>
            {
                float elapsed = (float)stopwatch.Elapsed.TotalMilliseconds;
                float t = Mathf.Clamp01(elapsed / ZOOM_ANIMATION_DURATION_MS);
                t = CubicEaseOut(t);

                mZoomLevel = Mathf.Lerp(startZoom, targetZoom, t);
                SetupImageSize(mZoomLevel);

                if (t >= 1f)
                {
                    mZoomAnimation.Pause();
                    mZoomAnimation = null;
                }
            }).Every(ANIMATION_FRAME_INTERVAL_MS);
        }

        void CancelZoomAnimation()
        {
            if (mZoomAnimation == null)
                return;

            mZoomAnimation.Pause();
            mZoomAnimation = null;
        }

        static float CubicEaseOut(float t)
        {
            t -= 1f;
            return t * t * t + 1f;
        }

        void SetupImageSize(float zoomLevel)
        {
            Vector2 imageSize = mImageView.ImageSize;

            float targetWidth = imageSize.x * zoomLevel;
            float targetHeight = imageSize.y * zoomLevel;

            mImageView.SetSize(targetWidth, targetHeight);
            UpdateScrollerVisibility(targetWidth, targetHeight);
        }

        void UpdateScrollerVisibility(float imageWidth, float imageHeight)
        {
            if (mHideScrollbars)
                return;

            float viewportWidth = mScrollView.layout.width;
            float viewportHeight = mScrollView.layout.height;

            if (float.IsNaN(viewportWidth) || float.IsNaN(viewportHeight)
                || viewportWidth <= 0 || viewportHeight <= 0)
                return;

            mScrollView.horizontalScrollerVisibility =
                imageWidth > viewportWidth
                    ? ScrollerVisibility.Auto
                    : ScrollerVisibility.Hidden;

            mScrollView.verticalScrollerVisibility =
                imageHeight > viewportHeight
                    ? ScrollerVisibility.Auto
                    : ScrollerVisibility.Hidden;
        }

        void BuildComponents()
        {
            style.flexGrow = 1;
            style.overflow = Overflow.Hidden;

            mScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            mScrollView.style.flexGrow = 1;
            mScrollView.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            mScrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;

            mScrollView.contentContainer.style.alignItems = Align.Center;
            mScrollView.contentContainer.style.justifyContent = Justify.Center;
            mScrollView.contentContainer.style.flexGrow = 1;

            mScrollView.RegisterCallback<GeometryChangedEvent>(OnContentViewportGeometryChanged);
            schedule.Execute(UpdateContentContainerMinSize);

            mScrollView.Add((VisualElement)mImageView);

            mScrollView.RegisterCallback<WheelEvent>(OnWheelEvent, TrickleDown.TrickleDown);
            mScrollView.RegisterCallback<PointerDownEvent>(OnPointerDown);
            mScrollView.RegisterCallback<PointerUpEvent>(OnPointerUp);
            mScrollView.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            mScrollView.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);

            Add(mScrollView);
        }

        void OnContentViewportGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateContentContainerMinSize();
        }

        void UpdateContentContainerMinSize()
        {
            float viewportHeight = mScrollView.layout.height;
            float viewportWidth = mScrollView.layout.width;

            if (viewportHeight > 0 && viewportWidth > 0)
            {
                mScrollView.contentContainer.style.minHeight = viewportHeight;
                mScrollView.contentContainer.style.minWidth = viewportWidth;
            }
        }

        void OnWheelEvent(WheelEvent evt)
        {
            if (!mIsInitialized)
                return;

            if (!evt.ctrlKey)
                return;

            evt.StopPropagation();

            CancelZoomAnimation();

            float oldZoom = mZoomLevel;
            float newZoom = evt.delta.y > 0
                ? oldZoom / WHEEL_ZOOM_FACTOR
                : oldZoom * WHEEL_ZOOM_FACTOR;

            newZoom = Mathf.Max(newZoom, MIN_ZOOM_LEVEL);

            if (Mathf.Approximately(oldZoom, newZoom))
                return;

            Vector2 mouseInViewport =
                mScrollView.contentViewport.WorldToLocal(evt.mousePosition);
            Vector2 mouseInContent = mScrollView.scrollOffset + mouseInViewport;

            float ratio = newZoom / oldZoom;

            mZoomLevel = newZoom;
            SetupImageSize(newZoom);

            Vector2 newScrollOffset = mouseInContent * ratio - mouseInViewport;
            mScrollView.scrollOffset = newScrollOffset;
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            mDragOrigin = (Vector2)evt.localPosition;
            mScrollOffsetAtDragStart = mScrollView.scrollOffset;
            mIsDragging = true;

            mScrollView.CapturePointer(evt.pointerId);
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            mIsDragging = false;

            if (mScrollView.HasPointerCapture(evt.pointerId))
                mScrollView.ReleasePointer(evt.pointerId);
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (!mIsDragging)
                return;

            Vector2 delta = mDragOrigin - (Vector2)evt.localPosition;

            mScrollView.scrollOffset = new Vector2(
                mScrollOffsetAtDragStart.x + delta.x,
                mScrollOffsetAtDragStart.y + delta.y);
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            mIsDragging = false;
        }

        Vector2 mDragOrigin;
        Vector2 mScrollOffsetAtDragStart;
        bool mIsDragging;
        bool mIsInitialized;
        bool mHideScrollbars;
        float mZoomLevel = 1f;

        ScrollView mScrollView;
        IZoomableImageView mImageView;
        IVisualElementScheduledItem mZoomAnimation;

        const float MIN_ZOOM_LEVEL = 0.08f;
        const float ZOOM_STEP = 0.1f;
        const float WHEEL_ZOOM_FACTOR = 1.1f;
    }
}
