using System;
using net.rs64.ParserUtility;
using net.rs64.PSDParser;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.PSDParser;
using Unity.Collections;
using UnityEngine;
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

            (uint x, uint y) size = ((uint)MaskImageData.RectTangle.GetWidth(), (uint)MaskImageData.RectTangle.GetHeight());
            var piv = ((uint)PivotT.x, (uint)PivotT.y);

            if (size.Item1 is 0 || size.Item2 is 0) { return; }

            if (psdCanvasDesc.BitDepth is 8 && psdCanvasDesc.IsPSB is false)
            {
                using var ch = ttce.GetComputeHandler(ttce.GetExKeyQuery<IQuayGeneraleComputeKey>().GenealCompute["Decompress8BitPSDRLE"]);

                int heightDataLen = (int)(size.y * 2);
                var mRLEBytes = PSDImportedRasterImage.GetSliceSafe(psdBinary.PSDByteArray, (int)MaskImageData.MaskImage.ImageDataAddress.StartAddress, PSDImportedRasterImage.HeightAndMultipleOf4(heightDataLen, (int)MaskImageData.MaskImage.ImageDataAddress.Length));
                PSDImportedRasterImage.DecompressRLE8BitPSDWithTTCE(ttce, size, piv, writeTarget, (uint)SwizzlingChannel.A, ch, mRLEBytes);
                ttce.Swizzling(writeTarget, SwizzlingChannel.A, SwizzlingChannel.A, SwizzlingChannel.A, SwizzlingChannel.A);
            }
            else
            {
                Profiler.BeginSample("BaseFallBack");
                base.LoadImage(importSource, ttce, writeTarget);
                Profiler.EndSample();
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
                        var forByteLen = writeTarget.Length / 2;
                        for (var i = 0; forByteLen > i; i += 1)
                        {
                            BitConverter.TryWriteBytes(writeTarget.Slice(i * 2, 2), writeValue);
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
            using var data = new NativeArray<byte>(ChannelImageDataParser.GetImageByteCount(MaskImageData.RectTangle, psdCanvasDesc.BitDepth), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            psdBinary.PSDByteArray.LongCopyTo(MaskImageData.MaskImage.ImageDataAddress.StartAddress, buffer.AsSpan());
            MaskImageData.MaskImage.GetImageData((psdCanvasDesc.BitDepth, psdCanvasDesc.IsPSB), buffer, MaskImageData.RectTangle, data);


            var ppB = EnginUtil.GetPixelParByte(psdCanvasDesc.ImportedImageFormat, TexTransCoreTextureChannel.R);
            var width = MaskImageData.RectTangle.GetWidth();
            var height = MaskImageData.RectTangle.GetHeight();

            var pivot = PivotT;
            for (var y = 0; height > y; y += 1)
            {
                for (var x = 0; width > x; x += 1)
                {
                    var flipY = height - 1 - y;

                    var writeX = pivot.x + x;
                    var writeY = pivot.y + flipY;

                    var readIndex = (y * width + x) * ppB;
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
    }
}

