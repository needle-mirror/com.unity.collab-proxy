using System;
using System.IO;
using System.Reflection;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Unity.PlasticSCM.Editor.Diff.Texture
{
    internal static class TextureLoader
    {
        internal static Texture2D LoadFromFile(string filePath)
        {
            Texture2D texture = TryLoadWithImageConversion(filePath);

            if (texture != null)
                return texture;

            return LoadWithUnityDecoder(filePath);
        }

        static Texture2D TryLoadWithImageConversion(string filePath)
        {
            MethodInfo method = GetLoadImageDataAtPathMethod();

            if (method == null)
                return null;

            object[] args = new object[]
            {
                filePath, 0, 0, 0, GraphicsFormat.None
            };

            NativeArray<byte> pixelData = (NativeArray<byte>)method.Invoke(null, args);

            try
            {
                int width = (int)args[1];
                int height = (int)args[2];
                int rowBytes = (int)args[3];
                GraphicsFormat format = (GraphicsFormat)args[4];

                if (width <= 0 || height <= 0)
                    return null;

                Texture2D texture = new Texture2D(
                    width, height, format, TextureCreationFlags.None);
                texture.filterMode = FilterMode.Bilinear;
                texture.hideFlags = HideFlags.HideAndDontSave;

                int bytesPerPixel = rowBytes / width;
                int tightRowBytes = width * bytesPerPixel;

                if (rowBytes == tightRowBytes)
                {
                    texture.SetPixelData(pixelData, 0);
                }
                else
                {
                    NativeArray<byte> tightData = new NativeArray<byte>(
                        tightRowBytes * height, Allocator.Temp);

                    for (int y = 0; y < height; y++)
                    {
                        NativeArray<byte>.Copy(
                            pixelData, y * rowBytes,
                            tightData, y * tightRowBytes,
                            tightRowBytes);
                    }

                    texture.SetPixelData(tightData, 0);
                    tightData.Dispose();
                }

                texture.Apply();
                return texture;
            }
            finally
            {
                pixelData.Dispose();
            }
        }

        static Texture2D LoadWithUnityDecoder(string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;

            if (!texture.LoadImage(bytes))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw new InvalidOperationException(
                    "Failed to load image. The format may not be supported.");
            }

            return texture;
        }

        static MethodInfo GetLoadImageDataAtPathMethod()
        {
            if (mbIsMethodCached)
                return mCachedMethod;

            // we cannot access the internal ImageConversion.LoadImageDataAtPath
            // in the UnityEngine.ImageConversionModule assembly,
            // so we use reflection to try to get it
            mCachedMethod = typeof(ImageConversion).GetMethod(
                "LoadImageDataAtPath",
                BindingFlags.Static | BindingFlags.NonPublic);

            mbIsMethodCached = true;
            return mCachedMethod;
        }

        static MethodInfo mCachedMethod;
        static bool mbIsMethodCached;
    }
}
