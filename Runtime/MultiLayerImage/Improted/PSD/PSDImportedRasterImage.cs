using System.Linq;
using System.Threading.Tasks;
using net.rs64.MultiLayerImage.Parser.PSD;
using net.rs64.TexTransCore;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    internal class PSDImportedRasterImage : TTTImportedImage
    {
        [SerializeField] internal PSDImportedRasterImageData RasterImageData;

        internal override Vector2Int Pivot => new Vector2Int(RasterImageData.RectTangle.Left, CanvasDescription.Height - RasterImageData.RectTangle.Bottom);

        internal override JobResult<NativeArray<Color32>> LoadImage(byte[] importSource, NativeArray<Color32>? writeTarget = null)
        {
            Profiler.BeginSample("Init");
            var nativeArray = writeTarget ?? new NativeArray<Color32>(CanvasDescription.Width * CanvasDescription.Height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var canvasSize = new int2(CanvasDescription.Width, CanvasDescription.Height);

            TexTransCore.Unsafe.UnsafeNativeArrayUtility.ClearMemory(nativeArray);

            Profiler.EndSample();
            Profiler.BeginSample("RLE");

            Task<NativeArray<byte>>[] getImageTask = new Task<NativeArray<byte>>[4];
            getImageTask[0] = Task.Run(() => RasterImageData.R.GetImageData(importSource, RasterImageData.RectTangle));
            getImageTask[1] = Task.Run(() => RasterImageData.G.GetImageData(importSource, RasterImageData.RectTangle));
            getImageTask[2] = Task.Run(() => RasterImageData.B.GetImageData(importSource, RasterImageData.RectTangle));
            if (RasterImageData.A != null) { getImageTask[3] = Task.Run(() => RasterImageData.A.GetImageData(importSource, RasterImageData.RectTangle)); }

            var sourceTexSize = new int2(RasterImageData.RectTangle.GetWidth(), RasterImageData.RectTangle.GetHeight());
            var image = WeightTask(getImageTask).Result;

            Profiler.EndSample();
            Profiler.BeginSample("OffsetJobSetUp");

            JobHandle offsetJobHandle;

            if (RasterImageData.A != null)
            {
                var offset = new OffsetMoveAlphaJob()
                {
                    Target = nativeArray,
                    R = image[0],
                    G = image[1],
                    B = image[2],
                    A = image[3],
                    Offset = new int2(Pivot.x, Pivot.y),
                    SourceSize = sourceTexSize,
                    TargetSize = canvasSize,
                };
                offsetJobHandle = offset.Schedule(image[0].Length, 32);
            }
            else
            {
                var offset = new OffsetMoveJob()
                {
                    Target = nativeArray,
                    R = image[0],
                    G = image[1],
                    B = image[2],
                    Offset = new int2(Pivot.x, Pivot.y),
                    SourceSize = sourceTexSize,
                    TargetSize = canvasSize,
                };
                offsetJobHandle = offset.Schedule(image[0].Length, 32);
            }

            Profiler.EndSample();


            return new(nativeArray, offsetJobHandle, () =>
            {
                image[0].Dispose();
                image[1].Dispose();
                image[2].Dispose();
                if (RasterImageData.A != null) { image[3].Dispose(); }
            });
        }

        internal override void LoadImage(byte[] importSource, RenderTexture WriteTarget)
        {
            // var timer = System.Diagnostics.Stopwatch.StartNew();
            var containsAlpha = RasterImageData.A.Length != 0;
            Task<NativeArray<byte>>[] getImageTask = new Task<NativeArray<byte>>[4];
            getImageTask[0] = Task.Run(() => RasterImageData.R.GetImageData(importSource, RasterImageData.RectTangle));
            getImageTask[1] = Task.Run(() => RasterImageData.G.GetImageData(importSource, RasterImageData.RectTangle));
            getImageTask[2] = Task.Run(() => RasterImageData.B.GetImageData(importSource, RasterImageData.RectTangle));
            if (containsAlpha) { getImageTask[3] = Task.Run(() => RasterImageData.A.GetImageData(importSource, RasterImageData.RectTangle)); }

            var texWidth = RasterImageData.RectTangle.GetWidth();
            var texHeight = RasterImageData.RectTangle.GetHeight();

            var texR = new Texture2D(texWidth, texHeight, TextureFormat.R8, false);
            texR.filterMode = FilterMode.Point;
            var texG = new Texture2D(texWidth, texHeight, TextureFormat.R8, false);
            texG.filterMode = FilterMode.Point;
            var texB = new Texture2D(texWidth, texHeight, TextureFormat.R8, false);
            texB.filterMode = FilterMode.Point;
            var texA = containsAlpha ? new Texture2D(texWidth, texHeight, TextureFormat.R8, false) : null;
            if (containsAlpha) { texA.filterMode = FilterMode.Point; }

            if (s_tempMat == null) { s_tempMat = new Material(MargeColorAndOffsetShader); }
            s_tempMat.SetTexture("_RTex", texR);
            s_tempMat.SetTexture("_GTex", texG);
            s_tempMat.SetTexture("_BTex", texB);
            s_tempMat.SetTexture("_ATex", texA);
            s_tempMat.SetVector("_Offset", new Vector4(Pivot.x / (float)CanvasDescription.Width, Pivot.y / (float)CanvasDescription.Height, texWidth / (float)CanvasDescription.Width, texHeight / (float)CanvasDescription.Height));
            s_tempMat.EnableKeyword(SHADER_KEYWORD_SRGB);

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

            s_tempMat.DisableKeyword(SHADER_KEYWORD_SRGB);
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

        async static Task<NativeArray<byte>[]> WeightTask(Task<NativeArray<byte>>[] tasks)
        {
            return await Task.WhenAll(tasks.Where(i => i != null)).ConfigureAwait(false);
        }
        [TexTransInitialize]
        public static void Init()
        {
            MargeColorAndOffsetShader = Shader.Find(MARGE_COLOR_AND_OFFSET_SHADER);
        }
        internal const string MARGE_COLOR_AND_OFFSET_SHADER = "Hidden/MargeColorAndOffset";
        internal static Shader MargeColorAndOffsetShader;
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
