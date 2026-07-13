using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using PlasticGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Purged
{
    internal class PurgedStatePanel : VisualElement
    {
        internal PurgedStatePanel()
        {
            BuildComponents();
        }

        internal void Dispose()
        {
            mLearnMoreLabel.UnregisterCallback<ClickEvent>(
                OnLearnMoreClicked);
        }

        void OnLearnMoreClicked(ClickEvent evt)
        {
            Application.OpenURL(PURGE_DOCUMENTATION_URL);
        }

        void BuildComponents()
        {
            style.flexGrow = 1;
            style.minWidth = 150;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            style.paddingLeft = 16;
            style.paddingRight = 16;

            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.FlexStart;
            container.style.flexWrap = Wrap.Wrap;
            container.style.flexShrink = 1;
            container.style.minWidth = 0;
            container.style.maxWidth = Length.Percent(100);

            Image icon = new Image();
            icon.image = EditorGUIUtility.IconContent(
                "console.infoicon").image;
            icon.style.width = ICON_SIZE;
            icon.style.height = ICON_SIZE;
            icon.style.flexShrink = 0;
            icon.style.marginRight = 16;
            icon.style.marginBottom = 8;

            Label title = new Label(
                PlasticLocalization.Name.PurgedRevision
                    .GetString());
            title.style.unityFontStyleAndWeight =
                FontStyle.Bold;
            title.style.fontSize = 14;
            title.style.whiteSpace = WhiteSpace.Normal;
            title.style.flexShrink = 1;
            title.style.marginBottom = 4;

            Label description = new Label(
                PlasticLocalization.Name
                    .PurgedRevisionMessage.GetString());
            description.style.whiteSpace =
                WhiteSpace.Normal;
            description.style.minWidth = 0;
            description.style.flexShrink = 1;
            description.style.marginBottom = 4;

            mLearnMoreLabel = new Label(
                PlasticLocalization.Name.LearnMore
                    .GetString());
            mLearnMoreLabel.style.color =
                UnityStyles.Colors.Link;
            mLearnMoreLabel.style.alignSelf =
                Align.FlexStart;
            mLearnMoreLabel.SetMouseCursor(
                MouseCursor.Link);
            mLearnMoreLabel.RegisterCallback<ClickEvent>(
                OnLearnMoreClicked);

            VisualElement messagePanel = new VisualElement();
            messagePanel.style.flexGrow = 1;
            messagePanel.style.flexShrink = 1;
            messagePanel.style.minWidth = 0;
            messagePanel.style.maxWidth = Length.Percent(100);
            messagePanel.Add(title);
            messagePanel.Add(description);
            messagePanel.Add(mLearnMoreLabel);

            container.Add(icon);
            container.Add(messagePanel);

            Add(container);
        }

        Label mLearnMoreLabel;

        const string PURGE_DOCUMENTATION_URL =
            "https://docs.unity.com/en-us/unity-version-control/uvcs-cli/purge";
        const int ICON_SIZE = 32;
    }
}

