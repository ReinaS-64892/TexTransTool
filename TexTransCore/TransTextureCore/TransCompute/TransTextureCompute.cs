
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;
using System.Collections.Generic;
using System.Linq;
using System;

namespace net.rs64.TexTransCore.TransTextureCore.TransCompute
{
    public class TransTextureCompute
    {
        ComputeShader TransMapperCS;
        ComputeShader TransCompilerCS;

        public TransTextureCompute(ComputeShader transMapperCS, ComputeShader transCompilerCS)
        {
            this.TransMapperCS = transMapperCS;
            this.TransCompilerCS = transCompilerCS;
        }

        public void TransTextureUseCS(
            TwoDimensionalMap<TransColor> targetTexture,
            TwoDimensionalMap<Color> SouseTexture,
            TransTexture.TransData TransUVData,
            float? Padding = null,
            TextureWrap wrapMode = null
            )
        {
            Padding = CSPadding(Padding);
            var transMap = new TransMapData(Padding.Value, targetTexture.MapSize);
            TransMapGenerateUseComputeShader(transMap, TransUVData.TrianglesToIndex, TransUVData.TargetUV, TransUVData.SourceUV);
            TransCompileUseComputeShader(SouseTexture, transMap, targetTexture, wrapMode);
        }

        public static float CSPadding(float? Padding)
        {
            if (Padding.HasValue) { return Mathf.Abs(Padding.Value) * -2; }
            else { return 0f; }
        }

        public TransMapData TransMapGenerateUseComputeShader(
        TransMapData TransMap,
        List<TriangleIndex> TrianglesToIndex,
        List<Vector2> TargetUV,
        List<Vector2> SourceUV,
        PaddingType paddingType = PaddingType.EdgeBase
        )
        {
            var TargetScaleUV = new List<Vector2>(TargetUV);
            UVtoTexScale(TargetScaleUV, TransMap.Map.MapSize);

            Vector2Int threadGroupSize = TransMap.Map.MapSize / 32;
            int kernelIndex = -1;
            switch (paddingType)
            {
                case PaddingType.EdgeBase:
                    kernelIndex = TransMapperCS.FindKernel("TransMapGeneratPaddingEdgeBase");
                    break;
                case PaddingType.VertexBase:
                    kernelIndex = TransMapperCS.FindKernel("TransMapGeneratPaddingVartexBase");
                    break;
            }

            var resBuffer = new ComputeBuffer(TransMap.Map.Array.Length, 12);
            resBuffer.SetData(TransMap.Map.Array);
            TransMapperCS.SetBuffer(kernelIndex, "Result", resBuffer);

            var TriBufferSize = TrianglesToIndex.Count * 6;
            var TriBuffer = new ComputeBuffer(TriBufferSize, 8);
            var TriangleList = new List<Vector2>(TriBufferSize);
            foreach (var TriangleToIndex in TrianglesToIndex)
            {
                TriangleList.Add(TargetScaleUV[TriangleToIndex[0]]);
                TriangleList.Add(TargetScaleUV[TriangleToIndex[1]]);
                TriangleList.Add(TargetScaleUV[TriangleToIndex[2]]);
                TriangleList.Add(SourceUV[TriangleToIndex[0]]);
                TriangleList.Add(SourceUV[TriangleToIndex[1]]);
                TriangleList.Add(SourceUV[TriangleToIndex[2]]);
            }
            TriBuffer.SetData<Vector2>(TriangleList);
            TransMapperCS.SetBuffer(kernelIndex, "Triangles", TriBuffer);
            TransMapperCS.SetInt("Size", TransMap.Map.MapSize.x);

            TransMapperCS.Dispatch(kernelIndex, threadGroupSize.x, threadGroupSize.y, TrianglesToIndex.Count);

            resBuffer.GetData(TransMap.Map.Array);

            resBuffer.Release();
            TriBuffer.Release();

            return TransMap;
        }
        static void UVtoTexScale(List<Vector2> UV, Vector2Int TexSize)
        {
            for (int i = 0; i < UV.Count; i++)
            {
                UV[i] = new Vector2(UV[i].x * TexSize.x, UV[i].y * TexSize.y);
            }
        }
        public enum PaddingType
        {
            EdgeBase,
            VertexBase,
        }


        public TwoDimensionalMap<TransColor> TransCompileUseComputeShader(
            TwoDimensionalMap<Color> SouseTex,
            TransMapData TransMap,
            TwoDimensionalMap<TransColor> targetTex,
            TextureWrap wrapMode
            )
        {
            return TransCompileUseComputeShader(SouseTex, new TransMapData[] { TransMap }, targetTex, wrapMode);
        }
        public TwoDimensionalMap<TransColor> TransCompileUseComputeShader(
            TwoDimensionalMap<Color> SouseTex,
            IEnumerable<TransMapData> TransMaps,
            TwoDimensionalMap<TransColor> targetTex,
            TextureWrap wrapMode = null
            )
        {
            var texSize = targetTex.MapSize;
            if (TransMaps.Any(i => i.Map.MapSize != texSize)) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            if (wrapMode == null) { wrapMode = TextureWrap.NotWrap; }


            var sColors = SouseTex.Array;
            var tColors = TransColor.GetColorArray(targetTex.Array);
            var tDistance = TransColor.GetDistanceArray(targetTex.Array);

            Vector2Int threadGroupSize = texSize / 32;
            var kernelIndex = TransCompilerCS.FindKernel(wrapMode.ToString());


            var souseTexCB = new ComputeBuffer(sColors.Length, 16);
            souseTexCB.SetData(sColors);
            TransCompilerCS.SetBuffer(kernelIndex, "Source", souseTexCB);


            TransCompilerCS.SetInts("SourceTexSize", new int[2] { SouseTex.MapSize.x, SouseTex.MapSize.y });


            var atlasMapBuffer = new ComputeBuffer(tColors.Length, 12);

            var targetBuffer = new ComputeBuffer(tColors.Length, 16);
            targetBuffer.SetData(tColors);
            TransCompilerCS.SetBuffer(kernelIndex, "Target", targetBuffer);


            var targetDistanceBuffer = new ComputeBuffer(tDistance.Length, 4);
            targetDistanceBuffer.SetData(tDistance);
            TransCompilerCS.SetBuffer(kernelIndex, "TargetDistansMap", targetDistanceBuffer);


            TransCompilerCS.SetInts("TargetTexSize", new int[2] { texSize.x, texSize.y });
            TransCompilerCS.SetBool("IsOutRange", wrapMode.WarpRange.HasValue);
            if (wrapMode.WarpRange.HasValue)
            {
                TransCompilerCS.SetFloats("OutRange", new float[2] { wrapMode.WarpRange.Value.x, wrapMode.WarpRange.Value.y });
            }

            foreach (var transMap in TransMaps)
            {
                atlasMapBuffer.SetData(transMap.Map.Array);
                TransCompilerCS.SetBuffer(kernelIndex, "AtlasMap", atlasMapBuffer);

                TransCompilerCS.Dispatch(kernelIndex, threadGroupSize.x, threadGroupSize.y, 1);
            }


            targetBuffer.GetData(tColors);
            TransColor.SetColorArray(targetTex.Array, tColors);

            targetDistanceBuffer.GetData(tDistance);
            TransColor.SetDistanceArray(targetTex.Array, tDistance);


            atlasMapBuffer.Release();
            targetDistanceBuffer.Release();
            targetBuffer.Release();
            souseTexCB.Release();

            return targetTex;
        }

    }
}