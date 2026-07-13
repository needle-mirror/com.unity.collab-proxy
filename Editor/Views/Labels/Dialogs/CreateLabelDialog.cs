using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.QueryViews.Labels;
using Unity.PlasticSCM.Editor.UI.UIElements;
using Unity.PlasticSCM.Editor.Views.Changesets;

using ImguiProgressControls = Unity.PlasticSCM.Editor.UI.Progress.ProgressControlsForDialogs;

namespace Unity.PlasticSCM.Editor.Views.Labels.Dialogs
{
    internal class CreateLabelDialog : EditorWindow
    {
        internal const string CHANGESET_SELECT_BUTTON = "create-label-changeset-select-button";
        internal const string CHANGESET_BACK_BUTTON = "create-label-changeset-back-button";

        internal static LabelCreationData CreateLabel(
            EditorWindow parentWindow,
            WorkspaceInfo wkInfo)
        {
            RepositorySpec repSpec = PlasticGui.Plastic.API.GetRepositorySpec(wkInfo);
            long currentChangesetId =
                PlasticGui.Plastic.API.GetCurrentChangesetOnWorkspace(wkInfo);

            CreateLabelDialog dialog = CreateInstance<CreateLabelDialog>();
            dialog.mWkInfo = wkInfo;
            dialog.mRepSpec = repSpec;
            dialog.mInitialChangesetId = currentChangesetId;
            dialog.titleContent = new GUIContent(
                PlasticLocalization.Name.CreateLabelTitle.GetString());
            dialog.SetFixedSize(FORM_HEIGHT);
            dialog.CenterOnMainWindow(FORM_HEIGHT);

            dialog.ShowModalUtility();

            return dialog.mResult ?? new LabelCreationData();
        }

        internal void OkButtonAction()
        {
            ChangesetInfo changesetInfo =
                ChangesetsSelection.GetSelectedChangeset(mChangesetExplorerView.Table);

            if (changesetInfo == null)
                return;

            mCreateLabelView.SetChangesetId(changesetInfo.ChangesetId);

            ShowForm();
        }

        void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = MARGIN;
            root.style.paddingBottom = MARGIN;
            root.style.paddingLeft = MARGIN;
            root.style.paddingRight = MARGIN;

            mExplanationLabel = new Label(
                PlasticLocalization.Name.CreateLabelExplanation.GetString());
            mExplanationLabel.style.marginBottom = MARGIN;
            mExplanationLabel.style.whiteSpace = WhiteSpace.Normal;
            root.Add(mExplanationLabel);

            mCreateLabelView = new CreateLabelViewUIToolkit(mRepSpec, mInitialChangesetId);
            mCreateLabelView.Confirmed += OnFormConfirmed;
            mCreateLabelView.Cancelled += OnFormCancelled;
            mCreateLabelView.ChooseChangesetRequested += ShowChangesetExplorer;
            root.Add(mCreateLabelView);

            mExplorerProgressControls = new ImguiProgressControls();
            mChangesetExplorerView = new ChangesetExplorerView(
                this, mWkInfo, mExplorerProgressControls);

            mExplorerContainer = BuildExplorerContainer();
            mExplorerContainer.style.display = DisplayStyle.None;
            root.Add(mExplorerContainer);
        }

        VisualElement BuildExplorerContainer()
        {
            VisualElement container = new VisualElement();
            container.style.flexGrow = 1;

            Label explanation = new Label(
                PlasticLocalization.Name.SelectChangesetBelow.GetString());
            explanation.style.marginBottom = MARGIN;
            explanation.style.whiteSpace = WhiteSpace.Normal;

            IMGUIContainer explorer = new IMGUIContainer(OnExplorerGUI);
            explorer.style.flexGrow = 1;

            VisualElement buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.justifyContent = Justify.FlexEnd;
            buttons.style.marginTop = ROW_SPACING;

            Button selectButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.OkButton.GetString(), OkButtonAction);
            selectButton.name = CHANGESET_SELECT_BUTTON;
            selectButton.style.minWidth = BUTTON_WIDTH;

            Button backButton = ControlBuilder.Button.CreateButton(
                PlasticLocalization.Name.BackButton.GetString(), ShowForm);
            backButton.name = CHANGESET_BACK_BUTTON;
            backButton.style.minWidth = BUTTON_WIDTH;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                buttons.Add(selectButton);
                buttons.Add(backButton);
            }
            else
            {
                buttons.Add(backButton);
                buttons.Add(selectButton);
            }

            container.Add(explanation);
            container.Add(explorer);
            container.Add(buttons);

            container.RegisterCallback<KeyDownEvent>(
                OnExplorerKeyDown, TrickleDown.TrickleDown);

            return container;
        }

        void OnExplorerKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Return ||
                e.keyCode == KeyCode.KeypadEnter)
            {
                OkButtonAction();
                e.StopPropagation();
                return;
            }

            if (e.keyCode == KeyCode.Escape)
            {
                ShowForm();
                e.StopPropagation();
            }
        }

        void OnExplorerGUI()
        {
            mChangesetExplorerView.OnGUI();
            mExplorerProgressControls.UpdateProgress(this);
        }

        void ShowChangesetExplorer()
        {
            mExplanationLabel.style.display = DisplayStyle.None;
            mCreateLabelView.style.display = DisplayStyle.None;
            mExplorerContainer.style.display = DisplayStyle.Flex;

            SetFixedSize(EXPLORER_HEIGHT);

            titleContent.text = PlasticLocalization.Name.AvailableChangesets.GetString();
        }

        void SetFixedSize(float height)
        {
            Vector2 size = new Vector2(DIALOG_WIDTH, height);
            minSize = size;
            maxSize = size;
        }

        void CenterOnMainWindow(float height)
        {
            Rect mainWindow = EditorGUIUtility.GetMainWindowPosition();
            position = new Rect(
                mainWindow.x + (mainWindow.width - DIALOG_WIDTH) / 2f,
                mainWindow.y + (mainWindow.height - height) / 2f,
                DIALOG_WIDTH,
                height);
        }

        void ShowForm()
        {
            mExplorerContainer.style.display = DisplayStyle.None;
            mExplanationLabel.style.display = DisplayStyle.Flex;
            mCreateLabelView.style.display = DisplayStyle.Flex;

            SetFixedSize(FORM_HEIGHT);

            titleContent.text = PlasticLocalization.Name.CreateLabelTitle.GetString();
        }

        void OnFormConfirmed()
        {
            mResult = mCreateLabelView.BuildCreationData();
            mResult.Result = true;

            Close();
        }

        void OnFormCancelled()
        {
            Close();
        }

        CreateLabelViewUIToolkit mCreateLabelView;
        ChangesetExplorerView mChangesetExplorerView;
        ImguiProgressControls mExplorerProgressControls;
        VisualElement mExplorerContainer;
        Label mExplanationLabel;

        LabelCreationData mResult;
        WorkspaceInfo mWkInfo;
        RepositorySpec mRepSpec;
        long mInitialChangesetId;

        const float DIALOG_WIDTH = 710f;
        const float FORM_HEIGHT = 340f;
        const float EXPLORER_HEIGHT = 480f;
        const float MARGIN = 12f;
        const float ROW_SPACING = 5f;
        const float BUTTON_WIDTH = 80f;
    }
}
