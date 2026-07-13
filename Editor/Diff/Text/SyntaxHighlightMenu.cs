using MergetoolGui;
using UnityEditor;
using UnityEngine;
using XDiffGui.Options;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class SyntaxHighlightMenu
    {
        internal SyntaxHighlightMenu(ISyntaxHighlightListener syntaxHighlightListener)
        {
            mSyntaxHighlightListener = syntaxHighlightListener;
        }

        internal void SetSyntaxHighlight(Language targetLanguage)
        {
            mCurrentLanguage = targetLanguage;
        }

        internal void BuildMenuItems(GenericMenu menu, string submenuPath)
        {
            foreach (Language language in TextMateSupportedLanguages.Get())
            {
                string label = LanguageString.FromLanguage(language);
                bool isSelected = language == mCurrentLanguage;
                Language capturedLanguage = language;

                menu.AddItem(
                    new GUIContent(submenuPath + label.Replace("/", "\u2215")),
                    isSelected,
                    () => LanguageEntry_Click(capturedLanguage));
            }
        }

        void LanguageEntry_Click(Language language)
        {
            mCurrentLanguage = language;

            mSyntaxHighlightListener.OnSyntaxHighlightChanged(
                LanguageString.FromLanguage(language));
        }

        Language mCurrentLanguage = Language.PlainText;

        readonly ISyntaxHighlightListener mSyntaxHighlightListener;
    }
}
