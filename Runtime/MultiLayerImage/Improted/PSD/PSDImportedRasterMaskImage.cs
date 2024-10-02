using net.rs64.MultiLayerImage.Parser.PSD;
using net.rs64.TexTransUnityCore.BlendTexture;
using net.rs64.TexTransUnityCore;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using net.rs64.MultiLayerImage.Parser;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    internal class PSDImportedRasterMaskImage : TTTImportedImage
    {
        [SerializeField] internal PSDImportedRasterMaskImageData MaskImageData;

        internal override Vector2Int Pivot => new Vector2Int(MaskImageData.RectTangle.Left, CanvasDescription.Height - MaskImageData.RectTangle.Bottom);

        internal override JobResult<NativeArray<Color32>> LoadImage(byte[] importSource, NativeArray<Color32>? writeTarget = null)
        {
            Profiler.BeginSample("Init");
            var native2DArray = writeTarget ?? new NativeArray<Color32>(CanvasDescription.Width * CanvasDescription.Height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            TexTransUnityCore.Unsafe.UnsafeNativeArrayUtility.ClearMemoryOnColor(native2DArray, MaskImageData.DefaultValue);

            var canvasSize = new int2(CanvasDescription.Width, CanvasDescription.Height);
            var sourceTexSize = new int2(MaskImageData.RectTangle.GetWidth(), MaskImageData.RectTangle.GetHeight());

            Profiler.EndSample();

            JobHandle offsetJobHandle;
            if ((MaskImageData.RectTangle.GetWidth() * MaskImageData.RectTangle.GetHeight()) == 0) { return new(native2DArray); }

            Profiler.BeginSample("RLE");

            var dataSpan = MaskImageData.MaskImage.GetImageData(importSource, MaskImageData.RectTangle);
            var data = new NativeArray<byte>(dataSpan.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            data.AsSpan().CopyFrom(dataSpan);

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
        internal override void LoadImage(byte[] importSource, RenderTexture WriteTarget)
        {
            var isZeroSize = (MaskImageData.RectTangle.GetWidth() * MaskImageData.RectTangle.GetHeight()) == 0;
            if (PSDImportedRasterImage.s_tempMat == null) { PSDImportedRasterImage.s_tempMat = new Material(PSDImportedRasterImage.MergeColorAndOffsetShader); }
            var mat = PSDImportedRasterImage.s_tempMat;
            var texR = new Texture2D(MaskImageData.RectTangle.GetWidth(), MaskImageData.RectTangle.GetHeight(), TextureFormat.R8, false);
            texR.filterMode = FilterMode.Point;

            TextureBlend.ColorBlit(WriteTarget, new Color32(MaskImageData.DefaultValue, MaskImageData.DefaultValue, MaskImageData.DefaultValue, MaskImageData.DefaultValue));

            if (!isZeroSize)
            {
                var dataSpan = MaskImageData.MaskImage.GetImageData(importSource, MaskImageData.RectTangle);
                using (var data = new NativeArray<byte>(dataSpan.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
                {
                    dataSpan.CopyTo(data);
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

        internal static NativeArray<byte> LoadPSDMaskImageData(PSDImportedRasterMaskImageData maskImageData, byte[] importSource)
        {
            var dataSpan = maskImageData.MaskImage.GetImageData(importSource, maskImageData.RectTangle);
            var data = new NativeArray<byte>(dataSpan.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            ChannelImageDataParser.ChannelImageData.HeightInvert(dataSpan, data, maskImageData.RectTangle.GetWidth(), maskImageData.RectTangle.GetHeight());
            return data;
        }

    }
}
