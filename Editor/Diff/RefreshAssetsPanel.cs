using PlasticGui;
using UnityEditor;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff
{
    internal class RefreshAssetsPanel : VisualElement
    {
        internal RefreshAssetsPanel()
        {
            BuildComponents();
            Hide();
        }

        internal void ShowIfNeeded()
        {
            style.display = DisplayStyle.Flex;
        }

        void OnRefreshClicked()
        {
            Hide();
            AssetDatabase.Refresh();
        }

        void OnDismissClicked()
        {
            Hide();
        }

        void Hide()
        {
            style.display = DisplayStyle.None;
        }

        void BuildComponents()
        {
            style.backgroundColor = UnityStyles.Colors.Diff.InfoColor;
            style.height = PANEL_HEIGHT;
            style.flexShrink = 0;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            Label label = ControlBuilder.Label.CreateSelectableLabel();
            label.text = PlasticLocalization.GetString(
                PlasticLocalization.Name.AssetsModifiedRefreshMessage);
            label.style.marginLeft = DEFAULT_MARGIN;
            label.style.marginRight = DEFAULT_MARGIN;
            label.style.flexGrow = 1;
            label.style.overflow = Overflow.Hidden;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;

            VisualElement buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Row;
            buttonsContainer.style.alignItems = Align.Center;
            buttonsContainer.style.marginRight = DEFAULT_MARGIN;

            mRefreshButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.RefreshButton),
                OnRefreshClicked);

            mDismissButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.DismissButton),
                OnDismissClicked);

            buttonsContainer.Add(mRefreshButton);
            buttonsContainer.Add(mDismissButton);

            Add(label);
            Add(buttonsContainer);
        }

        Button mRefreshButton;
        Button mDismissButton;

        const int PANEL_HEIGHT = 34;
        const int DEFAULT_MARGIN = 6;
    }
}
