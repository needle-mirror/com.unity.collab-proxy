using Unity.PlasticSCM.Editor.Diff.Asset.Content.Property;
using Unity.PlasticSCM.Editor.UI;
using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content
{
    internal class ContentPropertyRow : VisualElement
    {
        internal ContentPropertyRow()
        {
            CreateGUI();
        }

        internal void Bind(
            PropertyContent prop,
            string searchFilter,
            int indentLevel)
        {
            float padding = PropertyRow.BASE_PADDING_LEFT
                + indentLevel * PropertyRow.INDENT_PX;

            mPanel.style.paddingLeft = padding;
            mPanel.style.backgroundColor =
                UnityStyles.Colors.Diff.AssetDiff.PropertyRowBackground;

            string label = prop.DisplayName ?? prop.Path;
            bool hasSearch = !string.IsNullOrEmpty(searchFilter);
            string displayLabel = hasSearch
                ? SearchHighlight.Apply(label, searchFilter)
                : label;

            PropertyRow.ConfigureNameLabel(
                mLabel, hasContent: true, hasSearch, displayLabel, prop.Path);

            mValue.style.display = DisplayStyle.Flex;
            mValue.SetData(prop.Tag, prop.Value, hasContent: true, counterpartTag: null);
        }

        void CreateGUI()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;

            mPanel = PropertyRow.BuildPanel(out mLabel);

            mValue = new DiffValueDisplay();
            mPanel.Add(mValue);

            Add(mPanel);
        }

        VisualElement mPanel;
        Label mLabel;
        DiffValueDisplay mValue;
    }
}
