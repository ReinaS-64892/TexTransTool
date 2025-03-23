#nullable enable
#if UNITY_EDITOR_WIN
#define SYSTEM_DRAWING
#endif
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
#if SYSTEM_DRAWING
using System.Drawing;
using System.Drawing.Imaging;
#endif
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using net.rs64.TexTransCoreEngineForUnity;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;
using Graphics = UnityEngine.Graphics;

namespace net.rs64.TexTransTool.Utils
{

    internal static class EditorTextureUtility
    {
#if SYSTEM_DRAWING
        public static (Task<Func<Texture2D>> task, (int x, int y) originalSize) AsyncGetUncompressed(Texture2D firstTexture)
        {
            Func<Texture2D> origTexReturn = () => firstTexture;
            Task<Func<Texture2D>> origTexTask = Task.FromResult(origTexReturn);
            var fallBackSize = (firstTexture.width, firstTexture.height);

            if (!AssetDatabase.Contains(firstTexture)) { return (origTexTask, fallBackSize); }

            var path = @"\\?\" + Path.GetFullPath(AssetDatabase.GetAssetPath(firstTexture));

            if (Path.GetExtension(path) is not (".png" or ".jpeg" or ".jpg"))
            {
                return (origTexTask, fallBackSize);
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Default)
            {
                return (origTexTask, fallBackSize);
            }

            // Start async texture loading
            Profiler.BeginSample("CoGetUncompressed.Sync", firstTexture);
            Bitmap bitmap;
            Texture2D texture;
            NativeArray<UInt32> rawTexData;
            try
            {
                bitmap = new Bitmap(path);
                // Note: Unity and C# use different endianness for Texture2D data (at least on x86_64 windows).
                texture = new Texture2D(bitmap.Width, bitmap.Height, TextureFormat.BGRA32, true, false, true);
                rawTexData = texture.GetRawTextureData<UInt32>();
            }
            finally
            {
                Profiler.EndSample();
            }

            if (rawTexData.Length < bitmap.Width * bitmap.Height)
            {
                throw new Exception($"rawTexData.Length {rawTexData.Length} < bitmap.Width {bitmap.Width} * bitmap.Height {bitmap.Height} * 4 [{texture.width} x {texture.height}]");
            }

            // Run on C# thread pool, not Unity's main thread
            var syncContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            var task = Task.Run(() =>
            {
                RuntimeHelpers.GetHashCode(texture); // force texture to not be GC'd until this task completes

                try
                {
                    unsafe
                    {
                        var data = new BitmapData()
                        {
                            Height = bitmap.Height,
                            Width = bitmap.Width,
                            // Unity and C#'s System.Drawing use a different vertical order for pixels, so set a negative
                            // stride to flip the image vertically.
                            Stride = bitmap.Width * -4,
                            Scan0 = (IntPtr)rawTexData.GetUnsafePtr() + bitmap.Width * (bitmap.Height - 1) * 4,
                            PixelFormat = PixelFormat.Format32bppArgb
                        };

                        Profiler.BeginSample("TryGetUnCompress.LockBits", firstTexture);
                        var locked = bitmap.LockBits(
                            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                            ImageLockMode.ReadOnly | ImageLockMode.UserInputBuffer,
                            PixelFormat.Format32bppArgb,
                            data
                        );
                        bitmap.UnlockBits(locked);
                        Profiler.EndSample();
                    }
                }
                finally
                {
                    bitmap.Dispose();
                }
            });

            var timeout = Task.Delay(10_000);
            var anyCompleted = Task.WhenAny(task, timeout);

            var result = anyCompleted.ContinueWith(_ =>
            {
                if (!task.IsCompleted)
                {
                    Debug.LogError($"Texture loading timed out: {path}");
                    return origTexReturn;
                }

                return () =>
                {
                    Profiler.BeginSample("TryGetUnCompress.Apply", firstTexture);
                    texture.Apply(true);
                    Profiler.EndSample();

                    return texture;
                };
            });

            SynchronizationContext.SetSynchronizationContext(syncContext);

            return (result, (texture.width, texture.height));
        }
#endif

        static bool TryGetUnCompress(Texture2D firstTexture, out Texture2D unCompress)
        {
#if SYSTEM_DRAWING
            var task = AsyncGetUncompressed(firstTexture).task;
            if (!task.Wait(60_000))
            {
                throw new TimeoutException("Texture loading timed out");
            }

            unCompress = task.Result();

            return unCompress != firstTexture;
#else
            if (!AssetDatabase.Contains(firstTexture)) { unCompress = firstTexture; return false; }
            var path = AssetDatabase.GetAssetPath(firstTexture);
            if (Path.GetExtension(path) == ".png" || Path.GetExtension(path) == ".jpeg" || Path.GetExtension(path) == ".jpg")
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureType != TextureImporterType.Default) { unCompress = firstTexture; return false; }
                unCompress = new Texture2D(2, 2);
                unCompress.LoadImage(File.ReadAllBytes(path));
                return true;
            }
            else { unCompress = firstTexture; return false; }
#endif
        }
        public static (bool isLoadableOrigin, bool IsNormalMap) GetOriginInformation(Texture2D tex)
        {
            if (AssetDatabase.Contains(tex) is false) { return (false, false); }
            var path = AssetDatabase.GetAssetPath(tex);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            var isNormalMap = (importer?.textureType ?? TextureImporterType.Default) is TextureImporterType.NormalMap;
            if (Path.GetExtension(path) is not (".png" or ".jpeg" or ".jpg")) { return (false, isNormalMap); }
            return (true, isNormalMap);
        }

        public static Texture2D TryGetUnCompress(this Texture2D tex)
        { return TryGetUnCompress(tex, out Texture2D outUnCompress) ? outUnCompress : tex; }

    }
}
