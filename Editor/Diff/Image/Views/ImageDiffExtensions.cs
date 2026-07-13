using UnityEngine;

namespace Unity.PlasticSCM.Editor.Diff.Texture.Views
{
    internal static class ImageDiffExtensions
    {
        internal static float CalculateZoomToFit(
            float width, float height,
            float targetWidth, float targetHeight)
        {
            if (width <= 0 || height <= 0)
                return 1f;

            return Mathf.Min(targetWidth / width, targetHeight / height);
        }

        internal static bool IsImageBiggerThanFrame(
            Vector2 frameSize, Vector2 imageSize)
        {
            return imageSize.x > frameSize.x || imageSize.y > frameSize.y;
        }
    }
}
