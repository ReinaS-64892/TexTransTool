using System;
using System.Buffers.Binary;
using System.Linq;
using System.Threading.Tasks;
using net.rs64.PSDParser;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.PSDParser;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using Vector4 = UnityEngine.Vector4;

namespace net.rs64.TexTransTool.PSDImporter
{
    public class PSDImportedRasterImage : TTTImportedImage
    {
        [SerializeField] public PSDImportedRasterImageData RasterImageData;
        public override void LoadImage<TTCE>(ITTImportedCanvasSource importSource, TTCE ttce, ITTRenderTexture writeTarget)
        {
            var psdBinary = importSource as PSDImportedCanvasDescription.PSDBinaryHolder;
            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;

            var size = ((uint)RasterImageData.RectTangle.GetWidth(), (uint)RasterImageData.RectTangle.GetHeight());
            var piv = ((uint)PivotT.x, (uint)PivotT.y);

            if (size.Item1 is 0 || size.Item2 is 0) { return; }

            if (psdCanvasDesc.BitDepth is 8 && psdCanvasDesc.IsPSB is false)
            {
                using var ch = ttce.GetComputeHandler(ttce.GenealCompute["Decompress8BitPSDRLE"]);

                DecompressRLE8BitPSDWithTTCE(size, piv, writeTarget, 0, ch, psdBinary.PSDByteArray.AsSpan((int)RasterImageData.R.ImageDataAddress.StartAddress, (int)RasterImageData.R.ImageDataAddress.Length));
                DecompressRLE8BitPSDWithTTCE(size, piv, writeTarget, 1, ch, psdBinary.PSDByteArray.AsSpan((int)RasterImageData.G.ImageDataAddress.StartAddress, (int)RasterImageData.G.ImageDataAddress.Length));
                DecompressRLE8BitPSDWithTTCE(size, piv, writeTarget, 2, ch, psdBinary.PSDByteArray.AsSpan((int)RasterImageData.B.ImageDataAddress.StartAddress, (int)RasterImageData.B.ImageDataAddress.Length));
                DecompressRLE8BitPSDWithTTCE(size, piv, writeTarget, 3, ch, psdBinary.PSDByteArray.AsSpan((int)RasterImageData.A.ImageDataAddress.StartAddress, (int)RasterImageData.A.ImageDataAddress.Length));
            }
            else
            {
                base.LoadImage(importSource, ttce, writeTarget);
            }


        }

        public static void DecompressRLE8BitPSDWithTTCE((uint x, uint y) size, (uint x, uint y) pivot, ITTRenderTexture writeTarget, uint channel, ITTComputeHandler computeHandler, Span<byte> rleSource)
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
            computeHandler.UploadStorageBuffer<uint>(spanBufferID, spanBuf);
            computeHandler.UploadStorageBuffer<byte>(rleBufferID, dSpan);

            computeHandler.SetTexture(texID, writeTarget);

            computeHandler.Dispatch(1, (size.y + 255) / 256, 1);
        }
        public override void LoadImage(ITTImportedCanvasSource importSource, Span<byte> writeTarget)
        {
            var psdBinary = importSource as PSDImportedCanvasDescription.PSDBinaryHolder;
            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;

            Task<NativeArray<byte>>[] getImageTask = new Task<NativeArray<byte>>[4];
            getImageTask[0] = Task.Run(() => LoadToNativeArray(RasterImageData.R, psdBinary.PSDByteArray));
            getImageTask[1] = Task.Run(() => LoadToNativeArray(RasterImageData.G, psdBinary.PSDByteArray));
            getImageTask[2] = Task.Run(() => LoadToNativeArray(RasterImageData.B, psdBinary.PSDByteArray));
            if (RasterImageData.A != null) { getImageTask[3] = Task.Run(() => LoadToNativeArray(RasterImageData.A, psdBinary.PSDByteArray)); }
            var image = WeightTask(getImageTask).Result;
            try
            {
                var ppB = EnginUtil.GetPixelParByte(psdCanvasDesc.ImportedImageFormat, TexTransCoreTextureChannel.R);

                var pivot = PivotT;
                for (var y = 0; RasterImageData.RectTangle.GetHeight() > y; y += 1)
                {
                    for (var x = 0; RasterImageData.RectTangle.GetWidth() > x; x += 1)
                    {
                        var flipY = RasterImageData.RectTangle.GetHeight() - 1 - y;

                        var writeX = pivot.x + x;
                        var writeY = pivot.y + flipY;

                        if (writeX < 0 || writeX >= psdCanvasDesc.Width) { continue; }
                        if (writeY < 0 || writeY >= psdCanvasDesc.Height) { continue; }

                        var readIndex = (x + y * RasterImageData.RectTangle.GetWidth()) * ppB;
                        var writeIndex = (writeX + writeY * psdCanvasDesc.Width) * ppB * 4;

                        switch (ppB)
                        {
                            default:
                            case 1:
                                {
                                    writeTarget[writeIndex + 0] = image[0][readIndex];
                                    writeTarget[writeIndex + 1] = image[1][readIndex];
                                    writeTarget[writeIndex + 2] = image[2][readIndex];
                                    writeTarget[writeIndex + 3] = RasterImageData.A is not null ? image[3][readIndex] : byte.MaxValue;
                                    break;
                                }
                            case 4:
                            case 8:
                                {
                                    image[0].AsSpan().Slice(readIndex, ppB).CopyTo(writeTarget.Slice(writeIndex + 0 * ppB, ppB));
                                    image[1].AsSpan().Slice(readIndex, ppB).CopyTo(writeTarget.Slice(writeIndex + 1 * ppB, ppB));
                                    image[2].AsSpan().Slice(readIndex, ppB).CopyTo(writeTarget.Slice(writeIndex + 2 * ppB, ppB));
                                    if (RasterImageData.A is not null)
                                    {
                                        image[3].AsSpan().Slice(readIndex, ppB).CopyTo(writeTarget.Slice(writeIndex + 3 * ppB, ppB));
                                    }
                                    else
                                    {
                                        var span = writeTarget.Slice(writeIndex + 3 * ppB, ppB);
                                        if (ppB is 4) { BitConverter.TryWriteBytes(span, ushort.MaxValue); }
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
                if (RasterImageData.A != null) image[3].Dispose();
            }
        }


        protected (int x, int y) PivotT => (RasterImageData.RectTangle.Left, CanvasDescription.Height - RasterImageData.RectTangle.Bottom);
        protected Vector2Int Pivot => new Vector2Int(RasterImageData.RectTangle.Left, CanvasDescription.Height - RasterImageData.RectTangle.Bottom);

        internal NativeArray<byte> LoadToNativeArray(ChannelImageDataParser.ChannelImageData imageData, byte[] importSource)
        {
            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;
            var rawByteCount = ChannelImageDataParser.ChannelImageData.GetImageByteCount(RasterImageData.RectTangle, psdCanvasDesc.BitDepth);

            var writeArray = new NativeArray<byte>(rawByteCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            using (var buffer = new NativeArray<byte>((int)imageData.ImageDataAddress.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
                imageData.GetImageData(importSource, RasterImageData.RectTangle, buffer, writeArray);
            return writeArray;
        }

        protected void LoadImage(byte[] importSource, RenderTexture WriteTarget)
        {
            // var timer = System.Diagnostics.Stopwatch.StartNew();
            var containsAlpha = RasterImageData.A.ImageDataAddress.Length != 0;
            Task<NativeArray<byte>>[] getImageTask = new Task<NativeArray<byte>>[4];
            getImageTask[0] = Task.Run(() => LoadToNativeArray(RasterImageData.R, importSource));
            getImageTask[1] = Task.Run(() => LoadToNativeArray(RasterImageData.G, importSource));
            getImageTask[2] = Task.Run(() => LoadToNativeArray(RasterImageData.B, importSource));
            if (containsAlpha) { getImageTask[3] = Task.Run(() => LoadToNativeArray(RasterImageData.A, importSource)); }

            var texWidth = RasterImageData.RectTangle.GetWidth();
            var texHeight = RasterImageData.RectTangle.GetHeight();
            var psdCanvasDesc = CanvasDescription as PSDImportedCanvasDescription;

            var format = BitDepthToTextureFormat(psdCanvasDesc.BitDepth);

            var texR = new Texture2D(texWidth, texHeight, GraphicsFormat.R8_UNorm, TextureCreationFlags.None);
            texR.filterMode = FilterMode.Point;
            var texG = new Texture2D(texWidth, texHeight, GraphicsFormat.R8_UNorm, TextureCreationFlags.None);
            texG.filterMode = FilterMode.Point;
            var texB = new Texture2D(texWidth, texHeight, GraphicsFormat.R8_UNorm, TextureCreationFlags.None);
            texB.filterMode = FilterMode.Point;
            var texA = containsAlpha ? new Texture2D(texWidth, texHeight, GraphicsFormat.R8_UNorm, TextureCreationFlags.None) : null;
            if (containsAlpha) { texA.filterMode = FilterMode.Point; }

            if (s_tempMat == null) { s_tempMat = new Material(MergeColorAndOffsetShader); }
            s_tempMat.SetTexture("_RTex", texR);
            s_tempMat.SetTexture("_GTex", texG);
            s_tempMat.SetTexture("_BTex", texB);
            s_tempMat.SetTexture("_ATex", texA);
            s_tempMat.SetVector("_Offset", new Vector4(Pivot.x / (float)CanvasDescription.Width, Pivot.y / (float)CanvasDescription.Height, texWidth / (float)CanvasDescription.Width, texHeight / (float)CanvasDescription.Height));
            // s_tempMat.EnableKeyword(SHADER_KEYWORD_SRGB);

            // timer.Stop(); Debug.Log(name + "+SetUp:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();
            var image = WeightTask(getImageTask).Result;
            // timer.Stop(); Debug.Log("TaskAwait:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();

            texR.LoadRawTextureData(image[0]); texR.Apply();
            texG.LoadRawTextureData(image[1]); texG.Apply();
            texB.LoadRawTextureData(image[2]); texB.Apply();
            if (containsAlpha) { texA.LoadRawTextureData(image[3]); texA.Apply(); }

            // timer.Stop(); Debug.Log("LoadRawDataAndApply:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();

            Graphics.Blit(null, WriteTarget, s_tempMat);

            // timer.Stop(); Debug.Log("Blit:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();

            // s_tempMat.DisableKeyword(SHADER_KEYWORD_SRGB);
            image[0].Dispose();
            image[1].Dispose();
            image[2].Dispose();
            if (containsAlpha) { image[3].Dispose(); }

            UnityEngine.Object.DestroyImmediate(texR);
            UnityEngine.Object.DestroyImmediate(texG);
            UnityEngine.Object.DestroyImmediate(texB);
            if (containsAlpha) { UnityEngine.Object.DestroyImmediate(texA); }

            // timer.Stop(); Debug.Log("Dispose:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();
        }

        public static TextureFormat BitDepthToTextureFormat(int bitDepth)
        {
            return BitDepthToTextureFormat(bitDepth, 1);
        }
        public static TextureFormat BitDepthToTextureFormat(int bitDepth, int channelCount)
        {
            switch (bitDepth, channelCount)
            {
                case (1, 1):
                case (8, 1):
                    { return TextureFormat.R8; }
                case (8, 3):
                    { return TextureFormat.RGB24; }
                case (8, 4):
                    { return TextureFormat.RGBA32; }
                case (16, 1):
                    { return TextureFormat.R16; }
                case (16, 3):
                    { return TextureFormat.RGB48; }
                case (16, 4):
                    { return TextureFormat.RGBA64; }

                case (32, 1):
                    { return TextureFormat.RFloat; }
                case (32, 4):
                    { return TextureFormat.RGBAFloat; }
            }

            throw new ArgumentOutOfRangeException();
        }

        async static Task<NativeArray<byte>[]> WeightTask(Task<NativeArray<byte>>[] tasks)
        {
            return await Task.WhenAll(tasks.Where(i => i != null)).ConfigureAwait(false);
        }
        [TexTransInitialize]
        public static void Init()
        {
            MergeColorAndOffsetShader = Shader.Find(MERGE_COLOR_AND_OFFSET_SHADER);
        }


        internal const string MERGE_COLOR_AND_OFFSET_SHADER = "Hidden/MergeColorAndOffset";
        internal static Shader MergeColorAndOffsetShader;
        internal static Material s_tempMat;
        internal const string SHADER_KEYWORD_SRGB = "COLOR_SPACE_SRGB";

        [BurstCompile]
        internal struct OffsetMoveAlphaJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly] public NativeArray<Color32> Target;
            [ReadOnly] public NativeArray<byte> R;
            [ReadOnly] public NativeArray<byte> G;
            [ReadOnly] public NativeArray<byte> B;
            [ReadOnly] public NativeArray<byte> A;
            public int2 TargetSize;

            public int2 SourceSize;
            public int2 Offset;
            public void Execute(int index)
            {
                var sourcePos = CovInt2(index, SourceSize.x);
                sourcePos.y = SourceSize.y - sourcePos.y;
                var writePos = Offset + sourcePos;

                if (writePos.x < 0) { return; }
                if (writePos.y < 0) { return; }
                if (writePos.x >= TargetSize.x) { return; }
                if (writePos.y >= TargetSize.y) { return; }

                Target[CovInt(writePos, TargetSize.x)] = new Color32(R[index], G[index], B[index], A[index]);
            }
            public static int2 CovInt2(int i, int width)
            {
                return new int2(i % width, i / width);
            }
            public static int CovInt(int2 i, int width)
            {
                return (i.y * width) + i.x;
            }
        }
        [BurstCompile]
        internal struct OffsetMoveJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly] public NativeArray<Color32> Target;
            [ReadOnly] public NativeArray<byte> R;
            [ReadOnly] public NativeArray<byte> G;
            [ReadOnly] public NativeArray<byte> B;
            public int2 TargetSize;

            public int2 SourceSize;
            public int2 Offset;
            public void Execute(int index)
            {
                var sourcePos = OffsetMoveAlphaJob.CovInt2(index, SourceSize.x);
                sourcePos.y = SourceSize.y - sourcePos.y;
                var writePos = Offset + sourcePos;

                if (writePos.x < 0) { return; }
                if (writePos.y < 0) { return; }
                if (writePos.x >= TargetSize.x) { return; }
                if (writePos.y >= TargetSize.y) { return; }

                Target[OffsetMoveAlphaJob.CovInt(writePos, TargetSize.x)] = new Color32(R[index], G[index], B[index], byte.MaxValue);
            }

        }

    }
}
