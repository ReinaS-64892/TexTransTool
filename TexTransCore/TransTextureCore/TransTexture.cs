using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using net.rs64.TexTransCore.BlendTexture;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEditor;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace net.rs64.TexTransCore.TransTextureCore
{
    internal static class TransTexture
    {
        public struct TransData<UVDimension> where UVDimension : struct
        {
            public NativeArray<TriangleIndex> TrianglesToIndex;
            public NativeArray<Vector2> TargetUV;
            public NativeArray<UVDimension> SourceUV;

            public TransData(
                IEnumerable<TriangleIndex> trianglesToIndex,
                IEnumerable<Vector2> targetUV,
                IEnumerable<UVDimension> sourceUV
            )
            {
                // TODO - このコンストラクタを呼び出してるところをNativeArrayに切り替える
                TrianglesToIndex = new NativeArray<TriangleIndex>(trianglesToIndex.ToArray(), Allocator.TempJob);
                TargetUV = new NativeArray<Vector2>(targetUV.ToArray(), Allocator.TempJob);
                SourceUV = new NativeArray<UVDimension>(sourceUV.ToArray(), Allocator.TempJob);

                var self = this;
                EditorApplication.delayCall += () =>
                {
                    self.TrianglesToIndex.Dispose();
                    self.TargetUV.Dispose();
                    self.SourceUV.Dispose();
                };
            }

            public TransData(NativeArray<TriangleIndex> trianglesToIndex, NativeArray<Vector2> targetUV, NativeArray<UVDimension> sourceUV)
            {
                TrianglesToIndex = trianglesToIndex;
                TargetUV = targetUV;
                SourceUV = sourceUV;
            }

            public Mesh GenerateTransMesh()
            {
                var mda = Mesh.AllocateWritableMeshData(1);
                var mda_mesh = mda[0];

                mda_mesh.SetVertexBufferParams(
                    TargetUV.Length,
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, UnsafeUtility.SizeOf<UVDimension>() / 4, stream: 1)
                );
                mda_mesh.SetIndexBufferParams(TrianglesToIndex.Length * 3, IndexFormat.UInt32);

                var pos_array = mda_mesh.GetVertexData<Vector3>(0);
                var uv_array = mda_mesh.GetVertexData<UVDimension>(1);
                var dst_triangles = mda_mesh.GetIndexData<int>();

                var job1 = new CopyPos { Source = TargetUV, Destination = pos_array }.Schedule(TargetUV.Length, 64);
                var job2 = new CopyJob<UVDimension> { Source = SourceUV, Destination = uv_array }.Schedule(SourceUV.Length, 64, job1);
                var job3 = new UnpackTriangleJob { Source = TrianglesToIndex, Destination = dst_triangles }.Schedule(dst_triangles.Length, 64, job2);

                var mesh = new Mesh();

                job3.Complete();

                mda_mesh.subMeshCount = 1;
                mda_mesh.SetSubMesh(0, new SubMeshDescriptor(0, dst_triangles.Length, MeshTopology.Triangles));

                Mesh.ApplyAndDisposeWritableMeshData(mda, mesh);

                return mesh;
            }
        }

        [UsedImplicitly]
        private static void BurstInstantiate()
        {
            new CopyJob<Vector2>().Schedule(1, 1);
            new CopyJob<Vector3>().Schedule(1, 1);
            new CopyJob<Vector4>().Schedule(1, 1);
        }

        [BurstCompile]
        struct UnpackTriangleJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<TriangleIndex> Source;
            [WriteOnly] public NativeArray<int> Destination;

            public void Execute(int index)
            {
                var tri_index = index / 3;
                var coord = index % 3;

                Destination[index] = Source[tri_index][coord];
            }
        }

        [BurstCompile]
        struct CopyPos : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector2> Source;
            [WriteOnly] public NativeArray<Vector3> Destination;

            public void Execute(int index)
            {
                Destination[index] = Source[index];
            }
        }

        [BurstCompile]
        struct CopyJob<T> : IJobParallelFor where T : struct
        {
            [ReadOnly] public NativeArray<T> Source;
            [WriteOnly] public NativeArray<T> Destination;

            public void Execute(int index)
            {
                Destination[index] = Source[index];
            }
        }
        public static void Init()
        {
            s_transShader = Shader.Find(TRANS_SHADER);
            s_depthShader = Shader.Find(DEPTH_WRITER_SHADER);
        }

        public const string TRANS_SHADER = "Hidden/TransTexture";
        static Shader s_transShader;
        public const string DEPTH_WRITER_SHADER = "Hidden/DepthWriter";
        static Shader s_depthShader;

        public static void ForTrans<UVDimension>(
            RenderTexture targetTexture,
            Texture souseTexture,
            TransData<UVDimension> transUVData,
            float? padding = null,
            TextureWrap? argTexWrap = null,
            bool highQualityPadding = false,
            bool? depthInvert = null
            ) where UVDimension : struct
        {
            Profiler.BeginSample("GenerateTransMesh");
            var mesh = transUVData.GenerateTransMesh();
            Profiler.EndSample();

            var preBias = souseTexture.mipMapBias;
            souseTexture.mipMapBias = souseTexture.mipmapCount * -1;
            var preWarp = souseTexture.wrapMode;

            if (argTexWrap == null) { argTexWrap = TextureWrap.NotWrap; }
            var texWrap = argTexWrap.Value;
            souseTexture.wrapMode = texWrap.ConvertTextureWrapMode;




            Profiler.BeginSample("Material Setup");
            var material = new Material(s_transShader);
            material.SetTexture("_MainTex", souseTexture);
            if (padding.HasValue) material.SetFloat("_Padding", padding.Value);
            if (padding.HasValue && highQualityPadding)
            {
                mesh.TTNormalCal();
                material.EnableKeyword("HighQualityPadding");
            }

            if (texWrap.WarpRange != null)
            {
                material.EnableKeyword("WarpRange");
                material.SetFloat("_WarpRangeX", texWrap.WarpRange.Value.x);
                material.SetFloat("_WarpRangeY", texWrap.WarpRange.Value.y);
            }
            Profiler.EndSample();


            RenderTexture depthRt = null;
            if (depthInvert.HasValue)
            {
                depthRt = RenderTexture.GetTemporary(targetTexture.width, targetTexture.height, 8, RenderTextureFormat.RFloat);
                depthRt.Clear();
                material.EnableKeyword(depthInvert.Value ? "InvertDepth" : "DepthDecal");

                using (new RTActiveSaver())
                {
                    var depthMat = new Material(s_depthShader);
                    RenderTexture.active = depthRt;

                    depthMat.SetPass(0);
                    Profiler.BeginSample("depthInvert DrawMeshNow");
                    Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                    Profiler.EndSample();

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
                RenderTexture.active = targetTexture;
                Profiler.BeginSample("DrawMeshNow");
                material.SetPass(0);
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                Profiler.EndSample();
                if (padding != null)
                {
                    Profiler.BeginSample("DrawMeshNow - padding");
                    material.SetPass(1);
                    Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                    Profiler.EndSample();
                }

            }
            souseTexture.mipMapBias = preBias;
            souseTexture.wrapMode = preWarp;
            UnityEngine.Object.DestroyImmediate(mesh);
            if (depthRt != null) { RenderTexture.ReleaseTemporary(depthRt); }
        }
        public static void ForTrans<T>(
            RenderTexture targetTexture,
            Texture souseTexture,
            IEnumerable<TransData<T>> transUVDataEnumerable,
            float? padding = null,
            TextureWrap? warpRange = null
            ) where T : struct
        {
            foreach (var transUVData in transUVDataEnumerable)
            {
                ForTrans(targetTexture, souseTexture, transUVData, padding, warpRange);
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
                var range = rangeList[0];
                return RadToVector3(FromMiddle(range.inRad, range.OutRad));
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
