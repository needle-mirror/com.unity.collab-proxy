using Codice.CM.Client.Differences;

namespace Unity.PlasticSCM.Editor.Diff
{
    internal class UnityBigFileChecker : IBigFileChecker
    {
        bool IBigFileChecker.IsBigFile(string extension, long size)
        {
            if (DiffViewerDataExtensions.IsSerializedAsset(extension))
                return size > BIG_SERIALIZED_ASSET_SIZE;

            if (DiffViewerDataExtensions.IsSupportedImage(extension))
                return size > BIG_TEXTURE_SIZE;

            return size > BIG_FILE_DEFAULT_SIZE;
        }

        const long BIG_SERIALIZED_ASSET_SIZE = 10 * 1024 * 1024; // 10Mb
        const long BIG_TEXTURE_SIZE = 20 * 1024 * 1024; // 20Mb
        const long BIG_FILE_DEFAULT_SIZE = 2 * 1024 * 1024; // 2Mb
    }
}
