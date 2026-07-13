using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.UI.UIElements
{
    internal static class Styles
    {
        internal static class Toolbar
        {
            internal static void ApplyImageButtonStyle(ToolbarButton button)
            {
                button.style.width = ImageButtonWidth;
                button.style.justifyContent = Justify.Center;
                button.style.alignItems = Align.Center;
                button.style.paddingBottom = 1;
                button.focusable = false;
            }

            internal static void ApplyButtonStyle(ToolbarButton button)
            {
                button.style.unityTextAlign = TextAnchor.MiddleCenter;
                button.style.height = TextButtonHeight;
                button.style.paddingBottom = 1;
                button.focusable = false;
            }

            static StyleLength ImageButtonWidth = 28;
            static StyleLength TextButtonHeight = 20;
        }

        internal static class Button
        {
            internal static void ApplyImageButtonStyle(UnityEngine.UIElements.Button button)
            {
                button.style.justifyContent = Justify.Center;
                button.style.alignItems = Align.Center;
            }
        }

        internal static class ButtonGroup
        {
            internal static void ApplyTopButtonStyle(UnityEngine.UIElements.Button button)
            {
                button.style.marginBottom = 0;
                button.style.borderBottomLeftRadius = 0;
                button.style.borderBottomRightRadius = 0;
            }

            internal static void ApplyBottomButtonStyle(UnityEngine.UIElements.Button button)
            {
                button.style.marginTop = 0;
                button.style.borderTopWidth = 0;
                button.style.borderTopLeftRadius = 0;
                button.style.borderTopRightRadius = 0;
            }
        }
    }

    internal static class ControlBuilder
    {
        internal static class Toolbar
        {
            internal static UnityEditor.UIElements.Toolbar Create()
            {
                return new UnityEditor.UIElements.Toolbar();
            }

            internal static ToolbarSearchField CreateSearchField(string tooltip = null)
            {
                ToolbarSearchField searchField = new ToolbarSearchField();
                searchField.tooltip = tooltip;
                searchField.style.width = 200;
                return searchField;
            }

            public static ToolbarButton CreateButtonLeft(string text, Action onClick)
            {
                ToolbarButton button = CreateButton(text, onClick);

                // apply a negative margin to blend with previous button
                button.style.marginRight = -1;
                return button;
            }

            public static ToolbarButton CreateButton(string text, Action onClick)
            {
                ToolbarButton button = new ToolbarButton(onClick);

                Styles.Toolbar.ApplyButtonStyle(button);

                button.text = text;
                return button;
            }

            internal static ToolbarButton CreateImageButtonLeft(
                Texture image,
                string tooltip,
                Action onClick)
            {
                ToolbarButton button = CreateImageButton(
                    image,
                    tooltip,
                    onClick);

                // apply a negative margin to blend with next button
                button.style.marginRight = -1;
                return button;
            }

            internal static ToolbarButton CreateImageButton(
                Texture image,
                string tooltip,
                Action onClick)
            {
                var button = new ToolbarButton(onClick)
                {
                    tooltip = tooltip,
                };

                var icon = new Image
                {
                    image = image,
                };

                Styles.Toolbar.ApplyImageButtonStyle(button);

                button.Add(icon);

                return button;
            }

            internal static ToolbarToggle CreateToggleLeft(
                string text,
                string tooltip)
            {
                ToolbarToggle toggle = CreateToggle(text, tooltip);

                // apply a negative margin to blend with next button
                toggle.style.marginRight = -1;
                return toggle;
            }

            internal static ToolbarToggle CreateToggle(
                string text,
                string tooltip)
            {
                ToolbarToggle toggle = new ToolbarToggle();
                toggle.text = text;
                toggle.tooltip = tooltip;
                toggle.style.unityTextAlign = TextAnchor.MiddleCenter;
                return toggle;
            }

            internal static MultiSelectDropdown CreateMultiSelectDropdownLeft(
                string labelPrefix)
            {
                MultiSelectDropdown dropdown = CreateMultiSelectDropdown(labelPrefix);

                // apply a negative margin to blend with next button
                dropdown.style.marginRight = -1;
                return dropdown;
            }

            internal static MultiSelectDropdown CreateMultiSelectDropdown(
                string labelPrefix)
            {
                return new MultiSelectDropdown(labelPrefix);
            }

            internal static ToolbarToggle CreateImageToggleLeft(
                Texture image,
                string tooltip)
            {
                ToolbarToggle toggle = CreateImageToggle(image, tooltip);
                toggle.style.marginRight = -1;
                return toggle;
            }

            internal static ToolbarToggle CreateImageToggle(
                Texture image,
                string tooltip)
            {
                ToolbarToggle toggle = new ToolbarToggle();
                toggle.tooltip = tooltip;

                Image icon = new Image();
                icon.image = image;
                toggle.Add(icon);

                return toggle;
            }
        }

        internal static class Button
        {
            internal static UnityEngine.UIElements.Button CreateImageButton(
                Texture image,
                string tooltip,
                System.Action onClick)
            {
                UnityEngine.UIElements.Button button = new UnityEngine.UIElements.Button(onClick)
                {
                    tooltip = tooltip,
                    focusable = false
                };

                Image icon = new Image()
                {
                    image = image,
                };

                Styles.Button.ApplyImageButtonStyle(button);

                button.Add(icon);

                return button;
            }

            public static UnityEngine.UIElements.Button CreateButton(string text, Action handler)
            {
                UnityEngine.UIElements.Button result = new UnityEngine.UIElements.Button(handler);
                result.text = text;
                return result;
            }

            public static UnityEngine.UIElements.Button CreateDropDownButton(string text, Action handler)
            {
                UnityEngine.UIElements.Button button = new UnityEngine.UIElements.Button(handler);
                button.style.flexDirection = FlexDirection.Row;

                TextElement textElement = new TextElement();
                textElement.text = text;

                VisualElement dropDownIcon = new VisualElement();
                dropDownIcon.AddToClassList("unity-base-popup-field__arrow");
                dropDownIcon.style.marginLeft = 4;

                button.Add(textElement);
                button.Add(dropDownIcon);

                return button;
            }
        }

        internal static class ButtonGroup
        {
            internal static UnityEngine.UIElements.Button CreateImageTopButton(
                Texture image,
                string tooltip,
                System.Action onClick)
            {
                UnityEngine.UIElements.Button button =
                    Button.CreateImageButton(image, tooltip, onClick);

                Styles.ButtonGroup.ApplyTopButtonStyle(button);

                return button;
            }

            internal static UnityEngine.UIElements.Button CreateImageBottomButton(
                Texture image,
                string tooltip,
                System.Action onClick)
            {
                UnityEngine.UIElements.Button button =
                    Button.CreateImageButton(image, tooltip, onClick);

                Styles.ButtonGroup.ApplyBottomButtonStyle(button);

                return button;
            }
        }

        internal static class Label
        {
            internal static UnityEngine.UIElements.Label CreateSelectableLabel(string text = null)
            {
                UnityEngine.UIElements.Label result = new UnityEngine.UIElements.Label();
                result.selection.isSelectable = true;
                result.SetMouseCursor(MouseCursor.Text);
                result.text = text;

                return result;
            }
        }
    }
}
