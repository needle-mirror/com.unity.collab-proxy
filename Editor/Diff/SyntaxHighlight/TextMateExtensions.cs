using TextMateSharp.Grammars;
using Unity.CodeEditor;
using Unity.CodeEditor.Rendering;
using Unity.CodeEditor.TextMate;
using UnityEditor;
using XDiffGui.Options;

namespace Unity.PlasticSCM.Editor.Diff.SyntaxHighlight
{
    internal static class TextMateExtensions
    {
        internal static void SetLanguage(
            this TextMate.Installation textMateInstallation,
            XDiffGui.Options.Language language)
        {
            RegistryOptions registryOptions = (RegistryOptions)textMateInstallation.RegistryOptions;

            if (IsMinifiedDocument(textMateInstallation.EditorModel))
            {
                textMateInstallation.SetGrammar(null);
                return;
            }

            string languageId = TextMateLanguageId.Get(language);

            textMateInstallation.SetGrammar(
                registryOptions.GetScopeByLanguageId(languageId));
        }

        internal static TextMate.Installation InstallTextMate(this TextEditor editor)
        {
            ThemeName defaultTheme = EditorGUIUtility.isProSkin ?
                ThemeName.DarkPlus : ThemeName.LightPlus;

            return editor.InstallTextMate(
                new RegistryOptions(defaultTheme),
                false);
        }

        static bool IsMinifiedDocument(TextEditorModel textEditorModel)
        {
            if (textEditorModel == null)
                return false;

            // we consider a document minified if it has only one line and it's longer than 3000 characters
            return textEditorModel.GetNumberOfLines() == 1 &&
                   textEditorModel.GetLineLength(0) > VisualLine.LENGTH_LIMIT;
        }
    }
}
