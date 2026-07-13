using Unity.CodeEditor;
using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class TextEditorCollapsedLinesProvider : ICollapsedLinesProvider
    {
        internal TextEditorCollapsedLinesProvider(TextEditor textEditor)
        {
            mTextEditor = textEditor;
        }

        int ICollapsedLinesProvider.GetCollapsedLineNumber(int lineNumber)
        {
            return lineNumber;
        }

        TextEditor mTextEditor;
    }
}
