using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.PlasticSCM.Editor.Diff.Texture
{
    internal static class DrawTexture
    {
        internal static void WithColorMask(
            Rect position,
            UnityEngine.Texture image,
            ScaleMode scaleMode,
            ColorWriteMask colorWriteMask)
        {
            UnityEditor.EditorGUI.DrawTextureTransparent(
                position,
                image,
                scaleMode,
                0,
                -1,
                colorWriteMask | ColorWriteMask.Alpha);
        }

        internal static void DrawWithColorMaskAndOpacity(
            Rect rect,
            UnityEngine.Texture texture,
            ScaleMode scaleMode,
            ColorWriteMask mode,
            float opacity)
        {
            Color prevColor = GUI.color;

            try
            {
                Color tint = ToTintColor(mode);
                GUI.color = new Color(tint.r, tint.g, tint.b, opacity);
                GUI.DrawTexture(rect, texture, scaleMode);
            }
            finally
            {
                GUI.color = prevColor;
            }
        }

        static Color ToTintColor(ColorWriteMask mode)
        {
            float r = (mode & ColorWriteMask.Red) != 0 ? 1f : 0f;
            float g = (mode & ColorWriteMask.Green) != 0 ? 1f : 0f;
            float b = (mode & ColorWriteMask.Blue) != 0 ? 1f : 0f;
            return new Color(r, g, b, 1f);
        }
    }
}
