using Unity.PlasticSCM.Editor.Diff.Texture.Views;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.UIElements;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Toolbar
{
    internal class ZoomOptionsView : VisualElement
    {
        internal ZoomOptionsView(IImageDiffView imageDiffView)
        {
            mImageDiffView = imageDiffView;

            CreateGUI();
        }

        internal void EnableButtons()
        {
            SetEnabled(true);
        }

        internal void DisableButtons()
        {
            SetEnabled(false);
        }

        internal void Dispose()
        {
            mZoomInButton.clicked -= OnZoomInClicked;
            mZoomOutButton.clicked -= OnZoomOutClicked;
            mZoomToFitButton.clicked -= OnZoomToFitClicked;
            mZoomOriginalSizeButton.clicked -= OnZoomOriginalSizeClicked;
        }

        void CreateGUI()
        {
            style.position = Position.Absolute;
            style.right = 20;
            style.bottom = 20;

            mZoomToFitButton = ControlBuilder.Button.CreateImageButton(
                Images.GetZoomToFitIcon(), "Zoom to Fit", OnZoomToFitClicked);
            mZoomToFitButton.style.marginRight = 0;
            mZoomToFitButton.style.width = BUTTON_SIZE;
            mZoomToFitButton.style.height = BUTTON_SIZE;
            Add(mZoomToFitButton);

            mZoomOriginalSizeButton = ControlBuilder.Button.CreateImageButton(
                Images.GetOriginalSizeIcon(), "Original Size", OnZoomOriginalSizeClicked);
            mZoomOriginalSizeButton.style.marginRight = 0;
            mZoomOriginalSizeButton.style.width = BUTTON_SIZE;
            mZoomOriginalSizeButton.style.height = BUTTON_SIZE;
            Add(mZoomOriginalSizeButton);

            mZoomInButton = ControlBuilder.ButtonGroup.CreateImageTopButton(
                Images.GetZoomInIcon(), "Zoom In", OnZoomInClicked);
            mZoomInButton.style.marginRight = 0;
            mZoomInButton.style.width = BUTTON_SIZE;
            mZoomInButton.style.height = BUTTON_SIZE;
            Add(mZoomInButton);

            mZoomOutButton = ControlBuilder.ButtonGroup.CreateImageBottomButton(
                Images.GetZoomOutIcon(), "Zoom Out", OnZoomOutClicked);
            mZoomOutButton.style.width = BUTTON_SIZE;
            mZoomOutButton.style.height = BUTTON_SIZE;
            mZoomOutButton.style.marginRight = 0;
            mZoomOutButton.style.marginBottom = 0;
            Add(mZoomOutButton);
        }

        void OnZoomInClicked()
        {
            mImageDiffView.ZoomIn();
        }

        void OnZoomOutClicked()
        {
            mImageDiffView.ZoomOut();
        }

        void OnZoomToFitClicked()
        {
            mImageDiffView.ZoomToFit();
        }

        void OnZoomOriginalSizeClicked()
        {
            mImageDiffView.ZoomOneToOne();
        }

        Button mZoomInButton;
        Button mZoomOutButton;
        Button mZoomOriginalSizeButton;
        Button mZoomToFitButton;

        readonly IImageDiffView mImageDiffView;

        const float BUTTON_SIZE = 30;
    }
}
