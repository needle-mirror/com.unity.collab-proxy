using System;
using System.Collections.Generic;

using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;

using Unity.CodeEditor;
using Unity.CodeEditor.Document;
using Unity.CodeEditor.Editing;
using Unity.CodeEditor.Rendering;

using UnityEngine;
using UnityEngine.UIElements;

using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class DiffScroll : IDiffSummaryListener
    {
        internal Unity.CodeEditor.TextEditor LeftTextEditor { get { return mLeftTextEditor; } }
        internal Unity.CodeEditor.TextEditor RightTextEditor { get { return mRightTextEditor; } }

        internal Scroller VerticalScroller { get { return mVerticalScroller; } }
        internal Scroller HorizontalScroller { get { return mHorizontalScroller; } }

        internal DiffScroll(
            DiffSplitter diffSplitter,
            Unity.CodeEditor.TextEditor leftTextEditor,
            Unity.CodeEditor.TextEditor rightTextEditor,
            LineNumbersView leftLineNumbers,
            LineNumbersView rightLineNumbers,
            ActionBar leftActionBar,
            ActionBar rightActionBar)
        {
            mDiffSplitter = diffSplitter;
            mLeftTextEditor = leftTextEditor;
            mRightTextEditor = rightTextEditor;

            mLeftLineNumbers = leftLineNumbers;
            mRightLineNumbers = rightLineNumbers;

            mLeftActionBar = leftActionBar;
            mRightActionBar = rightActionBar;

            mVerticalScroller = new Scroller(
                0, 100, OnVerticalScrollerChanged, SliderDirection.Vertical);
            mVerticalScroller.style.position = Position.Relative;

            mHorizontalScroller = new Scroller(
                0, 100, OnHorizontalScrollerChanged, SliderDirection.Horizontal);
            mHorizontalScroller.style.position = Position.Relative;

            AttachWheelEvents(mLeftTextEditor);
            AttachWheelEvents(mRightTextEditor);

            mDiffSplitter.RegisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);
            mLeftLineNumbers.RegisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);
            mRightLineNumbers.RegisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);
            mLeftActionBar.RegisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);
            mRightActionBar.RegisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);

            mLeftEditorScrollEvents = new TextEditorScrollEvents(
                mLeftTextEditor,
                OnEditorVerticalScrollChanged,
                OnEditorHorizontalScrollChanged,
                OnEditorLayoutUpdated);

            mRightEditorScrollEvents = new TextEditorScrollEvents(
                mRightTextEditor,
                OnEditorVerticalScrollChanged,
                OnEditorHorizontalScrollChanged,
                OnEditorLayoutUpdated);
        }

        internal void GoToLineOnRightTextView(int line)
        {
            GoToLineOnTextView(line, mMapping.Right);
        }

        internal void GoToLineOnLeftTextView(int line)
        {
            GoToLineOnTextView(line, mMapping.Left);
        }

        internal void DisableScrollEvents()
        {
            mLeftEditorScrollEvents.DisableScrollEvents();
            mRightEditorScrollEvents.DisableScrollEvents();
        }

        internal void EnableScrollEvents()
        {
            mLeftEditorScrollEvents.EnableScrollEvents();
            mRightEditorScrollEvents.EnableScrollEvents();
        }

        internal void InvalidateViews()
        {
            mLeftTextEditor.TextArea.TextView.InvalidateVisual();
            mRightTextEditor.TextArea.TextView.InvalidateVisual();
            mLeftLineNumbers.InvalidateVisual();
            mRightLineNumbers.InvalidateVisual();
            mLeftActionBar.UpdateActions();
            mRightActionBar.UpdateActions();
            mDiffSplitter.MarkDirtyRepaint();
        }

        internal void UpdateVirtualMapping(VirtualLinesMapping mapping)
        {
            mMapping = mapping;

            if (!mRightTextEditor.TextArea.TextView.VisualLinesValid)
                mRightTextEditor.TextArea.TextView.EnsureVisualLines();

            VisualLine fullyVisibleTopLine =
                mRightTextEditor.TextArea.TextView.GetFullyVisibleTopLine();

            if (fullyVisibleTopLine == null)
            {
                UpdateScrollBarMetrics();
                return;
            }

            int virtualLine = GetVirtualLine(
                fullyVisibleTopLine.FirstDocumentLine.LineNumber,
                mMapping.Right) + 1;

            mOffsetY = Mathf.Max(0, Mathf.Min(virtualLine,
                mTotalLines - mVisibleLines));

            UpdateScrollBarMetrics();
        }

        internal void SetDiffPosition(int line)
        {
            mForceNavigateToPosition = true;
            try
            {
                SetVerticalOffset(
                    line - 1 - TopLineSynchronizer.DEFAULT_SYNC_LINE_POSITION);
            }
            finally
            {
                mForceNavigateToPosition = false;
            }
        }

        void GoToLineOnTextView(
            int line,
            List<int> mapping)
        {
            int virtualLine = (line == 0) ? 0 : GetVirtualLine(
                line + 1, mapping);

            if (virtualLine == -1)
                return;

            mForceNavigateToPosition = true;
            try
            {
                SetVerticalOffset(virtualLine);
            }
            finally
            {
                mForceNavigateToPosition = false;
            }
        }

        void IDiffSummaryListener.Clicked(int line)
        {
            SetDiffPosition(line);
        }

        internal void Dispose()
        {
            DetachWheelEvents(mLeftTextEditor);
            DetachWheelEvents(mRightTextEditor);

            mDiffSplitter.UnregisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);
            mLeftLineNumbers.UnregisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);
            mRightLineNumbers.UnregisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);
            mLeftActionBar.UnregisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);
            mRightActionBar.UnregisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);

            mLeftEditorScrollEvents.Dispose();
            mRightEditorScrollEvents.Dispose();
        }

        internal void BeginUpdate()
        {
            mIsUpdating = true;
        }

        internal void EndUpdate()
        {
            mIsUpdating = false;
            UpdateScrollBarMetrics();
        }

        internal void DisableTextBoxScrollEvents()
        {
            mAreTextBoxForScrollEventsDisabled = true;
        }

        internal void EnableTextBoxScrollEvents()
        {
            mAreTextBoxForScrollEventsDisabled = false;
        }

        internal void UpdateScrollBarMetrics()
        {
            UpdateVScrollMetrics();
            UpdateHScrollMetrics();

            UpdateScrollerValues();
        }

        void OnVerticalScrollerChanged(float value)
        {
            if (mUpdatingScroller)
                return;

            SetVerticalOffset(value);
        }

        void OnHorizontalScrollerChanged(float value)
        {
            if (mUpdatingScroller)
                return;

            SetHorizontalOffset(value);
        }

        void AttachWheelEvents(Unity.CodeEditor.TextEditor editor)
        {
            editor.TextArea.RegisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);

            foreach (AbstractMargin margin in editor.TextArea.LeftMargins)
                margin.RegisterCallback<WheelEvent>(
                    OnWheelEvent, TrickleDown.TrickleDown);
        }

        void DetachWheelEvents(Unity.CodeEditor.TextEditor editor)
        {
            editor.TextArea.UnregisterCallback<WheelEvent>(
                OnWheelEvent, TrickleDown.TrickleDown);

            foreach (AbstractMargin margin in editor.TextArea.LeftMargins)
                margin.UnregisterCallback<WheelEvent>(
                    OnWheelEvent, TrickleDown.TrickleDown);
        }

        void OnWheelEvent(WheelEvent evt)
        {
            evt.StopPropagation();

            if (evt.delta.x != 0)
                SetHorizontalOffset(mOffsetX + evt.delta.x * HORIZONTAL_DELTA_MULTIPLIER);

            if (evt.delta.y != 0)
                SetVerticalOffset(mOffsetY + evt.delta.y * VERTICAL_DELTA_MULTIPLIER);

            UpdateScrollerValues();
        }

        void OnEditorHorizontalScrollChanged(TextView sender)
        {
            if (mAreTextBoxForScrollEventsDisabled)
                return;

            Unity.CodeEditor.TextEditor editor =
                (sender == mLeftTextEditor.TextArea.TextView) ?
                mLeftTextEditor : mRightTextEditor;

            OnHorizontalScrollOffsetValueChanged(editor);
        }

        void OnEditorVerticalScrollChanged(TextView sender)
        {
            if (mAreTextBoxForScrollEventsDisabled)
                return;

            Unity.CodeEditor.TextEditor editor =
                (sender == mLeftTextEditor.TextArea.TextView) ?
                mLeftTextEditor : mRightTextEditor;

            OnVerticalScrollOffsetValueChanged(editor);
        }

        void OnHorizontalScrollOffsetValueChanged(
            Unity.CodeEditor.TextEditor editor)
        {
            if (mUsableWidth + editor.HorizontalOffset > mTotalWidth)
                mTotalWidth = mUsableWidth + editor.HorizontalOffset;
        }

        void OnVerticalScrollOffsetValueChanged(
            Unity.CodeEditor.TextEditor editor)
        {
            UpdateHScrollMetrics();

            // don't synchronize here: visual lines are stale because
            // ScrollOffsetChanged fires before they are rebuilt.
            // The sync runs in OnEditorLayoutUpdated once visual
            // lines match the new scroll offset
            mEditorPendingVerticalSync = editor;
        }

        void UpdateVerticalOffsetFromTextBox(
            Unity.CodeEditor.TextEditor editor,
            int line)
        {
            if (mMapping == null || mMapping.Left == null ||
                mMapping.Left.Count == 0 ||
                mMapping.Right == null || mMapping.Right.Count == 0)
                return;

            int virtualLine = -1;

            if (editor == mLeftTextEditor)
                virtualLine = line == 1 ? 1 :
                    GetVirtualLine(line, mMapping.Left);

            if (editor == mRightTextEditor)
                virtualLine = line == 1 ? 1 :
                    GetVirtualLine(line, mMapping.Right);

            if (virtualLine == -1)
                return;

            if (mVisibleLines + virtualLine > mTotalLines)
                mTotalLines = mVisibleLines + virtualLine;

            mOffsetY = virtualLine - 1;

            mForceNavigateToPosition = true;
            try
            {
                SetVerticalPosition(mOffsetY);
                UpdateScrollerValues();
            }
            finally
            {
                mForceNavigateToPosition = false;
            }
        }

        void OnEditorLayoutUpdated(TextArea textArea)
        {
            UpdateScrollBarMetrics();

            if (mEditorPendingVerticalSync == null)
                return;

            if (textArea != mEditorPendingVerticalSync.TextArea)
                return;

            Unity.CodeEditor.TextEditor editor = mEditorPendingVerticalSync;
            mEditorPendingVerticalSync = null;

            UpdateVerticalOffsetFromEditor(editor);
        }

        void UpdateVerticalOffsetFromEditor(Unity.CodeEditor.TextEditor editor)
        {
            TextView textView = editor.TextArea.TextView;

            if (!textView.VisualLinesValid)
                return;

            VisualLine fullyVisibleTopLine = textView.GetFullyVisibleTopLine();

            if (fullyVisibleTopLine == null)
                return;

            UpdateVerticalOffsetFromTextBox(
                editor,
                fullyVisibleTopLine.FirstDocumentLine.LineNumber);
        }

        void UpdateVScrollMetrics()
        {
            if (mIsUpdating)
                return;

            if (mMapping == null || mMapping.Left == null ||
                mMapping.Left.Count == 0 ||
                mMapping.Right == null || mMapping.Right.Count == 0)
                return;

            float totalLines = Mathf.Max(
                mMapping.Left.Count, mMapping.Right.Count);

            float leftViewport = mLeftTextEditor.TextArea.TextView.ScrollViewport.y;
            float rightViewport = mRightTextEditor.TextArea.TextView.ScrollViewport.y;
            float leftLineHeight = mLeftTextEditor.TextArea.TextView.DefaultLineHeight;
            float rightLineHeight = mRightTextEditor.TextArea.TextView.DefaultLineHeight;

            if (leftLineHeight <= 0) leftLineHeight = 1;
            if (rightLineHeight <= 0) rightLineHeight = 1;

            float visibleLines = Mathf.Min(
                leftViewport / leftLineHeight,
                rightViewport / rightLineHeight);

            bool allowToScrollBelowDocument =
                mLeftTextEditor.Options.AllowScrollBelowDocument &&
                mRightTextEditor.Options.AllowScrollBelowDocument;

            bool isScrollVisible =
                mLeftTextEditor.TextArea.TextView.ScrollExtent.y >
                    mLeftTextEditor.TextArea.TextView.ScrollViewport.y ||
                mRightTextEditor.TextArea.TextView.ScrollExtent.y >
                    mRightTextEditor.TextArea.TextView.ScrollViewport.y;

            if (isScrollVisible && allowToScrollBelowDocument)
                totalLines += visibleLines;

            UpdateVScrollData(visibleLines, totalLines);
        }

        void UpdateHScrollMetrics()
        {
            if (mIsUpdating)
                return;

            float maxTotalWidth = Mathf.Max(
                mLeftTextEditor.TextArea.TextView.ScrollExtent.x,
                mRightTextEditor.TextArea.TextView.ScrollExtent.x);

            float maxUsableWidth = Mathf.Min(
                mLeftTextEditor.TextArea.TextView.ScrollViewport.x,
                mRightTextEditor.TextArea.TextView.ScrollViewport.x);

            UpdateHScrollData(maxUsableWidth, maxTotalWidth);
        }

        void UpdateHScrollData(float usableWidth, float totalWidth)
        {
            mTotalWidth = totalWidth;
            mUsableWidth = usableWidth;

            mOffsetX = Mathf.Max(0,
                Mathf.Min(mOffsetX, mTotalWidth - mUsableWidth));
        }

        void UpdateVScrollData(float visibleLines, float totalLines)
        {
            mTotalLines = totalLines;
            mVisibleLines = visibleLines - 1;

            mOffsetY = Mathf.Max(0,
                Mathf.Min(mOffsetY, mTotalLines - mVisibleLines));
        }

        void SetHorizontalOffset(float offset)
        {
            if (offset < 0)
                offset = 0;

            if (Mathf.Approximately(mOffsetX, offset))
                return;

            offset = Mathf.Max(0, Mathf.Min(offset, mTotalWidth - mUsableWidth));
            mOffsetX = offset;
            SetHorizontalPosition(mOffsetX);

            UpdateScrollerValues();
        }

        void SetVerticalOffset(float offset)
        {
            offset = Mathf.Max(0, Mathf.Min(offset,
                mTotalLines - mVisibleLines));

            if (Mathf.Approximately(mOffsetY, offset) && !mForceNavigateToPosition)
                return;

            mOffsetY = offset;
            SetVerticalPosition(mOffsetY);

            UpdateScrollerValues();
        }

        void SetVerticalPosition(float position)
        {
            if (mMapping == null || mMapping.Left == null ||
                mMapping.Left.Count == 0 ||
                mMapping.Right == null || mMapping.Right.Count == 0)
                return;

            mLeftEditorScrollEvents.DisableScrollEvents();
            mRightEditorScrollEvents.DisableScrollEvents();

            try
            {
                int maxVirtualLinesCount = Mathf.Max(
                    mMapping.Left.Count - 1, mMapping.Right.Count - 1);

                if (position > maxVirtualLinesCount)
                    position = maxVirtualLinesCount;

                int leftPhysicalLine = TopLineSynchronizer.GetPhysicalTopLine(
                    mMapping.Left, (int)position, mForceNavigateToPosition);

                int rightPhysicalLine = TopLineSynchronizer.GetPhysicalTopLine(
                    mMapping.Right, (int)position, mForceNavigateToPosition);

                float lineOffsetPercent = position - Mathf.Floor(position);

                if (leftPhysicalLine > 0)
                    SetLineOnTop(mLeftTextEditor, leftPhysicalLine, lineOffsetPercent);

                if (rightPhysicalLine > 0)
                    SetLineOnTop(mRightTextEditor, rightPhysicalLine, lineOffsetPercent);
            }
            finally
            {
                mLeftEditorScrollEvents.EnableScrollEvents();
                mRightEditorScrollEvents.EnableScrollEvents();

                InvalidateViews();
            }
        }

        void SetHorizontalPosition(float position)
        {
            mLeftEditorScrollEvents.DisableScrollEvents();
            mRightEditorScrollEvents.DisableScrollEvents();

            try
            {
                Vector2 leftOffset = mLeftTextEditor.TextArea.TextView.ScrollOffset;
                mLeftTextEditor.TextArea.TextView.ScrollOffset =
                    new Vector2(position, leftOffset.y);

                Vector2 rightOffset = mRightTextEditor.TextArea.TextView.ScrollOffset;
                mRightTextEditor.TextArea.TextView.ScrollOffset =
                    new Vector2(position, rightOffset.y);

                mLeftTextEditor.TextArea.TextView.UpdateVisualLines();
                mRightTextEditor.TextArea.TextView.UpdateVisualLines();
            }
            finally
            {
                mLeftEditorScrollEvents.EnableScrollEvents();
                mRightEditorScrollEvents.EnableScrollEvents();
            }
        }

        void SetLineOnTop(
            Unity.CodeEditor.TextEditor editor,
            int line,
            float lineOffsetPercent)
        {
            TextDocument document = editor.TextArea.TextView.Document;
            if (document == null)
                return;

            if (line < 1) line = 1;
            if (line > document.LineCount) line = document.LineCount;

            VisualLine visualLine = editor.TextArea.TextView.GetOrConstructVisualLine(
                document.GetLineByNumber(line));

            float y = visualLine.VisualTop + (lineOffsetPercent * visualLine.Height);

            Vector2 currentOffset = editor.TextArea.TextView.ScrollOffset;
            if (Mathf.RoundToInt(currentOffset.y) == Mathf.RoundToInt(y))
                return;

            editor.TextArea.TextView.ScrollOffset =
                new Vector2(currentOffset.x, Mathf.Max(0, y));
            editor.TextArea.TextView.UpdateVisualLines();
        }

        static int GetVirtualLine(
            int realLine,
            List<int> virtualLineMapping)
        {
            return virtualLineMapping.IndexOf(realLine);
        }

        void UpdateScrollerValues()
        {
            mUpdatingScroller = true;
            try
            {
                float vMax = Mathf.Max(0, mTotalLines - mVisibleLines);
                mVerticalScroller.lowValue = 0;
                mVerticalScroller.highValue = vMax;
                mVerticalScroller.slider.pageSize = Mathf.Max(1, mVisibleLines);
                mVerticalScroller.value = mOffsetY;
                float vFactor = mTotalLines > 0
                    ? Mathf.Clamp01(mVisibleLines / mTotalLines) : 1f;
                mVerticalScroller.Adjust(vFactor);
                mVerticalScroller.style.visibility =
                    vMax > 0 ? Visibility.Visible : Visibility.Hidden;

                float hMax = Mathf.Max(0, mTotalWidth - mUsableWidth);
                mHorizontalScroller.lowValue = 0;
                mHorizontalScroller.highValue = hMax;
                mHorizontalScroller.slider.pageSize = Mathf.Max(1, mUsableWidth);
                mHorizontalScroller.value = mOffsetX;
                float hFactor = mTotalWidth > 0
                    ? Mathf.Clamp01(mUsableWidth / mTotalWidth) : 1f;
                mHorizontalScroller.Adjust(hFactor);
                mHorizontalScroller.style.visibility =
                    hMax > 0 ? Visibility.Visible : Visibility.Hidden;
            }
            finally
            {
                mUpdatingScroller = false;
            }
        }

        VirtualLinesMapping mMapping;

        float mTotalLines;
        float mTotalWidth;

        float mOffsetX;
        float mOffsetY;

        float mVisibleLines;
        float mUsableWidth;

        bool mForceNavigateToPosition;
        bool mAreTextBoxForScrollEventsDisabled;

        bool mIsUpdating;
        bool mUpdatingScroller;
        Unity.CodeEditor.TextEditor mEditorPendingVerticalSync;

        TextEditorScrollEvents mLeftEditorScrollEvents;
        TextEditorScrollEvents mRightEditorScrollEvents;

        readonly Scroller mVerticalScroller;
        readonly Scroller mHorizontalScroller;

        readonly DiffSplitter mDiffSplitter;
        readonly Unity.CodeEditor.TextEditor mLeftTextEditor;
        readonly Unity.CodeEditor.TextEditor mRightTextEditor;
        readonly LineNumbersView mLeftLineNumbers;
        readonly LineNumbersView mRightLineNumbers;
        readonly ActionBar mLeftActionBar;
        readonly ActionBar mRightActionBar;

        const float HORIZONTAL_DELTA_MULTIPLIER = 12f;
        const float VERTICAL_DELTA_MULTIPLIER = 2.6f;
    }
}
