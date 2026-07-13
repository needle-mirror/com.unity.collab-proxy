using System;

using Codice.Client.Common.Threading;
using Codice.CM.Client.Differences;
using Codice.CM.Client.Differences.Graphic;
using MergetoolGui;

using Unity.PlasticSCM.Editor.Diff.Text;
using Unity.PlasticSCM.Editor.Diff.Purged;
using Unity.PlasticSCM.Editor.Diff.Texture.Toolbar;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Texture
{
    internal class ImageDiffControl : VisualElement, IBigFilePanelListener
    {
        internal ImageDiffControl(
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

            ShowDiff();
        }

        internal void Dispose()
        {
            if (mBigFileMessagePanel != null)
                mBigFileMessagePanel.Dispose();

            if (mPurgedRevisionControl != null)
                mPurgedRevisionControl.Dispose();

            mImageDiffToolbar.Dispose();
            mImageDiffViewer.Dispose();
        }

        void IBigFilePanelListener.OnCalculateDifferencesButtonClick()
        {
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

                    ShowDiff();
                });
        }

        void ShowDiff()
        {
            try
            {
                mImageDiffViewer.SetInfo(
                    mDiffViewerData.Left.File,
                    mDiffViewerData.Right.File,
                    mDiffViewerData.Left.SymbolicName,
                    mDiffViewerData.Right.SymbolicName);

                mMessagePanel.HandleMessage(mDiffViewerData.Message);

                EnableToolBar();
                ReplaceView(mImageDiffViewer);
            }
            catch (Exception ex)
            {
                mMessagePanel.HandleMessage(ex.Message);
                DisableToolbar();
            }
        }

        void ShowBigFileMessage()
        {
            if (mBigFileMessagePanel == null)
                mBigFileMessagePanel = new BigFileMessagePanel(this, true);

            mBigFileMessagePanel.UpdateDisplayData(
                BigFileDisplayData.Build(
                    mDiffViewerData, mBigFileDownloader));

            DisableToolbar();
            mMessagePanel.HandleMessage(string.Empty);
            ReplaceView(mBigFileMessagePanel);
        }

        void DisableToolbar()
        {
            mImageDiffToolbar.SetEnabled(false);
        }

        void EnableToolBar()
        {
            mImageDiffToolbar.SetEnabled(true);
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

            mImageDiffToolbar = new ImageDiffToolbar();
            mImageDiffToolbar.ViewModeChanged += OnViewModeChanged;
            mImageDiffToolbar.ChannelModeChanged += OnChannelModeChanged;

            mImageDiffViewer = new ImageDiffViewer(
                mImageDiffToolbar.ModeSwitcher);

            mContainerPanel = new VisualElement();
            mContainerPanel.style.flexGrow = 1;

            style.flexGrow = 1;

            mNormalContentPanel = new VisualElement();
            mNormalContentPanel.style.flexGrow = 1;
            mNormalContentPanel.Add(mMessagePanel);
            mNormalContentPanel.Add(mImageDiffToolbar);
            mNormalContentPanel.Add(mContainerPanel);

            Add(mNormalContentPanel);

            ReplaceView(mImageDiffViewer);
        }

        void OnViewModeChanged(ImageDiffMode mode)
        {
            mImageDiffViewer.SetViewMode(mode);
        }

        void OnChannelModeChanged(ColorWriteMask mode)
        {
            mImageDiffViewer.SetChannelMode(mode);
        }

        VisualElement mNormalContentPanel;
        VisualElement mContainerPanel;
        VisualElement mCurrentView;
        MessagePanel mMessagePanel;
        ImageDiffToolbar mImageDiffToolbar;
        ImageDiffViewer mImageDiffViewer;
        PurgedRevisionControl mPurgedRevisionControl;
        BigFileMessagePanel mBigFileMessagePanel;

        DiffViewerData mDiffViewerData;

        readonly IBigFileDownloader mBigFileDownloader;
        readonly IBigFileChecker mBigFileChecker;
    }
}
