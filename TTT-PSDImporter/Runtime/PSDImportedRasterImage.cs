using System;
using System.Buffers.Binary;
using System.Linq;
using System.Threading.Tasks;
using net.rs64.ParserUtility;
using net.rs64.PSDParser;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.PSDParser;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.PSDImporter
{
    public class PSDImportedRasterImage : TTTImportedImage
    {
        [SerializeField] public PSDImportedRasterImageData RasterImageData;
        public override void LoadImage<TTCE>(ITTImportedCanvasSource importSource, TTCE ttce, ITTRenderTexture writeTarget)
        {
            Profiler.BeginSample("PSDImportedRasterImage-LoadImage");
            var psdBinary = importSource as PSDImportedCanvasDescription.PSDBinaryHolder;
            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;

            var size = ((uint)RasterImageData.RectTangle.GetWidth(), (uint)RasterImageData.RectTangle.GetHeight());
            var piv = ((uint)PivotT.x, (uint)PivotT.y);

            if (size.Item1 is 0 || size.Item2 is 0) { return; }

            var isAll8BitRLE = RasterImageData.B.Compression == ChannelImageDataParser.ChannelImageData.CompressionEnum.RLECompressed
                && RasterImageData.R.Compression == ChannelImageDataParser.ChannelImageData.CompressionEnum.RLECompressed
                && RasterImageData.G.Compression == ChannelImageDataParser.ChannelImageData.CompressionEnum.RLECompressed
                && (RasterImageData.A.ImageDataAddress.Length != 0 ? RasterImageData.A.Compression == ChannelImageDataParser.ChannelImageData.CompressionEnum.RLECompressed : true);

            if (psdCanvasDesc.BitDepth is 8 && psdCanvasDesc.IsPSB is false && isAll8BitRLE)
            {
                Profiler.BeginSample("RLE");
                using var ch = ttce.GetComputeHandler(ttce.GenealCompute["Decompress8BitPSDRLE"]);

                // コピーを避けるために 4の倍数に切り上げます、まぁ問題にはならないでしょうけど...ファイルの終端にぶち当たった場合は頭を抱え、コピーが走るようにします...
                DecompressRLE8BitPSDWithTTCE(ttce, size, piv, writeTarget, 0, ch, psdBinary.PSDByteArray.AsSpan((int)RasterImageData.R.ImageDataAddress.StartAddress, TTMath.NormalizeOf4Multiple((int)RasterImageData.R.ImageDataAddress.Length)));
                DecompressRLE8BitPSDWithTTCE(ttce, size, piv, writeTarget, 1, ch, psdBinary.PSDByteArray.AsSpan((int)RasterImageData.G.ImageDataAddress.StartAddress, TTMath.NormalizeOf4Multiple((int)RasterImageData.G.ImageDataAddress.Length)));
                DecompressRLE8BitPSDWithTTCE(ttce, size, piv, writeTarget, 2, ch, psdBinary.PSDByteArray.AsSpan((int)RasterImageData.B.ImageDataAddress.StartAddress, TTMath.NormalizeOf4Multiple((int)RasterImageData.B.ImageDataAddress.Length)));
                if (RasterImageData.A.ImageDataAddress.Length != 0) DecompressRLE8BitPSDWithTTCE(ttce, size, piv, writeTarget, 3, ch, psdBinary.PSDByteArray.AsSpan((int)RasterImageData.A.ImageDataAddress.StartAddress, TTMath.NormalizeOf4Multiple((int)RasterImageData.A.ImageDataAddress.Length)));
                else ttce.AlphaFill(writeTarget, 1f);
                Profiler.EndSample();
            }
            else
            {
                Profiler.BeginSample("BaseFallBack");
                base.LoadImage(importSource, ttce, writeTarget);
                Profiler.EndSample();
            }

            if (psdCanvasDesc.BitDepth is 32)
            {
                // float の場合 リニア空間で保存されてるっぽい ... 本当にこの認識で正しいのだろうか ... ?
                ttce.LinearToGamma(writeTarget);
            }

            Profiler.EndSample();
        }

        public static void DecompressRLE8BitPSDWithTTCE<TTCE>(TTCE engine, (uint x, uint y) size, (uint x, uint y) pivot, ITTRenderTexture writeTarget, uint channel, ITTComputeHandler computeHandler, Span<byte> rleSource)
        where TTCE : ITexTransDriveStorageBufferHolder
        {
            var gvID = computeHandler.NameToID("gv");

            var spanBufferID = computeHandler.NameToID("SpanBuffer");
            var rleBufferID = computeHandler.NameToID("RLEBuffer");
            var texID = computeHandler.NameToID("Tex");

            var heightDataLen = (int)(size.y * 2);

            var hSpan = rleSource[..heightDataLen];
            var dSpan = rleSource[heightDataLen..];

            Span<uint> gvBuf = stackalloc uint[6];
            gvBuf[0] = channel;
            gvBuf[1] = 0; // p1

            (gvBuf[2], gvBuf[3]) = size;
            (gvBuf[4], gvBuf[5]) = pivot;

            Span<uint> spanBuf = heightDataLen > 1024 ? stackalloc uint[heightDataLen] : new uint[heightDataLen];

            uint offset = 0;
            for (var y = 0; size.y > y; y += 1)
            {
                var i = y * 2;
                var count = BinaryPrimitives.ReadUInt16BigEndian(hSpan.Slice(i, 2));

                spanBuf[i] = offset;
                spanBuf[i + 1] = count;

                offset += count;
            }

            computeHandler.UploadConstantsBuffer<uint>(gvID, gvBuf);
            engine.SetStorageBufferFromUpload<TTCE, uint>(computeHandler, spanBufferID, spanBuf);
            engine.SetStorageBufferFromUpload<TTCE, byte>(computeHandler, rleBufferID, dSpan);

            computeHandler.SetTexture(texID, writeTarget);

            computeHandler.Dispatch(1, (size.y + 255) / 256, 1);
        }
        protected override void LoadImage(ITTImportedCanvasSource importSource, Span<byte> writeTarget)
        {
            var psdBinary = importSource as PSDImportedCanvasDescription.PSDBinaryHolder;
            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;

            var containsAlpha = RasterImageData.A.ImageDataAddress.Length != 0;

            Task<NativeArray<byte>>[] getImageTask = new Task<NativeArray<byte>>[4];
            getImageTask[0] = Task.Run(() => LoadToNativeArray(RasterImageData.R, psdBinary.PSDByteArray));
            getImageTask[1] = Task.Run(() => LoadToNativeArray(RasterImageData.G, psdBinary.PSDByteArray));
            getImageTask[2] = Task.Run(() => LoadToNativeArray(RasterImageData.B, psdBinary.PSDByteArray));
            if (containsAlpha) { getImageTask[3] = Task.Run(() => LoadToNativeArray(RasterImageData.A, psdBinary.PSDByteArray)); }
            var image = WeightTask(getImageTask).Result;
            var height = RasterImageData.RectTangle.GetHeight();
            var width = RasterImageData.RectTangle.GetWidth();
            try
            {
                var ppB = EnginUtil.GetPixelParByte(psdCanvasDesc.ImportedImageFormat, TexTransCoreTextureChannel.R);

                var pivot = PivotT;
                for (var y = 0; height > y; y += 1)
                {
                    for (var x = 0; width > x; x += 1)
                    {
                        var flipY = height - 1 - y;

                        var writeX = pivot.x + x;
                        var writeY = pivot.y + flipY;

                        if (writeX < 0 || writeX >= psdCanvasDesc.Width) { continue; }
                        if (writeY < 0 || writeY >= psdCanvasDesc.Height) { continue; }

                        var readIndex = (y * width + x) * ppB;
                        var writeIndex = ((writeY * psdCanvasDesc.Width) + writeX) * ppB * 4;

                        switch (ppB)
                        {
                            default:
                            case 1:// 1bit or 8bit
                                {
                                    writeTarget[writeIndex + 0] = image[0][readIndex];
                                    writeTarget[writeIndex + 1] = image[1][readIndex];
                                    writeTarget[writeIndex + 2] = image[2][readIndex];
                                    writeTarget[writeIndex + 3] = containsAlpha ? image[3][readIndex] : byte.MaxValue;
                                    break;
                                }
                            case 2:// 16bit
                            case 4:// 32bit
                                {
                                    image[0].AsSpan().Slice(readIndex, ppB).CopyTo(writeTarget.Slice(writeIndex + (0 * ppB), ppB));
                                    image[1].AsSpan().Slice(readIndex, ppB).CopyTo(writeTarget.Slice(writeIndex + (1 * ppB), ppB));
                                    image[2].AsSpan().Slice(readIndex, ppB).CopyTo(writeTarget.Slice(writeIndex + (2 * ppB), ppB));
                                    if (containsAlpha)
                                    {
                                        image[3].AsSpan().Slice(readIndex, ppB).CopyTo(writeTarget.Slice(writeIndex + (3 * ppB), ppB));
                                    }
                                    else
                                    {
                                        var span = writeTarget.Slice(writeIndex + 3 * ppB, ppB);
                                        if (ppB is 2) { BitConverter.TryWriteBytes(span, ushort.MaxValue); }
                                        else { BitConverter.TryWriteBytes(span, 1f); }
                                    }
                                    break;
                                }
                        }


                    }
                }
            }
            finally
            {
                image[0].Dispose();
                image[1].Dispose();
                image[2].Dispose();
                if (containsAlpha) image[3].Dispose();
            }
        }


        protected (int x, int y) PivotT => (RasterImageData.RectTangle.Left, CanvasDescription.Height - RasterImageData.RectTangle.Bottom);

        internal NativeArray<byte> LoadToNativeArray(ChannelImageDataParser.ChannelImageData imageData, byte[] importSourcePSDBytes)
        {
            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;
            var rawByteCount = ChannelImageDataParser.GetImageByteCount(RasterImageData.RectTangle, psdCanvasDesc.BitDepth);

            var writeArray = new NativeArray<byte>(rawByteCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            using (var buffer = new NativeArray<byte>((int)imageData.ImageDataAddress.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            {
                importSourcePSDBytes.LongCopyTo(imageData.ImageDataAddress.StartAddress, buffer.AsSpan());
                imageData.GetImageData((psdCanvasDesc.BitDepth, psdCanvasDesc.IsPSB), buffer, RasterImageData.RectTangle, writeArray);
            }
            return writeArray;
        }
        async static Task<NativeArray<byte>[]> WeightTask(Task<NativeArray<byte>>[] tasks)
        {
            return await Task.WhenAll(tasks.Where(i => i != null)).ConfigureAwait(false);
        }
    }
}
