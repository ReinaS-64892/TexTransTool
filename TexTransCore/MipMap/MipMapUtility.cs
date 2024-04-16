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
        public static JobResult<NativeArray<Color32>[]> GenerateAverageMips(NativeArray<Color32> tex, int2 texSize, int GenerateCount)
        {
            var mipMaps = new NativeArray<Color32>[GenerateCount + 1];
            mipMaps[0] = tex;

            var handle = default(JobHandle);

            var upSize = texSize;
            var downSize = texSize;

            for (var i = 1; mipMaps.Length > i; i += 1)
            {
                upSize = downSize;
                downSize = downSize / 2;

                var up = mipMaps[i - 1];
                var down = mipMaps[i] = new NativeArray<Color32>(up.Length / 4, Unity.Collections.Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                var ave = new AverageJobByte()
                {
                    RTex = up,
                    RTexSize = upSize,
                    WTex = down,
                    WTexSize = downSize,
                    PixelRatio = upSize / downSize,
                };

                handle = ave.Schedule(down.Length, 16, handle);

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
        struct AverageJobByte : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Color32> RTex;
            [WriteOnly] public NativeArray<Color32> WTex;
            public int2 PixelRatio;
            public int2 RTexSize;
            public int2 WTexSize;
            public void Execute(int index)
            {
                Average(CovInt2(index, WTexSize.x));
            }


            void Average(int2 id)
            {
                int3 wcol = int3(0, 0, 0);
                int3 col = int3(0, 0, 0);
                int alpha = 0;
                int count = 0;

                int2 readPosOffset = id.xy * PixelRatio;
                for (int y = 0; PixelRatio.y > y; y += 1)
                {
                    for (int x = 0; PixelRatio.x > x; x += 1)
                    {
                        var rCol = RTex[CovInt(readPosOffset + int2(x, y), RTexSize.x)];
                        wcol.x += rCol.r * rCol.a;
                        wcol.y += rCol.g * rCol.a;
                        wcol.z += rCol.b * rCol.a;

                        col.x += rCol.r;
                        col.y += rCol.g;
                        col.z += rCol.b;

                        alpha += rCol.a;
                        count += 1;
                    }
                }

                wcol.x = (int)round(wcol.x / (float)alpha);
                wcol.y = (int)round(wcol.y / (float)alpha);
                wcol.z = (int)round(wcol.z / (float)alpha);
                col.x = (int)round(col.x / (float)count);
                col.y = (int)round(col.y / (float)count);
                col.z = (int)round(col.z / (float)count);
                var resAlpha = (byte)round(alpha / (float)count);
                var writeCol = alpha != 0 ? wcol : col;
                var writeIndex = CovInt(id.xy, WTexSize.x);
                WTex[writeIndex] = new Color32((byte)writeCol.x, (byte)writeCol.y, (byte)writeCol.z, resAlpha);
            }
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
    public enum DownScalingAlgorism
    {
        Average = 0,
    }
}
