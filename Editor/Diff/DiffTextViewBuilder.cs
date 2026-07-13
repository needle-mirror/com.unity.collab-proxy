using Unity.PlasticSCM.Editor.Diff.Text;
using Unity.CodeEditor;
using Unity.CodeEditor.Indentation.CSharp;

namespace Unity.PlasticSCM.Editor.Diff
{
    internal static class DiffTextViewBuilder
    {
        internal static TextEditor CreateTextEditor()
        {
            TextEditor result = new TextEditor();
            result.IsReadOnly = true;
            result.Options.AllowScrollBelowDocument = true;
            result.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            result.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //result.Background = ThemeBrushes.GetBrush(ThemeBrushes.Name.ControlBrush);
            //result.TextArea.TextView.LinkTextForegroundBrush = ThemeBrushes.GetBrush(ThemeBrushes.Name.HighlightBrush);
            result.TextArea.TextView.Options.EnableVirtualSpace = false;
            /*result.FontFamily = ThemeFontFamilies.GetFromName(
                PlasticGuiConfig.Get().Configuration.TextEditorFontFamily,
                ThemeFontFamilies.Name.DefaultMonospaceFontFamily);*/
            result.ShowLineNumbers = false;
            result.Options.ConvertTabsToSpaces = true;
            result.TextArea.RightClickMovesCaret = true;
            result.TextArea.IndentationStrategy = new CSharpIndentationStrategy(result.Options);
            result.TextArea.TextView.BackgroundRenderers.Add(
                new TextEditorDiffBackgroundRenderer(result));
            //result.SearchResultsBrush = ThemeBrushes.GetBrush(ThemeBrushes.Name.TextEditorSearchResultBrush);

            return result;
        }
    }
}
