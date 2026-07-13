using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Codice.Client.Common;
using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;
using MergetoolGui;
using PlasticGui;
using Unity.PlasticSCM.Editor.Diff.SyntaxHighlight;

using Unity.CodeEditor;
using Unity.CodeEditor.Document;
using Unity.CodeEditor.TextMate;
using Unity.PlasticSCM.Editor.Diff.Purged;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;

using UnityEngine;
using UnityEngine.UIElements;
using XDiffGui;
using XDiffGui.Drawing;
using XDiffGui.Options;
using XDiffGui.Semantic;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class DiffControl :
        VisualElement,
        IDifferencesNavigator,
        IComparisonMethodListener,
        ISyntaxHighlightListener,
        IEncodingListener,
        IBigFilePanelListener,
        ISaveChangesListener,
        IActionBarClickListener,
        IDiffContentLoader,
        DiffControlOperations.IDiffControl,
        ICodeEditorSettingsChangeListener
    {
        internal DiffControl(
            IAfterSaveChangesListener afterSaveChangesListener,
            IBigFileDownloader bigFileDownloader,
            IBigFileChecker bigFileChecker)
        {
            mAfterSaveChangesListener = afterSaveChangesListener;
            mBigFileDownloader = bigFileDownloader;
            mBigFileChecker = bigFileChecker;

            style.flexGrow = 1;

            CreateGUI();
        }

        internal void ShowData(DiffViewerData diffData)
        {
            FinishPendingDraftSave();

            mDiffDrawingInfo = null;
            mRightContentForEdition?.Dispose();
            mRightContentForEdition = null;

            mDiffViewerData = diffData;

            if (diffData == null)
                return;

            string leftContributor;
            string rightContributor;

            SymbolicNameParser.GetContributorSpecs(diffData.Left.SymbolicName,
                diffData.Right.SymbolicName, out leftContributor, out rightContributor);

            mContributorsHeaderPanel.SetNames(leftContributor, rightContributor);

            ReplaceMainView(mContentPanel);

            if (BigFileDiffCalculator.IsBigFileDiff(diffData, mBigFileDownloader, mBigFileChecker))
            {
                ShowBigFileMessage();
                return;
            }

            if (diffData.Left?.IsPurged == true || diffData.Right?.IsPurged == true)
            {
                ShowPurgedRevision(diffData);
                return;
            }

            DiffControlOperations.AsyncCalculateDiff(
                this,
                mDiffViewerData,
                mMovedDetectionOptions,
                mComparisonMethod,
                mRightContentForEdition,
                this,
                mSyncCalculator);
        }

        internal void Dispose()
        {
            FinishPendingDraftSave();

            mNavigationPanel.Dispose();
            mSaveChangesPanel.Dispose();
            mContributorsHeaderPanel.Dispose();
            mOptionsContextMenuButton.Dispose();

            if (mSrcTextEditor != null)
            {
                mSrcTextEditor.TextArea.UnregisterCallback<PointerUpEvent>(
                    TextEditorTextArea_PointerUp);

                mSrcTextMateInstallation.Dispose();

                mSrcDataContextMenu.Dispose();
            }

            if (mDstTextEditor != null)
            {
                mDstTextEditor.TextArea.UnregisterCallback<PointerUpEvent>(
                    TextEditorTextArea_PointerUp);

                mDstTextMateInstallation.Dispose();
                mDstTextChangedEvents.Dispose();

                mDstDataContextMenu.Dispose();
            }

            if (mDiffSplitter != null)
                mDiffSplitter.Dispose();

            if (mDiffScroll != null)
            {
                mDiffScroll.VerticalScroller.UnregisterCallback<GeometryChangedEvent>(OnVerticalScrollerGeometryChanged);
                mDiffScroll.Dispose();
            }

            if (mBigFileMessagePanel != null)
                mBigFileMessagePanel.Dispose();

            if (mSrcLineNumbers != null)
                mSrcLineNumbers.Dispose();

            if (mDstLineNumbers != null)
                mDstLineNumbers.Dispose();

            if (mActionBarsHandler != null)
                mActionBarsHandler.Dispose();

            if (mPurgedRevisionControl != null)
                mPurgedRevisionControl.Dispose();
        }

        void IBigFilePanelListener.OnCalculateDifferencesButtonClick()
        {
            DiffControlOperations.CalculateDifferencesButtonClick(
                mBigFileDownloader,
                this,
                mDiffViewerData,
                mMovedDetectionOptions,
                mComparisonMethod,
                mRightContentForEdition,
                this,
                mSyncCalculator,
                GetDefaultEncoding(),
                onPurgedDetected: ShowPurgedRevision);
        }

        void IDifferencesNavigator.GoToLine(int line)
        {
            GoToLine(line);
        }

        int IDifferencesNavigator.SetCurrentDifference(int difference)
        {
            return SetCurrentDifference(difference);
        }

        int SetCurrentDifference(int difference)
        {
            if (difference == -1)
            {
                mNavigationPanel.SetNavigationInfo();
                return -1;
            }

            int line = mNavigationPanel.SetCurrentDifference(difference);

            SetCurrentDifferenceOnTextBoxes(difference);
            SetCurrentDifferenceOnDiffSplitter(difference);

            return line;
        }

        string IDiffContentLoader.LoadLeftContent(Encoding encoding)
        {
            if (mDiffViewerData.Left.File == null)
                return mDiffViewerData.Left.Content;

            return FileReader.ReadFile(mDiffViewerData.Left.File, encoding);
        }

        string IDiffContentLoader.LoadRightContent(Encoding encoding)
        {
            if (mDiffViewerData.IsEditable &&
                DraftStorage.TryLoadDraft(
                    mDiffViewerData.PathForEdition, out string draftContent))
            {
                mDraftLoaded = true;
                return draftContent;
            }

            mDraftLoaded = false;

            if (mDiffViewerData.Right.File == null)
                return mDiffViewerData.Right.Content;

            return FileReader.ReadFile(mDiffViewerData.Right.File, encoding);
        }

        void IComparisonMethodListener.OnComparisonMethodChanged(
            ComparisonMethodTypes comparisonMethod)
        {
            if (mComparisonMethodSaver != null)
                mComparisonMethodSaver.OnComparisonMethodChanged(comparisonMethod);

            mOptionsContextMenuButton.ComparisonMenu.UpdateIsCheckedValueForMenuItems(comparisonMethod);

            mComparisonMethod = comparisonMethod;

            DiffControlOperations.ComparisonMethodChanged(
                comparisonMethod,
                this,
                mDiffViewerData,
                mMovedDetectionOptions,
                mRightContentForEdition,
                this,
                mSyncCalculator);
        }

        bool IEncodingListener.OnEncodingChanged(
            TextBoxContributor contributor, Encoding encoding)
        {
            DraftStorage.DeleteDraft(mDiffViewerData.PathForEdition);

            return DiffControlOperations.EncodingChanged(
                contributor,
                encoding,
                this,
                mDiffViewerData,
                mMovedDetectionOptions,
                mComparisonMethod,
                mRightContentForEdition,
                this,
                mSyncCalculator);
        }

        void ISyntaxHighlightListener.OnSyntaxHighlightChanged(string languageString)
        {
            SetSyntaxLanguage(LanguageString.FromString(languageString));
        }

        void ISaveChangesListener.OnSaveChanges()
        {
            if (!mbIsRightTextBoxDirty)
                return;

            SaveChanges();
        }

        void ISaveChangesListener.OnDiscardChanges()
        {
            DraftStorage.DeleteDraft(mDiffViewerData.PathForEdition);

            mRightContentForEdition?.Dispose();
            mRightContentForEdition = null;

            DiffControlOperations.AsyncCalculateDiff(
                this,
                mDiffViewerData,
                mMovedDetectionOptions,
                mComparisonMethod,
                mRightContentForEdition,
                this,
                mSyncCalculator);
        }

        void OnDstTextEditorTextChanged()
        {
            mLastRightTextBoxCaretLine = mDstTextEditor.TextArea.Caret.Line;

            mDiffScroll.UpdateScrollBarMetrics();

            OnRightTextBoxDirty();
            ScheduleDraftSave();
        }

        void OnDstTextEditorDelayedTextChanged()
        {
            DiffControlOperations.OnRightTextViewContentChanged(
                this,
                mDiffViewerData,
                mMovedDetectionOptions,
                mComparisonMethod,
                mbIsRightTextBoxDirty,
                mRightContentForEdition,
                this,
                mSyncCalculator);
        }

        void IActionBarClickListener.OnButtonClick(DiffAction action, DiffButtonActions actionType)
        {
            OnRightTextBoxDirty();

            ExecuteDiffAction(action, actionType);

            NaturalDifferencesNavigation.SetupNavigationAfterDiffAction(
                mNavigationPanel.Navigation, this, action.DiffIndex);
        }

        void DiffControlOperations.IDiffControl.ShowDiffPanel()
        {
            ShowDiffPanel();
        }

        void DiffControlOperations.IDiffControl.ShowWaitingAnimation()
        {
            ShowWaitingAnimation();
        }

        void DiffControlOperations.IDiffControl.HideWaitingAnimation()
        {
            ((IProgressControls)mOverlayProgressControls).HideProgress();

            if (mDiffPanel == null)
                return;

            ReplaceView(mDiffPanel);
        }

        void DiffControlOperations.IDiffControl.ClearNavigationPanel()
        {
            mNavigationPanel.ClearNavigation();
        }

        void DiffControlOperations.IDiffControl.SetOptionsContextMenuButtonInfo(
            ComparisonMethodTypes comparisonMethod,
            Language syntaxHighlightLanguage, DiffViewerData diffInfo)
        {
            mOptionsContextMenuButton.SetInfo(
                comparisonMethod, syntaxHighlightLanguage, diffInfo);
        }

        void DiffControlOperations.IDiffControl.UpdateTextContent(
            DiffContent leftContent, DiffContent rightContent)
        {
            mDiffScroll?.DisableScrollEvents();

            try
            {
                UpdateLeftTextBoxContent(leftContent);

                if (!mbIsRightTextBoxDirty)
                    UpdateRightTextBoxContent(rightContent);

                if (!mDiffViewerData.IsEditable)
                {
                    mRightContentForEdition?.Dispose();
                    mRightContentForEdition = null;
                    return;
                }

                if (mbIsRightTextBoxDirty)
                    return;

                mRightContentForEdition?.Dispose();

                if (mDstTextMateInstallation.EditorModel != null)
                {
                    mRightContentForEdition = new EditorDiffContent(
                        mDstTextMateInstallation.EditorModel.DocumentSnapshot);
                }

                if (mDraftLoaded)
                {
                    mDraftLoaded = false;
                    OnRightTextBoxDirty();
                }
            }
            finally
            {
                mDiffScroll?.EnableScrollEvents();
            }
        }

        void DiffControlOperations.IDiffControl.SetSyntaxLanguage(
            Language language)
        {
            SetSyntaxLanguage(language);
        }

        Language DiffControlOperations.IDiffControl.GetSyntaxLanguage()
        {
            return mCurrentSyntaxLanguage;
        }

        void DiffControlOperations.IDiffControl.SetEditable(bool editable)
        {
            mDstTextEditor.IsReadOnly = !editable;
            mActionBarsHandler?.SetActionBarsVisibility(editable);
        }

        void DiffControlOperations.IDiffControl.OnRightTextBoxClean()
        {
            OnRightTextBoxClean();
        }

        void DiffControlOperations.IDiffControl.SetDiffDrawingInfo(
            DiffDrawingInfo diffDrawingInfo)
        {
            mDiffDrawingInfo = diffDrawingInfo;
        }

        void DiffControlOperations.IDiffControl.SetDifferencesInfo(
            DiffDrawingInfo diffDrawingInfo,
            DiffViewerData diffData)
        {
            if (diffDrawingInfo == null)
                return;

            mNavigationPanel.UpdateDiffPositions(
                GenerateDiffPositions(diffDrawingInfo));

            SetDrawingInfo(diffDrawingInfo);

            UpdateDiffDrawingInfoColors();

            SetInitialDiffPosition(diffDrawingInfo);

            mNavigationPanel.SetNavigationInfo();

            mMessagePanel.HandleMessage(
                DiffMessageBuilder.AppendContentRelatedMessages(
                    diffData, diffDrawingInfo.HasDifferences));
        }

        void DiffControlOperations.IDiffControl.UpdateDifferencesInfoSilently(
            DiffDrawingInfo diffDrawingInfo)
        {
            mDiffScroll?.UpdateVirtualMapping(diffDrawingInfo.Mapping);

            SetDrawingInfo(diffDrawingInfo);

            mDiffScroll?.InvalidateViews();

            mNavigationPanel.UpdateDiffPositions(
                GenerateDiffPositions(diffDrawingInfo));

            mMessagePanel.HandleMessage(DiffMessage.Get(
                mDiffViewerData.Message, diffDrawingInfo.HasDifferences));

            NaturalDifferencesNavigation.UpdateCurrentDifference(
                mNavigationPanel.Navigation,
                this,
                diffDrawingInfo,
                mLastRightTextBoxCaretLine != -1,
                mLastRightTextBoxCaretLine);
        }

        void DiffControlOperations.IDiffControl.HandleException(Exception e)
        {
            mMessagePanel.HandleException(e);
        }

        void DiffControlOperations.IDiffControl.EnableBigFileMessagePanel()
        {
            mBigFileMessagePanel.Enable();
        }

        void DiffControlOperations.IDiffControl.DisableBigFileMessagePanel()
        {
            mBigFileMessagePanel.Disable();
        }

        void ICodeEditorSettingsChangeListener.OnFontFamilyChanged(string font)
        {
            if (string.IsNullOrEmpty(font) || font == ChangeFontDialog.DEFAULT_FONT)
            {
                if (mSrcTextEditor != null)
                    mSrcTextEditor.Font = null;
                if (mDstTextEditor != null)
                    mDstTextEditor.Font = null;
                return;
            }

            if (mSrcTextEditor != null)
                mSrcTextEditor.Font = Font.CreateDynamicFontFromOSFont(
                    font, mSrcTextEditor.FontSize);
            if (mDstTextEditor != null)
                mDstTextEditor.Font = Font.CreateDynamicFontFromOSFont(
                    font, mDstTextEditor.FontSize);
        }

        void ICodeEditorSettingsChangeListener.OnTabSizeChanged(int tabSize)
        {
            SetTabSize(tabSize);

            mOptionsContextMenuButton.TextEditorOptionsMenu.SetTabSize(tabSize);
        }

        void ICodeEditorSettingsChangeListener.OnColumnGuidesChanged(IEnumerable<int> columnGuides)
        {
            SetColumnGuides(columnGuides);

            mOptionsContextMenuButton.TextEditorOptionsMenu.SetColumnGuides(columnGuides);
        }

        void ICodeEditorSettingsChangeListener.OnConvertTabsToWhitespacesChanged(bool value)
        {
            SetConvertTabsToSpaces(value);

            mOptionsContextMenuButton.TextEditorOptionsMenu.SetConvertTabsToSpaces(value);
        }

        void ICodeEditorSettingsChangeListener.OnViewWhitespacesChanged(bool value)
        {
            SetViewWhitespaces(value);

            mOptionsContextMenuButton.TextEditorOptionsMenu.SetViewWhitespaces(value);
        }

        void ICodeEditorSettingsChangeListener.OnViewEOLChanged(bool value)
        {
            SetViewEOL(value);

            mOptionsContextMenuButton.TextEditorOptionsMenu.SetViewEOL(value);
        }

        VisualElement BuildDiffPanel(ContributorsHeaderPanel contributorsHeaderPanel)
        {
            mSrcTextEditor = DiffTextViewBuilder.CreateTextEditor();
            mSrcTextEditor.name = "Source TextEditor";
            mSrcTextMateInstallation = mSrcTextEditor.InstallTextMate();

            mSrcDataContextMenu = new DiffTextViewContextMenu(mSrcTextEditor);

            mSrcTextEditor.TextArea.RegisterCallback<PointerUpEvent>(
                TextEditorTextArea_PointerUp);

            mDstTextEditor = DiffTextViewBuilder.CreateTextEditor();
            mDstTextEditor.name = "Destination TextEditor";
            mDstTextMateInstallation = mDstTextEditor.InstallTextMate();

            mDstDataContextMenu = new DiffTextViewContextMenu(mDstTextEditor);

            mDstTextEditor.TextArea.RegisterCallback<PointerUpEvent>(
                TextEditorTextArea_PointerUp);

            mDstTextChangedEvents = new TextEditorTextChangedEvents(
                mDstTextEditor,
                OnDstTextEditorTextChanged,
                OnDstTextEditorDelayedTextChanged);

            TextEditorSaveKeyBinding.InitForEditor(mDstTextEditor, this);

            mSrcLineNumbers = new LineNumbersView(mSrcTextEditor);
            mDstLineNumbers = new LineNumbersView(mDstTextEditor);

            mActionBarsHandler = new ActionBarsHandler(this);
            mActionBarsHandler.BuildLeftActionBar(mSrcTextEditor);
            mActionBarsHandler.BuildRightActionBar(mDstTextEditor);

            VisualElement srcEditorPanel = BuildLeftEditorPanel(
                mSrcTextEditor,
                mSrcLineNumbers,
                mActionBarsHandler.LeftActionBar);

            VisualElement dstEditorPanel = BuildRightEditorPanel(
                mDstTextEditor,
                mDstLineNumbers,
                mActionBarsHandler.RightActionBar);

            mDiffSplitter = new DiffSplitter(
                mSrcTextEditor, mDstTextEditor,
                srcEditorPanel, dstEditorPanel);
            mDiffSplitter.style.width = DIFF_SPLITTER_WIDTH;
            mDiffSplitter.style.flexGrow = 0;
            mDiffSplitter.style.flexShrink = 0;

            mDiffScroll = new DiffScroll(
                mDiffSplitter,
                mSrcTextEditor,
                mDstTextEditor,
                mSrcLineNumbers,
                mDstLineNumbers,
                mActionBarsHandler.LeftActionBar,
                mActionBarsHandler.RightActionBar);

            mScrollSummaryView = new ScrollSummaryView(mDiffScroll);
            mScrollSummaryView.style.width = DIFF_SUMMARY_WIDTH;

            VisualElement editorsRow = new VisualElement();
            editorsRow.style.flexGrow = 1;
            editorsRow.style.flexDirection = FlexDirection.Row;

            editorsRow.Add(srcEditorPanel);
            editorsRow.Add(mDiffSplitter);
            editorsRow.Add(dstEditorPanel);
            editorsRow.Add(mDiffScroll.VerticalScroller);
            editorsRow.Add(mScrollSummaryView);

            VisualElement bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.flexShrink = 0;

            mDiffScroll.HorizontalScroller.style.flexGrow = 1;
            mDiffScroll.HorizontalScroller.style.flexShrink = 1;

            mVerticalScrollerFiller = new VisualElement();
            mVerticalScrollerFiller.style.flexShrink = 0;

            VisualElement scrollSummaryFiller = new VisualElement();
            scrollSummaryFiller.style.width = DIFF_SUMMARY_WIDTH;
            scrollSummaryFiller.style.flexShrink = 0;

            mDiffScroll.VerticalScroller.RegisterCallback<GeometryChangedEvent>(OnVerticalScrollerGeometryChanged);

            bottomRow.Add(mDiffScroll.HorizontalScroller);
            bottomRow.Add(mVerticalScrollerFiller);
            bottomRow.Add(scrollSummaryFiller);

            VisualElement mainPanel = new VisualElement();
            mainPanel.style.flexGrow = 1;

            mainPanel.Add(editorsRow);
            mainPanel.Add(bottomRow);

            LoadTextEditorOptions();

            return mainPanel;
        }

        static VisualElement BuildLeftEditorPanel(
            Unity.CodeEditor.TextEditor editor,
            LineNumbersView lineNumbersView,
            ActionBar leftActionBar)
        {
            VisualElement result = new VisualElement();
            result.style.flexGrow = 1;
            result.style.flexBasis = 0;
            result.style.flexDirection = FlexDirection.Row;
            result.style.overflow = Overflow.Hidden;

            result.Add(lineNumbersView);
            result.Add(editor);
            result.Add(leftActionBar);

            return result;
        }

        static VisualElement BuildRightEditorPanel(
            Unity.CodeEditor.TextEditor editor,
            LineNumbersView lineNumbersView,
            ActionBar rightActionBar)
        {
            VisualElement result = new VisualElement();
            result.style.flexGrow = 1;
            result.style.flexBasis = 0;
            result.style.flexDirection = FlexDirection.Row;
            result.style.overflow = Overflow.Hidden;

            result.Add(rightActionBar);
            result.Add(lineNumbersView);
            result.Add(editor);

            return result;
        }

        void OnVerticalScrollerGeometryChanged(GeometryChangedEvent evt)
        {
            mVerticalScrollerFiller.style.width = evt.newRect.width;
        }

        void LoadTextEditorOptions()
        {
            PlasticGuiConfigData config = PlasticGuiConfig.Get().Configuration;

            mOptionsContextMenuButton.TextEditorOptionsMenu.SetOptions(
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
        }

        void ShowDiffPanel()
        {
            if (mDiffPanel == null)
                mDiffPanel = BuildDiffPanel(mContributorsHeaderPanel);

            EnableToolbar();
            ReplaceView(mDiffPanel);
        }

        void ShowWaitingAnimation()
        {
            mOverlayProgressControls.ShowProgress(
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.DiffWaitingAnimation),
                TimeSpan.FromMilliseconds(750));
        }

        void SetViewWhitespaces(bool isEnabled)
        {
            if (mSrcTextEditor != null)
            {
                mSrcTextEditor.Options.ShowSpaces = isEnabled;
                mSrcTextEditor.Options.ShowTabs = isEnabled;
            }

            if (mDstTextEditor != null)
            {
                mDstTextEditor.Options.ShowSpaces = isEnabled;
                mDstTextEditor.Options.ShowTabs = isEnabled;
            }
        }

        void SetViewEOL(bool isEnabled)
        {
            if (mSrcTextEditor != null)
                mSrcTextEditor.Options.ShowEndOfLine = isEnabled;
            if (mDstTextEditor != null)
                mDstTextEditor.Options.ShowEndOfLine = isEnabled;
        }

        void SetTabSize(int tabSize)
        {
            if (tabSize < 1)
                return;

            if (mSrcTextEditor != null)
                mSrcTextEditor.Options.IndentationSize = tabSize;
            if (mDstTextEditor != null)
                mDstTextEditor.Options.IndentationSize = tabSize;
        }

        void SetConvertTabsToSpaces(bool isEnabled)
        {
            if (mSrcTextEditor != null)
                mSrcTextEditor.Options.ConvertTabsToSpaces = isEnabled;
            if (mDstTextEditor != null)
                mDstTextEditor.Options.ConvertTabsToSpaces = isEnabled;
        }

        void SetColumnGuides(IEnumerable<int> columnGuides)
        {
            bool showColumnRulers = columnGuides != null && columnGuides.Count() > 0;

            if (mSrcTextEditor != null)
            {
                mSrcTextEditor.Options.ShowColumnRulers = showColumnRulers;
                mSrcTextEditor.Options.ColumnRulerPositions = columnGuides;
            }

            if (mDstTextEditor != null)
            {
                mDstTextEditor.Options.ShowColumnRulers = showColumnRulers;
                mDstTextEditor.Options.ColumnRulerPositions = columnGuides;
            }
        }

        void ExecuteDiffAction(DiffAction action, DiffButtonActions actionType)
        {
            mLastRightTextBoxCaretLine = -1;

            if (actionType == DiffButtonActions.Delete)
            {
                DeleteDifference(action);
                return;
            }

            RestoreDifference(action);
        }

        void GoToLine(int line)
        {
            int currentDifference = mNavigationPanel.CurrentDifference;

            SetCurrentDifference(currentDifference);

            SetDiffPosition(line);
        }

        void TextEditorTextArea_PointerUp(PointerUpEvent evt)
        {
            if (mDiffDrawingInfo == null || mSrcTextEditor == null ||
                mDstTextEditor == null || mNavigationPanel == null ||
                mDiffScroll == null)
                return;

            Unity.CodeEditor.TextEditor textEditor =
                (mSrcTextEditor.TextArea == evt.currentTarget) ?
                    mSrcTextEditor : mDstTextEditor;

            List<ColorTextRegion> regions = (textEditor == mSrcTextEditor) ?
                mDiffDrawingInfo.DiffRegions.Left :
                mDiffDrawingInfo.DiffRegions.Right;

            int caretLine = textEditor.TextArea.Caret.Line;
            if (caretLine == -1)
                return;

            int diff = DifferenceOfRealLineProvider.GetDifference(
                caretLine, regions);

            if (diff == -1)
                return;

            mNavigationPanel.Navigation.ResetSkipOnce();
            SetCurrentDifference(diff);

            mDiffScroll.InvalidateViews();
        }

        void SetCurrentDifferenceOnTextBoxes(int difference)
        {
            UpdateCurrentLines.FromCurrentDifference(
                mSrcTextBoxDrawingInfo, difference);
            UpdateCurrentLines.FromCurrentDifference(
                mDstTextBoxDrawingInfo, difference);

            mSrcTextEditor.TextArea.TextView.InvalidateVisual();
            mDstTextEditor.TextArea.TextView.InvalidateVisual();
        }

        void SetCurrentDifferenceOnDiffSplitter(int currentDifference)
        {
            if (currentDifference < 0)
                return;

            ColorTextRegion currentLeftRegion =
                (currentDifference < mDiffSplitter.DrawingInfo.Left.Count) ?
                    mDiffSplitter.DrawingInfo.Left[currentDifference] : null;

            mDiffSplitter.DrawingInfo.CurrentLines = CurrentLines.From(currentLeftRegion);
            mDiffSplitter.Redraw();
        }

        void SetInitialDiffPosition(DiffDrawingInfo diffDrawingInfo)
        {
            if (diffDrawingInfo.HasDifferences)
            {
                SetFirstDiffPosition();
                return;
            }

            SetDiffPosition(1);
        }

        void SetFirstDiffPosition()
        {
            int line = SetCurrentDifference(0);
            SetDiffPosition(line);
        }

        void SetDiffPosition(int line)
        {
            mDiffScroll?.SetDiffPosition(line);
        }

        List<int> GenerateDiffPositions(DiffDrawingInfo diffDrawingInfo)
        {
            return DiffPositions.GenerateDiffPositions(
                diffDrawingInfo.DiffRegions, diffDrawingInfo.Mapping,
                new TextEditorCollapsedLinesProvider(mSrcTextEditor),
                new TextEditorCollapsedLinesProvider(mDstTextEditor));
        }

        void UpdateDiffDrawingInfoColors()
        {
            if (mDiffDrawingInfo == null || !mDiffDrawingInfo.HasDifferences)
                return;

            ColorChanger.ChangeColor(
                mDiffDrawingInfo,
                ColorConfiguration.Value.BaseColor,
                ColorConfiguration.Value.SourceColor);

            SetDrawingInfo(mDiffDrawingInfo);
        }

        void UpdateLeftTextBoxContent(DiffContent leftContent)
        {
            if (leftContent == null)
                return;

            mSrcTextEditor.Document = new TextDocument(
                leftContent.TextFile);
        }

        void UpdateRightTextBoxContent(DiffContent rightContent)
        {
            if (rightContent == null)
                return;

            mDstTextChangedEvents.DisableEvents();
            try
            {
                mDstTextEditor.Document = new TextDocument(
                    rightContent.TextFile);
            }
            finally
            {
                mDstTextChangedEvents.EnableEvents();
            }
        }

        void SetDrawingInfo(DiffDrawingInfo diffDrawingInfo)
        {
            mSrcTextBoxDrawingInfo = new TextBoxDrawingInfo(
                diffDrawingInfo.DiffRegions.Left,
                diffDrawingInfo.OriginalDiffRegions.Left,
                diffDrawingInfo.InsideLineRegions.Left,
                diffDrawingInfo.MoveInfo.Regions.Left,
                diffDrawingInfo.MoveInfo.DiffRegions.Left,
                diffDrawingInfo.MoveInfo.InsideLineRegions.Left,
                diffDrawingInfo.Mapping.Left);

            mSrcTextEditor.Tag = mSrcTextBoxDrawingInfo;
            mSrcTextEditor.TextArea.TextView.InvalidateVisual();

            mDstTextBoxDrawingInfo = new TextBoxDrawingInfo(
                diffDrawingInfo.DiffRegions.Right,
                diffDrawingInfo.OriginalDiffRegions.Right,
                diffDrawingInfo.InsideLineRegions.Right,
                diffDrawingInfo.MoveInfo.Regions.Right,
                diffDrawingInfo.MoveInfo.DiffRegions.Right,
                diffDrawingInfo.MoveInfo.InsideLineRegions.Right,
                diffDrawingInfo.Mapping.Right);

            mDstTextEditor.Tag = mDstTextBoxDrawingInfo;
            mDstTextEditor.TextArea.TextView.InvalidateVisual();
            mDstLineNumbers.InvalidateVisual();

            mDiffSplitter.SetDrawingInfo(new DiffSplitterDrawingInfo()
            {
                Left = diffDrawingInfo.DiffRegions.Left,
                Right = diffDrawingInfo.DiffRegions.Right,
                MovedRegions = diffDrawingInfo.MoveInfo.Regions,
                IsSemanticDiff = diffDrawingInfo.IsSemanticDiff
            });

            mDiffScroll.UpdateVirtualMapping(diffDrawingInfo.Mapping);

            List<DiffSummaryDraw> summaryDraws = DiffSummaryDrawBuilder.BuildSummaryDraws(
                mDiffDrawingInfo.OriginalDiffRegions.Left,
                mDiffDrawingInfo.Mapping.Left,
                mDiffDrawingInfo.OriginalDiffRegions.Right,
                mDiffDrawingInfo.Mapping.Right);

            mScrollSummaryView.UpdateDrawingInfo(
                summaryDraws, mDiffDrawingInfo.Mapping.Left.Count);

            mActionBarsHandler.SetDrawingInfo(diffDrawingInfo.DiffRegions);

            UpdateCurrentLines.FromCurrentDifference(
                mSrcTextBoxDrawingInfo, mNavigationPanel.CurrentDifference);
            UpdateCurrentLines.FromCurrentDifference(
                mDstTextBoxDrawingInfo, mNavigationPanel.CurrentDifference);
        }

        void DeleteDifference(DiffAction action)
        {
            mDstTextChangedEvents.DisableEvents();
            try
            {
                DiffBarActions.DeleteDifference(
                    action,
                    mDstTextEditor.Document,
                    mDiffDrawingInfo.DiffRegions.Right);

                ((TextBoxDrawingInfo)mDstTextEditor.Tag).Update(
                    mDiffDrawingInfo.DiffRegions.Right);
            }
            finally
            {
                mDstTextChangedEvents.EnableEvents();
                mDstTextEditor.TextArea.TextView.InvalidateVisual();
            }

            DiffControlOperations.OnRightTextViewContentChanged(
                this,
                mDiffViewerData,
                mMovedDetectionOptions,
                mComparisonMethod,
                mbIsRightTextBoxDirty,
                mRightContentForEdition,
                this,
                mSyncCalculator);
        }

        void RestoreDifference(DiffAction action)
        {
            mDstTextChangedEvents.DisableEvents();

            try
            {
                DiffBarActions.RestoreDifference(
                    action,
                    mSrcTextEditor.Document,
                    mDstTextEditor.Document,
                    mDiffDrawingInfo.DiffRegions.Right);

                ((TextBoxDrawingInfo)mDstTextEditor.Tag).Update(
                    mDiffDrawingInfo.DiffRegions.Right);
            }
            finally
            {
                mDstTextChangedEvents.EnableEvents();
                mDstTextEditor.TextArea.TextView.InvalidateVisual();
            }

            DiffControlOperations.OnRightTextViewContentChanged(
                this,
                mDiffViewerData,
                mMovedDetectionOptions,
                mComparisonMethod,
                mbIsRightTextBoxDirty,
                mRightContentForEdition,
                this,
                mSyncCalculator);
        }

        void OnRightTextBoxDirty()
        {
            mbIsRightTextBoxDirty = true;

            mContributorsHeaderPanel.OnRightTextBoxDirty();

            mSaveChangesPanel.style.display = DisplayStyle.Flex;
        }

        void OnRightTextBoxClean()
        {
            mbIsRightTextBoxDirty = false;

            mSaveChangesPanel.style.display = DisplayStyle.None;

            mContributorsHeaderPanel.OnRightTextBoxClean();
        }

        void ScheduleDraftSave()
        {
            if (mDiffViewerData == null ||
                string.IsNullOrEmpty(mDiffViewerData.PathForEdition))
                return;

            mScheduledDraftSave?.Pause();
            mScheduledDraftSave = this.schedule
                .Execute(SaveDraft)
                .StartingIn(DRAFT_SAVE_DELAY_MS);
        }

        void FinishPendingDraftSave()
        {
            mScheduledDraftSave?.Pause();
            mScheduledDraftSave = null;

            SaveDraftIfDirty();
        }

        void SaveDraftIfDirty()
        {
            if (!mbIsRightTextBoxDirty)
                return;

            if (mDiffViewerData == null ||
                string.IsNullOrEmpty(mDiffViewerData.PathForEdition))
                return;

            DraftStorage.SaveDraft(
                mDiffViewerData.PathForEdition, mDstTextEditor.Text);
        }

        void SaveDraft()
        {
            mScheduledDraftSave = null;

            SaveDraftIfDirty();
        }

        void SetSyntaxLanguage(Language language)
        {
            mCurrentSyntaxLanguage = language;

            mSrcTextMateInstallation.SetLanguage(language);
            mDstTextMateInstallation.SetLanguage(language);
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

        void ShowBigFileMessage()
        {
            if (mBigFileMessagePanel == null)
                mBigFileMessagePanel = new BigFileMessagePanel(this, true);

            mBigFileMessagePanel.UpdateDisplayData(
                BigFileDisplayData.Build(
                    mDiffViewerData, mBigFileDownloader));

            DisableToolbar();
            mMessagePanel.HandleMessage(string.Empty);
            ReplaceView(mBigFileMessagePanel);
        }

        void ReplaceMainView(VisualElement panel)
        {
            if (hierarchy.childCount == 1 && hierarchy[0] == panel)
                return;

            Clear();
            Add(panel);
            panel.Focus();
        }

        void ReplaceView(VisualElement view)
        {
            if (mCurrentView == view)
                return;

            if (mCurrentView != null)
                mContainerPanel.Remove(mCurrentView);

            mCurrentView = view;
            mContainerPanel.Add(view);
        }

        void EnableToolbar()
        {
            mNavigationPanel.Enable();
            mOptionsContextMenuButton.Enable();
        }

        void DisableToolbar()
        {
            mNavigationPanel.Disable();
            mOptionsContextMenuButton.Disable();
        }

        Encoding GetDefaultEncoding()
        {
            return EncodingManager.GetEncodingFromType(
                PlasticGuiConfig.Get().Configuration.Encoding);
        }

        void SaveChanges()
        {
            try
            {
                if (!File.Exists(mDiffViewerData.PathForEdition))
                    File.Create(mDiffViewerData.PathForEdition).Close();

                SaveFileOperations.SaveAs(
                    mDiffViewerData.PathForEdition, mRightContentForEdition.Text,
                    mDiffViewerData.Right.Encoding);

                mDiffViewerData.Right.File = mDiffViewerData.PathForEdition;

                DraftStorage.DeleteDraft(mDiffViewerData.PathForEdition);
                OnRightTextBoxClean();

                mAfterSaveChangesListener?.AfterSaveChanges(mDiffViewerData.PathForEdition);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        void CreateGUI()
        {
            mMessagePanel = new MessagePanel();
            VisualElement toolbarPanel = CreateToolbarPanel();
            VisualElement diffSplitView = CreateDiffSplitView();

            mContentPanel = new VisualElement();
            mContentPanel.style.flexGrow = 1;

            mContentPanel.Add(mMessagePanel);
            mContentPanel.Add(toolbarPanel);
            mContentPanel.Add(diffSplitView);

            Add(mContentPanel);
        }

        VisualElement CreateToolbarPanel()
        {
            UnityEditor.UIElements.Toolbar toolbar = ControlBuilder.Toolbar.Create();

            mNavigationPanel = new NavigationPanel(this);

            toolbar.Add(mNavigationPanel);

            mSaveChangesPanel = new SaveChangesPanel(this);

            toolbar.Add(mSaveChangesPanel);

            mOptionsContextMenuButton = new OptionsContextMenuButton(
                this, this, this, this);
            VisualElement optionsButton = mOptionsContextMenuButton.CreateButton();
            optionsButton.style.flexShrink = 0;

            toolbar.Add(optionsButton);

            return toolbar;
        }

        VisualElement CreateDiffSplitView()
        {
            VisualElement result = new VisualElement();
            result.style.flexGrow = 1;

            VisualElement fileContainerPanel = CreateFileContainerPanel();
            result.Add(fileContainerPanel);

            return result;
        }

        VisualElement CreateFileContainerPanel()
        {
            VisualElement result = new VisualElement();
            result.style.flexGrow = 1;

            mContributorsHeaderPanel = new ContributorsHeaderPanel();
            mContainerPanel = new VisualElement();
            mContainerPanel.style.flexGrow = 1;

            mOverlayProgressControls = new OverlayProgressControls();

            result.Add(mContributorsHeaderPanel);
            result.Add(mContainerPanel);
            result.Add(mOverlayProgressControls);

            return result;
        }

        class TextEditorTextChangedEvents
        {
            internal TextEditorTextChangedEvents(
                Unity.CodeEditor.TextEditor textEditor,
                Action textChangedAction,
                Action delayedTextChangeAction)
            {
                mTextEditor = textEditor;

                mOnTextEditorTextChanged = textChangedAction;
                mOnTextEditorDelayedTextChanged = delayedTextChangeAction;

                textEditor.TextChanged += TextEditor_TextChanged;

                mRunner = new DelayedActionBySecondsRunner(
                    OnDelayedTextChanged,
                    UnityConstants.SEARCH_DELAYED_INPUT_ACTION_INTERVAL);
            }

            internal void EnableEvents()
            {
                mAreEventsEnabled = true;
            }

            internal void DisableEvents()
            {
                mAreEventsEnabled = false;
            }

            internal void Dispose()
            {
                mTextEditor.TextChanged += TextEditor_TextChanged;
            }

            void TextEditor_TextChanged(object sender, EventArgs e)
            {
                if (!mAreEventsEnabled)
                    return;

                mOnTextEditorTextChanged();
                mRunner.Run();
            }

            void OnDelayedTextChanged()
            {
                if (!mAreEventsEnabled)
                    return;

                mOnTextEditorDelayedTextChanged();
            }

            volatile bool mAreEventsEnabled = true;

            readonly DelayedActionBySecondsRunner mRunner;
            readonly Action mOnTextEditorTextChanged;
            readonly Action mOnTextEditorDelayedTextChanged;
            readonly Unity.CodeEditor.TextEditor mTextEditor;
        }

        PurgedRevisionControl mPurgedRevisionControl;

        readonly IAfterSaveChangesListener mAfterSaveChangesListener;
        readonly IBigFileDownloader mBigFileDownloader;
        readonly IBigFileChecker mBigFileChecker;

        ScrollSummaryView mScrollSummaryView;

        bool mbIsRightTextBoxDirty = false;
        bool mDraftLoaded = false;
        int mLastRightTextBoxCaretLine = -1;
        ComparisonMethodTypes mComparisonMethod;

        DiffDrawingInfo mDiffDrawingInfo;
        TextBoxDrawingInfo mSrcTextBoxDrawingInfo;
        TextBoxDrawingInfo mDstTextBoxDrawingInfo;
        EditorDiffContent mRightContentForEdition;
        DiffViewerData mDiffViewerData;

        MessagePanel mMessagePanel;
        NavigationPanel mNavigationPanel;
        SaveChangesPanel mSaveChangesPanel;
        OptionsContextMenuButton mOptionsContextMenuButton;
        ContributorsHeaderPanel mContributorsHeaderPanel;
        VisualElement mContainerPanel;
        DiffTextViewContextMenu mSrcDataContextMenu;
        DiffTextViewContextMenu mDstDataContextMenu;

        VisualElement mCurrentView;
        BigFileMessagePanel mBigFileMessagePanel;
        OverlayProgressControls mOverlayProgressControls;
        VisualElement mDiffPanel;

        Unity.CodeEditor.TextEditor mSrcTextEditor;
        TextMate.Installation mSrcTextMateInstallation;
        Unity.CodeEditor.TextEditor mDstTextEditor;
        TextMate.Installation mDstTextMateInstallation;
        TextEditorTextChangedEvents mDstTextChangedEvents;
        LineNumbersView mSrcLineNumbers;
        LineNumbersView mDstLineNumbers;
        ActionBarsHandler mActionBarsHandler;
        DiffSplitter mDiffSplitter;
        DiffScroll mDiffScroll;
        VisualElement mVerticalScrollerFiller;

        Language mCurrentSyntaxLanguage;

        VisualElement mContentPanel;
        IVisualElementScheduledItem mScheduledDraftSave;

        readonly MovedDetectionOptionsInfo mMovedDetectionOptions =
            MovedDetectionOptionsInfo.LoadConfig();
        readonly DiffCalculatorSync mSyncCalculator = new DiffCalculatorSync();
        readonly IComparisonMethodListener mComparisonMethodSaver = new SaveGuiClientConfig();

        const int DIFF_SPLITTER_WIDTH = 50;
        const int DIFF_SUMMARY_WIDTH = 14;
        const long DRAFT_SAVE_DELAY_MS = 1000;
    }
}
