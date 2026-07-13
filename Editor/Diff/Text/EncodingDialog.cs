using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Codice.Client.Common;
using MergetoolGui;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class EncodingDialog : EditorWindow
    {
        internal const string NAME_ENCODING_FIELD = "encoding-field";
        internal const string NAME_OK_BUTTON = "encoding-ok-button";
        internal const string NAME_CANCEL_BUTTON = "encoding-cancel-button";

        internal DropdownField GetEncodingField() =>
            rootVisualElement.Q<DropdownField>(NAME_ENCODING_FIELD);
        internal Button GetOkButton() =>
            rootVisualElement.Q<Button>(NAME_OK_BUTTON);
        internal Button GetCancelButton() =>
            rootVisualElement.Q<Button>(NAME_CANCEL_BUTTON);

        internal static void Show(
            Encoding encodingToSelect,
            Action<Encoding> onEncodingSelected)
        {
            EncodingDialog dialog = CreateInstance<EncodingDialog>();
            dialog.mEncodingToSelect = encodingToSelect;
            dialog.mOnEncodingSelected = onEncodingSelected;
            dialog.titleContent = new GUIContent(
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EncodingDialogTitle));

            Vector2 size = new Vector2(DIALOG_WIDTH, DIALOG_HEIGHT);
            dialog.minSize = size;
            dialog.maxSize = size;

            dialog.ShowUtility();
        }

        void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = MARGIN;
            root.style.paddingBottom = MARGIN;
            root.style.paddingLeft = MARGIN;
            root.style.paddingRight = MARGIN;

            root.Add(BuildEncodingArea());
            root.Add(BuildButtonsArea());

            root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        void OnDestroy()
        {
            rootVisualElement.UnregisterCallback<KeyDownEvent>(
                OnKeyDown, TrickleDown.TrickleDown);
        }

        VisualElement BuildEncodingArea()
        {
            VisualElement container = new VisualElement();
            container.style.flexGrow = 1;

            Label label = new Label(
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.EncodingDialogLabel));
            label.style.marginBottom = 4;

            mEncodings = BuildEncodingList(mEncodingToSelect, out int selectedIndex);

            List<string> encodingNames = mEncodings
                .Select(FormatEncoding)
                .ToList();

            mEncodingField = new DropdownField(encodingNames, selectedIndex);
            mEncodingField.name = NAME_ENCODING_FIELD;

            container.Add(label);
            container.Add(mEncodingField);

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

            Button okButton = ControlBuilder.Button.CreateButton(okText, Confirm);
            okButton.name = NAME_OK_BUTTON;
            Button cancelButton = ControlBuilder.Button.CreateButton(cancelText, Close);
            cancelButton.name = NAME_CANCEL_BUTTON;

            okButton.style.minWidth = BUTTON_WIDTH;
            cancelButton.style.minWidth = BUTTON_WIDTH;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                container.Add(okButton);
                container.Add(cancelButton);
            }
            else
            {
                container.Add(cancelButton);
                container.Add(okButton);
            }

            return container;
        }

        void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Return ||
                e.keyCode == KeyCode.KeypadEnter)
            {
                Confirm();
                e.StopPropagation();
                return;
            }

            if (e.keyCode == KeyCode.Escape)
            {
                Close();
                e.StopPropagation();
            }
        }

        void Confirm()
        {
            int selectedIndex = mEncodingField.index;
            Encoding selected = (selectedIndex >= 0 && selectedIndex < mEncodings.Count)
                ? mEncodings[selectedIndex]
                : null;

            mOnEncodingSelected(selected);
            Close();
        }

        static List<Encoding> BuildEncodingList(
            Encoding encodingToSelect, out int selectedIndex)
        {
            selectedIndex = 0;

            List<Encoding> items = EncodingManager.GetSystemEncodings()
                .Select(ei => ei.GetEncoding())
                .OrderBy(e => e.EncodingName)
                .ToList();

            if (encodingToSelect != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Equals(encodingToSelect))
                    {
                        selectedIndex = i;
                        break;
                    }
                }
            }

            return items;
        }

        static string FormatEncoding(Encoding encoding)
        {
            if (encoding == null)
                return string.Empty;

            return string.Format("{0} ({1}, {2})",
                encoding.EncodingName,
                encoding.WebName,
                encoding.CodePage);
        }

        Action<Encoding> mOnEncodingSelected;
        Encoding mEncodingToSelect;
        List<Encoding> mEncodings;
        DropdownField mEncodingField;

        const float DIALOG_WIDTH = 475f;
        const float DIALOG_HEIGHT = 105f;
        const float BUTTON_WIDTH = 80f;
        const float MARGIN = 12f;
    }
}
