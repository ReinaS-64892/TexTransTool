using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransCore.TransTextureCore.TransCompute;
using net.rs64.TexTransCore.TransTextureCore.Utils;

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


            if (Padding.HasValue)
            {

            }

            var material = new Material(Shader.Find("Hidden/TransTexture"));
            material.SetTexture("_MainTex", SouseTexture);
            if (Padding.HasValue) material.SetFloat("_Padding", Padding.Value);

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

        public static void TTNormalCal(this Mesh mesh)
        {
            var vertices = mesh.vertices;
            var posDict = new Dictionary<Vector3, List<Triangle>>();
            var posDictNormal = new Dictionary<Vector3, Vector3>();
            var normal = new Vector3[vertices.Length];


            foreach (var tri in mesh.GetTriangleIndex())
            {
                foreach (var i in tri)
                {
                    if (posDict.ContainsKey(vertices[i]))
                    {
                        posDict[vertices[i]].Add(new Triangle(tri, vertices));
                    }
                    else
                    {
                        posDict.Add(vertices[i], new List<Triangle>() { new Triangle(tri, vertices) });
                    }
                }
            }

            foreach (var posAndTri in posDict)
            {
                var pos = posAndTri.Key;
                var normalVec = Vector3.zero;

                foreach (var tri in posAndTri.Value)
                {
                    foreach (var vert in tri)
                    {
                        if (vert == pos) { continue; }
                        normalVec += vert - pos;
                    }
                }

                normalVec *= -1;

                foreach (var tri in posAndTri.Value)
                {
                    var trList = tri.ToList();
                    trList.Remove(pos);
                    trList[0] = trList[0] - pos;
                    trList[1] = trList[1] - pos;

                    var zeroCross = Vector3.Cross(trList[0], normalVec).z > 0;
                    var oneCross = Vector3.Cross(trList[1], normalVec).z > 0;

                    if (zeroCross == oneCross) { continue; }

                    var middleVec = trList[0] + trList[1];
                    if (Vector3.Dot(middleVec, normalVec) < 0) { continue; }


                    normalVec = Vector3.up;
                    break;
                }

                posDictNormal.Add(posAndTri.Key, normalVec);
            }

            for (var i = 0; vertices.Length > i; i += 1)
            {
                normal[i] = posDictNormal[vertices[i]].normalized;
            }

            mesh.normals = normal;

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