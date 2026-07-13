using System;

using PlasticGui;
using Unity.CodeEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal static class ChangeFont
    {
        internal static void Execute(
            ICodeEditorSettingsChangeListener settingsChangeListener)
        {
            try
            {
                string currentFont =
                    PlasticGuiConfig.Get().Configuration.TextEditorFontFamily;

                ChangeFontDialog.Show(
                    currentFont,
                    selectedFont => OnFontSelected(
                        selectedFont, settingsChangeListener));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        // internal for testing purposes
        internal static void OnFontSelected(
            string selectedFont,
            ICodeEditorSettingsChangeListener settingsChangeListener)
        {
            if (string.IsNullOrEmpty(selectedFont))
                return;

            settingsChangeListener.OnFontFamilyChanged(selectedFont);

            PlasticGuiConfig.Get().Configuration.TextEditorFontFamily = selectedFont;
            PlasticGuiConfig.Get().Save();
        }
    }
}
