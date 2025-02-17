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
        public static Task<Func<Texture2D>> AsyncGetUncompressed(Texture2D firstTexture)
        {
            Func<Texture2D> origTexReturn = () => firstTexture;
            Task<Func<Texture2D>> origTexTask = Task.FromResult(origTexReturn);

            if (!AssetDatabase.Contains(firstTexture)) { return origTexTask; }

            var path = @"\\?\" + Path.GetFullPath(AssetDatabase.GetAssetPath(firstTexture));

            if (Path.GetExtension(path) is not (".png" or ".jpeg" or ".jpg"))
            {
                return origTexTask;
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Default)
            {
                return origTexTask;
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

            return result;
        }
#endif

        public static bool TryGetUnCompress(Texture2D firstTexture, out Texture2D unCompress)
        {
#if SYSTEM_DRAWING
            var task = AsyncGetUncompressed(firstTexture);
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

        public static Texture2D TryGetUnCompress(this Texture2D tex)
        {
            return TryGetUnCompress(tex, out var outUnCompress) ? outUnCompress : tex;
        }
        public static Texture TryGetUnCompress(this Texture tex)
        {
            if (tex is Texture2D texture2D) { return TryGetUnCompress(texture2D, out var outUnCompress) ? outUnCompress : texture2D; }
            else { return tex; }
        }

        /// <summary>
        /// いろいろな設定をコピーしたような感じにする。
        /// ただしリサイズだけは行わない。
        /// 戻り値はクローンになる可能性があるため注意。
        /// ならない場合もあるため注意。
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="copySource"></param>
        /// <returns></returns>
        public static Texture2D CopySetting(this Texture2D tex, Texture2D copySource, bool copyCompress = true, TextureFormat? overrideFormat = null)
        {
            var textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(copySource)) as TextureImporter;
            if ((textureImporter != null) && (textureImporter.textureType == TextureImporterType.NormalMap)) tex = tex.ConvertNormalMap();
            if ((tex.mipmapCount > 1) != (copySource.mipmapCount > 1))
            {
                var newTex = new Texture2D(tex.width, tex.height, tex.format, copySource.mipmapCount > 1);
                var pixelData = tex.GetPixelData<Color32>(0);
                newTex.SetPixelData(pixelData, 0); pixelData.Dispose();
                newTex.name = tex.name;
                newTex.Apply(true);
                tex = newTex;
            }
            tex.CopyFilWrap2D(copySource);

            if (copyCompress && (tex.format != copySource.format))
            {
                var format = overrideFormat.HasValue ? overrideFormat.Value : copySource.format;
                EditorUtility.CompressTexture(tex, format, textureImporter == null ? 50 : textureImporter.compressionQuality);
            }

            return tex;
        }
        /// <summary>
        /// パフォーマンスはあまり良いとは思えないが、新しいインスタンスのTexture2Dを生成する。
        /// </summary>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        public static Texture2D CloneTexture2D(this Texture2D texture2D)
        {
            if (texture2D.isReadable)
            {
                return UnityEngine.Object.Instantiate(texture2D);
            }
            else
            {
                var newRt = TTRt.G(texture2D.width, texture2D.height);
                newRt.name = $"{texture2D.name}:CloneTexture2D-{newRt.width}x{newRt.height}";
                Graphics.Blit(texture2D, newRt);
                var cloneTex = newRt.CopyTexture2D().CopySetting(texture2D);
                TTRt.R(newRt);
                return cloneTex;
            }
        }
        public static Texture2D ConvertNormalMap(this Texture2D tex)
        {
            throw new NotImplementedException();
        }
        public static void NotFIlterAndReadWritTexture2D(ref Texture2D sourceTex)
        {
            var SourceTexPath = AssetDatabase.GetAssetPath(sourceTex);
            var isEmpty = string.IsNullOrEmpty(SourceTexPath);
            if (!isEmpty)
            {
                var textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sourceTex)) as TextureImporter;
                if (textureImporter == null) return;
                if (textureImporter.textureType == TextureImporterType.Default && textureImporter.isReadable) { return; }
            }
            else
            {
                if (sourceTex.isReadable) return;
            }

            byte[] imageBytes = isEmpty ? sourceTex.EncodeToPNG() : File.ReadAllBytes(SourceTexPath);
            var pngTex = new Texture2D(2, 2);
            pngTex.LoadImage(imageBytes);

            var newTex = new Texture2D(pngTex.width, pngTex.height, TextureFormat.RGBA32, false);
            newTex.SetPixels32(pngTex.GetPixels32());
            newTex.Apply();
            sourceTex = newTex;
        }

        public static Vector2Int NativeSize(this Texture2D SourceTex)
        {
            var sourceTexPath = AssetDatabase.GetAssetPath(SourceTex);
            Stream stream;
            bool isJPG = false;
            if (string.IsNullOrEmpty(sourceTexPath) || AssetDatabase.IsSubAsset(SourceTex))
            {
                //メモリに直接乗っかってる場合かunityアセットの場合、Texture2Dが誤った値を返さない。
                return new Vector2Int(SourceTex.width, SourceTex.height);
            }
            else if (Path.GetExtension(sourceTexPath) == ".png")
            {
                stream = File.OpenRead(sourceTexPath);
            }
            else if (Path.GetExtension(sourceTexPath) == ".jpg" || Path.GetExtension(sourceTexPath) == ".jpeg")
            {
                stream = File.OpenRead(sourceTexPath);
                isJPG = true;
            }
            else if (Path.GetExtension(sourceTexPath) == ".asset")
            {
                return new Vector2Int(SourceTex.width, SourceTex.height);
            }
            else
            {
                //非対応形式だった場合を雑に処理。
                return new Vector2Int(SourceTex.width, SourceTex.height);
            }



            return !isJPG ? PNGtoSize(stream) : JPGtoSize(stream);
        }


        public static Vector2Int JPGtoSize(Stream stream)
        {
            var SOI = new byte[2] { 0xFF, 0xD8 };

            var readSOI = new byte[2];
            stream.Read(readSOI, 0, 2);

            if (!readSOI.SequenceEqual(SOI))
            {
                throw new Exception("JPGファイルではありません");
            }

            var SOFFirstByte = 0xFF;
            var SOFSecondBytes = new byte[] {
                     0xC0 ,  0xC1 , 0xC2 , 0xC3 ,
                     0xC5 , 0xC6 , 0xC7 ,
                     0xC9 , 0xCA , 0xCB ,
                     0xCD , 0xCE , 0xCF
                    };
            var SOS = 0xDA;

            bool SOFHit = false;

            while (!SOFHit)
            {
                if ((Byte)stream.ReadByte() == SOFFirstByte)
                {
                    var SecondByte = (Byte)stream.ReadByte();
                    if (SOFSecondBytes.Contains(SecondByte))
                    {
                        SOFHit = true;
                    }
                    else if (SOS == SecondByte)
                    {
                        throw new Exception("JPGデータから画像サイズを取得できませんでした。");
                    }

                }
            }
            var LFP = new byte[3];
            stream.Read(LFP, 0, 3);


            var heightByte = new byte[2];
            var withByte = new byte[2];

            stream.Read(heightByte, 0, 2);
            stream.Read(withByte, 0, 2);

            if (BitConverter.IsLittleEndian)
            {
                heightByte = heightByte.Reverse().ToArray();
                withByte = withByte.Reverse().ToArray();
            }

            var height = BitConverter.ToUInt16(heightByte, 0);
            var with = BitConverter.ToUInt16(withByte, 0);

            return new Vector2Int((int)with, (int)height);
        }

        public static Vector2Int PNGtoSize(Stream stream)
        {
            var PNGHeader = new byte[8] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            var readByte = new byte[8];
            stream.Read(readByte, 0, 8);

            if (!readByte.SequenceEqual(PNGHeader))
            {
                throw new Exception("PNGファイルではありません");
            }

            var IHDRBytes = new byte[4] { 0x49, 0x48, 0x44, 0x52 };

            var IHDRHit = false;
            while (!IHDRHit)
            {
                foreach (var IHDRByte in IHDRBytes)
                {
                    var Byte = (Byte)stream.ReadByte();
                    if (IHDRByte == Byte)
                    {
                        IHDRHit = true;
                    }
                    else
                    {
                        IHDRHit = false;
                        break;
                    }
                }
            }

            var withByte = new byte[4];
            var heightByte = new byte[4];

            stream.Read(withByte, 0, 4);
            stream.Read(heightByte, 0, 4);

            if (BitConverter.IsLittleEndian)
            {
                withByte = withByte.Reverse().ToArray();
                heightByte = heightByte.Reverse().ToArray();
            }

            var with = BitConverter.ToUInt32(withByte, 0);
            var height = BitConverter.ToUInt32(heightByte, 0);

            return new Vector2Int((int)with, (int)height);
        }

    }
}
