using Unity.PlasticSCM.Editor.Diff.Texture;
using Unity.PlasticSCM.Editor.Diff.Texture.Views.Differences;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.Diff.Texture
{
    internal class ImageViewPanel : VisualElement
    {
        internal ImageViewPanel()
        {
            style.flexGrow = 1;

            mDifferencesView = new DifferencesView();
            Add(mDifferencesView);
        }

        internal void ShowImage(string file)
        {
            DestroyLoadedTexture();

            mLoadedTexture = TextureLoader.LoadFromFile(file);

            mDifferencesView.SetImage(mLoadedTexture);
            mDifferencesView.EnableZoomButtons();
        }

        internal void ClearImage()
        {
            mDifferencesView.CleanImage();
            mDifferencesView.DisableZoomButtons();
        }

        internal void SetChannelMode(ColorWriteMask mode)
        {
            mDifferencesView.SetChannelMode(mode);
        }

        internal void Dispose()
        {
            DestroyLoadedTexture();
            mDifferencesView.Dispose();
        }

        void DestroyLoadedTexture()
        {
            if (mLoadedTexture == null)
                return;

            UnityEngine.Object.DestroyImmediate(
                mLoadedTexture);
            mLoadedTexture = null;
        }

        DifferencesView mDifferencesView;
        UnityEngine.Texture mLoadedTexture;
    }
}

