using System;

using Codice.Client.Common.Threading;
using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;
using MergetoolGui;
using PlasticGui;

using Unity.PlasticSCM.Editor.Diff.Text;

using Unity.PlasticSCM.Editor.Diff.Purged;
using Unity.PlasticSCM.Editor.Diff.Texture.Toolbar;

using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Texture
{
    internal class ImageContentControl : VisualElement, IBigFilePanelListener
    {
        internal ImageContentControl(
            IBigFileDownloader bigFileDownloader,
            IBigFileChecker bigFileChecker)
        {
            mBigFileDownloader = bigFileDownloader;
            mBigFileChecker = bigFileChecker;

            CreateGUI();
        }

        internal void ShowData(DiffViewerData data)
        {
            mDiffViewerData = data;

            ReplaceMainView(mNormalContentPanel);

            if (BigFileDiffCalculator.IsBigFileDiff(data, mBigFileDownloader, mBigFileChecker))
            {
                ShowBigFileMessage();
                return;
            }

            if (data.Left?.IsPurged == true || data.Right?.IsPurged == true)
            {
                ShowPurgedRevision(data);
                return;
            }

            mMessagePanel.HandleMessage(data.Message);

            mFileTextView.text = PlasticLocalization.GetString(
                PlasticLocalization.Name.Content, data.Left.SymbolicName);

            ShowTexture(data.Left.File);
        }

        internal void Dispose()
        {
            if (mBigFileMessagePanel != null)
                mBigFileMessagePanel.Dispose();

            if (mPurgedRevisionControl != null)
                mPurgedRevisionControl.Dispose();

            mChannelOptionsView.Dispose();
            mImageViewPanel.Dispose();
        }

        void IBigFilePanelListener.OnCalculateDifferencesButtonClick()
        {
            if (mBigFileDownloader == null)
            {
                ShowTexture(mDiffViewerData.Left.File);
                return;
            }

            mBigFileMessagePanel.Disable();

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    mBigFileDownloader.DownloadFiles(mDiffViewerData, null);
                },
                /*afterOperationDelegate*/ delegate
                {
                    mBigFileMessagePanel.Enable();

                    if (waiter.Exception != null)
                    {
                        MergetoolExceptionsHandler.DisplayException(
                            waiter.Exception);
                        return;
                    }

                    if (mDiffViewerData.Left?.IsPurged == true ||
                        mDiffViewerData.Right?.IsPurged == true)
                    {
                        ShowPurgedRevision(mDiffViewerData);
                        return;
                    }

                    ShowTexture(mDiffViewerData.Left.File);
                });
        }

        void ShowTexture(string file)
        {
            try
            {
                mImageViewPanel.ShowImage(file);

                ReplaceView(mImageViewPanel);
            }
            catch (Exception ex)
            {
                mMessagePanel.HandleMessage(ex.Message);

                mImageViewPanel.ClearImage();
            }
        }

        void ShowBigFileMessage()
        {
            if (mBigFileMessagePanel == null)
                mBigFileMessagePanel = new BigFileMessagePanel(this, false);

            mBigFileMessagePanel.UpdateDisplayData(
                BigFileDisplayData.Build(
                    mDiffViewerData, mBigFileDownloader));

            mMessagePanel.HandleMessage(string.Empty);
            ReplaceView(mBigFileMessagePanel);
        }

        void ShowPurgedRevision(DiffViewerData data)
        {
            if (mPurgedRevisionControl == null)
                mPurgedRevisionControl = new PurgedRevisionControl();

            mPurgedRevisionControl.ShowData(data);
            ReplaceMainView(mPurgedRevisionControl);
        }

        void ReplaceMainView(VisualElement panel)
        {
            if (hierarchy.childCount == 1 && hierarchy[0] == panel)
                return;

            Clear();
            Add(panel);
        }

        void ReplaceView(VisualElement view)
        {
            if (mCurrentView == view)
                return;

            if (mCurrentView != null)
                mContainerPanel.Remove(mCurrentView);

            mContainerPanel.Add(view);
            mCurrentView = view;
        }

        void CreateGUI()
        {
            mMessagePanel = new MessagePanel();
            VisualElement toolBarPanel = BuildToolBarPanel();
            mContainerPanel = new VisualElement();
            mContainerPanel.style.flexGrow = 1;

            style.flexGrow = 1;

            mNormalContentPanel = new VisualElement();
            mNormalContentPanel.style.flexGrow = 1;
            mNormalContentPanel.Add(mMessagePanel);
            mNormalContentPanel.Add(toolBarPanel);
            mNormalContentPanel.Add(mContainerPanel);

            Add(mNormalContentPanel);

            mImageViewPanel = new ImageViewPanel();

            ReplaceView(mImageViewPanel);
        }

        VisualElement BuildToolBarPanel()
        {
            UnityEditor.UIElements.Toolbar toolbar = ControlBuilder.Toolbar.Create();

            mFileTextView = ControlBuilder.Label.CreateSelectableLabel();
            mFileTextView.style.unityTextAlign = UnityEngine.TextAnchor.MiddleLeft;
            mFileTextView.style.overflow = Overflow.Hidden;
            mFileTextView.style.textOverflow = TextOverflow.Ellipsis;
            mFileTextView.style.flexGrow = 1;
            mFileTextView.style.flexShrink = 1;
            toolbar.Add(mFileTextView);

            mChannelOptionsView = new ChannelOptionsView();
            mChannelOptionsView.ChannelModeChanged += OnChannelModeChanged;
            toolbar.Add(mChannelOptionsView);

            return toolbar;
        }

        void OnChannelModeChanged(ColorWriteMask mode)
        {
            mImageViewPanel.SetChannelMode(mode);
        }

        VisualElement mNormalContentPanel;
        VisualElement mContainerPanel;
        VisualElement mCurrentView;
        MessagePanel mMessagePanel;
        PurgedRevisionControl mPurgedRevisionControl;
        ImageViewPanel mImageViewPanel;
        ChannelOptionsView mChannelOptionsView;
        Label mFileTextView;
        BigFileMessagePanel mBigFileMessagePanel;

        DiffViewerData mDiffViewerData;

        readonly IBigFileDownloader mBigFileDownloader;
        readonly IBigFileChecker mBigFileChecker;
    }
}
