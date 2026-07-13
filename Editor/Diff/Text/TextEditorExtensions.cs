using Unity.CodeEditor.Rendering;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal static class TextEditorExtensions
    {
        internal static VisualLine GetFullyVisibleTopLine(this TextView textView)
        {
            foreach (VisualLine visualLine in textView.VisualLines)
            {
                if (visualLine.VisualTop >= textView.VerticalOffset)
                    return visualLine;
            }

            return null;
        }
    }
}
