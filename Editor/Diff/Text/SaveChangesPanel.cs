using MergetoolGui;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEngine.UIElements;
using XDiffGui;

namespace Unity.PlasticSCM.Editor.Diff.Text
{
    internal class SaveChangesPanel : VisualElement
    {
        internal SaveChangesPanel(ISaveChangesListener listener)
        {
            mListener = listener;

            BuildComponents();

            style.display = DisplayStyle.None;
        }

        internal void Dispose()
        {
            mSaveButton.clicked -= OnSaveButtonClicked;
            mDiscardButton.clicked -= OnDiscardButtonClicked;
        }

        void OnSaveButtonClicked()
        {
            mListener.OnSaveChanges();
        }

        void OnDiscardButtonClicked()
        {
            mListener.OnDiscardChanges();
        }

        void BuildComponents()
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            mSaveButton = ControlBuilder.Toolbar.CreateButtonLeft(
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.SaveButton),
                OnSaveButtonClicked);
            mSaveButton.tooltip = MergetoolLocalization.GetString(
                MergetoolLocalization.Name.SaveButtonTooltipWithCustomShortcut,
                GetPlasticShortcut.DisplayString(GetPlasticShortcut.ForSave()));

            mDiscardButton = ControlBuilder.Toolbar.CreateButtonLeft(
                MergetoolLocalization.GetString(
                    MergetoolLocalization.Name.DiscardButton),
                OnDiscardButtonClicked);
            mDiscardButton.tooltip = MergetoolLocalization.GetString(
                MergetoolLocalization.Name.DiscardButtonTooltip);

            Add(mSaveButton);
            Add(mDiscardButton);
        }

        Button mSaveButton;
        Button mDiscardButton;

        readonly ISaveChangesListener mListener;
    }
}
