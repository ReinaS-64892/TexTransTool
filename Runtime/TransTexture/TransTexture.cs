#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Rs64.TexTransTool
{
    public static class TransTexture
    {
        public struct TransUVData
        {
            public IReadOnlyList<TraiangleIndex> TrianglesToIndex;
            public IReadOnlyList<Vector2> TargetUV;
            public IReadOnlyList<Vector2> SourceUV;

            public TransUVData(IReadOnlyList<TraiangleIndex> TrianglesToIndex, IReadOnlyList<Vector2> TargetUV, IReadOnlyList<Vector2> SourceUV)
            {
                this.TrianglesToIndex = TrianglesToIndex;
                this.TargetUV = TargetUV;
                this.SourceUV = SourceUV;
            }

            public Mesh GenereateTransMesh()
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
            Texture2D SouseTexture,
            TransUVData TransUVData,
            float? Pading = null,
            Vector2? WarpRange = null,
            TexWrapMode wrapMode = TexWrapMode.Stretch
            )
        {
            var Mesh = TransUVData.GenereateTransMesh();

            var PreBias = SouseTexture.mipMapBias;
            SouseTexture.mipMapBias = SouseTexture.mipmapCount * -1;
            var PreWarp = SouseTexture.wrapMode;
            SouseTexture.wrapMode = wrapMode == TexWrapMode.Stretch ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;




            var Material = new Material(Shader.Find("Hidden/TransTexture"));
            Material.SetTexture("_MainTex", SouseTexture);
            if (Pading != null) Material.SetFloat("_Pading", Pading.Value);

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
                if (Pading != null)
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
            Texture2D SouseTexture,
            IEnumerable<TransUVData> TransUVData,
            float? Pading = null,
            Vector2? WarpRange = null)
        {
            foreach (var TUVD in TransUVData)
            {
                TransTextureToRenderTexture(TargetTexture, SouseTexture, TUVD, Pading, WarpRange);
            }
        }
        public static Texture2D CopyTexture2D(this RenderTexture Rt)
        {
            var Pre = RenderTexture.active;
            try
            {
                RenderTexture.active = Rt;
                var Texture = new Texture2D(Rt.width, Rt.height, Rt.graphicsFormat, UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain);
                Texture.ReadPixels(new Rect(0, 0, Rt.width, Rt.height), 0, 0);
                Texture.Apply();
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
            float? Pading = null,
            Vector2? WarpRange = null,
            TexWrapMode wrapMode = TexWrapMode.Stretch
            )
        {
            Pading = CSPading(Pading);
            var TransMap = new TransMapData(Pading.Value, targetTexture.DistansMap.MapSize);
            var TargetScaiUV = new List<Vector2>(TransUVData.TargetUV); TransMapper.UVtoTexScale(TargetScaiUV, targetTexture.DistansMap.MapSize);
            TransMapper.TransMapGeneratUseComputeSheder(null, TransMap, TransUVData.TrianglesToIndex, TargetScaiUV, TransUVData.SourceUV);
            Compiler.TransCompileUseComputeSheder(SouseTexture, TransMap, targetTexture, wrapMode, WarpRange);
        }

        public static float CSPading(float? Pading)
        {
            if (Pading.HasValue) { return Mathf.Abs(Pading.Value) * -2; }
            else { return 0f; }
        }

    }
}
#endif
