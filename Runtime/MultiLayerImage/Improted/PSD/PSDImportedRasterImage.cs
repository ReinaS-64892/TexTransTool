using System.Threading.Tasks;
using net.rs64.MultiLayerImage.Parser.PSD;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.TransTextureCore;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    internal class PSDImportedRasterImage : TTTImportedImage
    {
        [SerializeField] internal PSDImportedRasterImageData RasterImageData;

        internal override Vector2Int Pivot => new Vector2Int(RasterImageData.RectTangle.Left, CanvasDescription.Height - RasterImageData.RectTangle.Bottom);

        internal override JobResult<NativeArray<Color32>> LoadImage(byte[] importSouse, NativeArray<Color32>? writeTarget = null)
        {
            var nativeArray = writeTarget ?? new NativeArray<Color32>(CanvasDescription.Width * CanvasDescription.Height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var canvasSize = new int2(CanvasDescription.Width, CanvasDescription.Height);

            var initJob = new InitializeJob() { Target = nativeArray, Value = new Color32(0, 0, 0, 0) };
            var handle = initJob.Schedule(nativeArray.Length, 1024);

            Task<NativeArray<byte>>[] getImageTask = new Task<NativeArray<byte>>[4];
            getImageTask[0] = Task.Run(() => RasterImageData.R.GetImageData(importSouse, RasterImageData.RectTangle));
            getImageTask[1] = Task.Run(() => RasterImageData.G.GetImageData(importSouse, RasterImageData.RectTangle));
            getImageTask[2] = Task.Run(() => RasterImageData.B.GetImageData(importSouse, RasterImageData.RectTangle));
            if (RasterImageData.A != null) { getImageTask[3] = Task.Run(() => RasterImageData.A.GetImageData(importSouse, RasterImageData.RectTangle)); }

            var souseTexSize = new int2(RasterImageData.RectTangle.GetWidth(), RasterImageData.RectTangle.GetHeight());
            var image = WeightTask(getImageTask).Result;

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
                    SouseSize = souseTexSize,
                    TargetSize = canvasSize,
                };
                offsetJobHandle = offset.Schedule(image[0].Length, 64, handle);
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
                    SouseSize = souseTexSize,
                    TargetSize = canvasSize,
                };
                offsetJobHandle = offset.Schedule(image[0].Length, 64, handle);
            }



            return new(nativeArray, offsetJobHandle, () =>
            {
                image[0].Dispose();
                image[1].Dispose();
                image[2].Dispose();
                if (RasterImageData.A != null) { image[3].Dispose(); }
            });
        }

        internal override void LoadImage(byte[] importSouse, RenderTexture WriteTarget)
        {
            // var timer = System.Diagnostics.Stopwatch.StartNew();
            Task<NativeArray<byte>>[] getImageTask = new Task<NativeArray<byte>>[4];
            getImageTask[0] = Task.Run(() => RasterImageData.R.GetImageData(importSouse, RasterImageData.RectTangle));
            getImageTask[1] = Task.Run(() => RasterImageData.G.GetImageData(importSouse, RasterImageData.RectTangle));
            getImageTask[2] = Task.Run(() => RasterImageData.B.GetImageData(importSouse, RasterImageData.RectTangle));
            if (RasterImageData.A != null) { getImageTask[3] = Task.Run(() => RasterImageData.A.GetImageData(importSouse, RasterImageData.RectTangle)); }

            var texWidth = RasterImageData.RectTangle.GetWidth();
            var texHeight = RasterImageData.RectTangle.GetHeight();

            var texR = new Texture2D(texWidth, texHeight, TextureFormat.R8, false);
            texR.filterMode = FilterMode.Point;
            var texG = new Texture2D(texWidth, texHeight, TextureFormat.R8, false);
            texG.filterMode = FilterMode.Point;
            var texB = new Texture2D(texWidth, texHeight, TextureFormat.R8, false);
            texB.filterMode = FilterMode.Point;
            var texA = RasterImageData.A != null ? new Texture2D(texWidth, texHeight, TextureFormat.R8, false) : null;
            if (RasterImageData.A != null) { texA.filterMode = FilterMode.Point; }

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
            texA?.LoadRawTextureData(image[3]); texA?.Apply();

            // timer.Stop(); Debug.Log("LoadRawDataAndApply:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();

            Graphics.Blit(null, WriteTarget, s_tempMat);

            // timer.Stop(); Debug.Log("Blit:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();

            s_tempMat.DisableKeyword(SHADER_KEYWORD_SRGB);
            image[0].Dispose();
            image[1].Dispose();
            image[2].Dispose();
            if (RasterImageData.A != null) { image[3].Dispose(); }

            UnityEngine.Object.DestroyImmediate(texR);
            UnityEngine.Object.DestroyImmediate(texG);
            UnityEngine.Object.DestroyImmediate(texB);
            if (RasterImageData.A != null) { UnityEngine.Object.DestroyImmediate(texA); }

            // timer.Stop(); Debug.Log("Dispose:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();
        }

        async static Task<NativeArray<byte>[]> WeightTask(Task<NativeArray<byte>>[] tasks)
        {
            return await Task.WhenAll(tasks).ConfigureAwait(false);
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

        internal struct InitializeJob : IJobParallelFor
        {
            public Color32 Value;
            [WriteOnly] public NativeArray<Color32> Target;
            public void Execute(int index) { Target[index] = Value; }
        }
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

            public int2 SouseSize;
            public int2 Offset;
            public void Execute(int index)
            {
                var sousePos = CovInt2(index, SouseSize.x);
                sousePos.y = SouseSize.y - sousePos.y;
                var writePos = Offset + sousePos;

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

            public int2 SouseSize;
            public int2 Offset;
            public void Execute(int index)
            {
                var sousePos = OffsetMoveAlphaJob.CovInt2(index, SouseSize.x);
                sousePos.y = SouseSize.y - sousePos.y;
                var writePos = Offset + sousePos;

                if (writePos.x < 0) { return; }
                if (writePos.y < 0) { return; }
                if (writePos.x >= TargetSize.x) { return; }
                if (writePos.y >= TargetSize.y) { return; }

                Target[OffsetMoveAlphaJob.CovInt(writePos, TargetSize.x)] = new Color32(R[index], G[index], B[index], byte.MaxValue);
            }

        }

    }
}
