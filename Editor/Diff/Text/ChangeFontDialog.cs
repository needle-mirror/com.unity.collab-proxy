using System;
using System.Collections.Generic;
using System.Linq;

using PlasticGui;
using Unity.CodeEditor;
using Unity.CodeEditor.Document;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using TextEditor = Unity.CodeEditor.TextEditor;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class ChangeFontDialog : EditorWindow
    {
        internal const string DEFAULT_FONT = "Default";

        internal const string NAME_FONT_FIELD = "change-font-field";
        internal const string NAME_RESTORE_DEFAULT_BUTTON = "change-font-restore-default-button";
        internal const string NAME_OK_BUTTON = "change-font-ok-button";
        internal const string NAME_CANCEL_BUTTON = "change-font-cancel-button";

        internal DropdownField GetFontField() =>
            rootVisualElement.Q<DropdownField>(NAME_FONT_FIELD);
        internal Button GetRestoreDefaultButton() =>
            rootVisualElement.Q<Button>(NAME_RESTORE_DEFAULT_BUTTON);
        internal Button GetOkButton() =>
            rootVisualElement.Q<Button>(NAME_OK_BUTTON);
        internal Button GetCancelButton() =>
            rootVisualElement.Q<Button>(NAME_CANCEL_BUTTON);

        internal static void Show(
            string currentFontName,
            Action<string> onFontSelected)
        {
            ChangeFontDialog dialog = CreateInstance<ChangeFontDialog>();
            dialog.mCurrentFontName = currentFontName;
            dialog.mOnFontSelected = onFontSelected;
            dialog.titleContent = new GUIContent(
                PlasticLocalization.Name.FontSelection.GetString());

            Vector2 size = new Vector2(DIALOG_WIDTH, DIALOG_HEIGHT);
            dialog.minSize = size;
            dialog.maxSize = size;

            Rect mainWindow = EditorGUIUtility.GetMainWindowPosition();
            dialog.position = new Rect(
                mainWindow.x + (mainWindow.width - DIALOG_WIDTH) / 2f,
                mainWindow.y + (mainWindow.height - DIALOG_HEIGHT) / 2f,
                DIALOG_WIDTH,
                DIALOG_HEIGHT);

            dialog.ShowUtility();
        }

        void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = MARGIN;
            root.style.paddingBottom = MARGIN;
            root.style.paddingLeft = MARGIN;
            root.style.paddingRight = MARGIN;

            root.Add(BuildFontSelectionArea());
            root.Add(BuildSampleTextArea());
            root.Add(BuildButtonsArea());

            root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        void OnDestroy()
        {
            rootVisualElement.UnregisterCallback<KeyDownEvent>(
                OnKeyDown, TrickleDown.TrickleDown);

            mSampleTextEditor?.Dispose();
        }

        VisualElement BuildFontSelectionArea()
        {
            VisualElement container = new VisualElement();

            Label label = new Label(
                PlasticLocalization.Name.SelectAFont.GetString());
            label.style.marginBottom = 4;

            Label explanation = new Label(
                PlasticLocalization.Name.SelectFontExplanation.GetString());
            explanation.style.marginBottom = 8;
            explanation.style.whiteSpace = WhiteSpace.Normal;

            mFontNames = GetMonospaceFontNames(mCurrentFontName, out int selectedIndex);

            List<string> displayNames = mFontNames
                .Select(n => n ?? string.Empty)
                .ToList();

            mFontField = new DropdownField(displayNames, selectedIndex);
            mFontField.name = NAME_FONT_FIELD;
            mFontField.RegisterValueChangedCallback(OnFontSelectionChanged);

            container.Add(label);
            container.Add(explanation);
            container.Add(mFontField);

            return container;
        }

        VisualElement BuildSampleTextArea()
        {
            VisualElement container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.marginTop = 10;
            container.style.backgroundColor = TextEditorColors.Background;
            container.style.borderTopWidth = 1;
            container.style.borderBottomWidth = 1;
            container.style.borderLeftWidth = 1;
            container.style.borderRightWidth = 1;
            container.style.borderTopColor = UnityStyles.Colors.BarBorder;
            container.style.borderBottomColor = UnityStyles.Colors.BarBorder;
            container.style.borderLeftColor = UnityStyles.Colors.BarBorder;
            container.style.borderRightColor = UnityStyles.Colors.BarBorder;
            container.style.paddingTop = 6;
            container.style.paddingBottom = 6;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;

            mSampleTextEditor = new TextEditor();
            mSampleTextEditor.IsReadOnly = true;
            mSampleTextEditor.ShowLineNumbers = false;
            mSampleTextEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            mSampleTextEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            mSampleTextEditor.Document = new TextDocument(SAMPLE_TEXT);

            UpdateSampleFont(mFontField.value);

            container.Add(mSampleTextEditor);

            return container;
        }

        VisualElement BuildButtonsArea()
        {
            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.FlexEnd;
            container.style.marginTop = 10;

            mRestoreDefaultButton = new Button(RestoreDefaultFont);
            mRestoreDefaultButton.name = NAME_RESTORE_DEFAULT_BUTTON;
            mRestoreDefaultButton.text =
                PlasticLocalization.Name.RestoreDefaultFont.GetString();
            mRestoreDefaultButton.style.marginRight = StyleKeyword.Auto;

            string currentFont = mFontField.value;
            mRestoreDefaultButton.SetEnabled(
                !string.IsNullOrEmpty(currentFont) && currentFont != DEFAULT_FONT);

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

            container.Add(mRestoreDefaultButton);

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

        void OnFontSelectionChanged(ChangeEvent<string> evt)
        {
            UpdateSampleFont(evt.newValue);

            mRestoreDefaultButton.SetEnabled(
                !string.IsNullOrEmpty(evt.newValue) && evt.newValue != DEFAULT_FONT);
        }

        void RestoreDefaultFont()
        {
            mFontField.value = DEFAULT_FONT;
        }

        void UpdateSampleFont(string fontName)
        {
            if (string.IsNullOrEmpty(fontName) || fontName == DEFAULT_FONT)
            {
                mSampleTextEditor.Font = null;
                return;
            }

            mSampleTextEditor.Font =
                Font.CreateDynamicFontFromOSFont(fontName, mSampleTextEditor.FontSize);
        }

        void Confirm()
        {
            mOnFontSelected?.Invoke(mFontField.value);
            Close();
        }

        static List<string> GetMonospaceFontNames(
            string fontToSelect, out int selectedIndex)
        {
            selectedIndex = 0;

            List<string> fonts = new List<string>(
                Font.GetOSInstalledFontNames()
                    .Where(name => name.IndexOf(
                        "mono", StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(name => name));

            fonts.Insert(0, DEFAULT_FONT);

            if (!string.IsNullOrEmpty(fontToSelect))
            {
                int index = fonts.IndexOf(fontToSelect);
                if (index >= 0)
                    selectedIndex = index;
            }

            return fonts;
        }

        Action<string> mOnFontSelected;
        string mCurrentFontName;
        List<string> mFontNames;
        DropdownField mFontField;
        TextEditor mSampleTextEditor;
        Button mRestoreDefaultButton;

        const float DIALOG_WIDTH = 510f;
        const float DIALOG_HEIGHT = 360f;
        const float BUTTON_WIDTH = 80f;
        const float MARGIN = 12f;

        static readonly string SAMPLE_TEXT =
            "The quick brown fox jumps over the lazy dog." + Environment.NewLine + Environment.NewLine +
            "abcdefghijklmnopqrstuvwxyz" + Environment.NewLine +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + Environment.NewLine +
            "0123456789 (){}[]" + Environment.NewLine +
            "+-*/= .,;:!? #&$%@|^" + Environment.NewLine + Environment.NewLine +
            "<!-- -- != := === >= >- >=> |-> -> <$>" + Environment.NewLine +
            "</> #[ |||> |= ~@";
    }
}
