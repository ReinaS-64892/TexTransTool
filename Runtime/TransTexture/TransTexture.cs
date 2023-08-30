#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool
{
    public static class TransTexture
    {
        public struct TransUVData
        {
            public IReadOnlyList<TriangleIndex> TrianglesToIndex;
            public IReadOnlyList<Vector2> TargetUV;
            public IReadOnlyList<Vector2> SourceUV;

            public TransUVData(IReadOnlyList<TriangleIndex> TrianglesToIndex, IReadOnlyList<Vector2> TargetUV, IReadOnlyList<Vector2> SourceUV)
            {
                this.TrianglesToIndex = TrianglesToIndex;
                this.TargetUV = TargetUV;
                this.SourceUV = SourceUV;
            }

            public Mesh GenerateTransMesh()
            {
                var Mesh = new Mesh();
                var Vertices = TargetUV.Select(I => new Vector3(I.x, I.y, 0)).ToArray();
                var UV = SourceUV.ToArray();
                var Triangles = TrianglesToIndex.SelectMany(I => I).ToArray();
                Mesh.vertices = Vertices;
                Mesh.uv = UV;
                Mesh.triangles = Triangles;
                return Mesh;
            }
        }
        //sRGBのRenderTexture
        public static void TransTextureToRenderTexture(
            RenderTexture TargetTexture,
            Texture SouseTexture,
            TransUVData TransUVData,
            float? Padding = null,
            Vector2? WarpRange = null,
            TexWrapMode wrapMode = TexWrapMode.Stretch
            )
        {
            var mesh = TransUVData.GenerateTransMesh();

            var preBias = SouseTexture.mipMapBias;
            SouseTexture.mipMapBias = SouseTexture.mipmapCount * -1;
            var preWarp = SouseTexture.wrapMode;
            SouseTexture.wrapMode = wrapMode == TexWrapMode.Stretch ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;




            var material = new Material(Shader.Find("Hidden/TransTexture"));
            material.SetTexture("_MainTex", SouseTexture);
            if (Padding != null) material.SetFloat("_Padding", Padding.Value);

            if (WarpRange != null)
            {
                material.EnableKeyword("WarpRange");
                material.SetFloat("_WarpRangeX", WarpRange.Value.x);
                material.SetFloat("_WarpRangeY", WarpRange.Value.y);
            }




            var preRt = RenderTexture.active;

            try
            {
                RenderTexture.active = TargetTexture;
                material.SetPass(0);
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                if (Padding != null)
                {
                    material.SetPass(1);
                    Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                }

            }
            finally
            {
                RenderTexture.active = preRt;
                SouseTexture.mipMapBias = preBias;
                SouseTexture.wrapMode = preWarp;
            }
        }
        public static void TransTextureToRenderTexture(
            RenderTexture TargetTexture,
            Texture SouseTexture,
            IEnumerable<TransUVData> TransUVDataEnumerable,
            float? Padding = null,
            Vector2? WarpRange = null)
        {
            foreach (var transUVData in TransUVDataEnumerable)
            {
                TransTextureToRenderTexture(TargetTexture, SouseTexture, transUVData, Padding, WarpRange);
            }
        }
        public static Texture2D CopyTexture2D(this RenderTexture Rt)
        {
            var preRt = RenderTexture.active;
            try
            {
                RenderTexture.active = Rt;
                var texture = new Texture2D(Rt.width, Rt.height, Rt.graphicsFormat, Rt.useMipMap ? UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain : UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                texture.ReadPixels(new Rect(0, 0, Rt.width, Rt.height), 0, 0);
                texture.Apply();
                texture.name = Rt.name + "_CopyTex2D";
                return texture;
            }
            finally
            {
                RenderTexture.active = preRt;
            }
        }


        public static void TransTextureUseCS(
            TransTargetTexture targetTexture,
            Texture2D SouseTexture,
            TransUVData TransUVData,
            float? Padding = null,
            Vector2? WarpRange = null,
            TexWrapMode wrapMode = TexWrapMode.Stretch
            )
        {
            Padding = CSPadding(Padding);
            var transMap = new TransMapData(Padding.Value, targetTexture.DistanceMap.MapSize);
            var targetScaleUV = new List<Vector2>(TransUVData.TargetUV); TransMapper.UVtoTexScale(targetScaleUV, targetTexture.DistanceMap.MapSize);
            TransMapper.TransMapGenerateUseComputeShader(null, transMap, TransUVData.TrianglesToIndex, targetScaleUV, TransUVData.SourceUV);
            Compiler.TransCompileUseComputeShader(SouseTexture, transMap, targetTexture, wrapMode, WarpRange);
        }

        public static float CSPadding(float? Padding)
        {
            if (Padding.HasValue) { return Mathf.Abs(Padding.Value) * -2; }
            else { return 0f; }
        }

    }
}
#endif
