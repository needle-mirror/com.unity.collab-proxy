using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using PlasticGui;

namespace Unity.PlasticSCM.Editor.UI.UIElements
{
    internal class MultiSelectDropdownItem
    {
        internal readonly string Name;
        internal readonly Texture Icon;

        internal MultiSelectDropdownItem(string name, Texture icon)
        {
            Name = name;
            Icon = icon;
        }
    }

    internal class MultiSelectDropdown : VisualElement
    {
        internal event Action SelectionChanged;

        internal MultiSelectDropdown(string labelPrefix)
        {
            mLabelPrefix = labelPrefix;

            mLabelElement = new TextElement();
            mLabelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            mLabelElement.style.flexGrow = 1;

            VisualElement chevron = new VisualElement();
            chevron.AddToClassList("unity-base-popup-field__arrow");
            chevron.style.marginLeft = 4;

            mButton = new ToolbarButton(OnButtonClick);
            mButton.style.flexDirection = FlexDirection.Row;
            mButton.style.unityTextAlign = TextAnchor.MiddleLeft;
            mButton.style.height = 20;
            mButton.style.paddingBottom = 1;
            mButton.focusable = false;
            mButton.Add(mLabelElement);
            mButton.Add(chevron);

            Add(mButton);

            UpdateLabel();
            UpdateEnabledState();
        }

        internal void SetItems(IList<MultiSelectDropdownItem> items)
        {
            mItems = items ?? new List<MultiSelectDropdownItem>();
            UpdateLabel();
            UpdateEnabledState();
        }

        internal HashSet<string> Selected
        {
            get { return mSelected; }
            set
            {
                mSelected = value;
                UpdateLabel();
            }
        }

        void OnButtonClick()
        {
            if (mItems.Count == 0)
                return;

            MultiSelectDropdownPopupContent content = new MultiSelectDropdownPopupContent(
                mItems, mSelected, OnPopupSelectionChanged);

            UnityEditor.PopupWindow.Show(mButton.worldBound, content);
        }

        void OnPopupSelectionChanged(HashSet<string> newSelected)
        {
            mSelected = newSelected;
            UpdateLabel();

            if (SelectionChanged != null)
                SelectionChanged();
        }

        void UpdateLabel()
        {
            mLabelElement.text = BuildLabel();
        }

        string BuildLabel()
        {
            if (mSelected == null)
                return mLabelPrefix;

            string firstSelected;
            int totalSelected;
            CountVisibleSelected(out firstSelected, out totalSelected);

            if (totalSelected == 0)
                return string.Format("{0}: {1}",
                    mLabelPrefix,
                    PlasticLocalization.Name.MultiSelectNoneSelected.GetString());

            if (totalSelected == 1)
                return string.Format("{0}: {1}", mLabelPrefix, firstSelected);

            return string.Format("{0}: {1}",
                mLabelPrefix,
                PlasticLocalization.Name.MultiSelectOverflowFormat.GetString(
                    firstSelected, totalSelected - 1));
        }

        void CountVisibleSelected(out string firstSelected, out int totalSelected)
        {
            firstSelected = null;
            totalSelected = 0;

            foreach (MultiSelectDropdownItem item in mItems)
            {
                if (!mSelected.Contains(item.Name))
                    continue;

                if (firstSelected == null)
                    firstSelected = item.Name;

                totalSelected++;
            }
        }

        void UpdateEnabledState()
        {
            mButton.SetEnabled(mItems.Count > 0);
        }

        readonly string mLabelPrefix;
        readonly TextElement mLabelElement;
        readonly ToolbarButton mButton;

        IList<MultiSelectDropdownItem> mItems = new List<MultiSelectDropdownItem>();
        HashSet<string> mSelected;
    }

    internal class MultiSelectDropdownPopupContent : PopupWindowContent
    {
        internal MultiSelectDropdownPopupContent(
            IList<MultiSelectDropdownItem> items,
            HashSet<string> currentSelection,
            Action<HashSet<string>> onSelectionChanged)
        {
            mAllItems = items;
            mFilteredItems = new List<MultiSelectDropdownItem>(items);
            mSelected = currentSelection == null
                ? null
                : new HashSet<string>(currentSelection);
            mOnSelectionChanged = onSelectionChanged;
        }

        public override Vector2 GetWindowSize()
        {
            int itemCount = mAllItems == null ? 0 : mAllItems.Count;
            float contentHeight = SEARCH_AREA_HEIGHT
                + (itemCount * ROW_HEIGHT)
                + FOOTER_HEIGHT;
            float height = Mathf.Clamp(contentHeight, POPUP_HEIGHT_MIN, POPUP_HEIGHT_MAX);
            return new Vector2(POPUP_WIDTH, height);
        }

        public override void OnOpen()
        {
            BuildGUI(editorWindow.rootVisualElement);
        }

        public override void OnGUI(Rect rect)
        {
            if (Event.current.type != EventType.KeyDown)
                return;

            if (Event.current.keyCode != KeyCode.Escape)
                return;

            editorWindow.Close();
            Event.current.Use();
        }

        void BuildGUI(VisualElement root)
        {
            root.style.flexDirection = FlexDirection.Column;

            ToolbarSearchField searchField = new ToolbarSearchField();
            searchField.style.width = POPUP_WIDTH - 2 * SEARCH_HORIZONTAL_MARGIN;
            searchField.style.marginTop = SEARCH_HORIZONTAL_MARGIN;
            searchField.style.marginLeft = SEARCH_HORIZONTAL_MARGIN;
            searchField.style.marginRight = SEARCH_HORIZONTAL_MARGIN;
            searchField.style.marginBottom = SEARCH_HORIZONTAL_MARGIN;
            searchField.RegisterValueChangedCallback(OnSearchChanged);
            root.Add(searchField);

            mListView = new ListView();
            mListView.itemsSource = mFilteredItems;
            mListView.fixedItemHeight = ROW_HEIGHT;
            mListView.makeItem = MakeRow;
            mListView.bindItem = BindRow;
            mListView.selectionType = SelectionType.None;
            mListView.style.flexGrow = 1;
            mListView.style.minHeight = 0;
            root.Add(mListView);

            VisualElement footer = new VisualElement();
            footer.style.flexDirection = FlexDirection.Row;
            footer.style.justifyContent = Justify.FlexEnd;
            footer.style.flexShrink = 0;
            footer.style.paddingLeft = 4;
            footer.style.paddingRight = 4;
            footer.style.paddingBottom = 4;
            footer.style.paddingTop = 4;
            footer.style.borderTopWidth = 1;
            footer.style.borderTopColor = UnityStyles.Colors.BarBorder;

            Button resetButton = new Button(OnResetClick);
            resetButton.text = PlasticLocalization.Name.MultiSelectResetButton.GetString();
            resetButton.style.fontSize = ROW_FONT_SIZE;
            resetButton.style.minWidth = 60;
            footer.Add(resetButton);
            root.Add(footer);
        }

        VisualElement MakeRow()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingLeft = ROW_HORIZONTAL_PADDING;
            row.style.paddingRight = ROW_HORIZONTAL_PADDING;
            row.style.height = ROW_HEIGHT;

            Image icon = new Image();
            icon.name = ROW_ICON_NAME;
            icon.style.width = ICON_SIZE;
            icon.style.height = ICON_SIZE;
            icon.style.marginRight = 4;
            icon.scaleMode = ScaleMode.ScaleToFit;
            row.Add(icon);

            Label label = new Label();
            label.name = ROW_LABEL_NAME;
            label.style.flexGrow = 1;
            label.style.fontSize = ROW_FONT_SIZE;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.overflow = Overflow.Hidden;
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.marginLeft = 0;
            label.style.marginRight = 0;
            label.style.paddingLeft = 0;
            label.style.paddingRight = 0;
            row.Add(label);

            Button hoverLink = new Button();
            hoverLink.name = ROW_HOVER_LINK_NAME;
            hoverLink.style.color = UnityStyles.Colors.Link;
            hoverLink.style.fontSize = ROW_FONT_SIZE;
            hoverLink.style.marginLeft = 6;
            hoverLink.style.marginRight = 6;
            hoverLink.style.paddingLeft = 0;
            hoverLink.style.paddingRight = 0;
            hoverLink.style.borderTopWidth = 0;
            hoverLink.style.borderBottomWidth = 0;
            hoverLink.style.borderLeftWidth = 0;
            hoverLink.style.borderRightWidth = 0;
            hoverLink.style.backgroundColor = Color.clear;
            hoverLink.style.display = DisplayStyle.None;
            hoverLink.SetMouseCursor(MouseCursor.Link);
            hoverLink.clicked += () => OnHoverLinkClick(hoverLink);
            row.Add(hoverLink);

            Toggle checkbox = new Toggle();
            checkbox.name = ROW_CHECKBOX_NAME;
            checkbox.style.marginLeft = 0;
            checkbox.style.marginRight = 0;
            checkbox.RegisterValueChangedCallback(OnCheckboxChanged);
            row.Add(checkbox);

            row.RegisterCallback<MouseEnterEvent>(OnRowMouseEnter);
            row.RegisterCallback<MouseLeaveEvent>(OnRowMouseLeave);

            return row;
        }

        void BindRow(VisualElement element, int index)
        {
            if (index < 0 || index >= mFilteredItems.Count)
                return;

            MultiSelectDropdownItem item = mFilteredItems[index];

            Image icon = element.Q<Image>(ROW_ICON_NAME);
            Label label = element.Q<Label>(ROW_LABEL_NAME);
            Toggle checkbox = element.Q<Toggle>(ROW_CHECKBOX_NAME);
            Button hoverLink = element.Q<Button>(ROW_HOVER_LINK_NAME);

            icon.image = item.Icon;
            icon.style.display = item.Icon != null ? DisplayStyle.Flex : DisplayStyle.None;
            label.text = item.Name;

            bool isChecked = IsItemSelected(item.Name);
            checkbox.SetValueWithoutNotify(isChecked);
            checkbox.userData = item.Name;

            hoverLink.userData = item.Name;
            UpdateHoverLink(hoverLink, item.Name);
        }

        void UpdateHoverLink(Button hoverLink, string itemName)
        {
            if (mHoveredItem != itemName)
            {
                hoverLink.style.display = DisplayStyle.None;
                return;
            }

            hoverLink.text = IsOnlySelected(itemName)
                ? PlasticLocalization.Name.MultiSelectSelectAllAction.GetString()
                : PlasticLocalization.Name.MultiSelectOnlyAction.GetString();
            hoverLink.style.display = DisplayStyle.Flex;
        }

        bool IsItemSelected(string itemName)
        {
            return mSelected == null || mSelected.Contains(itemName);
        }

        void OnRowMouseEnter(MouseEnterEvent evt)
        {
            VisualElement row = (VisualElement)evt.currentTarget;
            Button hoverLink = row.Q<Button>(ROW_HOVER_LINK_NAME);

            mHoveredItem = (string)hoverLink.userData;
            row.style.backgroundColor = UnityStyles.Colors.HoverBackgroundColor;
            UpdateHoverLink(hoverLink, mHoveredItem);
        }

        void OnRowMouseLeave(MouseLeaveEvent evt)
        {
            VisualElement row = (VisualElement)evt.currentTarget;
            Button hoverLink = row.Q<Button>(ROW_HOVER_LINK_NAME);

            string itemName = (string)hoverLink.userData;
            if (mHoveredItem == itemName)
                mHoveredItem = null;

            row.style.backgroundColor = StyleKeyword.Null;
            hoverLink.style.display = DisplayStyle.None;
        }

        void OnHoverLinkClick(Button hoverLink)
        {
            string itemName = (string)hoverLink.userData;

            if (IsOnlySelected(itemName))
            {
                mSelected = null;
            }
            else
            {
                mSelected = new HashSet<string>();
                mSelected.Add(itemName);
            }

            FireSelectionChanged();
            mListView.RefreshItems();
        }

        void OnCheckboxChanged(ChangeEvent<bool> evt)
        {
            Toggle checkbox = (Toggle)evt.currentTarget;
            string itemName = (string)checkbox.userData;

            if (mSelected == null)
            {
                mSelected = new HashSet<string>();
                foreach (MultiSelectDropdownItem item in mAllItems)
                    mSelected.Add(item.Name);
            }

            if (evt.newValue)
                mSelected.Add(itemName);
            else
                mSelected.Remove(itemName);

            if (IsAllSelected())
                mSelected = null;

            FireSelectionChanged();
        }

        bool IsAllSelected()
        {
            if (mSelected == null)
                return true;

            if (mSelected.Count != mAllItems.Count)
                return false;

            foreach (MultiSelectDropdownItem item in mAllItems)
            {
                if (!mSelected.Contains(item.Name))
                    return false;
            }

            return true;
        }

        void OnSearchChanged(ChangeEvent<string> evt)
        {
            ApplySearchFilter(evt.newValue);
        }

        void ApplySearchFilter(string query)
        {
            mFilteredItems.Clear();

            if (string.IsNullOrEmpty(query))
            {
                foreach (MultiSelectDropdownItem item in mAllItems)
                    mFilteredItems.Add(item);
            }
            else
            {
                string lower = query.ToLowerInvariant();
                foreach (MultiSelectDropdownItem item in mAllItems)
                {
                    if (item.Name.ToLowerInvariant().Contains(lower))
                        mFilteredItems.Add(item);
                }
            }

            mListView.itemsSource = mFilteredItems;
            mListView.Rebuild();
        }

        void OnResetClick()
        {
            mSelected = null;
            FireSelectionChanged();
            mListView.RefreshItems();
        }

        bool IsOnlySelected(string itemName)
        {
            return mSelected != null
                && mSelected.Count == 1
                && mSelected.Contains(itemName);
        }

        void FireSelectionChanged()
        {
            if (mOnSelectionChanged == null)
                return;

            HashSet<string> snapshot = mSelected == null
                ? null
                : new HashSet<string>(mSelected);

            mOnSelectionChanged(snapshot);
        }

        readonly IList<MultiSelectDropdownItem> mAllItems;
        readonly List<MultiSelectDropdownItem> mFilteredItems;
        readonly Action<HashSet<string>> mOnSelectionChanged;

        HashSet<string> mSelected;
        string mHoveredItem;
        ListView mListView;

        const float POPUP_WIDTH = 280f;
        const float POPUP_HEIGHT_MIN = 80f;
        const float POPUP_HEIGHT_MAX = 360f;
        const float SEARCH_AREA_HEIGHT = 28f;
        const float FOOTER_HEIGHT = 32f;
        const int ROW_HEIGHT = 18;
        const int ROW_HORIZONTAL_PADDING = 4;
        const int ROW_FONT_SIZE = 12;
        const int ICON_SIZE = 14;
        const int SEARCH_HORIZONTAL_MARGIN = 4;
        const string ROW_ICON_NAME = "icon";
        const string ROW_LABEL_NAME = "label";
        const string ROW_HOVER_LINK_NAME = "hoverLink";
        const string ROW_CHECKBOX_NAME = "checkbox";
    }
}
