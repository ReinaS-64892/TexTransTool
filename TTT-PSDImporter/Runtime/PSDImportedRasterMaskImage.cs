using System;
using net.rs64.PSDParser;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.PSDParser;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using static net.rs64.TexTransCore.RenderTextureOperator;

namespace net.rs64.TexTransTool.PSDImporter
{
    public class PSDImportedRasterMaskImage : TTTImportedImage
    {
        [SerializeField] public PSDImportedRasterMaskImageData MaskImageData;
        public override void LoadImage<TTCE>(ITTImportedCanvasSource importSource, TTCE ttce, ITTRenderTexture writeTarget)
        {
            var psdBinary = importSource as PSDImportedCanvasDescription.PSDBinaryHolder;
            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;

            var defaultValue = MaskImageData.DefaultValue / 255;
            ttce.ColorFill(writeTarget, new(defaultValue, defaultValue, defaultValue, defaultValue));

            var size = ((uint)MaskImageData.RectTangle.GetWidth(), (uint)MaskImageData.RectTangle.GetHeight());
            var piv = ((uint)PivotT.x, (uint)PivotT.y);

            if (size.Item1 is 0 || size.Item2 is 0) { return; }

            if (psdCanvasDesc.BitDepth is 8 && psdCanvasDesc.IsPSB is false)
            {
                using var ch = ttce.GetComputeHandler(ttce.GenealCompute["Decompress8BitPSDRLE"]);

                PSDImportedRasterImage.DecompressRLE8BitPSDWithTTCE(size, piv, writeTarget, (uint)SwizzlingChannel.A, ch, psdBinary.PSDByteArray.AsSpan((int)MaskImageData.MaskImage.ImageDataAddress.StartAddress, (int)MaskImageData.MaskImage.ImageDataAddress.Length));
                ttce.Swizzling(writeTarget, SwizzlingChannel.A, SwizzlingChannel.A, SwizzlingChannel.A, SwizzlingChannel.A);
            }


        }

        protected override void LoadImage(ITTImportedCanvasSource importSource, Span<byte> writeTarget)
        {
            var psdBinary = importSource as PSDImportedCanvasDescription.PSDBinaryHolder;
            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;

            switch (psdCanvasDesc.ImportedImageFormat)
            {
                default:
                case TexTransCoreTextureFormat.Byte:
                    {
                        writeTarget.Fill(MaskImageData.DefaultValue);
                        break;
                    }
                case TexTransCoreTextureFormat.UShort:
                    {
                        var writeValue = (ushort)(MaskImageData.DefaultValue / (float)byte.MaxValue * ushort.MaxValue);
                        var forByteLen = writeTarget.Length / 4;
                        for (var i = 0; forByteLen > i; i += 1)
                        {
                            BitConverter.TryWriteBytes(writeTarget.Slice(i * 4, 4), writeValue);
                        }
                        break;
                    }
                case TexTransCoreTextureFormat.Float:
                    {
                        var writeValue = MaskImageData.DefaultValue / (float)byte.MaxValue;
                        var forByteLen = writeTarget.Length / 4;
                        for (var i = 0; forByteLen > i; i += 1)
                        {
                            BitConverter.TryWriteBytes(writeTarget.Slice(i * 4, 4), writeValue);
                        }
                        break;
                    }
            }

            using var buffer = new NativeArray<byte>((int)MaskImageData.MaskImage.ImageDataAddress.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            using var data = new NativeArray<byte>(ChannelImageDataParser.ChannelImageData.GetImageByteCount(MaskImageData.RectTangle, psdCanvasDesc.BitDepth), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            MaskImageData.MaskImage.GetImageData(psdBinary.PSDByteArray, MaskImageData.RectTangle, buffer, data);


            var ppB = EnginUtil.GetPixelParByte(psdCanvasDesc.ImportedImageFormat, TexTransCoreTextureChannel.R);

            var pivot = PivotT;
            for (var y = 0; MaskImageData.RectTangle.GetHeight() > y; y += 1)
            {
                for (var x = 0; MaskImageData.RectTangle.GetWidth() > x; x += 1)
                {
                    var flipY = MaskImageData.RectTangle.GetHeight() - 1 - y;

                    var writeX = pivot.x + x;
                    var writeY = pivot.y + flipY;

                    var readIndex = (x + y * MaskImageData.RectTangle.GetWidth()) * ppB;
                    var writeIndex = (writeX + writeY * psdCanvasDesc.Width) * ppB * 4;

                    if (writeX < 0 || writeX >= psdCanvasDesc.Width) { continue; }
                    if (writeY < 0 || writeY >= psdCanvasDesc.Height) { continue; }
                    switch (ppB)
                    {
                        default:
                        case 1:
                            {
                                writeTarget[writeIndex + 0] = data[readIndex];
                                writeTarget[writeIndex + 1] = data[readIndex];
                                writeTarget[writeIndex + 2] = data[readIndex];
                                writeTarget[writeIndex + 3] = data[readIndex];
                                break;
                            }
                        case 4:
                        case 8:
                            {
                                var sourceSpan = data.AsSpan().Slice(readIndex, ppB);
                                sourceSpan.CopyTo(writeTarget.Slice(writeIndex + 0 * ppB, ppB));
                                sourceSpan.CopyTo(writeTarget.Slice(writeIndex + 1 * ppB, ppB));
                                sourceSpan.CopyTo(writeTarget.Slice(writeIndex + 2 * ppB, ppB));
                                sourceSpan.CopyTo(writeTarget.Slice(writeIndex + 3 * ppB, ppB));
                                break;
                            }
                    }


                }
            }

        }

        protected (int x, int y) PivotT => (MaskImageData.RectTangle.Left, CanvasDescription.Height - MaskImageData.RectTangle.Bottom);
        protected Vector2Int Pivot => new Vector2Int(MaskImageData.RectTangle.Left, CanvasDescription.Height - MaskImageData.RectTangle.Bottom);

        protected JobResult<NativeArray<Color32>> LoadImage(byte[] importSource, NativeArray<Color32>? writeTarget = null)
        {
            Profiler.BeginSample("Init");
            var native2DArray = writeTarget ?? new NativeArray<Color32>(CanvasDescription.Width * CanvasDescription.Height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            Unsafe.UnsafeNativeArrayUtility.ClearMemoryOnColor(native2DArray, MaskImageData.DefaultValue);

            var canvasSize = new int2(CanvasDescription.Width, CanvasDescription.Height);
            var sourceTexSize = new int2(MaskImageData.RectTangle.GetWidth(), MaskImageData.RectTangle.GetHeight());

            Profiler.EndSample();

            JobHandle offsetJobHandle;
            if ((MaskImageData.RectTangle.GetWidth() * MaskImageData.RectTangle.GetHeight()) == 0) { return new(native2DArray); }

            Profiler.BeginSample("RLE");

            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;
            var data = new NativeArray<byte>(ChannelImageDataParser.ChannelImageData.GetImageByteCount(MaskImageData.RectTangle, psdCanvasDesc.BitDepth), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var buffer = new NativeArray<byte>((int)MaskImageData.MaskImage.ImageDataAddress.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            MaskImageData.MaskImage.GetImageData(importSource, MaskImageData.RectTangle, buffer, data);

            Profiler.EndSample();
            Profiler.BeginSample("OffsetMoveAlphaJobSetUp");

            var offset = new PSDImportedRasterImage.OffsetMoveAlphaJob()
            {
                Target = native2DArray,
                R = data,
                G = data,
                B = data,
                A = data,
                Offset = new int2(Pivot.x, Pivot.y),
                SourceSize = sourceTexSize,
                TargetSize = canvasSize,
            };
            offsetJobHandle = offset.Schedule(data.Length, 64);

            Profiler.EndSample();
            return new(native2DArray, offsetJobHandle, () => { data.Dispose(); });
        }
        protected void LoadImage(byte[] importSource, RenderTexture WriteTarget)
        {
            var isZeroSize = (MaskImageData.RectTangle.GetWidth() * MaskImageData.RectTangle.GetHeight()) == 0;
            if (PSDImportedRasterImage.s_tempMat == null) { PSDImportedRasterImage.s_tempMat = new Material(PSDImportedRasterImage.MergeColorAndOffsetShader); }
            var mat = PSDImportedRasterImage.s_tempMat;

            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;
            var format = PSDImportedRasterImage.BitDepthToTextureFormat(psdCanvasDesc.BitDepth);

            var texR = new Texture2D(MaskImageData.RectTangle.GetWidth(), MaskImageData.RectTangle.GetHeight(), GraphicsFormat.R8_UNorm, TextureCreationFlags.None);
            texR.filterMode = FilterMode.Point;

            TextureBlend.FillColor(WriteTarget, new Color32(MaskImageData.DefaultValue, MaskImageData.DefaultValue, MaskImageData.DefaultValue, MaskImageData.DefaultValue));

            if (!isZeroSize)
            {
                using (var data = new NativeArray<byte>(ChannelImageDataParser.ChannelImageData.GetImageByteCount(MaskImageData.RectTangle, psdCanvasDesc.BitDepth), Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
                using (var buffer = new NativeArray<byte>((int)MaskImageData.MaskImage.ImageDataAddress.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
                {
                    MaskImageData.MaskImage.GetImageData(importSource, MaskImageData.RectTangle, buffer, data);
                    texR.LoadRawTextureData(data); texR.Apply();
                }

                mat.SetTexture("_RTex", texR);
                mat.SetTexture("_GTex", texR);
                mat.SetTexture("_BTex", texR);
                mat.SetTexture("_ATex", texR);

                mat.SetVector("_Offset", new Vector4(Pivot.x / (float)CanvasDescription.Width, Pivot.y / (float)CanvasDescription.Height, MaskImageData.RectTangle.GetWidth() / (float)CanvasDescription.Width, MaskImageData.RectTangle.GetHeight() / (float)CanvasDescription.Height));
                Graphics.Blit(null, WriteTarget, mat);
            }

            UnityEngine.Object.DestroyImmediate(texR);
        }
    }
}

