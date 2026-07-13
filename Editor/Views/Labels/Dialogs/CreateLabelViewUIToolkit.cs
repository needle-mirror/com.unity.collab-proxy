using System;

using UnityEngine;
using UnityEngine.UIElements;

using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.QueryViews.Labels;

using Unity.PlasticSCM.Editor.UI.UIElements;

namespace Unity.PlasticSCM.Editor.Views.Labels.Dialogs
{
    internal class CreateLabelViewUIToolkit : VisualElement, IPlasticDialogCloser
    {
        internal const string NAME_FIELD = "create-label-name-field";
        internal const string COMMENT_FIELD = "create-label-comment-field";
        internal const string CHANGESET_ID_LABEL = "create-label-changeset-id-label";
        internal const string CHOOSE_BUTTON = "create-label-choose-button";
        internal const string LABEL_ALL_XLINKS_TOGGLE = "create-label-all-xlinks-toggle";
        internal const string SWITCH_TO_LABEL_TOGGLE = "create-label-switch-to-label-toggle";
        internal const string OK_BUTTON = "create-label-ok-button";
        internal const string CANCEL_BUTTON = "create-label-cancel-button";

        internal string LabelName { get { return mNameField.value; } }
        internal string Comment { get { return mCommentField.value; } }
        internal long ChangesetId { get { return mChangesetId; } }
        internal bool LabelAllXlinks { get { return mLabelAllXlinksToggle.value; } }
        internal bool SwitchToLabel { get { return mSwitchToLabelToggle.value; } }
        internal bool WasConfirmed { get { return mbConfirmed; } }

        internal CreateLabelViewUIToolkit(RepositorySpec repSpec, long changesetId)
        {
            mRepSpec = repSpec;
            mChangesetId = changesetId;

            BuildComponents();

            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            schedule.Execute(() =>
            {
                mNameField.Focus();
                mNameField.SelectAll();
            });
        }

        internal LabelCreationData BuildCreationData()
        {
            mResultData = new LabelCreationData(
                mRepSpec,
                mChangesetId,
                mNameField.value,
                mCommentField.value,
                mSwitchToLabelToggle.value,
                mLabelAllXlinksToggle.value,
                (mResultData != null) ? mResultData.XlinksToLabel : null);

            return mResultData;
        }

        internal void SetChangesetId(long changesetId)
        {
            mChangesetId = changesetId;
            mChangesetIdLabel.text = changesetId.ToString();
        }

        void IPlasticDialogCloser.CloseDialog()
        {
            mbConfirmed = true;
            Confirmed?.Invoke();
        }

        void BuildComponents()
        {
            mNameField = new TextField();
            mNameField.name = NAME_FIELD;

            mCommentField = new TextField();
            mCommentField.name = COMMENT_FIELD;
            mCommentField.multiline = true;
            mCommentField.style.height = COMMENT_HEIGHT;

            mChangesetIdLabel = new Label(mChangesetId.ToString());
            mChangesetIdLabel.name = CHANGESET_ID_LABEL;

            mChooseButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.ChooseMessage.GetString(), OnChooseClicked);
            mChooseButton.name = CHOOSE_BUTTON;

            mLabelAllXlinksToggle = new Toggle(
                PlasticLocalization.Name.LabelAllXlinksCheckButton.GetString());
            mLabelAllXlinksToggle.name = LABEL_ALL_XLINKS_TOGGLE;
            mLabelAllXlinksToggle.style.marginLeft = 0;
            mLabelAllXlinksToggle.focusable = false;

            mSwitchToLabelToggle = new Toggle(
                PlasticLocalization.Name.SwitchToLabelCheckButton.GetString());
            mSwitchToLabelToggle.name = SWITCH_TO_LABEL_TOGGLE;
            mSwitchToLabelToggle.style.marginLeft = 0;
            mSwitchToLabelToggle.focusable = false;

            mOkButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.CreateButton.GetString(), OnOkClicked);
            mOkButton.name = OK_BUTTON;
            mOkButton.style.minWidth = BUTTON_WIDTH;

            mCancelButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.CancelButton.GetString(), OnCancelClicked);
            mCancelButton.name = CANCEL_BUTTON;
            mCancelButton.style.minWidth = BUTTON_WIDTH;

            mProgressControls = new ProgressControlsForDialogs(
                new VisualElement[] { mOkButton, mCancelButton, mChooseButton });

            Add(BuildLabeledRow(
                PlasticLocalization.Name.LabelNameEntry.GetString(), mNameField));
            Add(BuildLabeledRow(
                PlasticLocalization.Name.CommentsEntry.GetString(), mCommentField));
            Add(BuildChangesetRow());
            Add(mLabelAllXlinksToggle);
            Add(mSwitchToLabelToggle);
            Add(BuildButtonsArea());
            Add(mProgressControls);
        }

        VisualElement BuildLabeledRow(string labelText, VisualElement field)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = ROW_SPACING;

            Label label = new Label(labelText);
            label.style.width = LABEL_WIDTH;
            label.style.flexShrink = 0;

            field.style.flexGrow = 1;

            row.Add(label);
            row.Add(field);

            return row;
        }

        VisualElement BuildChangesetRow()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = ROW_SPACING;

            Label label = new Label(
                PlasticLocalization.Name.ChangesetToLabelEntry.GetString());
            label.style.width = LABEL_WIDTH;
            label.style.flexShrink = 0;

            mChangesetIdLabel.style.flexGrow = 1;

            row.Add(label);
            row.Add(mChangesetIdLabel);
            row.Add(mChooseButton);

            return row;
        }

        VisualElement BuildButtonsArea()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.FlexEnd;
            row.style.marginTop = ROW_SPACING;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                row.Add(mOkButton);
                row.Add(mCancelButton);
            }
            else
            {
                row.Add(mCancelButton);
                row.Add(mOkButton);
            }

            return row;
        }

        void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Return ||
                e.keyCode == KeyCode.KeypadEnter)
            {
                if (IsCommentFieldFocused())
                    return;

                OnOkClicked();
                e.StopPropagation();
                return;
            }

            if (e.keyCode == KeyCode.Escape)
            {
                OnCancelClicked();
                e.StopPropagation();
            }
        }

        bool IsCommentFieldFocused()
        {
            if (mCommentField.panel == null)
                return false;

            VisualElement focused =
                mCommentField.panel.focusController.focusedElement as VisualElement;

            return focused != null &&
                (focused == mCommentField || mCommentField.Contains(focused));
        }

        void OnOkClicked()
        {
            LabelCreationValidation.AsyncValidation(
                BuildCreationData(), this, mProgressControls);
        }

        void OnCancelClicked()
        {
            Cancelled?.Invoke();
        }

        void OnChooseClicked()
        {
            ChooseChangesetRequested?.Invoke();
        }

        internal event Action Confirmed;
        internal event Action Cancelled;
        internal event Action ChooseChangesetRequested;

        TextField mNameField;
        TextField mCommentField;
        Label mChangesetIdLabel;
        Button mChooseButton;
        Toggle mLabelAllXlinksToggle;
        Toggle mSwitchToLabelToggle;
        Button mOkButton;
        Button mCancelButton;
        ProgressControlsForDialogs mProgressControls;

        LabelCreationData mResultData;
        long mChangesetId;
        bool mbConfirmed;

        readonly RepositorySpec mRepSpec;

        const float LABEL_WIDTH = 150f;
        const float BUTTON_WIDTH = 80f;
        const float COMMENT_HEIGHT = 100f;
        const float ROW_SPACING = 5f;
    }
}
