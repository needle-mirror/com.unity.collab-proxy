using System;
using System.Collections.Generic;

using Unity.CodeEditor;
using Unity.CodeEditor.Editing;
using Unity.CodeEditor.Rendering;

using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class TextEditorScrollEvents
    {
        internal TextEditorScrollEvents(
            Unity.CodeEditor.TextEditor editor,
            Action<TextView> verticalScrollChangedAction,
            Action<TextView> horizontalScrollChangedAction,
            Action<TextArea> layoutUpdatedAction)
        {
            mEditor = editor;
            mVerticalScrollChangedAction = verticalScrollChangedAction;
            mHorizontalScrollChangedAction = horizontalScrollChangedAction;
            mLayoutUpdatedAction = layoutUpdatedAction;

            mEditor.TextArea.TextView.ScrollOffsetChanged +=
                TextView_ScrollOffsetChanged;

            mEditor.TextArea.DocumentChanged +=
                TextArea_DocumentChanged;

            mEditor.TextArea.TextView.VisualLinesChanged +=
                TextView_VisualLinesChanged;
        }

        internal void DisableScrollEvents()
        {
            mAreEventsEnabled = false;
        }

        internal void EnableScrollEvents()
        {
            mAreEventsEnabled = true;
        }

        internal void Dispose()
        {
            mEditor.TextArea.TextView.ScrollOffsetChanged -=
                TextView_ScrollOffsetChanged;

            mEditor.TextArea.DocumentChanged -=
                TextArea_DocumentChanged;

            mEditor.TextArea.TextView.VisualLinesChanged -=
                TextView_VisualLinesChanged;
        }

        void TextView_ScrollOffsetChanged(object sender, EventArgs e)
        {
            Vector2 lastOffset = mLastOffset;
            Vector2 newOffset = mEditor.TextArea.TextView.ScrollOffset;

            mLastOffset = newOffset;

            if (mHorizontalScrollChangedAction != null &&
                Mathf.Abs(lastOffset.x - newOffset.x) > TOLERANCE &&
                mAreEventsEnabled)
                mHorizontalScrollChangedAction(mEditor.TextArea.TextView);

            if (IsBeyondDocumentWithTwoOrLessVisibleLines())
                return;

            if (mVerticalScrollChangedAction != null &&
                Mathf.Abs(lastOffset.y - newOffset.y) > TOLERANCE &&
                mAreEventsEnabled)
                mVerticalScrollChangedAction(mEditor.TextArea.TextView);
        }

        bool IsBeyondDocumentWithTwoOrLessVisibleLines()
        {
            TextView textView = mEditor.TextArea.TextView;

            if (!textView.VisualLinesValid)
                return false;

            IReadOnlyList<VisualLine> visibleLines = textView.VisualLines;
            if (visibleLines == null || visibleLines.Count == 0)
                return false;

            float maxVerticalOffset = textView.DocumentHeight - textView.ScrollViewport.y;
            if (textView.ScrollOffset.y < maxVerticalOffset)
                return false;

            float lastLinesHeight = 0;
            int linesToMeasure = Mathf.Min(2, visibleLines.Count);

            for (int i = visibleLines.Count - linesToMeasure; i < visibleLines.Count; i++)
                lastLinesHeight += visibleLines[i].Height;

            float remainingContentHeight = textView.DocumentHeight - textView.ScrollOffset.y;

            return remainingContentHeight < lastLinesHeight;
        }

        void TextView_VisualLinesChanged(object sender, EventArgs e)
        {
            if (mLayoutUpdatedAction != null && mAreEventsEnabled)
            {
                mLayoutUpdatedAction(mEditor.TextArea);
            }
        }

        void TextArea_DocumentChanged(object sender, EventArgs e)
        {
            mLastOffset = new Vector2(-1, -1);
        }

        bool mAreEventsEnabled = true;
        Vector2 mLastOffset = new Vector2(-1, -1);

        const float TOLERANCE = 0.01f;

        readonly Action<TextView> mHorizontalScrollChangedAction;
        readonly Action<TextView> mVerticalScrollChangedAction;
        readonly Action<TextArea> mLayoutUpdatedAction;
        readonly Unity.CodeEditor.TextEditor mEditor;
    }
}
