using Codice.CM.Client.Differences;
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
    internal class OptionsContextMenuButton
    {
        internal ComparisonMethodMenu ComparisonMenu { get { return mComparisonMenu; } }
        internal TextEditorOptionsMenu TextEditorOptionsMenu { get { return mTextEditorOptionsMenu; } }

        internal OptionsContextMenuButton(
            IComparisonMethodListener comparisonMethodListener,
            ISyntaxHighlightListener syntaxHighlightListener,
            IEncodingListener encodingListener,
            ICodeEditorSettingsChangeListener settingsChangeListener)
        {
            mComparisonMenu = new ComparisonMethodMenu(comparisonMethodListener);

            mSyntaxMenu = new SyntaxHighlightMenu(syntaxHighlightListener);

            mLeftEncodingMenu = new EncodingMenu(encodingListener);
            mRightEncodingMenu = new EncodingMenu(encodingListener);

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

        internal void SetInfo(
            ComparisonMethodTypes comparisonMethod,
            Language language,
            DiffViewerData diffViewerData)
        {
            mButton.SetEnabled(true);

            mComparisonMenu.SetComparisonMethod(comparisonMethod);
            mSyntaxMenu.SetSyntaxHighlight(language);
            mLeftEncodingMenu.SetEncodingMenu(
                TextBoxContributor.Left, diffViewerData.Left);
            mRightEncodingMenu.SetEncodingMenu(
                TextBoxContributor.Right, diffViewerData.Right);
        }

        internal void Enable()
        {
            mButton.SetEnabled(true);
        }

        internal void Disable()
        {
            mButton.SetEnabled(false);
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
            mComparisonMenu.BuildMenuItems(menu,
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.ComparisonMethodMenuTitle) + "/");

            mSyntaxMenu.BuildMenuItems(menu,
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.SyntaxHighlight) + "/");

            menu.AddSeparator(string.Empty);

            mLeftEncodingMenu.BuildMenuItems(menu,
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.LeftEncoding) + "/");

            mRightEncodingMenu.BuildMenuItems(menu,
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.RightEncoding) + "/");

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

        readonly ComparisonMethodMenu mComparisonMenu;
        readonly SyntaxHighlightMenu mSyntaxMenu;
        readonly EncodingMenu mLeftEncodingMenu;
        readonly EncodingMenu mRightEncodingMenu;
        readonly TextEditorOptionsMenu mTextEditorOptionsMenu;
        readonly ICodeEditorSettingsChangeListener mSettingsChangeListener;
    }
}
