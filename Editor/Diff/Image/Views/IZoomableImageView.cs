using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Views
{
    internal interface IZoomableImageView
    {
        Vector2 ImageSize { get; }
        void SetSize(float width, float height);
    }
}
