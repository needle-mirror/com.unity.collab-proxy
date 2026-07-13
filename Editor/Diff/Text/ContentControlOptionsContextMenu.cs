using Codice.CM.Client.Differences.Graphic;
using MergetoolGui;
using PlasticGui;
using Unity.CodeEditor;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using XDiffGui.Options;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class ContentControlOptionsContextMenu
    {
        internal EncodingMenu EncodingMenu { get { return mEncodingMenu; } }
        internal TextEditorOptionsMenu TextEditorOptionsMenu { get { return mTextEditorOptionsMenu; } }

        internal ContentControlOptionsContextMenu(
            ISyntaxHighlightListener syntaxHighlightListener,
            IEncodingListener encodingListener,
            ICodeEditorSettingsChangeListener settingsChangeListener)
        {
            mSyntaxMenu = new SyntaxHighlightMenu(syntaxHighlightListener);

            mEncodingMenu = new EncodingMenu(encodingListener);

            mTextEditorOptionsMenu = new TextEditorOptionsMenu(settingsChangeListener);

            mSettingsChangeListener = settingsChangeListener;
        }

        internal ToolbarButton CreateButton()
        {
            mButton = ControlBuilder.Toolbar.CreateImageButtonLeft(
                Images.GetSettingsIcon(),
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EditorOptions),
                OnButtonClicked);

            return mButton;
        }

        internal void SetEntryData(EntryData entryData)
        {
            mEncodingMenu.SetEncodingMenu(TextBoxContributor.Left, entryData);
        }

        internal void SetLanguage(Language language)
        {
            mSyntaxMenu.SetSyntaxHighlight(language);
        }

        internal void Dispose()
        {
            mButton.clicked -= OnButtonClicked;
        }

        void OnButtonClicked()
        {
            GenericMenu menu = new GenericMenu();

            BuildMenu(menu);

            menu.DropDown(mButton.worldBound);
        }

        void BuildMenu(GenericMenu menu)
        {
            mSyntaxMenu.BuildMenuItems(menu,
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.SyntaxHighlight) + "/");

            mEncodingMenu.BuildMenuItems(menu,
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.Encoding) + "/");

            menu.AddSeparator(string.Empty);

            mTextEditorOptionsMenu.BuildMenuItems(menu,
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EditorOptions) + "/");

            menu.AddItem(
                new GUIContent(
                    PlasticLocalization.Name.ChangeEditorFont.GetString()),
                false,
                ChangeFontMenuItem_Click);
        }

        void ChangeFontMenuItem_Click()
        {
            ChangeFont.Execute(mSettingsChangeListener);
        }

        ToolbarButton mButton;

        readonly SyntaxHighlightMenu mSyntaxMenu;
        readonly EncodingMenu mEncodingMenu;
        readonly TextEditorOptionsMenu mTextEditorOptionsMenu;
        readonly ICodeEditorSettingsChangeListener mSettingsChangeListener;
    }
}
