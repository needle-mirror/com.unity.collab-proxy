using System;
using JetBrains.Annotations;
using Unity.Cloud.Collaborate.Assets;
using Unity.Cloud.Collaborate.UserInterface;
using Unity.Cloud.Collaborate.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Cloud.Collaborate.Components
{
    internal class ChangesGroupHeader : VisualElement
    {
        public const string UssClassName = "changes-group-header";
        public const string NameUssClassName = UssClassName + "__name";
        public const string OverflowButtonUssClassName = UssClassName + "__overflow-button";
        public const string RefreshButtonUssClassName = UssClassName + "__refresh-button";

        static readonly string k_LayoutPath = $"{CollaborateWindow.LayoutPath}/{nameof(ChangesGroupHeader)}.uxml";
        static readonly string k_StylePath = $"{CollaborateWindow.StylePath}/{nameof(ChangesGroupHeader)}.uss";

        readonly Label m_GroupName;
        readonly IconButton m_OverflowButton;
        readonly IconButton m_RefreshButton;

        public event Action<float, float> OnOverflowButtonClicked;
        public event Action OnRefreshButtonClicked;

        public ChangesGroupHeader()
        {
            // Get the layout and style sheet
            AddToClassList(UssClassName);
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_LayoutPath).CloneTree(this);
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StylePath));

            // Initialise fields
            m_GroupName = this.Q<Label>(className: NameUssClassName);
            m_OverflowButton = this.Q<IconButton>(className: OverflowButtonUssClassName);
            m_RefreshButton = this.Q<IconButton>(className: RefreshButtonUssClassName);

            // Wire up overflow button
            m_OverflowButton.Clicked += TriggerOverflowMenu;

            void TriggerRefreshButton() {
                OnRefreshButtonClicked?.Invoke();
            }
            m_RefreshButton.Clicked += TriggerRefreshButton;
        }

        public void SetEnableOverflowMenu(bool enable)
        {
            if (enable)
            {
                m_OverflowButton.RemoveFromClassList(UiConstants.ussHidden);
            }
            else
            {
                m_OverflowButton.AddToClassList(UiConstants.ussHidden);
            }
        }

        void TriggerOverflowMenu()
        {
            var (x, y) = MenuUtilities.GetMenuPosition(m_OverflowButton, MenuUtilities.AnchorPoint.BottomRight);
            OnOverflowButtonClicked?.Invoke(x, y);
        }

        public void UpdateGroupName(string text)
        {
            m_GroupName.text = text;
        }

        [UsedImplicitly]
        public new class UxmlFactory : UxmlFactory<ChangesGroupHeader> { }
    }
}
