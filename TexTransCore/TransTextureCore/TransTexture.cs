using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using net.rs64.TexTransCore.BlendTexture;

namespace net.rs64.TexTransCore.TransTextureCore
{
    internal static class TransTexture
    {
        public struct TransData<UVDimension> where UVDimension : struct
        {
            public readonly IEnumerable<TriangleIndex> TrianglesToIndex;
            public readonly IEnumerable<Vector2> TargetUV;
            public readonly IEnumerable<UVDimension> SourceUV;

            public TransData(IEnumerable<TriangleIndex> TrianglesToIndex, IEnumerable<Vector2> TargetUV, IEnumerable<UVDimension> SourceUV)
            {
                this.TrianglesToIndex = TrianglesToIndex;
                this.TargetUV = TargetUV;
                this.SourceUV = SourceUV;
            }

            public Mesh GenerateTransMesh()
            {
                var Mesh = new Mesh();
                var Vertices = TargetUV.Select(I => new Vector3(I.x, I.y, 0)).ToArray();
                var Triangles = TrianglesToIndex.SelectMany(I => I).ToArray();
                Mesh.vertices = Vertices;
                var NativeArray = new Unity.Collections.NativeArray<UVDimension>(SourceUV.ToArray(), Unity.Collections.Allocator.Temp);
                Mesh.SetUVs(0, NativeArray);
                NativeArray.Dispose();
                Mesh.triangles = Triangles;
                return Mesh;
            }
        }


        public const string TRANS_SHADER = "Hidden/TransTexture";
        public const string DEPTH_WRITER_SHADER = "Hidden/DepthWriter";
        public static void ForTrans<UVDimension>(
            RenderTexture TargetTexture,
            Texture SouseTexture,
            TransData<UVDimension> TransUVData,
            float? Padding = null,
            TextureWrap TexWrap = null,
            bool HighQualityPadding = false,
            bool? DepthInvert = null
            ) where UVDimension : struct
        {
            var mesh = TransUVData.GenerateTransMesh();

            var preBias = SouseTexture.mipMapBias;
            SouseTexture.mipMapBias = SouseTexture.mipmapCount * -1;
            var preWarp = SouseTexture.wrapMode;

            if (TexWrap == null) { TexWrap = TextureWrap.NotWrap; }
            SouseTexture.wrapMode = TexWrap.ConvertTextureWrapMode;




            var material = new Material(Shader.Find(TRANS_SHADER));
            material.SetTexture("_MainTex", SouseTexture);
            if (Padding.HasValue) material.SetFloat("_Padding", Padding.Value);
            if (Padding.HasValue && HighQualityPadding)
            {
                mesh.TTNormalCal();
                material.EnableKeyword("HighQualityPadding");
            }

            if (TexWrap.WarpRange != null)
            {
                material.EnableKeyword("WarpRange");
                material.SetFloat("_WarpRangeX", TexWrap.WarpRange.Value.x);
                material.SetFloat("_WarpRangeY", TexWrap.WarpRange.Value.y);
            }


            RenderTexture depthRt = null;
            if (DepthInvert.HasValue)
            {
                depthRt = RenderTexture.GetTemporary(TargetTexture.width, TargetTexture.height, 8, RenderTextureFormat.RFloat);
                material.EnableKeyword(DepthInvert.Value ? "InvertDepth" : "DepthDecal");

                using (new RTActiveSaver())
                {
                    var depthMat = new Material(Shader.Find(DEPTH_WRITER_SHADER));
                    RenderTexture.active = depthRt;

                    depthMat.SetPass(0);
                    Graphics.DrawMeshNow(mesh, Matrix4x4.identity);

                    UnityEngine.Object.DestroyImmediate(depthMat);
                }

                material.SetTexture("_DepthTex", depthRt);
            }
            else
            {
                material.EnableKeyword("NotDepth");
            }




            using (new RTActiveSaver())
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
            SouseTexture.mipMapBias = preBias;
            SouseTexture.wrapMode = preWarp;
            UnityEngine.Object.DestroyImmediate(mesh);
            if (depthRt != null) { RenderTexture.ReleaseTemporary(depthRt); }
        }
        public static void ForTrans<T>(
            RenderTexture TargetTexture,
            Texture SouseTexture,
            IEnumerable<TransData<T>> TransUVDataEnumerable,
            float? Padding = null,
            TextureWrap WarpRange = null
            ) where T : struct
        {
            foreach (var transUVData in TransUVDataEnumerable)
            {
                ForTrans(TargetTexture, SouseTexture, transUVData, Padding, WarpRange);
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

            RagNormalCalTask(posDict, posDictNormal).Wait();

            for (var i = 0; vertices.Length > i; i += 1)
            {
                if (!posDictNormal.ContainsKey(vertices[i])) { continue; }
                normal[i] = posDictNormal[vertices[i]].normalized;
            }

            mesh.normals = normal;


        }

        private static async Task RagNormalCalTask(Dictionary<Vector3, List<Triangle>> posDict, Dictionary<Vector3, Vector3> posDictNormal)
        {
            var tasks = new Dictionary<Vector3, ConfiguredTaskAwaitable<Vector3>>(posDict.Count);
            foreach (var posAndTri in posDict)
            {
                tasks.Add(posAndTri.Key, Task.Run<Vector3>(LocalRagNormalCal).ConfigureAwait(false));
                Vector3 LocalRagNormalCal()
                {
                    return RagNormalCal(posAndTri.Key, posAndTri.Value);
                }
            }
            foreach (var task in tasks)
            {
                posDictNormal.Add(task.Key, await task.Value);
            }
        }

        static Vector3 RadToVector3(float rad) => new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

        static Vector3 RagNormalCal(Vector3 pos, List<Triangle> triangle)
        {

            var vecTriList = new List<(Vector3 VecZero, Vector3 VecOne)>();
            foreach (var tri in triangle)
            {
                var trList = tri.ToList();
                trList.Remove(pos);
                trList[0] = trList[0] - pos;
                trList[1] = trList[1] - pos;
                if (Mathf.Abs(Vector3.Cross(trList[0], trList[1]).z) < float.Epsilon) { continue; }
                if (Vector3.Cross(trList[0], trList[1]).z > 0)
                {
                    vecTriList.Add((trList[0], trList[1]));
                }
                else
                {
                    vecTriList.Add((trList[1], trList[0]));
                }
            }

            var ragTriList = new List<(float TriRadZero, float TriRadOne)>();
            foreach (var tri in vecTriList)
            {
                var ragZero = Mathf.Atan2(tri.VecZero.y, tri.VecZero.x);
                var ragOne = Mathf.Atan2(tri.VecOne.y, tri.VecOne.x);

                if (!(ragZero > 0 && ragOne < 0)) { ragTriList.Add((ragZero, ragOne)); continue; }

                ragTriList.Add((ragZero, 181 * Mathf.Deg2Rad));
                ragTriList.Add((-181 * Mathf.Deg2Rad, ragOne));
            }


            var rangeList = new List<(float inRad, float OutRad)>();
            var inRadFlag = false;
            (float inRad, float OutRad) temp = (0, 0);
            for (int i = -180; 180 >= i; i += 1)
            {
                var rad = i * Mathf.Deg2Rad;
                var isIn = false;

                foreach (var range in ragTriList)
                {
                    isIn = IsIn(range.TriRadZero, range.TriRadOne, rad);
                    if (isIn) { break; }
                }

                if (!inRadFlag)
                {
                    if (isIn) { continue; }

                    temp.inRad = rad;
                    inRadFlag = true;
                }
                else
                {
                    if (!isIn) { continue; }

                    temp.OutRad = rad;
                    rangeList.Add(temp);
                    temp = (0, 0);
                    inRadFlag = false;
                }
            }
            if (inRadFlag)
            {
                temp.OutRad = 180 * Mathf.Deg2Rad;
                rangeList.Add(temp);
                temp = (0, 0);
                inRadFlag = false;
            }


            rangeList.RemoveAll(I => (I.OutRad - I.inRad) < 3 * Mathf.Deg2Rad);
            if (rangeList.Count == 0) { return Vector3.forward; }

            if (rangeList.Count == 1)
            {
                var renge = rangeList[0];
                return RadToVector3(FromMiddle(renge.inRad, renge.OutRad));
            }

            if (Mathf.Approximately(rangeList[0].inRad, -180 * Mathf.Deg2Rad))
            {
                var first = rangeList[0];
                var last = rangeList[rangeList.Count - 1];
                if (Mathf.Approximately(last.OutRad, 180 * Mathf.Deg2Rad))
                {
                    rangeList.RemoveAt(rangeList.Count - 1);
                    rangeList.RemoveAt(0);
                    rangeList.Add((last.inRad + (-360 * Mathf.Deg2Rad), first.OutRad));
                    if (rangeList.Count == 1)
                    {
                        var Range = rangeList[0];
                        return RadToVector3(FromMiddle(Range.inRad, Range.OutRad));
                    }
                }
            }


            rangeList.Sort((L, R) => Mathf.RoundToInt((Mathf.Abs(L.OutRad - L.inRad) - Mathf.Abs(R.OutRad - R.inRad)) * 100));
            var maxRange = rangeList[rangeList.Count - 1];
            return RadToVector3(FromMiddle(maxRange.inRad, maxRange.OutRad));
        }

        public static bool IsIn(float v1, float v2, float t)
        {
            var min = Mathf.Min(v1, v2);
            var max = Mathf.Max(v1, v2);
            return min < t && t < max;
        }

        public static float FromMiddle(float v1, float v2)
        {
            var min = Mathf.Min(v1, v2);
            var max = Mathf.Max(v1, v2);
            return min + ((max - min) / 2);
        }



    }
}