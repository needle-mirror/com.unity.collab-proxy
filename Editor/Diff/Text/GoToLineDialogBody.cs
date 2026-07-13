using System;

using UnityEngine;
using UnityEngine.UIElements;

using PlasticGui;

using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class GoToLineDialogBody : VisualElement
    {
        internal const string NAME_LINE_NUMBER_FIELD = "go-to-line-number-field";
        internal const string NAME_OK_BUTTON = "go-to-line-ok-button";
        internal const string NAME_CANCEL_BUTTON = "go-to-line-cancel-button";

        internal event Action<int> Confirmed;
        internal event Action Cancelled;

        internal GoToLineDialogBody(int currentLine, int totalLines)
        {
            mCurrentLine = currentLine;
            mTotalLines = totalLines;

            Add(BuildInputArea());
            Add(BuildButtonsArea());

            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            schedule.Execute(() =>
            {
                mLineNumberField.Focus();
                mLineNumberField.SelectAll();
            });
        }

        internal static bool IsValidInput(string text, int totalLines)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            int line;
            if (!int.TryParse(text, out line))
                return false;

            return line >= 1 && line <= totalLines;
        }

        VisualElement BuildInputArea()
        {
            VisualElement container = new VisualElement();
            container.style.flexGrow = 1;

            Label label = new Label(
                string.Format(
                    PlasticLocalization.GetString(PlasticLocalization.Name.LineNumberRange),
                    1,
                    mTotalLines));
            label.style.marginBottom = 4;

            mLineNumberField = new TextField();
            mLineNumberField.name = NAME_LINE_NUMBER_FIELD;
            mLineNumberField.value = mCurrentLine.ToString();
            mLineNumberField.RegisterValueChangedCallback(OnLineNumberChanged);

            container.Add(label);
            container.Add(mLineNumberField);

            return container;
        }

        VisualElement BuildButtonsArea()
        {
            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.FlexEnd;
            container.style.marginTop = 10;

            string okText = PlasticLocalization.GetString(
                PlasticLocalization.Name.OkButton);
            string cancelText = PlasticLocalization.GetString(
                PlasticLocalization.Name.CancelButton);

            mOkButton = ControlBuilder.Button.CreateButton(okText, Confirm);
            mOkButton.name = NAME_OK_BUTTON;
            Button cancelButton = ControlBuilder.Button.CreateButton(cancelText, Cancel);
            cancelButton.name = NAME_CANCEL_BUTTON;

            mOkButton.style.minWidth = BUTTON_WIDTH;
            cancelButton.style.minWidth = BUTTON_WIDTH;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                container.Add(mOkButton);
                container.Add(cancelButton);
            }
            else
            {
                container.Add(cancelButton);
                container.Add(mOkButton);
            }

            return container;
        }

        void OnDetachFromPanel(DetachFromPanelEvent e)
        {
            UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            if (mLineNumberField != null)
                mLineNumberField.UnregisterValueChangedCallback(OnLineNumberChanged);
        }

        void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Return ||
                e.keyCode == KeyCode.KeypadEnter)
            {
                if (IsValidInput(mLineNumberField.value, mTotalLines))
                    Confirm();

                e.StopPropagation();
                return;
            }

            if (e.keyCode == KeyCode.Escape)
            {
                Cancel();
                e.StopPropagation();
            }
        }

        void OnLineNumberChanged(ChangeEvent<string> e)
        {
            mOkButton.SetEnabled(IsValidInput(e.newValue, mTotalLines));
        }

        void Confirm()
        {
            if (!IsValidInput(mLineNumberField.value, mTotalLines))
                return;

            Confirmed?.Invoke(int.Parse(mLineNumberField.value));
        }

        void Cancel()
        {
            Cancelled?.Invoke();
        }

        int mCurrentLine;
        int mTotalLines;
        TextField mLineNumberField;
        Button mOkButton;

        const float BUTTON_WIDTH = 80f;
    }
}
