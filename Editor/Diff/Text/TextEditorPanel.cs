using System;
using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.UIElements;

using Codice.Client.BaseCommands;
using Codice.CM.Client.Differences.Graphic;
using MergetoolGui;

using Unity.CodeEditor;
using Unity.CodeEditor.Document;
using Unity.CodeEditor.TextMate;
using Unity.PlasticSCM.Editor.Diff;
using Unity.PlasticSCM.Editor.Diff.SyntaxHighlight;
using Unity.PlasticSCM.Editor.Diff.Text;

using XDiffGui;
using XDiffGui.Options;

using Language = XDiffGui.Options.Language;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class TextEditorPanel : VisualElement
    {
        internal event Action<bool> DirtyStateChanged;

        internal bool IsTextDirty { get; private set; }
        internal Language CurrentLanguage => mCurrentLanguage;
        internal Unity.CodeEditor.TextEditor TextEditor =>
            mTextEditor;

        internal TextEditorPanel(
            ISaveChangesListener saveChangesListener)
        {
            style.flexGrow = 1;

            mTextEditor =
                DiffTextViewBuilder.CreateTextEditor();
            mTextEditor.Options.AllowScrollBelowDocument =
                false;
            mTextEditor.HorizontalScrollBarVisibility =
                ScrollBarVisibility.Auto;
            mTextEditor.VerticalScrollBarVisibility =
                ScrollBarVisibility.Auto;
            mTextEditor.ShowLineNumbers = true;
            mTextMateInstallation =
                mTextEditor.InstallTextMate();

            mTextEditorContextMenu =
                new DiffTextViewContextMenu(mTextEditor);

            mTextEditor.TextChanged +=
                TextEditor_TextChanged;

            mTextEditorSaveKeyBinding =
                TextEditorSaveKeyBinding.InitForEditor(
                    mTextEditor, saveChangesListener);

            Add(mTextEditor);
        }

        internal void ShowFileContent(
            string file,
            Encoding encoding,
            string pathForEdition)
        {
            mPathForEdition = pathForEdition;
            mEncoding = encoding;

            long fileSize =
                BaseServices.CalcFileSize(file, true);

            string content = ContentDataReader.GetContent(
                file,
                encoding,
                pathForEdition,
                out bool allowEdit);

            Language language =
                SyntaxLanguageFactory.GetSyntaxLanguage(
                    fileSize,
                    Path.GetExtension(file),
                    UnitySyntaxLanguages.AdditionalExtensions);

            if (allowEdit &&
                DraftStorage.TryLoadDraft(
                    pathForEdition, out string draftContent))
            {
                ShowContent(draftContent, language, allowEdit);
                SetDirtyState(true);
                return;
            }

            ShowContent(content, language, allowEdit);
        }

        internal void ShowContent(
            string content,
            Language language,
            bool isEditable)
        {
            if (content == null)
                return;

            mbIsUpdatingTextBox = true;

            try
            {
                if (language != mCurrentLanguage)
                    mTextMateInstallation.SetLanguage(
                        Language.PlainText);

                mCurrentLanguage = language;

                mTextEditor.Document =
                    new TextDocument(content);
                mTextEditor.IsReadOnly = !isEditable;

                mTextMateInstallation.SetLanguage(language);

                SetDirtyState(false);
            }
            finally
            {
                mbIsUpdatingTextBox = false;
            }
        }

        internal void SetLanguage(Language language)
        {
            mCurrentLanguage = language;
            mTextMateInstallation.SetLanguage(language);
        }

        internal void SaveChanges(
            IAfterSaveChangesListener afterSaveChangesListener)
        {
            try
            {
                SaveFileOperations.SaveAs(
                    mPathForEdition,
                    mTextEditor.Text,
                    mEncoding);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            DraftStorage.DeleteDraft(mPathForEdition);
            SetDirtyState(false);

            afterSaveChangesListener?.AfterSaveChanges(
                mPathForEdition);
        }

        internal void DiscardChanges()
        {
            DraftStorage.DeleteDraft(mPathForEdition);
            SetDirtyState(false);

            ShowContent(
                FileReader.ReadFile(mPathForEdition, mEncoding),
                SyntaxLanguageFactory.GetSyntaxLanguage(
                    BaseServices.CalcFileSize(
                        mPathForEdition, true),
                    Path.GetExtension(mPathForEdition),
                    UnitySyntaxLanguages.AdditionalExtensions),
                true);
        }

        internal void DeleteDraft()
        {
            DraftStorage.DeleteDraft(mPathForEdition);
        }

        internal void FinishPendingDraftSave()
        {
            mScheduledDraftSave?.Pause();
            mScheduledDraftSave = null;

            SaveDraftIfDirty();
        }

        internal void Dispose()
        {
            FinishPendingDraftSave();

            mTextEditor.TextChanged -=
                TextEditor_TextChanged;

            mTextMateInstallation.Dispose();
            mTextEditorContextMenu.Dispose();
            mTextEditorSaveKeyBinding.Dispose();
        }

        void TextEditor_TextChanged(
            object sender, EventArgs e)
        {
            if (mbIsUpdatingTextBox)
                return;

            SetDirtyState(true);
            ScheduleDraftSave();
        }

        void SetDirtyState(bool isDirty)
        {
            bool changed = IsTextDirty != isDirty;
            IsTextDirty = isDirty;

            if (changed)
                DirtyStateChanged?.Invoke(isDirty);
        }

        void ScheduleDraftSave()
        {
            if (string.IsNullOrEmpty(mPathForEdition))
                return;

            mScheduledDraftSave?.Pause();
            mScheduledDraftSave = this.schedule
                .Execute(SaveDraft)
                .StartingIn(DRAFT_SAVE_DELAY_MS);
        }

        void SaveDraftIfDirty()
        {
            if (!IsTextDirty)
                return;

            DraftStorage.SaveDraft(
                mPathForEdition, mTextEditor.Text);
        }

        void SaveDraft()
        {
            mScheduledDraftSave = null;

            SaveDraftIfDirty();
        }

        Unity.CodeEditor.TextEditor mTextEditor;
        TextMate.Installation mTextMateInstallation;
        DiffTextViewContextMenu mTextEditorContextMenu;
        TextEditorSaveKeyBinding mTextEditorSaveKeyBinding;

        string mPathForEdition;
        Encoding mEncoding;
        bool mbIsUpdatingTextBox;
        Language mCurrentLanguage = Language.PlainText;
        IVisualElementScheduledItem mScheduledDraftSave;

        const long DRAFT_SAVE_DELAY_MS = 1000;
    }
}

