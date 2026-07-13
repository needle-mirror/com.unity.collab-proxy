#if AIA_PRESENT
using System.Collections.Generic;
using Unity.AI.Assistant.FunctionCalling;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Assistant.UI.Interactions
{
    class RadioGroupInteraction : BaseInteraction<string>
    {
        string m_SelectedValue;

        public RadioGroupInteraction(
            string title,
            string description,
            List<OptionChoice> options,
            string confirmLabel = "Select")
        {
            style.paddingTop = 8;
            style.paddingBottom = 8;

            var titleLabel = new Label(title);
            titleLabel.style.fontSize = 14;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 4;
            Add(titleLabel);

            if (!string.IsNullOrEmpty(description))
            {
                var descLabel = new Label(description);
                descLabel.style.marginBottom = 8;
                descLabel.style.whiteSpace = WhiteSpace.Normal;
                Add(descLabel);
            }

            m_SelectedValue = options.Count > 0 ? options[0].Label : string.Empty;

            var radioGroup = new GroupBox();
            radioGroup.style.marginBottom = 8;

            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];
                var optionContainer = new VisualElement();
                optionContainer.style.marginBottom = i < options.Count - 1 ? 8 : 0;

                var radio = new RadioButton();
                radio.value = i == 0;
                radio.style.flexDirection = FlexDirection.RowReverse;
                var capturedLabel = option.Label;
                radio.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        m_SelectedValue = capturedLabel;
                });

                var labelContainer = new VisualElement();
                labelContainer.style.marginLeft = 4;
                labelContainer.style.flexGrow = 1;

                var optionLabel = new Label(option.Label);
                optionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                labelContainer.Add(optionLabel);

                if (!string.IsNullOrEmpty(option.Description))
                {
                    var optionDesc = new Label(option.Description);
                    optionDesc.style.whiteSpace = WhiteSpace.Normal;
                    optionDesc.style.opacity = 0.7f;
                    optionDesc.style.fontSize = 12;
                    optionDesc.style.marginTop = 2;
                    labelContainer.Add(optionDesc);
                }

                optionContainer.style.flexDirection = FlexDirection.Row;
                optionContainer.style.alignItems = Align.FlexStart;
                optionContainer.Add(radio);
                optionContainer.Add(labelContainer);

                var capturedRadio = radio;
                labelContainer.RegisterCallback<ClickEvent>(evt => capturedRadio.value = true);

                radioGroup.Add(optionContainer);
            }

            Add(radioGroup);

            var buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.marginTop = 4;

            var confirmButton = new Button(() => CompleteInteraction(m_SelectedValue));
            confirmButton.text = confirmLabel;
            buttonRow.Add(confirmButton);

            var cancelButton = new Button(CancelInteraction);
            cancelButton.text = "Cancel";
            buttonRow.Add(cancelButton);

            Add(buttonRow);
        }
    }
}
#endif
