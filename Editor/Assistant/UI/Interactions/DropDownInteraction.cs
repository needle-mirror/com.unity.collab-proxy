#if AIA_PRESENT
using System.Collections.Generic;
using Unity.AI.Assistant.FunctionCalling;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Assistant.UI.Interactions
{
    class DropDownInteraction : BaseInteraction<string>
    {
        // DropdownField's menu splits item names on '/' into submenu paths with no
        // escape character. Substitute '/' with U+29F8 BIG SOLIDUS only at the menu
        // construction layer (formatListItemCallback); choices and combo.value stay
        // untouched, and the field text restores the real '/' via the selected callback.
        const char k_MenuSlash = '\u29F8';

        public DropDownInteraction(
            string title,
            string description,
            string fieldLabel,
            string confirmLabel,
            List<string> choices)
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

            var combo = new DropdownField(
                fieldLabel,
                choices,
                0,
                formatSelectedValueCallback: val => val,
                formatListItemCallback: val => val?.Replace('/', k_MenuSlash));
            Add(combo);

            var buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.marginTop = 8;

            var confirmButton = new Button(() => CompleteInteraction(combo.value));
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
