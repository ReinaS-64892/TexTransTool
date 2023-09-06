using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransCore.TransTextureCore.TransCompute;

namespace net.rs64.TexTransCore.TransTextureCore
{
    public static class TransTexture
    {
        public struct TransData
        {
            public readonly List<TriangleIndex> TrianglesToIndex;
            public readonly List<Vector2> TargetUV;
            public readonly List<Vector2> SourceUV;

            public TransData(List<TriangleIndex> TrianglesToIndex, List<Vector2> TargetUV, List<Vector2> SourceUV)
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
        public static void TransTextureToRenderTexture(
            RenderTexture TargetTexture,
            Texture SouseTexture,
            TransData TransUVData,
            float? Padding = null,
            TextureWrap TexWrap = null
            )
        {
            var mesh = TransUVData.GenerateTransMesh();

            var preBias = SouseTexture.mipMapBias;
            SouseTexture.mipMapBias = SouseTexture.mipmapCount * -1;
            var preWarp = SouseTexture.wrapMode;

            if (TexWrap == null) { TexWrap = TextureWrap.NotWrap; }
            SouseTexture.wrapMode = TexWrap.ConvertTextureWrapMode;




            var material = new Material(Shader.Find("Hidden/TransTexture"));
            material.SetTexture("_MainTex", SouseTexture);
            if (Padding != null) material.SetFloat("_Padding", Padding.Value);

            if (TexWrap.WarpRange != null)
            {
                material.EnableKeyword("WarpRange");
                material.SetFloat("_WarpRangeX", TexWrap.WarpRange.Value.x);
                material.SetFloat("_WarpRangeY", TexWrap.WarpRange.Value.y);
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
            IEnumerable<TransData> TransUVDataEnumerable,
            float? Padding = null,
            TextureWrap WarpRange = null)
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
    }
}