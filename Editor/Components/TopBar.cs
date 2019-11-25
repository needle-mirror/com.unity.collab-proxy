using System;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.Cloud.Collaborate.Assets;
using Unity.Cloud.Collaborate.Components.Menus;
using Unity.Cloud.Collaborate.UserInterface;
using Unity.Cloud.Collaborate.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Cloud.Collaborate.Components
{
    [UsedImplicitly]
    internal class TopBar : VisualElement
    {
        public const string UssClassName = "top-bar";
        public const string IconUssClassName = UssClassName + "__icon";
        public const string BranchInfoUssClassName = UssClassName + "__branch-info";
        public const string OverflowMenuUssClassName = UssClassName + "__overflow-button";
        public const string BackUssClassName = UssClassName + "__back";
        public const string BackButtonUssClassName = UssClassName + "__back-button";
        public const string BackTextUssClassName = UssClassName + "__back-text";

        static readonly string k_LayoutPath = $"{CollaborateWindow.LayoutPath}/{nameof(TopBar)}.uxml";
        static readonly string k_StylePath = $"{CollaborateWindow.StylePath}/{nameof(TopBar)}.uss";

        readonly VisualElement m_Icon;
        readonly TextElement m_BranchInfo;
        readonly IconButton m_OverflowMenu;
        readonly VisualElement m_BackContainer;
        readonly IconButton m_BackButton;
        readonly TextElement m_BackText;

        (string id, string text, Action backEvent)? m_BackNavigation;

        [CanBeNull]
        string m_BranchName;

        public TopBar()
        {
            // Get the layout
            AddToClassList(UssClassName);
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_LayoutPath).CloneTree(this);
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StylePath));

            // Initialise fields
            m_Icon = this.Q<VisualElement>(className: IconUssClassName);
            m_BranchInfo = this.Q<TextElement>(className: BranchInfoUssClassName);
            m_OverflowMenu = this.Q<IconButton>(className: OverflowMenuUssClassName);
            m_BackContainer = this.Q<VisualElement>(className: BackUssClassName);
            m_BackButton = this.Q<IconButton>(className: BackButtonUssClassName);
            m_BackText = this.Q<TextElement>(className: BackTextUssClassName);

            m_OverflowMenu.Clicked += ClickableOnClicked;
            m_BackButton.Clicked += BackButtonOnClicked;

            UpdateBackVisibility();

            GlobalEvents.RegisteredBackNavigation += RegisteredBackNavigation;
            GlobalEvents.UnregisteredBackNavigation += UnregisteredBackNavigation;
            GlobalEvents.WindowClosed += OnWindowClosed;
        }

        /// <summary>
        /// Register back navigation to be made available to the user to navigate backwards in the UI.
        /// </summary>
        /// <param name="id">Id for the back event.</param>
        /// <param name="text">Destination of the back navigation</param>
        /// <param name="backEvent">Action required to navigate backwards.</param>
        void RegisteredBackNavigation([NotNull] string id, [NotNull] string text, [NotNull] Action backEvent)
        {
            Assert.Null(m_BackNavigation, "There should only be one back navigation registered at a time.");
            m_BackNavigation = (id, text, backEvent);
            UpdateBackVisibility();
        }

        /// <summary>
        /// Unregister back navigation if the given id matches the currently displayed back navigation.
        /// </summary>
        /// <param name="id">Id for the back event.</param>
        /// <returns>True if id matched.</returns>
        bool UnregisteredBackNavigation(string id)
        {
            if (m_BackNavigation?.id != id) return false;

            m_BackNavigation = null;
            UpdateBackVisibility();
            return true;
        }

        /// <summary>
        /// Clear the back button on window close. Fixes issue with double OnEnable from window being closed then
        /// reopened.
        /// </summary>
        void OnWindowClosed()
        {
            m_BackNavigation = null;
        }

        void ClickableOnClicked()
        {
            var (x, y) = MenuUtilities.GetMenuPosition(m_OverflowMenu, MenuUtilities.AnchorPoint.BottomRight);
            new FloatingMenu()
                .AddEntry("Invite Teammate", OpenLinksUtility.OpenMembersLink, true)
                .SetOpenDirection(MenuUtilities.OpenDirection.DownLeft)
                .Open(x, y);
        }

        void BackButtonOnClicked()
        {
            Assert.NotNull(m_BackNavigation, "There is no available back navigation.");
            m_BackNavigation?.backEvent();
            m_BackNavigation = null;
            UpdateBackVisibility();
        }

        void UpdateBackVisibility()
        {
            // Hide back controls if no back event registered. Otherwise, show back controls.
            if (m_BackNavigation == null)
            {
                m_BackContainer.AddToClassList(UiConstants.ussHidden);
                m_BackButton.SetEnabled(false);
                m_BackText.text = string.Empty;
            }
            else
            {
                m_BackText.text = m_BackNavigation.Value.text;
                m_BackButton.SetEnabled(true);
                m_BackContainer.RemoveFromClassList(UiConstants.ussHidden);
            }
        }

        [UsedImplicitly]
        public new class UxmlFactory : UxmlFactory<TopBar> { }
    }
}
