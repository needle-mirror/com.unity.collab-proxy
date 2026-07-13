using System;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class MessagePanel : VisualElement
    {
        internal Label MessageLabel { get { return mText; } }

        internal MessagePanel()
        {
            BuildComponents();
            HideMessage();
        }

        internal void HandleMessage(string message)
        {
            bool bShouldHideMessage = !mbIsDisplayingException;
            mbIsDisplayingException = false;

            HideAction();

            if (string.IsNullOrEmpty(message))
            {
                if (bShouldHideMessage)
                    HideMessage();
                return;
            }

            ShowMessage(message);
        }

        internal void HandleMessage(
            string message,
            string actionText,
            Action onAction)
        {
            HandleMessage(message);

            if (string.IsNullOrEmpty(message) ||
                string.IsNullOrEmpty(actionText) ||
                onAction == null)
            {
                return;
            }

            ShowAction(actionText, onAction);
        }

        internal void HandleException(Exception e)
        {
            mbIsDisplayingException = true;
            HideAction();
            ShowMessage(e.Message);
        }

        internal void Hide()
        {
            mbIsDisplayingException = false;
            HideAction();
            HideMessage();
        }

        void ShowMessage(string message)
        {
            mText.text = message;
            style.display = DisplayStyle.Flex;
        }

        void HideMessage()
        {
            style.display = DisplayStyle.None;
        }

        void ShowAction(string actionText, Action onAction)
        {
            mActionButton.text = actionText;

            if (mActionHandler != null)
                mActionButton.clicked -= mActionHandler;

            mActionHandler = onAction;
            mActionButton.clicked += mActionHandler;
            mActionButton.style.display = DisplayStyle.Flex;
        }

        void HideAction()
        {
            if (mActionHandler != null)
            {
                mActionButton.clicked -= mActionHandler;
                mActionHandler = null;
            }
            mActionButton.style.display = DisplayStyle.None;
        }

        void BuildComponents()
        {
            style.backgroundColor = UnityStyles.Colors.Diff.InfoColor;
            style.height = NOTIFICATION_PANEL_HEIGHT;
            style.flexShrink = 0;
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            mText = ControlBuilder.Label.CreateSelectableLabel();
            mText.style.flexGrow = 1;
            mText.style.marginLeft = DEFAULT_MARGIN;
            mText.style.marginRight = DEFAULT_MARGIN;
            mText.style.whiteSpace = WhiteSpace.Normal;
            mText.style.overflow = Overflow.Hidden;
            mText.style.unityTextAlign = TextAnchor.MiddleLeft;

            Add(mText);

            mActionButton = new Button();
            mActionButton.style.flexShrink = 0;
            mActionButton.style.marginLeft = DEFAULT_MARGIN;
            mActionButton.style.marginRight = DEFAULT_MARGIN;
            mActionButton.style.display = DisplayStyle.None;

            Add(mActionButton);
        }

        Label mText;
        Button mActionButton;
        Action mActionHandler;
        bool mbIsDisplayingException;

        const int NOTIFICATION_PANEL_HEIGHT = 34;
        const int DEFAULT_MARGIN = 6;
    }
}
