using System;
using System.Linq;
using Unity.CodeEditor.Document;
using Unity.CodeEditor.Editing;
using Unity.CodeEditor.Rendering;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Selection = Unity.CodeEditor.Editing.Selection;
using TextAnchor = UnityEngine.TextAnchor;

namespace Unity.CodeEditor.Search
{
    internal class SearchPanel : VisualElement
    {
        readonly TextEditor _textEditor;
        readonly TextArea _textArea;
        readonly SearchResultBackgroundRenderer _renderer;

        TextField _searchTextField;
        TextField _replaceTextField;
        Label _searchCounterLabel;

        Button _matchCaseButton;
        Button _wholeWordButton;
        Button _regexButton;

        Button _previousButton;
        Button _nextButton;
        Button _closeButton;

        Button _replaceNextButton;
        Button _replaceAllButton;

        VisualElement _replaceRow;
        VisualElement _messageRow;
        Button _expandReplaceButton;

        ISearchStrategy _strategy;
        int _currentSearchResultIndex = -1;

        static readonly EventCallback<MouseEnterEvent> _onButtonMouseEnter =
            e => ((VisualElement)e.currentTarget).style.backgroundColor = StyleKeyword.Null;
        static readonly EventCallback<MouseLeaveEvent> _onButtonMouseLeave =
            e => ((VisualElement)e.currentTarget).style.backgroundColor = Color.clear;

        bool _matchCase;
        bool _wholeWords;
        bool _useRegex;
        bool _isReplaceMode;

        const int SEARCH_FIELD_FONT_SIZE = 11;
        const int BUTTON_FONT_SIZE = 11;
        const int SEARCH_FIELD_WIDTH = 215;

        internal bool IsClosed { get; private set; } = true;

        internal bool IsOpened => !IsClosed;

        internal string SearchPattern
        {
            get => _searchTextField?.value ?? string.Empty;
            set
            {
                if (_searchTextField != null)
                    _searchTextField.value = value ?? string.Empty;
            }
        }

        internal string ReplacePattern
        {
            get => _replaceTextField?.value ?? string.Empty;
        }

        internal bool IsReplaceMode
        {
            get => _isReplaceMode;
            set
            {
                if (_textEditor.IsReadOnly)
                    value = false;

                _isReplaceMode = value;
                UpdateReplaceModeVisibility();
            }
        }

        internal event EventHandler<SearchOptionsChangedEventArgs> SearchOptionsChanged;

        internal SearchPanel(TextEditor textEditor)
        {
            _textEditor = textEditor ?? throw new ArgumentNullException(nameof(textEditor));
            _textArea = textEditor.TextArea;

            _renderer = new SearchResultBackgroundRenderer(TextEditorColors.SearchResultColor);

            BuildUI();

            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            IsClosed = true;
            style.display = DisplayStyle.None;
        }

        internal void Dispose()
        {
            UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            _searchTextField.UnregisterValueChangedCallback(OnSearchTextChanged);
            _textArea.DocumentChanged -= OnDocumentChanged;

            if (_textArea.Document != null)
                _textArea.Document.TextChanged -= OnDocumentTextChanged;

            _textArea.TextView.BackgroundRenderers.Remove(_renderer);
        }

        internal void Open()
        {
            if (!IsClosed)
            {
                Reactivate();
                return;
            }

            IsClosed = false;
            style.display = DisplayStyle.Flex;
            BringToFront();

            _expandReplaceButton.style.display = _textEditor.IsReadOnly
                ? DisplayStyle.None : DisplayStyle.Flex;

            if (!_textArea.TextView.BackgroundRenderers.Contains(_renderer))
                _textArea.TextView.BackgroundRenderers.Add(_renderer);

            if (!(_textArea.Selection.IsEmpty || _textArea.Selection.IsMultiline))
                SearchPattern = _textArea.Selection.GetText();

            DoSearch(true);
            Reactivate();
        }

        internal void Close()
        {
            if (IsClosed)
                return;

            IsClosed = true;
            style.display = DisplayStyle.None;

            if (_messageRow != null)
                _messageRow.style.display = DisplayStyle.None;

            _textArea.TextView.BackgroundRenderers.Remove(_renderer);
            CleanSearchResults();
            _textArea.TextView.InvalidateLayer(KnownLayer.Selection);

            schedule.Execute(() =>
                _textArea.Focus());
        }

        internal void Reactivate()
        {
            if (_searchTextField == null)
                return;

            _searchTextField.Focus();
            _searchTextField.SelectAll();
        }

        internal void FindNext(int startOffset = -1)
        {
            var result = _renderer.CurrentResults.FindFirstSegmentWithStartAfter(
                startOffset == -1 ? _textArea.Caret.Offset : startOffset)
                ?? _renderer.CurrentResults.FirstSegment;

            if (result != null)
                SetCurrentSearchResult(result);
        }

        internal void FindPrevious()
        {
            int offset = Math.Max(
                _textArea.Caret.Offset - _textArea.Selection.Length, 0);
            var result = _renderer.CurrentResults.FindFirstSegmentWithStartAfter(offset);

            if (result != null)
                result = _renderer.CurrentResults.GetPreviousSegment(result);
            if (result == null)
                result = _renderer.CurrentResults.LastSegment;

            if (result != null)
                SetCurrentSearchResult(result);
        }

        internal void ReplaceNext()
        {
            if (!IsReplaceMode)
                return;

            FindNext(Math.Max(_textArea.Caret.Offset - _textArea.Selection.Length, 0));
            if (!_textArea.Selection.IsEmpty)
                _textArea.Selection.ReplaceSelectionWithText(ReplacePattern ?? string.Empty);

            UpdateSearch();
        }

        internal void ReplaceAll()
        {
            if (!IsReplaceMode)
                return;

            var replacement = ReplacePattern ?? string.Empty;
            var document = _textArea.Document;
            using (document.RunUpdate())
            {
                var segments = _renderer.CurrentResults
                    .OrderByDescending(x => x.EndOffset).ToArray();
                foreach (var segment in segments)
                {
                    document.Replace(segment.StartOffset, segment.Length,
                        new StringTextSource(replacement));
                }
            }
        }

        #region UI Construction

        void BuildUI()
        {
            style.position = Position.Absolute;
            style.top = 0;
            style.right = 0;

            style.flexDirection = FlexDirection.Column;
            style.paddingLeft = 10;
            style.paddingRight = 5;
            style.paddingTop = 5;
            style.paddingBottom = 5;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderBottomColor = TextEditorColors.Border;
            style.borderLeftColor = TextEditorColors.Border;
            style.borderRightColor = TextEditorColors.Border;
            style.borderBottomLeftRadius = 4;
            style.borderBottomRightRadius = 4;
            style.backgroundColor = TextEditorColors.Background;

            Add(BuildSearchRow());

            _replaceRow = BuildReplaceRow();
            _replaceRow.style.display = DisplayStyle.None;
            Add(_replaceRow);

            _messageRow = BuildMessageRow();
            _messageRow.style.display = DisplayStyle.None;
            Add(_messageRow);

            _textArea.DocumentChanged += OnDocumentChanged;
            if (_textArea.Document != null)
                _textArea.Document.TextChanged += OnDocumentTextChanged;
        }

        VisualElement BuildSearchRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.height = 24;

            _expandReplaceButton = CreateIconButton(
                "\u25B6", SearchPanelLocalization.ToggleReplaceTooltip, OnExpandReplaceClicked);
            _expandReplaceButton.style.width = 16;
            _expandReplaceButton.style.height = 16;
            _expandReplaceButton.style.fontSize = 8;
            _expandReplaceButton.style.marginRight = 5;
            row.Add(_expandReplaceButton);

            var searchFieldWrapper = new VisualElement();
            searchFieldWrapper.style.width = SEARCH_FIELD_WIDTH;

            _searchTextField = new TextField();
            _searchTextField.style.fontSize = SEARCH_FIELD_FONT_SIZE;
            _searchTextField.style.marginLeft = 0;
            _searchTextField.style.marginRight = 0;
            _searchTextField.style.marginTop = 0;
            _searchTextField.style.marginBottom = 0;

            var textInput = _searchTextField.Q("unity-text-input");
            if (textInput != null)
            {
                textInput.style.paddingRight = 72;
                textInput.style.overflow = Overflow.Visible;
            }

            _searchTextField.SetTextCursor();

            _searchTextField.RegisterValueChangedCallback(OnSearchTextChanged);
            SetupPlaceholder(_searchTextField, SearchPanelLocalization.FindPlaceholder);
            searchFieldWrapper.Add(_searchTextField);

            var toggleOverlay = new VisualElement();
            toggleOverlay.style.position = Position.Absolute;
            toggleOverlay.style.right = 2;
            toggleOverlay.style.top = 0;
            toggleOverlay.style.bottom = 0;
            toggleOverlay.style.flexDirection = FlexDirection.Row;
            toggleOverlay.style.alignItems = Align.Center;
            toggleOverlay.pickingMode = PickingMode.Ignore;

            _matchCaseButton = CreateToggleButton(
                "Aa", SearchPanelLocalization.MatchCaseTooltip,
                () => { _matchCase = !_matchCase; UpdateSearch(); },
                () => _matchCase);
            toggleOverlay.Add(_matchCaseButton);

            _wholeWordButton = CreateToggleButton(
                "W", SearchPanelLocalization.MatchWholeWordTooltip,
                () => { _wholeWords = !_wholeWords; UpdateSearch(); },
                () => _wholeWords);
            toggleOverlay.Add(_wholeWordButton);

            _regexButton = CreateToggleButton(
                ".*", SearchPanelLocalization.UseRegularExpressionsTooltip,
                () => { _useRegex = !_useRegex; UpdateSearch(); },
                () => _useRegex);
            toggleOverlay.Add(_regexButton);

            if (textInput != null)
                textInput.Add(toggleOverlay);
            else
                searchFieldWrapper.Add(toggleOverlay);
            row.Add(searchFieldWrapper);

            _previousButton = CreateIconButton(
                "\u2191", SearchPanelLocalization.PreviousMatchTooltip, () => FindPrevious());
            _previousButton.style.marginLeft = 5;
            row.Add(_previousButton);

            _nextButton = CreateIconButton(
                "\u2193", SearchPanelLocalization.NextMatchTooltip, () => FindNext());
            row.Add(_nextButton);

            _closeButton = CreateIconButton(
                "\u2715", SearchPanelLocalization.CloseTooltip, () => Close());
            row.Add(_closeButton);

            return row;
        }

        VisualElement BuildReplaceRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.height = 24;

            var spacer = new VisualElement();
            spacer.style.width = 21;
            row.Add(spacer);

            _replaceTextField = new TextField();
            _replaceTextField.style.fontSize = SEARCH_FIELD_FONT_SIZE;
            _replaceTextField.style.width = SEARCH_FIELD_WIDTH;
            _replaceTextField.style.marginLeft = 0;
            _replaceTextField.style.marginRight = 0;
            _replaceTextField.style.marginTop = 0;
            _replaceTextField.style.marginBottom = 0;

            _replaceTextField.SetTextCursor();

            SetupPlaceholder(_replaceTextField, SearchPanelLocalization.ReplacePlaceholder);
            row.Add(_replaceTextField);

            _replaceNextButton = CreateIconButton(
                "R", SearchPanelLocalization.ReplaceNextTooltip, () => ReplaceNext());
            _replaceNextButton.style.marginLeft = 5;
            row.Add(_replaceNextButton);

            _replaceAllButton = CreateIconButton(
                "RA", SearchPanelLocalization.ReplaceAllTooltip, () => ReplaceAll());
            row.Add(_replaceAllButton);

            return row;
        }

        VisualElement BuildMessageRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 2;

            if (!_textEditor.IsReadOnly)
            {
                var spacer = new VisualElement();
                spacer.style.width = 21;
                row.Add(spacer);
            }

            _searchCounterLabel = new Label();
            _searchCounterLabel.style.fontSize = 11;
            row.Add(_searchCounterLabel);

            return row;
        }

        static void SetupPlaceholder(TextField textField, string placeholderText)
        {
            var placeholder = new Label(placeholderText);
            placeholder.style.position = Position.Absolute;
            placeholder.style.left = 2;
            placeholder.style.top = 0;
            placeholder.style.bottom = 0;
            placeholder.style.unityTextAlign = TextAnchor.MiddleLeft;
            placeholder.style.color = TextEditorColors.SecondaryText;
            placeholder.style.fontSize = 11;
            placeholder.pickingMode = PickingMode.Ignore;

            var textInput = textField.Q("unity-text-input");
            textInput?.Add(placeholder);

            placeholder.style.display = string.IsNullOrEmpty(textField.value)
                ? DisplayStyle.Flex : DisplayStyle.None;
            textField.RegisterValueChangedCallback(e =>
                placeholder.style.display = string.IsNullOrEmpty(e.newValue)
                    ? DisplayStyle.Flex : DisplayStyle.None);
        }

        Button CreateIconButton(string text, string tooltip, Action onClick)
        {
            var button = CreateBaseButton(text, tooltip, onClick);
            button.style.width = 20;
            button.style.height = 16;
            button.RegisterCallback(_onButtonMouseEnter);
            button.RegisterCallback(_onButtonMouseLeave);

            return button;
        }

        Button CreateToggleButton(
            string text, string tooltip, Action onClick, Func<bool> isActive)
        {
            var button = CreateBaseButton(text, tooltip, null);
            button.style.width = 20;
            button.style.height = 13;
            button.style.flexShrink = 0;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopColor = Color.clear;
            button.style.borderBottomColor = Color.clear;
            button.style.borderLeftColor = Color.clear;
            button.style.borderRightColor = Color.clear;
            button.SetMouseCursor(MouseCursor.Arrow);
            button.clickable = new Clickable(() =>
            {
                onClick();
                UpdateToggleButtonAppearance(button, isActive());
            });
            return button;
        }

        static Button CreateBaseButton(string text, string tooltip, Action onClick)
        {
            var button = new Button(onClick);
            button.text = text;
            button.tooltip = tooltip;
            button.style.marginLeft = 1;
            button.style.marginRight = 1;
            button.style.paddingLeft = 0;
            button.style.paddingRight = 0;
            button.style.paddingTop = 0;
            button.style.paddingBottom = 0;
            button.style.fontSize = BUTTON_FONT_SIZE;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.color = TextEditorColors.IconColor;
            button.style.backgroundColor = Color.clear;

            return button;
        }

        static void UpdateToggleButtonAppearance(Button button, bool active)
        {
            var color = active ? TextEditorColors.ToggleActive : Color.clear;
            button.style.backgroundColor = color;
            button.style.borderTopColor = color;
            button.style.borderBottomColor = color;
            button.style.borderLeftColor = color;
            button.style.borderRightColor = color;
        }

        void UpdateReplaceModeVisibility()
        {
            if (_replaceRow == null)
                return;

            _replaceRow.style.display = _isReplaceMode ? DisplayStyle.Flex : DisplayStyle.None;
            _expandReplaceButton.text = _isReplaceMode ? "\u25BC" : "\u25B6";
        }

        #endregion

        #region Search Logic

        void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            UpdateSearch();
        }

        void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            if (e.OldDocument != null)
                e.OldDocument.TextChanged -= OnDocumentTextChanged;
            if (e.NewDocument != null)
            {
                e.NewDocument.TextChanged += OnDocumentTextChanged;
                DoSearch(false);
            }
        }

        void OnDocumentTextChanged(object sender, EventArgs e)
        {
            DoSearch(false);
        }

        void UpdateSearch()
        {
            try
            {
                // Hide message while results exist to prevent "no matches found" flickering
                if (_renderer.CurrentResults.Count > 0 && _messageRow != null)
                    _messageRow.style.display = DisplayStyle.None;

                _strategy = SearchStrategyFactory.Create(
                    SearchPattern ?? "",
                    !_matchCase,
                    _wholeWords,
                    _useRegex ? SearchMode.RegEx : SearchMode.Normal);

                SearchOptionsChanged?.Invoke(this,
                    new SearchOptionsChangedEventArgs(
                        SearchPattern, _matchCase, _useRegex, _wholeWords));

                DoSearch(true);
            }
            catch (SearchPatternException)
            {
                CleanSearchResults();
                UpdateSearchLabel();
            }
        }

        void DoSearch(bool changeSelection)
        {
            if (IsClosed)
                return;

            CleanSearchResults();

            int offset = Math.Max(
                _textArea.Caret.Offset - _textArea.Selection.Length, 0);

            if (changeSelection)
                _textArea.ClearSelection();

            if (!string.IsNullOrEmpty(SearchPattern) && _strategy != null)
            {
                foreach (var result in _strategy
                    .FindAll(_textArea.Document, 0, _textArea.Document.TextLength)
                    .Cast<SearchResult>())
                {
                    _renderer.CurrentResults.Add(result);
                }

                if (changeSelection)
                {
                    var result = _renderer.CurrentResults
                        .FindFirstSegmentWithStartAfter(offset)
                        ?? _renderer.CurrentResults.FirstSegment;

                    if (result != null)
                        SelectResult(result);

                    _currentSearchResultIndex = _renderer.CurrentResults.Count > 0
                        ? GetSearchResultIndex(_renderer.CurrentResults, result)
                        : -1;
                }
            }

            UpdateSearchLabel();
            _textArea.TextView.InvalidateLayer(KnownLayer.Selection);
        }

        void CleanSearchResults()
        {
            _renderer.CurrentResults.Clear();
            _currentSearchResultIndex = -1;
        }

        void SetCurrentSearchResult(SearchResult result)
        {
            _currentSearchResultIndex = GetSearchResultIndex(_renderer.CurrentResults, result);
            SelectResult(result);
            UpdateSearchLabel();
        }

        void SelectResult(TextSegment result)
        {
            _textArea.Caret.Offset = result.EndOffset;
            _textArea.Selection = Selection.Create(
                _textArea, result.StartOffset, result.EndOffset);
            _textArea.Caret.BringCaretToView();
            _textArea.Caret.Show();
        }

        void UpdateSearchLabel()
        {
            if (_messageRow == null || _searchCounterLabel == null)
                return;

            if (string.IsNullOrEmpty(SearchPattern))
            {
                _messageRow.style.display = DisplayStyle.None;
                return;
            }

            _messageRow.style.display = DisplayStyle.Flex;

            int count = _renderer.CurrentResults.Count;
            if (count == 0)
            {
                _searchCounterLabel.text = SearchPanelLocalization.NoMatchesFound;
            }
            else if (_currentSearchResultIndex == -1)
            {
                _searchCounterLabel.text = count == 1
                    ? SearchPanelLocalization.OneMatch
                    : string.Format(SearchPanelLocalization.MultipleMatchesFormat, count);
            }
            else
            {
                _searchCounterLabel.text = string.Format(
                    SearchPanelLocalization.MatchIndexFormat,
                    _currentSearchResultIndex + 1, count);
            }
        }

        static int GetSearchResultIndex(
            TextSegmentCollection<SearchResult> searchResults, SearchResult match)
        {
            if (match == null)
                return -1;

            int index = 0;
            foreach (var searchResult in searchResults)
            {
                if (searchResult.Equals(match))
                    return index;
                index++;
            }
            return -1;
        }

        #endregion

        #region Keyboard Handling

        void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                e.StopPropagation();
                if (e.shiftKey)
                    FindPrevious();
                else
                    FindNext();
            }
            else if (e.keyCode == KeyCode.Escape)
            {
                e.StopPropagation();
                Close();
            }
        }

        void OnExpandReplaceClicked()
        {
            IsReplaceMode = !IsReplaceMode;
        }

        #endregion
    }

    internal class SearchOptionsChangedEventArgs : EventArgs
    {
        internal string SearchPattern { get; }
        internal bool MatchCase { get; }
        internal bool UseRegex { get; }
        internal bool WholeWords { get; }

        internal SearchOptionsChangedEventArgs(
            string searchPattern, bool matchCase, bool useRegex, bool wholeWords)
        {
            SearchPattern = searchPattern;
            MatchCase = matchCase;
            UseRegex = useRegex;
            WholeWords = wholeWords;
        }
    }
}
