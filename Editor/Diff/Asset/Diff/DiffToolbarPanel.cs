using System;
using System.Collections.Generic;

using PlasticGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff
{
    internal class DiffToolbarPanel : VisualElement
    {
        internal DiffToolbarPanel(
            Action<DiffFilter> setDiffFilter,
            Action<string> filterChanged,
            Action expandAll,
            Action collapseAll,
            Action<HashSet<string>> setGameObjectFilter,
            Action<HashSet<string>> setTypeFilter)
        {
            mSetDiffFilter = setDiffFilter;
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
            mFilterAllButton.UnregisterValueChangedCallback(
                OnFilterAllToggleChanged);
            mFilterModifiedButton.UnregisterValueChangedCallback(
                OnFilterModifiedToggleChanged);
            mFilterAddedButton.UnregisterValueChangedCallback(
                OnFilterAddedToggleChanged);
            mFilterRemovedButton.UnregisterValueChangedCallback(
                OnFilterRemovedToggleChanged);

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

        internal void UpdateFilterButtons(
            int modified,
            int added,
            int removed,
            DiffFilter diffFilter)
        {
            int total = modified + added + removed;

            mFilterAllButton.text =
                $"{PlasticLocalization.Name.DiffFilterAll.GetString()} ({total})";
            mFilterModifiedButton.text =
                $"{PlasticLocalization.Name.DiffFilterModified.GetString()} ({modified})";
            mFilterAddedButton.text =
                $"{PlasticLocalization.Name.DiffFilterAdded.GetString()} ({added})";
            mFilterRemovedButton.text =
                $"{PlasticLocalization.Name.DiffFilterRemoved.GetString()} ({removed})";

            mFilterAllButton.SetEnabled(total > 0);
            mFilterModifiedButton.SetEnabled(modified > 0);
            mFilterAddedButton.SetEnabled(added > 0);
            mFilterRemovedButton.SetEnabled(removed > 0);

            mFilterAllButton.SetValueWithoutNotify(diffFilter == DiffFilter.All);
            mFilterModifiedButton.SetValueWithoutNotify(diffFilter == DiffFilter.Modified);
            mFilterAddedButton.SetValueWithoutNotify(diffFilter == DiffFilter.Added);
            mFilterRemovedButton.SetValueWithoutNotify(diffFilter == DiffFilter.Removed);
        }

        void DelayedSearchChanged()
        {
            mFilterChanged(mSearchField.value);
        }

        void CreateGUI()
        {
            UnityEditor.UIElements.Toolbar toolbar = ControlBuilder.Toolbar.Create();

            mFilterAllButton = ControlBuilder.Toolbar.CreateToggle(
                PlasticLocalization.Name.DiffFilterAll.GetString(),
                PlasticLocalization.Name.DiffFilterAllTooltip.GetString());

            mFilterAllButton.RegisterValueChangedCallback(
                OnFilterAllToggleChanged);

            mFilterModifiedButton = ControlBuilder.Toolbar.CreateToggle(
                PlasticLocalization.Name.DiffFilterModified.GetString(),
                PlasticLocalization.Name.DiffFilterModifiedTooltip.GetString());

            mFilterModifiedButton.RegisterValueChangedCallback(
                OnFilterModifiedToggleChanged);

            mFilterAddedButton = ControlBuilder.Toolbar.CreateToggle(
                PlasticLocalization.Name.DiffFilterAdded.GetString(),
                PlasticLocalization.Name.DiffFilterAddedTooltip.GetString());

            mFilterAddedButton.RegisterValueChangedCallback(
                OnFilterAddedToggleChanged);

            mFilterRemovedButton = ControlBuilder.Toolbar.CreateToggleLeft(
                PlasticLocalization.Name.DiffFilterRemoved.GetString(),
                PlasticLocalization.Name.DiffFilterRemovedTooltip.GetString());

            mFilterRemovedButton.RegisterValueChangedCallback(
                OnFilterRemovedToggleChanged);

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

            toolbar.Add(mFilterAllButton);
            toolbar.Add(mFilterModifiedButton);
            toolbar.Add(mFilterAddedButton);
            toolbar.Add(mFilterRemovedButton);
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

        void OnFilterAllToggleChanged(ChangeEvent<bool> evt)
        {
            if (!evt.newValue)
                return;

            mSetDiffFilter(DiffFilter.All);
        }

        void OnFilterModifiedToggleChanged(ChangeEvent<bool> evt)
        {
            if (!evt.newValue)
                return;

            mSetDiffFilter(DiffFilter.Modified);
        }

        void OnFilterAddedToggleChanged(ChangeEvent<bool> evt)
        {
            if (!evt.newValue)
                return;

            mSetDiffFilter(DiffFilter.Added);
        }

        void OnFilterRemovedToggleChanged(ChangeEvent<bool> evt)
        {
            if (!evt.newValue)
                return;

            mSetDiffFilter(DiffFilter.Removed);
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

        readonly Action<DiffFilter> mSetDiffFilter;
        readonly Action<string> mFilterChanged;
        readonly Action mExpandAll;
        readonly Action mCollapseAll;
        readonly Action<HashSet<string>> mSetGameObjectFilter;
        readonly Action<HashSet<string>> mSetTypeFilter;
        readonly DelayedActionBySecondsRunner mDelayedFilterAction;

        ToolbarToggle mFilterAllButton;
        ToolbarToggle mFilterModifiedButton;
        ToolbarToggle mFilterAddedButton;
        ToolbarToggle mFilterRemovedButton;

        ToolbarButton mExpandAllButton;
        ToolbarButton mCollapseAllButton;

        MultiSelectDropdown mGameObjectDropdown;
        MultiSelectDropdown mTypeDropdown;

        ToolbarSearchField mSearchField;
    }
}
