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
            var Mesh = TransUVData.GenerateTransMesh();

            var PreBias = SouseTexture.mipMapBias;
            SouseTexture.mipMapBias = SouseTexture.mipmapCount * -1;
            var PreWarp = SouseTexture.wrapMode;
            SouseTexture.wrapMode = wrapMode == TexWrapMode.Stretch ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;




            var Material = new Material(Shader.Find("Hidden/TransTexture"));
            Material.SetTexture("_MainTex", SouseTexture);
            if (Padding != null) Material.SetFloat("_Padding", Padding.Value);

            if (WarpRange != null)
            {
                Material.EnableKeyword("WarpRange");
                Material.SetFloat("_WarpRangeX", WarpRange.Value.x);
                Material.SetFloat("_WarpRangeY", WarpRange.Value.y);
            }




            var Pre = RenderTexture.active;

            try
            {
                RenderTexture.active = TargetTexture;
                Material.SetPass(0);
                Graphics.DrawMeshNow(Mesh, Matrix4x4.identity);
                if (Padding != null)
                {
                    Material.SetPass(1);
                    Graphics.DrawMeshNow(Mesh, Matrix4x4.identity);
                }

            }
            finally
            {
                RenderTexture.active = Pre;
            }

            SouseTexture.mipMapBias = PreBias;
            SouseTexture.wrapMode = PreWarp;

        }
        public static void TransTextureToRenderTexture(
            RenderTexture TargetTexture,
            Texture SouseTexture,
            IEnumerable<TransUVData> TransUVData,
            float? Padding = null,
            Vector2? WarpRange = null)
        {
            foreach (var TUVD in TransUVData)
            {
                TransTextureToRenderTexture(TargetTexture, SouseTexture, TUVD, Padding, WarpRange);
            }
        }
        public static Texture2D CopyTexture2D(this RenderTexture Rt)
        {
            var Pre = RenderTexture.active;
            try
            {
                RenderTexture.active = Rt;
                var Texture = new Texture2D(Rt.width, Rt.height, Rt.graphicsFormat,Rt.useMipMap ? UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain : UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                Texture.ReadPixels(new Rect(0, 0, Rt.width, Rt.height), 0, 0);
                Texture.Apply();
                Texture.name = Rt.name + "_CopyTex2D";
                return Texture;
            }
            finally
            {
                RenderTexture.active = Pre;
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
            var TransMap = new TransMapData(Padding.Value, targetTexture.DistansMap.MapSize);
            var TargetScaleUV = new List<Vector2>(TransUVData.TargetUV); TransMapper.UVtoTexScale(TargetScaleUV, targetTexture.DistansMap.MapSize);
            TransMapper.TransMapGeneratUseComputeSheder(null, TransMap, TransUVData.TrianglesToIndex, TargetScaleUV, TransUVData.SourceUV);
            Compiler.TransCompileUseComputeSheder(SouseTexture, TransMap, targetTexture, wrapMode, WarpRange);
        }

        public static float CSPadding(float? Padding)
        {
            if (Padding.HasValue) { return Mathf.Abs(Padding.Value) * -2; }
            else { return 0f; }
        }

    }
}
#endif
