using System;
using System.Collections.Generic;

using Codice.Client.BaseCommands.Differences;
using Codice.CM.Client.Differences.Graphic;

namespace Unity.PlasticSCM.Editor.Diff
{
    internal static class DiffViewerDataExtensions
    {
        internal static bool IsSupportedImage(string extension)
        {
            return SupportedImageDiffFormats.IsSupportedExtension(
                extension, null);
        }

        internal static bool IsSerializedAsset(string extension)
        {
            return SERIALIZED_EXTENSIONS.Contains(extension);
        }

        internal static bool IsSupportedImage(this DiffViewerData data)
        {
            return IsSupportedImage(data.Extension);
        }

        internal static bool IsSerializedAsset(this DiffViewerData data)
        {
            return SERIALIZED_EXTENSIONS.Contains(data.Extension);
        }

        internal static readonly HashSet<string> SERIALIZED_EXTENSIONS =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".prefab", ".unity", ".asset", ".mat", ".controller", ".anim",
                ".overrideController", ".mask", ".preset",
                ".lighting", ".giparams", ".shadervariants", ".guiskin",
                ".fontsettings", ".physicmaterial", ".physicsmaterial2d",
                ".cubemap", ".flare", ".mixer", ".renderTexture",
                ".spriteatlas", ".terrainlayer", ".signal", ".playable",
                ".scenetemplate", ".meta"
            };
    }
}
