using Unity.CodeEditor.Rendering;
using UnityEditor;
using UnityEngine;
using XDiffGui.Drawing;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class TextEditorDiffBackgroundRenderer : IBackgroundRenderer
    {
        internal TextEditorDiffBackgroundRenderer(Unity.CodeEditor.TextEditor editor)
        {
            mEditor = editor;
        }

        KnownLayer IBackgroundRenderer.Layer => KnownLayer.Background;

        void IBackgroundRenderer.OnGUI(TextView textView, Rect drawingRect)
        {
            if (mEditor.Document == null)
                return;

            TextBoxDrawingInfo textboxDrawingInfo =
                mEditor.Tag as TextBoxDrawingInfo;

            if (textboxDrawingInfo == null)
                return;

            TextEditorDrawing.DrawTextRegions(
                textView.Bounds.width + textView.ScrollOffset.x,
                textboxDrawingInfo,
                mEditor,
                true);
        }

        readonly Unity.CodeEditor.TextEditor mEditor;
    }
}
