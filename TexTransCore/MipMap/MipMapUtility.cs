using System;
using net.rs64.TexTransCore.TransTextureCore;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using ReadOnlyAttribute = Unity.Collections.ReadOnlyAttribute;

namespace net.rs64.TexTransCore.MipMap
{
    internal static class MipMapUtility
    {
        [TexTransInitialize]
        public static void Init()
        {
            MipMapShader = TexTransCoreRuntime.LoadAsset("5f6d88c53276bb14eace10771023ae01", typeof(ComputeShader)) as ComputeShader;
        }
        public static ComputeShader MipMapShader;
        const string WTex = "WTex";
        const string RTex = "RTex";
        const string PixelRatio = "PixelRatio";

        public static bool GenerateMips(RenderTexture renderTexture, DownScalingAlgorism algorism)
        {
            if (!renderTexture.useMipMap || !renderTexture.enableRandomWrite) { return false; }
            bool result;

            switch (algorism)
            {
                case DownScalingAlgorism.Average: { result = Average(renderTexture); break; }
                default: { result = false; break; }
            }

            return result;
        }
        public static JobResult<NativeArray<float4>[]> GenerateAverageMips(NativeArray<float4> tex, int2 texSize, int GenerateCount)
        {
            var mipMaps = new NativeArray<float4>[GenerateCount + 1];
            mipMaps[0] = tex;

            var handle = default(JobHandle);

            var upSize = texSize;
            var downSize = texSize;

            for (var i = 1; mipMaps.Length > i; i += 1)
            {
                upSize = downSize;
                downSize = downSize / 2;

                var up = mipMaps[i - 1];
                var down = mipMaps[i] = new NativeArray<float4>(up.Length / 4, Unity.Collections.Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                var ave = new AverageJob()
                {
                    RTex = up,
                    RTexSize = upSize,
                    WTex = down,
                    WTexSize = downSize,
                    PixelRatio = upSize / downSize,
                };

                handle = ave.Schedule(down.Length, 64, handle);

            }

            return new(mipMaps, handle);
        }

        public static int MipMapCountFrom(int width, int targetWith)
        {
            if (width < targetWith) { return -1; }
            var count = 0;
            if (width == targetWith) { return count; }
            while (width >= 0 && count < 1024)//1024 回で止まるのはただのセーフティ
            {
                width /= 2;
                count += 1;
                if (width == targetWith) { return count; }
            }
            return -1;
        }
        public static int GetMipCount(int size)
        {
            var count = 0;
            while (true)
            {
                size /= 2;
                count += 1;
                if (size <= 1) { return count; }
            }
        }


        static bool Average(RenderTexture renderTexture)
        {
            var kernel32ID = MipMapShader.FindKernel("Average32");
            var kernel1ID = MipMapShader.FindKernel("Average1");

            var width = renderTexture.width;
            var height = renderTexture.height;

            bool useOne = false;
            for (var mipIndex = 0; renderTexture.mipmapCount - 1 > mipIndex; mipIndex += 1)
            {
                width /= 2;
                height /= 2;

                if (width < 32 || height < 32) { useOne = true; }

                var kernelID = useOne ? kernel1ID : kernel32ID;
                var kernelSize = useOne ? 1 : 32;

                MipMapShader.SetTexture(kernelID, RTex, renderTexture, mipIndex);
                MipMapShader.SetTexture(kernelID, WTex, renderTexture, mipIndex + 1);
                MipMapShader.SetInts(PixelRatio, 2, 2);
                MipMapShader.Dispatch(kernelID, width / kernelSize, height / kernelSize, 1);
            }

            return true;
        }



        [BurstCompile]
        struct AverageJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float4> RTex;
            [WriteOnly] public NativeArray<float4> WTex;
            public int2 PixelRatio;
            public int2 RTexSize;
            public int2 WTexSize;
            public void Execute(int index)
            {
                Average(CovInt2(index, WTexSize.x));
            }

            float3 ChakeNaN(float3 val, float3 replase)
            {
                return float3(
                    isnan(val.x) ? replase.x : val.x,
                    isnan(val.y) ? replase.y : val.y,
                    isnan(val.z) ? replase.z : val.z
                    );
            }

            void Average(int2 id)
            {
                float3 wcol = float3(0, 0, 0);
                float3 col = float3(0, 0, 0);
                float alpha = 0;
                int count = 0;

                int2 readPosOffset = id.xy * PixelRatio;
                for (int y = 0; PixelRatio.y > y; y += 1)
                {
                    for (int x = 0; PixelRatio.x > x; x += 1)
                    {
                        float4 rCol = RTex[CovInt(readPosOffset + int2(x, y), RTexSize.x)];
                        wcol += rCol.xyz * rCol.w;
                        col += rCol.xyz;
                        alpha += rCol.w;
                        count += 1;
                    }
                }

                wcol /= alpha;
                col /= count;
                WTex[CovInt(id.xy, WTexSize.x)] = float4(ChakeNaN(wcol, col), alpha / count);
            }
            public static int2 CovInt2(int i, int width)
            {
                return new int2(i % width, i / width);
            }
            public static int CovInt(int2 i, int width)
            {
                return i.y * width + i.x;
            }
        }

    }
    public enum DownScalingAlgorism
    {
        Average = 0,
    }
}
