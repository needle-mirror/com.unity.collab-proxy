using System;

using Codice.Client.Common;
using MergetoolGui;
using Unity.PlasticSCM.Editor.UI;
using UnityEngine;
using UnityEngine.UIElements;
using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class BigFileMessagePanel : VisualElement
    {
        internal BigFileMessagePanel(
            IBigFilePanelListener listener, bool isForDifferences)
        {
            mListener = listener;
            mIsForDifferences = isForDifferences;

            BuildComponents();
        }

        internal void UpdateDisplayData(BigFileDisplayData displayData)
        {
            SetData(displayData);
        }

        internal void Enable()
        {
            mCalculateDiffsButton.SetEnabled(true);
        }

        internal void Disable()
        {
            mCalculateDiffsButton.SetEnabled(false);
        }

        internal void Dispose()
        {
            mCalculateDiffsButton.clicked -= OnCalculateDiffsButtonClicked;
        }

        void OnCalculateDiffsButtonClicked()
        {
            mListener.OnCalculateDifferencesButtonClick();
        }

        void BuildComponents()
        {
            style.flexGrow = 1;
            style.justifyContent = Justify.Center;
            style.alignItems = Align.Center;

            mContentPanel = new VisualElement();
            mContentPanel.style.width = CONTENT_WIDTH;
            mContentPanel.style.alignItems = Align.Center;

            mBadgesPanel = new VisualElement();
            mBadgesPanel.style.flexDirection = FlexDirection.Row;
            mBadgesPanel.style.justifyContent = Justify.Center;
            mBadgesPanel.style.alignItems = Align.Center;
            mBadgesPanel.style.marginTop = MARGIN;

            mPerformanceLabel = new Label();
            mPerformanceLabel.style.whiteSpace = WhiteSpace.Normal;
            mPerformanceLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            mPerformanceLabel.style.marginTop = MARGIN;

            mStorageLabel = new Label();
            mStorageLabel.style.whiteSpace = WhiteSpace.Normal;
            mStorageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            mStorageLabel.style.color = UnityStyles.Colors.SecondaryLabel;
            mStorageLabel.style.marginTop = BADGE_SPACING;

            mCalculateDiffsButton = new Button();
            mCalculateDiffsButton.style.marginTop = MARGIN;
            mCalculateDiffsButton.clicked += OnCalculateDiffsButtonClicked;

            mContentPanel.Add(mBadgesPanel);
            mContentPanel.Add(mPerformanceLabel);
            mContentPanel.Add(mStorageLabel);
            mContentPanel.Add(mCalculateDiffsButton);

            Add(mContentPanel);
        }

        void SetData(BigFileDisplayData data)
        {
            mBadgesPanel.Clear();

            FillBadgesPanel(
                mIsForDifferences,
                mBadgesPanel,
                data);

            bool needsDownload = data.LeftNeedsDownload || data.RightNeedsDownload;

            mPerformanceLabel.text = MergetoolLocalization.GetString(
                MergetoolLocalization.Name.BigFilePerformanceWarning);

            mStorageLabel.text = MergetoolLocalization.GetString(needsDownload
                ? MergetoolLocalization.Name.BigFileDownloadStorageMessage
                : MergetoolLocalization.Name.BigFileLocalStorageMessage);

            mCalculateDiffsButton.text = GetButtonLabel(
                mIsForDifferences,
                needsDownload);
        }

        static void FillBadgesPanel(
            bool isForDifferences,
            VisualElement badgesPanel,
            BigFileDisplayData data)
        {
            if (isForDifferences)
            {
                AddDownloadBadge(
                    badgesPanel,
                    data.LeftNeedsDownload || data.RightNeedsDownload);
                badgesPanel.Add(CreateBadgeSeparator());
                AddSizeBadge(
                    badgesPanel,
                    MergetoolLocalization.Name.BigFileLeftSizeBadge,
                    data.LeftSize);
                badgesPanel.Add(CreateBadgeSeparator());
                AddSizeBadge(
                    badgesPanel,
                    MergetoolLocalization.Name.BigFileRightSizeBadge,
                    data.RightSize);
                badgesPanel.Add(CreateBadgeSeparator());
                AddDownloadBadge(
                    badgesPanel,
                    data.RightNeedsDownload);

                return;
            }

            AddSizeBadge(
                badgesPanel,
                MergetoolLocalization.Name.BigFileSizeBadge,
                Math.Max(data.LeftSize, data.RightSize));
            badgesPanel.Add(CreateBadgeSeparator());
            AddDownloadBadge(
                badgesPanel,
                data.LeftNeedsDownload || data.RightNeedsDownload);
        }

        static VisualElement CreateBadgeSeparator()
        {
            Label separator = new Label();
            separator.text = "\u00b7";
            separator.style.unityTextAlign = TextAnchor.MiddleCenter;
            separator.style.marginLeft = BADGE_SPACING / 2;
            separator.style.marginRight = BADGE_SPACING / 2;
            return separator;
        }

        static void AddSizeBadge(
            VisualElement badgesPanel,
            MergetoolLocalization.Name sizeBadgeName,
            long size)
        {
            VisualElement sizeBadge = BuildBadge(
                GetSizeBadgeText(sizeBadgeName, size),
                UnityStyles.Colors.Diff.BigFile.YellowBackgroundColor);
            badgesPanel.Add(sizeBadge);
        }

        static void AddDownloadBadge(
            VisualElement badgesPanel,
            bool needsDownload)
        {
            Color backgroundColor = needsDownload
                ? UnityStyles.Colors.Diff.BigFile.BlueBackgroundColor
                : UnityStyles.Colors.Diff.BigFile.GreenBackgroundColor;

            Texture2D statusIcon = needsDownload
                ? Images.GetArrowDownIcon()
                : Images.GetCheckMarkIcon();

            VisualElement statusBadge = BuildBadge(
                GetStatusBadgeText(needsDownload),
                backgroundColor,
                statusIcon);

            badgesPanel.Add(statusBadge);
        }

        static VisualElement BuildBadge(
            string text,
            Color backgroundColor,
            Texture2D icon = null)
        {
            Label label = new Label();
            label.text = text;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.fontSize = BADGE_FONT_SIZE;

            VisualElement badge = new VisualElement();
            badge.style.flexDirection = FlexDirection.Row;
            badge.style.alignItems = Align.Center;
            badge.style.backgroundColor = backgroundColor;
            badge.style.borderTopLeftRadius = BADGE_CORNER_RADIUS;
            badge.style.borderTopRightRadius = BADGE_CORNER_RADIUS;
            badge.style.borderBottomLeftRadius = BADGE_CORNER_RADIUS;
            badge.style.borderBottomRightRadius = BADGE_CORNER_RADIUS;
            badge.style.paddingLeft = BADGE_HORIZONTAL_PADDING;
            badge.style.paddingRight = BADGE_HORIZONTAL_PADDING;
            badge.style.paddingTop = BADGE_VERTICAL_PADDING;
            badge.style.paddingBottom = BADGE_VERTICAL_PADDING;

            if (icon != null)
            {
                Image iconImage = new Image();
                iconImage.image = icon;
                iconImage.style.width = BADGE_ICON_SIZE;
                iconImage.style.height = BADGE_ICON_SIZE;
                iconImage.style.marginRight = 4;
                badge.Add(iconImage);
            }

            badge.Add(label);

            return badge;
        }

        static string GetSizeBadgeText(
            MergetoolLocalization.Name badgeName, long size)
        {
            string sizeStr = size > 0
                ? SizeConverter.ConvertToSizeString(size)
                : "?";

            return badgeName.GetString(sizeStr);
        }

        static string GetStatusBadgeText(bool needsDownload)
        {
            if (!needsDownload)
            {
                return MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.BigFileLocalBadge);
            }

            return MergetoolLocalization.GetString(
                MergetoolLocalization.Name.BigFileDownloadRequiredBadge);
        }

        static string GetButtonLabel(
            bool isForDifferences,
            bool needsDownload)
        {
            if (isForDifferences)
            {
                return MergetoolLocalization.GetString(needsDownload
                    ? MergetoolLocalization.Name.DownloadAndFindDifferencesButton
                    : MergetoolLocalization.Name.CalculateDifferencesButton);
            }

            return MergetoolLocalization.GetString(needsDownload
                ? MergetoolLocalization.Name.DownloadAndShowContentButton
                : MergetoolLocalization.Name.ShowContentButton);
        }

        VisualElement mContentPanel;
        VisualElement mBadgesPanel;
        Label mPerformanceLabel;
        Label mStorageLabel;
        Button mCalculateDiffsButton;

        readonly IBigFilePanelListener mListener;
        readonly bool mIsForDifferences;

        const float CONTENT_WIDTH = 400;
        const float MARGIN = 20;
        const float BADGE_FONT_SIZE = 11;
        const float BADGE_CORNER_RADIUS = 10;
        const float BADGE_HORIZONTAL_PADDING = 10;
        const float BADGE_VERTICAL_PADDING = 4;
        const float BADGE_ICON_SIZE = 12;
        const float BADGE_SPACING = 8;
    }
}
