using System.Collections.Generic;

using MergetoolGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class NavigationPanel : VisualElement
    {
        internal int CurrentDifference { get { return mNavigation.CurrentDifference; } }
        internal DifferenceNavigation Navigation { get { return mNavigation; } }

        internal NavigationPanel(IDifferencesNavigator navigator)
        {
            mNavigator = navigator;
            mNavigation = new DifferenceNavigation(new List<int>());

            BuildComponents();
        }

        internal void UpdateDiffPositions(List<int> diffPositions)
        {
            mNavigation.UpdateDiffPositions(diffPositions);
        }

        internal int SetCurrentDifference(int difference)
        {
            int result = mNavigation.SetCurrentDifference(difference);
            SetNavigationInfo();
            return result;
        }

        internal void SetNavigationInfo()
        {
            int currentDifference = mNavigation.CurrentDifference + 1;
            int differencesCount = mNavigation.DifferencesCount;

            mNumDiffLabel.text = MergetoolLocalization.GetString(
                MergetoolLocalization.Name.CurrentDifference,
                currentDifference, differencesCount);
        }

        internal void ClearNavigation()
        {
            mNumDiffLabel.text = MergetoolLocalization.GetString(
                MergetoolLocalization.Name.CurrentDifference, 0, 0);
            mNavigation = new DifferenceNavigation(new List<int>());
        }

        internal void Enable()
        {
            SetNavigationButtonsEnablement(true);
        }

        internal void Disable()
        {
            SetNavigationButtonsEnablement(false);
        }

        internal void NavigateToFirstDifference()
        {
            if (mNavigation.DifferencesCount == 0)
                return;

            int line = mNavigation.FirstDifference();

            mNavigator.GoToLine(line);

            SetNavigationInfo();
        }

        internal void NavigateToPreviousDifference()
        {
            if (mNavigation.DifferencesCount == 0)
                return;

            int line = mNavigation.PreviousDifference();

            mNavigator.GoToLine(line);

            SetNavigationInfo();
        }

        internal void NavigateToNextDifference()
        {
            if (mNavigation.DifferencesCount == 0)
                return;

            int line = mNavigation.NextDifference();

            mNavigator.GoToLine(line);

            SetNavigationInfo();
        }

        internal void NavigateToLastDifference()
        {
            if (mNavigation.DifferencesCount == 0)
                return;

            int line = mNavigation.LastDifference();

            mNavigator.GoToLine(line);

            SetNavigationInfo();
        }

        internal void Dispose()
        {
            mFirstDiffButton.clicked -= OnFirstDiffButtonClicked;
            mPrevDiffButton.clicked -= OnPrevDiffButtonClicked;
            mNextDiffButton.clicked -= OnNextDiffButtonClicked;
            mLastDiffButton.clicked -= OnLastDiffButtonClicked;
        }

        void SetNavigationButtonsEnablement(bool enabled)
        {
            mFirstDiffButton.SetEnabled(enabled);
            mPrevDiffButton.SetEnabled(enabled);
            mNextDiffButton.SetEnabled(enabled);
            mLastDiffButton.SetEnabled(enabled);
        }

        void OnFirstDiffButtonClicked()
        {
            NavigateToFirstDifference();
        }

        void OnPrevDiffButtonClicked()
        {
            NavigateToPreviousDifference();
        }

        void OnNextDiffButtonClicked()
        {
            NavigateToNextDifference();
        }

        void OnLastDiffButtonClicked()
        {
            NavigateToLastDifference();
        }

        void BuildComponents()
        {
            style.flexGrow = 1;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;

            mFirstDiffButton = ControlBuilder.Toolbar.CreateImageButtonLeft(
                Images.GetFirstIcon(),
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.FirstDiffButtonTooltip),
                OnFirstDiffButtonClicked);

            mPrevDiffButton = ControlBuilder.Toolbar.CreateImageButtonLeft(
                Images.GetPrevIcon(),
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.PrevDiffButtonTooltip),
                OnPrevDiffButtonClicked);

            mNumDiffLabel = new Label();
            mNumDiffLabel.text = MergetoolLocalization.GetString(
                MergetoolLocalization.Name.CurrentDifference, 0, 0);
            mNumDiffLabel.style.marginLeft = DIFF_LABEL_MARGIN;
            mNumDiffLabel.style.marginRight = DIFF_LABEL_MARGIN;
            mNumDiffLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;

            mNextDiffButton = ControlBuilder.Toolbar.CreateImageButtonLeft(
                Images.GetNextIcon(),
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.NextDiffButtonTooltip),
                OnNextDiffButtonClicked);

            mLastDiffButton = ControlBuilder.Toolbar.CreateImageButtonLeft(
                Images.GetLastIcon(),
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.LastDiffButtonTooltip),
                OnLastDiffButtonClicked);

            Add(mFirstDiffButton);
            Add(mPrevDiffButton);
            Add(mNumDiffLabel);
            Add(mNextDiffButton);
            Add(mLastDiffButton);
        }

        DifferenceNavigation mNavigation;

        ToolbarButton mFirstDiffButton;
        ToolbarButton mPrevDiffButton;
        ToolbarButton mNextDiffButton;
        ToolbarButton mLastDiffButton;
        Label mNumDiffLabel;

        readonly IDifferencesNavigator mNavigator;

        const int DIFF_LABEL_MARGIN = 8;
    }
}
