using System;

using UnityEditor;
using UnityEngine.UIElements;

using Codice.CM.Client.Differences.Graphic;
using Unity.PlasticSCM.Editor.Diff.Text;
using Unity.PlasticSCM.Editor.Diff.Texture;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;

using XDiffGui;

using Language = XDiffGui.Options.Language;

namespace Unity.PlasticSCM.Editor.Diff.Purged
{
    internal class PurgedRevisionControl : VisualElement,
        ISaveChangesListener
    {
        internal VisualElement PurgedRevisionPanel => mPurgedRevisionPanel;
        internal VisualElement DiffPurgedPanel => mDiffPurgedPanel;
        internal VisualElement EditorPanel => mTextEditorPanel;
        internal ImageViewPanel ImageViewPanel => mImageViewPanel;
        internal VisualElement LeftPanel => mLeftPanel;
        internal VisualElement RightPanel => mRightPanel;

        internal PurgedRevisionControl(
            IAfterSaveChangesListener afterSaveChangesListener = null)
        {
            mAfterSaveChangesListener = afterSaveChangesListener;

            BuildComponents();
        }

        internal void ShowData(DiffViewerData diffData)
        {
            mTextEditorPanel.FinishPendingDraftSave();

            mContributorsHeaderPanel.OnLeftTextBoxClean();
            mContributorsHeaderPanel.OnRightTextBoxClean();

            if (IsSingleContributor(diffData))
            {
                ReplaceContent(mPurgedRevisionPanel);
                return;
            }

            ReplaceContent(mDiffPurgedPanel);

            SymbolicNameParser.GetContributorSpecs(
                diffData.Left?.SymbolicName,
                diffData.Right?.SymbolicName,
                out string leftContributor,
                out string rightContributor);

            mContributorsHeaderPanel.SetNames(
                leftContributor, rightContributor);

            mPathForEdition = diffData.PathForEdition;

            if (diffData.Left.IsPurged)
            {
                mbIsEditorOnLeft = false;

                ShowEntryDataContent(
                    diffData.Right,
                    diffData.Extension,
                    ReplaceRightPanel);

                ReplaceLeftPanel(mPurgedRevisionPanel);
                return;
            }

            mbIsEditorOnLeft = true;

            ShowEntryDataContent(
                diffData.Left,
                diffData.Extension,
                ReplaceLeftPanel);

            ReplaceRightPanel(mPurgedRevisionPanel);
        }

        internal bool HasUnsavedChanges()
        {
            return mTextEditorPanel.IsTextDirty;
        }

        internal void SaveChanges()
        {
            mTextEditorPanel.SaveChanges(
                mAfterSaveChangesListener);
        }

        internal void Dispose()
        {
            mTextEditorPanel.DirtyStateChanged -=
                OnTextEditorDirtyStateChanged;

            mPurgedRevisionPanel.Dispose();

            mSplitter.UnregisterCallback<PointerDownEvent>(
                OnSplitterPointerDown);
            mSplitter.UnregisterCallback<PointerMoveEvent>(
                OnSplitterPointerMove);
            mSplitter.UnregisterCallback<PointerUpEvent>(
                OnSplitterPointerUp);

            mContributorsHeaderPanel.Dispose();
            mSaveChangesPanel.Dispose();
            mTextEditorPanel.Dispose();

            if (mImageViewPanel != null)
                mImageViewPanel.Dispose();
        }

        void ISaveChangesListener.OnSaveChanges()
        {
            if (!HasUnsavedChanges())
                return;

            SaveChanges();
        }

        void ISaveChangesListener.OnDiscardChanges()
        {
            mTextEditorPanel.DiscardChanges();
        }

        void ShowEntryDataContent(
            EntryData data,
            string extension,
            Action<VisualElement> replaceSidePanel)
        {
            bool isImageContent =
                DiffViewerDataExtensions.IsSupportedImage(extension);

            if (isImageContent)
            {
                ShowImageContent(data.File, replaceSidePanel);
                return;
            }

            ShowTextContent(data, replaceSidePanel);
        }

        void ShowImageContent(
            string file,
            Action<VisualElement> replaceSidePanel)
        {
            if (mImageViewPanel == null)
                mImageViewPanel = new ImageViewPanel();

            try
            {
                mImageViewPanel.ShowImage(file);

                replaceSidePanel(mImageViewPanel);
            }
            catch (Exception)
            {
                mImageViewPanel.ClearImage();
            }
        }

        void ShowTextContent(EntryData data, Action<VisualElement> replaceSidePanel)
        {
            if (string.IsNullOrEmpty(data.File))
            {
                mTextEditorPanel.ShowContent(
                    data.Content, Language.PlainText, false);
                replaceSidePanel(mTextEditorPanel);
                return;
            }

            mTextEditorPanel.ShowFileContent(
                data.File,
                data.Encoding,
                mPathForEdition);
            replaceSidePanel(mTextEditorPanel);
        }

        void ReplaceContent(VisualElement panel)
        {
            mLeftPanel.Clear();
            mRightPanel.Clear();

            if (Contains(panel))
                return;

            Clear();
            Add(panel);
        }

        void ReplaceLeftPanel(VisualElement panel)
        {
            if (!mLeftPanel.Contains(panel))
                mLeftPanel.Add(panel);
        }

        void ReplaceRightPanel(VisualElement panel)
        {
            if (!mRightPanel.Contains(panel))
                mRightPanel.Add(panel);
        }

        void OnTextEditorDirtyStateChanged(bool isDirty)
        {
            if (isDirty)
            {
                OnTextEditorDirty();
                return;
            }

            OnTextEditorClean();
        }

        void OnTextEditorClean()
        {
            if (mbIsEditorOnLeft)
                mContributorsHeaderPanel.OnLeftTextBoxClean();
            else
                mContributorsHeaderPanel.OnRightTextBoxClean();

            mSaveChangesPanel.style.display =
                DisplayStyle.None;
        }

        void OnTextEditorDirty()
        {
            if (mbIsEditorOnLeft)
                mContributorsHeaderPanel.OnLeftTextBoxDirty();
            else
                mContributorsHeaderPanel.OnRightTextBoxDirty();

            mSaveChangesPanel.style.display =
                DisplayStyle.Flex;
        }


        void OnSplitterPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
                return;

            mbIsDragging = true;
            mDragStartX = evt.position.x;
            mStartLeftWidth = mLeftPanel.resolvedStyle.width;
            mStartRightWidth =
                mRightPanel.resolvedStyle.width;

            mSplitter.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void OnSplitterPointerMove(PointerMoveEvent evt)
        {
            if (!mbIsDragging)
                return;

            float deltaX = evt.position.x - mDragStartX;

            float newLeftWidth = mStartLeftWidth + deltaX;
            float newRightWidth = mStartRightWidth - deltaX;

            if (newLeftWidth < MIN_PANEL_WIDTH)
            {
                newLeftWidth = MIN_PANEL_WIDTH;
                newRightWidth = mStartLeftWidth
                    + mStartRightWidth - MIN_PANEL_WIDTH;
            }
            else if (newRightWidth < MIN_PANEL_WIDTH)
            {
                newRightWidth = MIN_PANEL_WIDTH;
                newLeftWidth = mStartLeftWidth
                    + mStartRightWidth - MIN_PANEL_WIDTH;
            }

            mLeftPanel.style.flexGrow = newLeftWidth;
            mRightPanel.style.flexGrow = newRightWidth;

            evt.StopPropagation();
        }

        void OnSplitterPointerUp(PointerUpEvent evt)
        {
            if (!mbIsDragging)
                return;

            mbIsDragging = false;
            mSplitter.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }

        void BuildComponents()
        {
            style.flexGrow = 1;

            mPurgedRevisionPanel = new PurgedStatePanel();

            mTextEditorPanel = new TextEditorPanel(this);
            mTextEditorPanel.DirtyStateChanged +=
                OnTextEditorDirtyStateChanged;

            mSaveChangesPanel = new SaveChangesPanel(this);
            mSaveChangesPanel.style.flexShrink = 0;

            mContributorsHeaderPanel =
                new ContributorsHeaderPanel();

            mLeftPanel = new VisualElement();
            mLeftPanel.style.flexGrow = 1;
            mLeftPanel.style.flexBasis = 0;

            mRightPanel = new VisualElement();
            mRightPanel.style.flexGrow = 1;
            mRightPanel.style.flexBasis = 0;

            mSplitter = new VisualElement();
            mSplitter.style.width = SPLITTER_WIDTH;
            mSplitter.style.backgroundColor =
                UnityStyles.Colors.Diff
                    .DiffSplitterBackgroundColor;
            mSplitter.SetMouseCursor(
                MouseCursor.SplitResizeLeftRight);
            mSplitter.RegisterCallback<PointerDownEvent>(
                OnSplitterPointerDown);
            mSplitter.RegisterCallback<PointerMoveEvent>(
                OnSplitterPointerMove);
            mSplitter.RegisterCallback<PointerUpEvent>(
                OnSplitterPointerUp);

            VisualElement diffContainer = new VisualElement();
            diffContainer.style.flexDirection =
                FlexDirection.Row;
            diffContainer.style.flexGrow = 1;
            diffContainer.Add(mLeftPanel);
            diffContainer.Add(mSplitter);
            diffContainer.Add(mRightPanel);

            UnityEditor.UIElements.Toolbar saveChangesRow = ControlBuilder.Toolbar.Create();
            saveChangesRow.style.flexDirection =
                FlexDirection.RowReverse;
            saveChangesRow.style.flexShrink = 0;
            saveChangesRow.Add(mSaveChangesPanel);

            mDiffPurgedPanel = new VisualElement();
            mDiffPurgedPanel.style.flexGrow = 1;
            mDiffPurgedPanel.Add(saveChangesRow);
            mDiffPurgedPanel.Add(mContributorsHeaderPanel);
            mDiffPurgedPanel.Add(diffContainer);
        }


        static bool IsSingleContributor(
            DiffViewerData diffData)
        {
            bool isLeftPurged = diffData.Left != null
                && diffData.Left.IsPurged;
            bool isRightPurged = diffData.Right != null
                && diffData.Right.IsPurged;
            bool isRightFirstRevision = diffData.Right != null
                && diffData.Right.IsFirstRevision;

            return isLeftPurged && isRightPurged ||
                   isLeftPurged && diffData.Right == null ||
                   isRightFirstRevision && isRightPurged;
        }

        PurgedStatePanel mPurgedRevisionPanel;
        VisualElement mDiffPurgedPanel;
        VisualElement mLeftPanel;
        VisualElement mRightPanel;
        VisualElement mSplitter;
        SaveChangesPanel mSaveChangesPanel;
        ContributorsHeaderPanel mContributorsHeaderPanel;
        TextEditorPanel mTextEditorPanel;
        ImageViewPanel mImageViewPanel;

        IAfterSaveChangesListener mAfterSaveChangesListener;

        string mPathForEdition;
        bool mbIsEditorOnLeft;
        bool mbIsDragging;
        float mDragStartX;
        float mStartLeftWidth;
        float mStartRightWidth;

        const int SPLITTER_WIDTH = 6;
        const float MIN_PANEL_WIDTH = 100f;
    }
}
