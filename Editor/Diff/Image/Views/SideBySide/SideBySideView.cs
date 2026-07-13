using System;
using System.Diagnostics;

using Unity.PlasticSCM.Editor.Diff.Texture.Toolbar;
using Unity.PlasticSCM.Editor.Diff.Texture.Views.ImageViewer;

using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Views.SideBySide
{
    internal class SideBySideView : VisualElement, IImageDiffView
    {
        internal SideBySideView()
        {
            BuildComponents();
        }

        internal void SetImages(
            Texture2D leftTexture, Texture2D rightTexture)
        {
            mLeftImageView.SetImage(leftTexture);
            mRightImageView.SetImage(rightTexture);

            if (resolvedStyle.width > 0 && resolvedStyle.height > 0)
            {
                InitZoom(leftTexture, rightTexture);
                return;
            }

            InitZoomWhenLayoutReady(leftTexture, rightTexture);
        }

        internal void SetChannelMode(ColorWriteMask mode)
        {
            mLeftImageView.SetChannelMode(mode);
            mRightImageView.SetChannelMode(mode);
        }

        internal void Dispose()
        {
            CancelExternalScrollersUpdate();
            UnregisterScrollSync();

            mPanelsContainer.UnregisterCallback<WheelEvent>(
                OnPanelsWheelEvent, TrickleDown.TrickleDown);
            UnregisterCallback<GeometryChangedEvent>(
                OnGeometryChanged);

            mLeftContentView.Dispose();
            mRightContentView.Dispose();
            mZoomOptionsView.Dispose();
        }

        void IImageDiffView.ZoomIn()
        {
            mLeftContentView.ZoomIn();
            mRightContentView.ZoomIn();

            ScheduleExternalScrollersUpdate();
        }

        void IImageDiffView.ZoomOut()
        {
            mLeftContentView.ZoomOut();
            mRightContentView.ZoomOut();

            ScheduleExternalScrollersUpdate();
        }

        void IImageDiffView.ZoomOneToOne()
        {
            mLeftContentView.ZoomOneToOne();
            mRightContentView.ZoomOneToOne();

            ScheduleExternalScrollersUpdate();
        }

        void IImageDiffView.ZoomToFit()
        {
            float zoomFit = GetSynchronizedZoomToFit();
            mLeftContentView.InitZoom(zoomFit);
            mRightContentView.InitZoom(zoomFit);

            ScheduleExternalScrollersUpdate();
        }

        float GetSynchronizedZoomToFit()
        {
            float leftFit = mLeftContentView.GetZoomValueToFit();
            float rightFit = mRightContentView.GetZoomValueToFit();
            return Math.Min(leftFit, rightFit);
        }

        void InitZoomWhenLayoutReady(
            Texture2D leftTexture, Texture2D rightTexture)
        {

            void OnInitZoomGeometryChanged(GeometryChangedEvent evt)
            {
                UnregisterCallback<GeometryChangedEvent>(
                    OnInitZoomGeometryChanged);
                InitZoom(leftTexture, rightTexture);
            }

            RegisterCallback<GeometryChangedEvent>(
                OnInitZoomGeometryChanged);
        }

        void InitZoom(Texture2D leftTexture, Texture2D rightTexture)
        {
            if (leftTexture == null && rightTexture == null)
                return;

            Vector2 frameSize = new Vector2(
                resolvedStyle.width / 2f, resolvedStyle.height);

            Vector2 leftSize = leftTexture != null
                ? new Vector2(leftTexture.width, leftTexture.height)
                : Vector2.zero;
            Vector2 rightSize = rightTexture != null
                ? new Vector2(rightTexture.width, rightTexture.height)
                : Vector2.zero;

            bool bNeedsFit =
                ImageDiffExtensions.IsImageBiggerThanFrame(
                    frameSize, leftSize) ||
                ImageDiffExtensions.IsImageBiggerThanFrame(
                    frameSize, rightSize);

            if (bNeedsFit)
            {
                float zoomFit = GetSynchronizedZoomToFit();
                mLeftContentView.InitZoom(zoomFit);
                mRightContentView.InitZoom(zoomFit);
            }
            else
            {
                mLeftContentView.InitZoom(1f);
                mRightContentView.InitZoom(1f);
            }

            schedule.Execute(UpdateExternalScrollers);
        }

        void BuildComponents()
        {
            style.flexGrow = 1;
            style.flexDirection = FlexDirection.Column;

            BuildMainRow();
            BuildBottomRow();
            BuildZoomOptions();

            mLeftContentView.HideScrollbars();
            mRightContentView.HideScrollbars();

            RegisterScrollSync();

            mPanelsContainer.RegisterCallback<WheelEvent>(
                OnPanelsWheelEvent, TrickleDown.TrickleDown);
            RegisterCallback<GeometryChangedEvent>(
                OnGeometryChanged);
        }

        void BuildMainRow()
        {
            VisualElement mainRow = new VisualElement();
            mainRow.style.flexDirection = FlexDirection.Row;
            mainRow.style.flexGrow = 1;

            mPanelsContainer = new VisualElement();
            mPanelsContainer.style.flexDirection = FlexDirection.Row;
            mPanelsContainer.style.flexGrow = 1;
            mPanelsContainer.style.overflow = Overflow.Hidden;

            mLeftImageView = new ImageView();
            mLeftImageView.SetBorderColor(
                UnityStyles.Colors.ImageDiff.LeftImageBorderColor);
            mLeftContentView = new ImageContentView(mLeftImageView);
            mLeftContentView.style.flexGrow = 1;
            mLeftContentView.style.flexBasis = 0;

            BuildSplitter();

            mRightImageView = new ImageView();
            mRightImageView.SetBorderColor(
                UnityStyles.Colors.ImageDiff.RightImageBorderColor);
            mRightContentView = new ImageContentView(mRightImageView);
            mRightContentView.style.flexGrow = 1;
            mRightContentView.style.flexBasis = 0;

            mPanelsContainer.Add(mLeftContentView);
            mPanelsContainer.Add(mSplitter);
            mPanelsContainer.Add(mRightContentView);

            mVerticalScroller = new Scroller(0, 0, OnExternalVerticalScroll,
                SliderDirection.Vertical);
            mVerticalScroller.style.width = SCROLLER_SIZE;

            mainRow.Add(mPanelsContainer);
            mainRow.Add(mVerticalScroller);

            Add(mainRow);
        }

        void BuildBottomRow()
        {
            VisualElement bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;

            mHorizontalScroller = new Scroller(0, 0, OnExternalHorizontalScroll,
                SliderDirection.Horizontal);
            mHorizontalScroller.style.flexGrow = 1;
            mHorizontalScroller.style.height = SCROLLER_SIZE;

            VisualElement corner = new VisualElement();
            corner.style.width = SCROLLER_SIZE;
            corner.style.height = SCROLLER_SIZE;

            bottomRow.Add(mHorizontalScroller);
            bottomRow.Add(corner);

            Add(bottomRow);
        }

        void BuildZoomOptions()
        {
            mZoomOptionsView = new ZoomOptionsView(this);
            Add(mZoomOptionsView);
        }

        void BuildSplitter()
        {
            mSplitter = new VisualElement();
            mSplitter.style.width = SPLITTER_WIDTH;
            mSplitter.style.justifyContent = Justify.Center;
            mSplitter.style.alignItems = Align.Center;
            mSplitter.SetMouseCursor(MouseCursor.SplitResizeLeftRight);

            VisualElement splitterLine = new VisualElement();
            splitterLine.style.width = 1;
            splitterLine.style.flexGrow = 1;
            splitterLine.style.backgroundColor =
                UnityStyles.Colors.BarBorder;
            splitterLine.pickingMode = PickingMode.Ignore;

            mSplitter.Add(splitterLine);

            mSplitter.RegisterCallback<PointerDownEvent>(OnSplitterPointerDown);
            mSplitter.RegisterCallback<PointerMoveEvent>(OnSplitterPointerMove);
            mSplitter.RegisterCallback<PointerUpEvent>(OnSplitterPointerUp);
        }

        void OnSplitterPointerDown(PointerDownEvent evt)
        {
            mbIsSplitterDragging = true;
            mSplitterDragStartX = evt.position.x;
            mSplitterDragStartLeftGrow = mLeftContentView.resolvedStyle.flexGrow;
            mSplitterDragStartRightGrow = mRightContentView.resolvedStyle.flexGrow;

            mSplitter.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void OnSplitterPointerMove(PointerMoveEvent evt)
        {
            if (!mbIsSplitterDragging)
                return;

            float containerWidth = mPanelsContainer.resolvedStyle.width
                - SPLITTER_WIDTH;

            if (containerWidth <= 0)
                return;

            float deltaX = evt.position.x - mSplitterDragStartX;
            float totalGrow = mSplitterDragStartLeftGrow
                + mSplitterDragStartRightGrow;

            float growDelta = totalGrow * (deltaX / containerWidth);

            float newLeftGrow = mSplitterDragStartLeftGrow + growDelta;
            float newRightGrow = mSplitterDragStartRightGrow - growDelta;

            float minGrow = totalGrow * (MIN_PANEL_WIDTH / containerWidth);

            if (newLeftGrow < minGrow || newRightGrow < minGrow)
                return;

            mLeftContentView.style.flexGrow = newLeftGrow;
            mRightContentView.style.flexGrow = newRightGrow;

            schedule.Execute(UpdateExternalScrollers);

            evt.StopPropagation();
        }

        void OnSplitterPointerUp(PointerUpEvent evt)
        {
            mbIsSplitterDragging = false;

            if (mSplitter.HasPointerCapture(evt.pointerId))
                mSplitter.ReleasePointer(evt.pointerId);

            evt.StopPropagation();
        }

        void RegisterScrollSync()
        {
            ScrollView leftScroll = mLeftContentView.ScrollView;
            ScrollView rightScroll = mRightContentView.ScrollView;

            leftScroll.horizontalScroller.valueChanged +=
                OnLeftHorizontalScrollChanged;
            leftScroll.verticalScroller.valueChanged +=
                OnLeftVerticalScrollChanged;
            rightScroll.horizontalScroller.valueChanged +=
                OnRightHorizontalScrollChanged;
            rightScroll.verticalScroller.valueChanged +=
                OnRightVerticalScrollChanged;
        }

        void UnregisterScrollSync()
        {
            ScrollView leftScroll = mLeftContentView.ScrollView;
            ScrollView rightScroll = mRightContentView.ScrollView;

            leftScroll.horizontalScroller.valueChanged -=
                OnLeftHorizontalScrollChanged;
            leftScroll.verticalScroller.valueChanged -=
                OnLeftVerticalScrollChanged;
            rightScroll.horizontalScroller.valueChanged -=
                OnRightHorizontalScrollChanged;
            rightScroll.verticalScroller.valueChanged -=
                OnRightVerticalScrollChanged;
        }

        void OnLeftHorizontalScrollChanged(float value)
        {
            if (mbIsSyncing)
                return;

            mbIsSyncing = true;
            mRightContentView.ScrollView.horizontalScroller.value = value;
            UpdateExternalHorizontalScrollerValue();
            mbIsSyncing = false;
        }

        void OnLeftVerticalScrollChanged(float value)
        {
            if (mbIsSyncing)
                return;

            mbIsSyncing = true;
            mRightContentView.ScrollView.verticalScroller.value = value;
            UpdateExternalVerticalScrollerValue();
            mbIsSyncing = false;
        }

        void OnRightHorizontalScrollChanged(float value)
        {
            if (mbIsSyncing)
                return;

            mbIsSyncing = true;
            mLeftContentView.ScrollView.horizontalScroller.value = value;
            UpdateExternalHorizontalScrollerValue();
            mbIsSyncing = false;
        }

        void OnRightVerticalScrollChanged(float value)
        {
            if (mbIsSyncing)
                return;

            mbIsSyncing = true;
            mLeftContentView.ScrollView.verticalScroller.value = value;
            UpdateExternalVerticalScrollerValue();
            mbIsSyncing = false;
        }

        void OnExternalHorizontalScroll(float value)
        {
            if (mbIsSyncing)
                return;

            mbIsSyncing = true;
            mLeftContentView.ScrollView.scrollOffset = new Vector2(
                value, mLeftContentView.ScrollView.scrollOffset.y);
            mRightContentView.ScrollView.scrollOffset = new Vector2(
                value, mRightContentView.ScrollView.scrollOffset.y);
            mbIsSyncing = false;
        }

        void OnExternalVerticalScroll(float value)
        {
            if (mbIsSyncing)
                return;

            mbIsSyncing = true;
            mLeftContentView.ScrollView.scrollOffset = new Vector2(
                mLeftContentView.ScrollView.scrollOffset.x, value);
            mRightContentView.ScrollView.scrollOffset = new Vector2(
                mRightContentView.ScrollView.scrollOffset.x, value);
            mbIsSyncing = false;
        }

        void OnPanelsWheelEvent(WheelEvent evt)
        {
            if (!evt.ctrlKey)
                return;

            evt.StopPropagation();

            float oldZoom = mLeftContentView.ZoomLevel;
            float newZoom = evt.delta.y > 0
                ? oldZoom / WHEEL_ZOOM_FACTOR
                : oldZoom * WHEEL_ZOOM_FACTOR;

            newZoom = Mathf.Max(newZoom, MIN_ZOOM_LEVEL);

            if (Mathf.Approximately(oldZoom, newZoom))
                return;

            bool bIsMouseOverLeft = IsMouseOverPanel(
                evt.mousePosition, mLeftContentView);

            ImageContentView anchorPanel = bIsMouseOverLeft
                ? mLeftContentView
                : mRightContentView;
            ImageContentView otherPanel = bIsMouseOverLeft
                ? mRightContentView
                : mLeftContentView;

            Vector2 mouseInViewport =
                anchorPanel.ScrollView.contentViewport.WorldToLocal(
                    evt.mousePosition);

            anchorPanel.ApplyZoomWithScrollAnchor(newZoom, mouseInViewport);
            otherPanel.ApplyZoom(newZoom);
            otherPanel.ScrollView.scrollOffset =
                anchorPanel.ScrollView.scrollOffset;

            UpdateExternalScrollers();
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            schedule.Execute(UpdateExternalScrollers);
        }

        void UpdateExternalScrollers()
        {
            UpdateExternalHorizontalScroller();
            UpdateExternalVerticalScroller();
        }

        void ScheduleExternalScrollersUpdate()
        {
            CancelExternalScrollersUpdate();

            Stopwatch stopwatch = Stopwatch.StartNew();

            mScrollersUpdateAnimation = schedule.Execute(() =>
            {
                UpdateExternalScrollers();

                float elapsed =
                    (float)stopwatch.Elapsed.TotalMilliseconds;

                if (elapsed >= ImageContentView.ZOOM_ANIMATION_DURATION_MS)
                {
                    mScrollersUpdateAnimation.Pause();
                    mScrollersUpdateAnimation = null;
                }
            }).Every(ImageContentView.ANIMATION_FRAME_INTERVAL_MS);
        }

        void CancelExternalScrollersUpdate()
        {
            if (mScrollersUpdateAnimation == null)
                return;

            mScrollersUpdateAnimation.Pause();
            mScrollersUpdateAnimation = null;
        }

        void UpdateExternalHorizontalScroller()
        {
            float leftExtent = GetHorizontalContentExtent(mLeftContentView);
            float rightExtent = GetHorizontalContentExtent(mRightContentView);

            float leftViewportWidth =
                mLeftContentView.ScrollView.contentViewport.resolvedStyle.width;
            float rightViewportWidth =
                mRightContentView.ScrollView.contentViewport.resolvedStyle.width;

            if (float.IsNaN(leftViewportWidth) || leftViewportWidth <= 0
                || float.IsNaN(rightViewportWidth) || rightViewportWidth <= 0)
                return;

            float minViewportWidth = Math.Min(
                leftViewportWidth, rightViewportWidth);
            float maxExtent = Math.Max(leftExtent, rightExtent);

            bool bOverflows = leftExtent > leftViewportWidth
                || rightExtent > rightViewportWidth;

            mHorizontalScroller.SetEnabled(bOverflows);
            mHorizontalScroller.lowValue = 0;
            mHorizontalScroller.highValue = bOverflows
                ? maxExtent - minViewportWidth : 0;
            mHorizontalScroller.slider.pageSize = minViewportWidth;
            mHorizontalScroller.Adjust(
                bOverflows ? minViewportWidth / maxExtent : 1f);

            if (bOverflows)
                UpdateExternalHorizontalScrollerValue();
            else
                mHorizontalScroller.value = 0;
        }

        void UpdateExternalVerticalScroller()
        {
            float leftExtent = GetVerticalContentExtent(mLeftContentView);
            float rightExtent = GetVerticalContentExtent(mRightContentView);

            float leftViewportHeight =
                mLeftContentView.ScrollView.contentViewport.resolvedStyle.height;
            float rightViewportHeight =
                mRightContentView.ScrollView.contentViewport.resolvedStyle.height;

            if (float.IsNaN(leftViewportHeight) || leftViewportHeight <= 0
                || float.IsNaN(rightViewportHeight) || rightViewportHeight <= 0)
                return;

            float minViewportHeight = Math.Min(
                leftViewportHeight, rightViewportHeight);
            float maxExtent = Math.Max(leftExtent, rightExtent);

            bool bOverflows = leftExtent > leftViewportHeight
                || rightExtent > rightViewportHeight;

            mVerticalScroller.SetEnabled(bOverflows);
            mVerticalScroller.lowValue = 0;
            mVerticalScroller.highValue = bOverflows
                ? maxExtent - minViewportHeight : 0;
            mVerticalScroller.slider.pageSize = minViewportHeight;
            mVerticalScroller.Adjust(
                bOverflows ? minViewportHeight / maxExtent : 1f);

            if (bOverflows)
                UpdateExternalVerticalScrollerValue();
            else
                mVerticalScroller.value = 0;
        }

        void UpdateExternalHorizontalScrollerValue()
        {
            float leftOffset =
                mLeftContentView.ScrollView.scrollOffset.x;
            float rightOffset =
                mRightContentView.ScrollView.scrollOffset.x;

            mHorizontalScroller.value = Math.Max(leftOffset, rightOffset);
        }

        void UpdateExternalVerticalScrollerValue()
        {
            float leftOffset =
                mLeftContentView.ScrollView.scrollOffset.y;
            float rightOffset =
                mRightContentView.ScrollView.scrollOffset.y;

            mVerticalScroller.value = Math.Max(leftOffset, rightOffset);
        }

        static float GetHorizontalContentExtent(ImageContentView contentView)
        {
            float contentWidth =
                contentView.ScrollView.contentContainer.resolvedStyle.width;

            return float.IsNaN(contentWidth) ? 0 : contentWidth;
        }

        static float GetVerticalContentExtent(ImageContentView contentView)
        {
            float contentHeight =
                contentView.ScrollView.contentContainer.resolvedStyle.height;

            return float.IsNaN(contentHeight) ? 0 : contentHeight;
        }

        static bool IsMouseOverPanel(
            Vector2 mousePosition, ImageContentView panel)
        {
            Vector2 localPos = panel.WorldToLocal(mousePosition);
            return panel.contentRect.Contains(localPos);
        }

        ImageView mLeftImageView;
        ImageView mRightImageView;
        ImageContentView mLeftContentView;
        ImageContentView mRightContentView;
        ZoomOptionsView mZoomOptionsView;
        VisualElement mPanelsContainer;
        VisualElement mSplitter;
        Scroller mHorizontalScroller;
        Scroller mVerticalScroller;
        bool mbIsSyncing;
        bool mbIsSplitterDragging;
        float mSplitterDragStartX;
        float mSplitterDragStartLeftGrow;
        float mSplitterDragStartRightGrow;
        IVisualElementScheduledItem mScrollersUpdateAnimation;

        const float SPLITTER_WIDTH = 8f;
        const float MIN_PANEL_WIDTH = 50f;
        const float MIN_ZOOM_LEVEL = 0.08f;
        const float WHEEL_ZOOM_FACTOR = 1.1f;
        const float SCROLLER_SIZE = 15f;
    }
}
