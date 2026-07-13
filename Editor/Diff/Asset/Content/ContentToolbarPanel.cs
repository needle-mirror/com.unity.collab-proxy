using System;
using System.Collections.Generic;

using PlasticGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content
{
    internal class ContentToolbarPanel : VisualElement
    {
        internal ContentToolbarPanel(
            Action<string> filterChanged,
            Action expandAll,
            Action collapseAll,
            Action<HashSet<string>> setGameObjectFilter,
            Action<HashSet<string>> setTypeFilter)
        {
            mFilterChanged = filterChanged;
            mExpandAll = expandAll;
            mCollapseAll = collapseAll;
            mSetGameObjectFilter = setGameObjectFilter;
            mSetTypeFilter = setTypeFilter;

            CreateGUI();

            mDelayedFilterAction = new DelayedActionBySecondsRunner(
                DelayedSearchChanged, UnityConstants.SEARCH_DELAYED_INPUT_ACTION_INTERVAL);
        }

        internal void Dispose()
        {
            mSearchField.UnregisterValueChangedCallback(OnSearchFieldValueChanged);

            mGameObjectDropdown.SelectionChanged -= OnGameObjectSelectionChanged;
            mTypeDropdown.SelectionChanged -= OnTypeSelectionChanged;
        }

        internal void UpdateAvailableFilters(
            IList<MultiSelectDropdownItem> gameObjects,
            IList<MultiSelectDropdownItem> types,
            bool show)
        {
            DisplayStyle display = show ? DisplayStyle.Flex : DisplayStyle.None;
            mGameObjectDropdown.style.display = display;
            mTypeDropdown.style.display = display;

            if (!show)
                return;

            mGameObjectDropdown.SetItems(gameObjects);
            mTypeDropdown.SetItems(types);
        }

        void DelayedSearchChanged()
        {
            mFilterChanged(mSearchField.value);
        }

        void CreateGUI()
        {
            UnityEditor.UIElements.Toolbar toolbar = ControlBuilder.Toolbar.Create();

            mGameObjectDropdown = ControlBuilder.Toolbar.CreateMultiSelectDropdownLeft(
                PlasticLocalization.Name.DiffGameObjectFilterLabel.GetString());
            mGameObjectDropdown.SelectionChanged += OnGameObjectSelectionChanged;
            mGameObjectDropdown.style.display = DisplayStyle.None;

            mTypeDropdown = ControlBuilder.Toolbar.CreateMultiSelectDropdown(
                PlasticLocalization.Name.DiffTypeFilterLabel.GetString());
            mTypeDropdown.SelectionChanged += OnTypeSelectionChanged;
            mTypeDropdown.style.display = DisplayStyle.None;

            mSearchField = ControlBuilder.Toolbar.CreateSearchField();
            mSearchField.RegisterValueChangedCallback(OnSearchFieldValueChanged);

            toolbar.Add(mGameObjectDropdown);
            toolbar.Add(mTypeDropdown);

            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            mExpandAllButton = ControlBuilder.Toolbar.CreateButtonLeft(
                PlasticLocalization.Name.DiffExpandAllButton.GetString(), mExpandAll);
            toolbar.Add(mExpandAllButton);

            mCollapseAllButton = ControlBuilder.Toolbar.CreateButton(
                PlasticLocalization.Name.DiffCollapseAllButton.GetString(), mCollapseAll);
            toolbar.Add(mCollapseAllButton);

            toolbar.Add(mSearchField);

            Add(toolbar);
        }

        void OnSearchFieldValueChanged(ChangeEvent<string> evt)
        {
            mDelayedFilterAction.Run();
        }

        void OnGameObjectSelectionChanged()
        {
            mSetGameObjectFilter(mGameObjectDropdown.Selected);
        }

        void OnTypeSelectionChanged()
        {
            mSetTypeFilter(mTypeDropdown.Selected);
        }

        readonly Action<string> mFilterChanged;
        readonly Action mExpandAll;
        readonly Action mCollapseAll;
        readonly Action<HashSet<string>> mSetGameObjectFilter;
        readonly Action<HashSet<string>> mSetTypeFilter;
        readonly DelayedActionBySecondsRunner mDelayedFilterAction;

        ToolbarButton mExpandAllButton;
        ToolbarButton mCollapseAllButton;

        MultiSelectDropdown mGameObjectDropdown;
        MultiSelectDropdown mTypeDropdown;

        ToolbarSearchField mSearchField;
    }
}
