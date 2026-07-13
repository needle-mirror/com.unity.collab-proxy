using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.Diff.Asset.Common;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Content
{
    internal class ContentHeaderRow : VisualElement
    {
        internal ContentHeaderRow()
        {
            CreateGUI();
        }

        internal void Bind(
            ObjectContent objContent,
            bool isExpanded,
            string searchFilter,
            int indentLevel)
        {
            mbIsHovered = false;

            float padding = HeaderRow.BASE_PADDING_LEFT
                + indentLevel * HeaderRow.INDENT_PX;

            HeaderRow.ApplyContentBorder(
                mContent, hasContent: true, padding, isExpanded,
                suppressBottomBorder: objContent.IsGameObject());

            string arrowText = isExpanded
                ? HeaderRow.ARROW_DOWN : HeaderRow.ARROW_RIGHT;
            UnityEngine.Texture icon = ObjectIconResolver.GetIcon(objContent);

            HeaderRow.ConfigurePanel(
                mArrow, mIcon, mLabel, mAnnotation, mPanel,
                hasContent: true, arrowText,
                HeaderRow.GetBackgroundColor(hasContent: true, mbIsHovered), icon,
                objContent.GetDisplayName(),
                searchFilter);
        }

        void CreateGUI()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;

            mPanel = HeaderRow.BuildPanel(
                out mContent, out mArrow, out mIcon, out mLabel, out mAnnotation);
            mPanel.RegisterCallback<MouseEnterEvent>(OnPanelMouseEnter);
            mPanel.RegisterCallback<MouseLeaveEvent>(OnPanelMouseLeave);
            Add(mPanel);
        }

        void OnPanelMouseEnter(MouseEnterEvent evt)
        {
            HeaderRow.SetHovered(
                ref mbIsHovered, hasContent: true, mPanel, bIsHovered: true);
        }

        void OnPanelMouseLeave(MouseLeaveEvent evt)
        {
            HeaderRow.SetHovered(
                ref mbIsHovered, hasContent: true, mPanel, bIsHovered: false);
        }

        VisualElement mPanel;
        VisualElement mContent;
        Label mArrow;
        Image mIcon;
        Label mLabel;
        Label mAnnotation;

        bool mbIsHovered;
    }
}
