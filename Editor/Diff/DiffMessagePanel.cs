using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff
{
    internal class DiffMessagePanel : VisualElement
    {
        internal DiffMessagePanel()
        {
            BuildComponents();
        }

        internal void ShowMessage(string message)
        {
            mMessageLabel.text = message ?? string.Empty;
        }

        void BuildComponents()
        {
            style.flexGrow = 1;
            style.minWidth = 150;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            style.paddingLeft = 16;
            style.paddingRight = 16;

            mMessageLabel = new Label();
            mMessageLabel.style.maxWidth = CONTENT_WIDTH;
            mMessageLabel.style.whiteSpace = WhiteSpace.Normal;
            mMessageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

            Add(mMessageLabel);
        }

        Label mMessageLabel;

        const int CONTENT_WIDTH = 420;
    }
}
