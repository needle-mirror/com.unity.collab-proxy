using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codice.Client.Common;
using Codice.Client.Common.Threading;
using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;
using MergetoolGui;
using PlasticGui;

using Unity.CodeEditor;
using Unity.PlasticSCM.Editor.Diff.Purged;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;

using UnityEngine;
using UnityEngine.UIElements;
using XDiffGui;
using XDiffGui.Options;
using Language = XDiffGui.Options.Language;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class ContentControl :
        VisualElement,
        ISaveChangesListener,
        ICodeEditorSettingsChangeListener,
        ISyntaxHighlightListener,
        IEncodingListener,
        IBigFilePanelListener
    {
        internal ContentControl(
            IAfterSaveChangesListener afterSaveChangesListener,
            IBigFileDownloader bigFileDownloader,
            IBigFileChecker bigFileChecker)
        {
            mAfterSaveChangesListener = afterSaveChangesListener;
            mBigFileDownloader = bigFileDownloader;
            mBigFileChecker = bigFileChecker;

            CreateGUI();

            LoadTextEditorOptions();
        }

        internal void ShowData(DiffViewerData data)
        {
            mTextEditorPanel.FinishPendingDraftSave();

            mDiffViewerData = data;

            ReplaceMainView(mNormalContentPanel);

            EntryData entryData = data.Left;

            mMessageView.HandleMessage(data.Message);

            mFileTextView.text = PlasticLocalization.GetString(
                PlasticLocalization.Name.Content, entryData.SymbolicName);

            mOptionsContextMenu.SetEntryData(entryData);

            mFile = entryData.File;
            mPathForEdition = data.PathForEdition;

            if (BigFileDiffCalculator.IsBigFileDiff(data, mBigFileDownloader, mBigFileChecker))
            {
                ShowBigFileMessage();
                return;
            }

            if (data.Left?.IsPurged == true || data.Right?.IsPurged == true)
            {
                ShowPurgedRevision(data);
                return;
            }

            ReplaceView(mTextEditorPanel);

            if (string.IsNullOrEmpty(entryData.File))
            {
                ShowEntryDataContent(entryData.Content);
                return;
            }

            ShowFileContent(entryData.File, entryData.Encoding);
        }

        internal void Dispose()
        {
            mTextEditorPanel.DirtyStateChanged -=
                OnDirtyStateChanged;

            mTextEditorPanel.Dispose();
            mSaveChangesPanel.Dispose();
            mOptionsContextMenu.Dispose();

            if (mBigFileMessagePanel != null)
                mBigFileMessagePanel.Dispose();

            if (mPurgedRevisionControl != null)
                mPurgedRevisionControl.Dispose();
        }

        void SaveChanges()
        {
            mTextEditorPanel.SaveChanges(
                mAfterSaveChangesListener);
        }

        void ICodeEditorSettingsChangeListener.OnFontFamilyChanged(string font)
        {
            Unity.CodeEditor.TextEditor textEditor = mTextEditorPanel.TextEditor;

            if (string.IsNullOrEmpty(font) || font == ChangeFontDialog.DEFAULT_FONT)
            {
                textEditor.Font = null;
                return;
            }

            textEditor.Font = Font.CreateDynamicFontFromOSFont(
                font, textEditor.FontSize);
        }

        void ICodeEditorSettingsChangeListener.OnTabSizeChanged(int tabSize)
        {
            SetTabSize(tabSize);
        }

        void ICodeEditorSettingsChangeListener.OnColumnGuidesChanged(IEnumerable<int> columnGuides)
        {
            SetColumnGuides(columnGuides);
        }

        void ICodeEditorSettingsChangeListener.OnConvertTabsToWhitespacesChanged(bool value)
        {
            SetConvertTabsToSpaces(value);
        }

        void ICodeEditorSettingsChangeListener.OnViewWhitespacesChanged(bool value)
        {
            SetViewWhitespaces(value);
        }

        void ICodeEditorSettingsChangeListener.OnViewEOLChanged(bool value)
        {
            SetViewEOL(value);
        }

        void ISyntaxHighlightListener.OnSyntaxHighlightChanged(string languageString)
        {
            Language language = LanguageString.FromString(languageString);
            mTextEditorPanel.SetLanguage(language);
            mOptionsContextMenu.SetLanguage(language);
        }

        bool IEncodingListener.OnEncodingChanged(
            TextBoxContributor contributor, Encoding encoding)
        {
            mTextEditorPanel.DeleteDraft();

            mTextEditorPanel.ShowFileContent(
                mPathForEdition ?? mFile, encoding, mPathForEdition);
            mOptionsContextMenu.SetLanguage(
                mTextEditorPanel.CurrentLanguage);

            return true;
        }

        void ISaveChangesListener.OnSaveChanges()
        {
            if (!mTextEditorPanel.IsTextDirty)
                return;

            SaveChanges();
        }

        void ISaveChangesListener.OnDiscardChanges()
        {
            mTextEditorPanel.DiscardChanges();
            mOptionsContextMenu.SetLanguage(
                mTextEditorPanel.CurrentLanguage);
        }

        void IBigFilePanelListener.OnCalculateDifferencesButtonClick()
        {
            mBigFileMessagePanel.Disable();

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    mBigFileDownloader.DownloadFiles(
                        mDiffViewerData, GetDefaultEncoding());
                },
                /*afterOperationDelegate*/ delegate
                {
                    mBigFileMessagePanel.Enable();

                    if (waiter.Exception != null)
                    {
                        MergetoolExceptionsHandler.DisplayException(
                            waiter.Exception);
                        return;
                    }

                    if (mDiffViewerData.Left?.IsPurged == true ||
                        mDiffViewerData.Right?.IsPurged == true)
                    {
                        ShowPurgedRevision(mDiffViewerData);
                        return;
                    }

                    mFile = mDiffViewerData.Left.File;

                    ReplaceView(mTextEditorPanel);

                    if (string.IsNullOrEmpty(mFile))
                    {
                        ShowEntryDataContent(mDiffViewerData.Left.Content);
                        return;
                    }

                    ShowFileContent(
                        mFile, mDiffViewerData.Left.Encoding);
                });
        }

        void ShowEntryDataContent(string content)
        {
            mTextEditorPanel.ShowContent(
                content, Language.PlainText, false);
            mOptionsContextMenu.SetLanguage(Language.PlainText);
        }

        void ShowBigFileMessage()
        {
            if (mBigFileMessagePanel == null)
                mBigFileMessagePanel = new BigFileMessagePanel(this, false);

            mBigFileMessagePanel.UpdateDisplayData(
                BigFileDisplayData.Build(
                    mDiffViewerData, mBigFileDownloader));

            mMessageView.HandleMessage(string.Empty);
            ReplaceView(mBigFileMessagePanel);
        }

        void ShowPurgedRevision(DiffViewerData data)
        {
            if (mPurgedRevisionControl == null)
            {
                mPurgedRevisionControl = new PurgedRevisionControl(
                    mAfterSaveChangesListener);
            }

            mPurgedRevisionControl.ShowData(data);
            ReplaceMainView(mPurgedRevisionControl);
        }

        void ReplaceMainView(VisualElement panel)
        {
            if (hierarchy.childCount == 1 && hierarchy[0] == panel)
                return;

            Clear();
            Add(panel);
        }

        void ReplaceView(VisualElement view)
        {
            if (mCurrentView == view)
                return;

            if (mCurrentView != null)
                mContainerPanel.Remove(mCurrentView);

            mContainerPanel.Add(view);
            mCurrentView = view;
        }

        Encoding GetDefaultEncoding()
        {
            return EncodingManager.GetEncodingFromType(
                PlasticGuiConfig.Get().Configuration.Encoding);
        }

        void ShowFileContent(string file, Encoding encoding)
        {
            mTextEditorPanel.ShowFileContent(
                file, encoding, mPathForEdition);
            mOptionsContextMenu.SetLanguage(
                mTextEditorPanel.CurrentLanguage);
        }

        void OnDirtyStateChanged(bool isDirty)
        {
            if (isDirty)
            {
                mFileTextView.style.color = UnityStyles.Colors.RedText;
                EditedTextBlockMarker.AddMark(mFileTextView);
                mSaveChangesPanel.style.display = DisplayStyle.Flex;
            }
            else
            {
                mFileTextView.style.color = StyleKeyword.Null;
                EditedTextBlockMarker.RemoveMark(mFileTextView);
                mSaveChangesPanel.style.display = DisplayStyle.None;
            }
        }

        void LoadTextEditorOptions()
        {
            PlasticGuiConfigData config = PlasticGuiConfig.Get().Configuration;

            mOptionsContextMenu.TextEditorOptionsMenu.SetOptions(
                config.EditorOptionsShowWhiteSpaces,
                config.EditorOptionsConvertTabsToSpaces,
                config.EditorOptionsShowEOL,
                config.EditorOptionsTabSize,
                config.EditorOptionsColumnGuides);

            SetViewWhitespaces(config.EditorOptionsShowWhiteSpaces);
            SetConvertTabsToSpaces(config.EditorOptionsConvertTabsToSpaces);
            SetViewEOL(config.EditorOptionsShowEOL);
            SetTabSize(config.EditorOptionsTabSize);
            SetColumnGuides(config.EditorOptionsColumnGuides);

            ((ICodeEditorSettingsChangeListener)this).OnFontFamilyChanged(
                config.TextEditorFontFamily);
        }

        void SetViewWhitespaces(bool isEnabled)
        {
            mTextEditorPanel.TextEditor.Options.ShowSpaces = isEnabled;
            mTextEditorPanel.TextEditor.Options.ShowTabs = isEnabled;
        }

        void SetViewEOL(bool isEnabled)
        {
            mTextEditorPanel.TextEditor.Options.ShowEndOfLine = isEnabled;
        }

        void SetTabSize(int tabSize)
        {
            if (tabSize < 1)
                return;

            mTextEditorPanel.TextEditor.Options.IndentationSize = tabSize;
        }

        void SetConvertTabsToSpaces(bool isEnabled)
        {
            mTextEditorPanel.TextEditor.Options.ConvertTabsToSpaces = isEnabled;
        }

        void SetColumnGuides(IEnumerable<int> columnGuides)
        {
            bool showColumnRulers = columnGuides != null && columnGuides.Count() > 0;

            mTextEditorPanel.TextEditor.Options.ShowColumnRulers = showColumnRulers;
            mTextEditorPanel.TextEditor.Options.ColumnRulerPositions = columnGuides != null
                ? new List<int>(columnGuides)
                : null;
        }

        void CreateGUI()
        {
            style.flexGrow = 1;

            mMessageView = new MessagePanel();
            VisualElement fileNamesView = CreateFileNameView();

            mTextEditorPanel = new TextEditorPanel(this);
            mTextEditorPanel.DirtyStateChanged +=
                OnDirtyStateChanged;

            mContainerPanel = new VisualElement();
            mContainerPanel.style.flexGrow = 1;

            mNormalContentPanel = new VisualElement();
            mNormalContentPanel.style.flexGrow = 1;
            mNormalContentPanel.Add(mMessageView);
            mNormalContentPanel.Add(fileNamesView);
            mNormalContentPanel.Add(mContainerPanel);

            Add(mNormalContentPanel);

            ReplaceView(mTextEditorPanel);
        }

        VisualElement CreateFileNameView()
        {
            UnityEditor.UIElements.Toolbar toolbar = ControlBuilder.Toolbar.Create();

            mFileTextView = ControlBuilder.Label.CreateSelectableLabel();
            mFileTextView.style.unityTextAlign = UnityEngine.TextAnchor.MiddleLeft;
            mFileTextView.style.overflow = Overflow.Hidden;
            mFileTextView.style.textOverflow = TextOverflow.Ellipsis;
            mFileTextView.style.flexGrow = 1;
            mFileTextView.style.flexShrink = 1;
            toolbar.Add(mFileTextView);

            mSaveChangesPanel = new SaveChangesPanel(this);
            mSaveChangesPanel.style.flexShrink = 0;
            toolbar.Add(mSaveChangesPanel);

            mOptionsContextMenu = new ContentControlOptionsContextMenu(
                this, this, this);
            VisualElement optionsButton = mOptionsContextMenu.CreateButton();
            optionsButton.style.flexShrink = 0;
            toolbar.Add(optionsButton);

            return toolbar;
        }

        MessagePanel mMessageView;
        Label mFileTextView;
        TextEditorPanel mTextEditorPanel;
        SaveChangesPanel mSaveChangesPanel;
        ContentControlOptionsContextMenu mOptionsContextMenu;
        VisualElement mNormalContentPanel;
        VisualElement mContainerPanel;
        VisualElement mCurrentView;
        BigFileMessagePanel mBigFileMessagePanel;
        PurgedRevisionControl mPurgedRevisionControl;

        readonly IAfterSaveChangesListener mAfterSaveChangesListener;
        readonly IBigFileDownloader mBigFileDownloader;
        readonly IBigFileChecker mBigFileChecker;
        DiffViewerData mDiffViewerData;
        string mFile;
        string mPathForEdition;
    }
}
