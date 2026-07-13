using Codice.CM.Common;
using PlasticGui;

using UnityEngine;
using UnityEngine.UIElements;

using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.History
{
    internal class MoveRealizationInfoDetailsPanel : VisualElement
    {
        internal Label TitleLabel => mTitleLabel;
        internal Label SrcPathLabel => mSrcPathLabel;
        internal Label DstPathLabel => mDstPathLabel;

        internal MoveRealizationInfoDetailsPanel()
        {
            BuildComponents();
        }

        internal void SetData(MoveRealizationInfo moveInfo)
        {
            SetFileData(
                moveInfo?.SrcCmPath,
                moveInfo?.SrcDirRev?.Type == EnumRevisionType.enDirectory,
                mSrcFileImage,
                mSrcPathLabel);
            SetFileData(
                moveInfo?.DstCmPath,
                moveInfo?.DstDirRev?.Type == EnumRevisionType.enDirectory,
                mDstFileImage,
                mDstPathLabel);
        }

        void BuildComponents()
        {
            style.flexGrow = 1;
            style.justifyContent = Justify.Center;
            style.alignItems = Align.Center;
            style.marginLeft = MARGIN;
            style.marginRight = MARGIN;
            style.marginTop = MARGIN;
            style.marginBottom = MARGIN;

            mTitleLabel = new Label(
                PlasticLocalization.Name.ThisItemWasMoved.GetString());
            mTitleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            mTitleLabel.style.fontSize = 14;
            mTitleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            mTitleLabel.style.marginBottom = 12;

            VisualElement rowsContainer = new VisualElement();
            rowsContainer.style.marginLeft = MARGIN;
            rowsContainer.style.marginRight = MARGIN;
            rowsContainer.style.minWidth = ROW_MIN_WIDTH;
            rowsContainer.Add(BuildPathRow(
                PlasticLocalization.Name.MovedFromLabel.GetString(),
                out mSrcFileImage, out mSrcPathLabel));
            rowsContainer.Add(BuildPathRow(
                PlasticLocalization.Name.MovedToLabel.GetString(),
                out mDstFileImage, out mDstPathLabel));

            Add(mTitleLabel);
            Add(rowsContainer);
        }

        static VisualElement BuildPathRow(
            string labelText,
            out Image fileImage,
            out Label pathLabel)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.FlexStart;
            row.style.marginTop = 10;

            Label label = new Label(labelText);
            label.style.width = LABEL_WIDTH;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;

            fileImage = new Image();
            fileImage.style.width = ICON_SIZE;
            fileImage.style.height = ICON_SIZE;
            fileImage.style.marginLeft = 4;

            pathLabel = ControlBuilder.Label.CreateSelectableLabel();
            pathLabel.style.marginLeft = 4;
            pathLabel.style.flexGrow = 1;
            pathLabel.style.flexShrink = 1;
            pathLabel.style.whiteSpace = WhiteSpace.Normal;

            row.Add(label);
            row.Add(fileImage);
            row.Add(pathLabel);

            return row;
        }

        static void SetFileData(
            string cmPath,
            bool isDirectory,
            Image fileImage,
            Label pathLabel)
        {
            pathLabel.text = cmPath ?? string.Empty;
            fileImage.image = GetIcon(isDirectory, cmPath);
        }

        static UnityEngine.Texture GetIcon(bool isDirectory, string cmPath)
        {
            if (isDirectory)
                return Images.GetFolderIcon();

            return string.IsNullOrEmpty(cmPath) ?
                Images.GetFileIcon() :
                Images.GetFileIconFromCmPath(cmPath);
        }

        Label mTitleLabel;
        Image mSrcFileImage;
        Image mDstFileImage;
        Label mSrcPathLabel;
        Label mDstPathLabel;

        const int ROW_MIN_WIDTH = 100;
        const int MARGIN = 24;
        const int LABEL_WIDTH = 70;
        const int ICON_SIZE = 16;
    }
}
