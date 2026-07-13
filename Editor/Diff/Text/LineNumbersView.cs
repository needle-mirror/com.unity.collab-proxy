using System;
using System.Collections.Generic;
using System.Globalization;

using Unity.CodeEditor;
using Unity.CodeEditor.Document;
using Unity.CodeEditor.Rendering;
using Unity.CodeEditor.Text;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TextAnchor = UnityEngine.TextAnchor;

using XDiffGui.Drawing;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class LineNumbersView : VisualElement
    {
        internal LineNumbersView(Unity.CodeEditor.TextEditor textEditor)
        {
            mTextEditor = textEditor;

            style.flexShrink = 0;
            style.overflow = Overflow.Hidden;
            style.backgroundColor = TextEditorColors.Background;

            mImguiContainer = new IMGUIContainer(OnGUI);
            mImguiContainer.pickingMode = PickingMode.Ignore;
            mImguiContainer.focusable = false;
            mImguiContainer.style.position = Position.Absolute;
            mImguiContainer.style.left = 0;
            mImguiContainer.style.top = 0;
            mImguiContainer.style.right = 0;
            mImguiContainer.style.bottom = 0;
            Add(mImguiContainer);

            mTextEditor.TextArea.TextView.VisualLinesChanged += OnVisualLinesChanged;
            mTextEditor.DocumentChanged += OnDocumentChanged;

            if (mTextEditor.Document != null)
            {
                TextDocumentWeakEventManager.LineCountChanged.AddHandler(
                    mTextEditor.Document, OnLineCountChanged);
            }
        }

        internal void Dispose()
        {
            mTextEditor.TextArea.TextView.VisualLinesChanged -= OnVisualLinesChanged;
            mTextEditor.DocumentChanged -= OnDocumentChanged;

            if (mTextEditor.Document != null)
            {
                TextDocumentWeakEventManager.LineCountChanged.RemoveHandler(
                    mTextEditor.Document, OnLineCountChanged);
            }
        }

        internal void InvalidateVisual()
        {
            UpdateLineNumbers();
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            TextView textView = mTextEditor.TextArea.TextView;
            if (textView == null)
                return;

            TextBoxDrawingInfo textBoxDrawingInfo = mTextEditor.Tag as TextBoxDrawingInfo;
            if (textBoxDrawingInfo != null)
            {
                TextEditorDrawing.DrawTextRegions(
                    resolvedStyle.width,
                    textBoxDrawingInfo,
                    mTextEditor);
            }

            TextParagraphProperties paragraphProperties = CreateTextParagraphProperties();
            GUIStyle guiStyle = paragraphProperties.GUIStyle;

            foreach (LineNumberEntry entry in mLineEntries)
            {
                Rect rect = new Rect(0, entry.Y, mTextAreaWidth, guiStyle.lineHeight);
                guiStyle.Draw(rect, entry.Content, false, false, false, false);
            }
        }

        void UpdateLineNumbers()
        {
            TextView textView = mTextEditor.TextArea.TextView;
            if (textView == null || !textView.VisualLinesValid)
                return;

            UpdateWidth();

            mLineEntries.Clear();
            foreach (VisualLine line in textView.VisualLines)
            {
                int lineNumber = line.FirstDocumentLine.LineNumber;
                float y = line.GetTextLineVisualYPosition(
                    line.TextLines[0], VisualYPosition.TextTop);

                mLineEntries.Add(new LineNumberEntry
                {
                    Content = new GUIContent(
                        lineNumber.ToString(CultureInfo.CurrentCulture)),
                    Y = Mathf.Round(y - textView.VerticalOffset),
                });
            }

            mImguiContainer.MarkDirtyRepaint();
        }

        void UpdateWidth()
        {
            TextView textView = mTextEditor.TextArea.TextView;
            if (textView == null) return;

            int digitCount = (mTextEditor.Document?.LineCount ?? 1)
                .ToString(CultureInfo.CurrentCulture).Length;

            if (digitCount < MIN_DIGIT_COUNT)
                digitCount = MIN_DIGIT_COUNT;

            if (digitCount == mCurrentDigitCount)
                return;

            mCurrentDigitCount = digitCount;

            TextFormatter textFormatter = new TextFormatter();
            TextParagraphProperties paragraphProperties = CreateTextParagraphProperties();
            Vector2 size = textFormatter.CalcSize(
                new string('9', digitCount),
                paragraphProperties);

            mTextAreaWidth = size.x + LEFT_MARGIN;
            style.width = mTextAreaWidth + RIGHT_MARGIN;
        }

        void OnVisualLinesChanged(object sender, EventArgs e)
        {
            UpdateLineNumbers();
        }

        void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            if (e.OldDocument != null)
            {
                TextDocumentWeakEventManager.LineCountChanged.RemoveHandler(
                    e.OldDocument, OnLineCountChanged);
            }

            if (e.NewDocument != null)
            {
                TextDocumentWeakEventManager.LineCountChanged.AddHandler(
                    e.NewDocument, OnLineCountChanged);
            }

            OnDocumentLineCountChanged();
        }

        void OnLineCountChanged(object sender, EventArgs e)
        {
            OnDocumentLineCountChanged();
        }

        void OnDocumentLineCountChanged()
        {
            int documentLineCount = mTextEditor.Document?.LineCount ?? 1;
            int newLength = documentLineCount
                .ToString(CultureInfo.CurrentCulture).Length;

            if (newLength < MIN_DIGIT_COUNT)
                newLength = MIN_DIGIT_COUNT;

            if (newLength != mCurrentDigitCount)
            {
                mCurrentDigitCount = newLength;
                UpdateLineNumbers();
            }
        }

        TextParagraphProperties CreateTextParagraphProperties()
        {
            TextView textView = mTextEditor.TextArea.TextView;

            TextParagraphProperties result = new TextParagraphProperties(
                false,
                0,
                textView.Font,
                textView.FontSize,
                textView.LineNumbersForegroundColor);

            result.GUIStyle.alignment = TextAnchor.MiddleRight;
            return result;
        }

        struct LineNumberEntry
        {
            internal GUIContent Content;
            internal float Y;
        }

        float mTextAreaWidth;
        int mCurrentDigitCount = -1;

        readonly Unity.CodeEditor.TextEditor mTextEditor;
        readonly IMGUIContainer mImguiContainer;

        readonly List<LineNumberEntry> mLineEntries = new List<LineNumberEntry>();

        const int RIGHT_MARGIN = 15;
        const int LEFT_MARGIN = 5;
        const int MIN_DIGIT_COUNT = 3;
    }
}
