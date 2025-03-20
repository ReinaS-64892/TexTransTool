using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace net.rs64.TexTransTool.Decal
{
    internal class MeshData : IDisposable
    {
        public readonly Renderer ReferenceRenderer;
        private readonly NativeArray<Vector3> _vertices;
        public NativeArray<Vector3> Vertices
        {
            get
            {
                _jobHandle.Complete();
                return _vertices;
            }
        }

        public readonly NativeArray<Vector2> VertexUV;

        private JobHandle _jobHandle, _destroyJobHandle;
        public readonly int SubMeshCount;
        public readonly NativeArray<TriangleIndex>[] TriangleIndex;

        private readonly NativeArray<TexTransUnityAABB> _calcAABBBuffer;
        private readonly JobHandle _calcAABBJobHandle;
        private TexTransUnityAABB? _axisAlignedBoundingBox;
        public TexTransUnityAABB AxisAlignedBoundingBox
        {
            get
            {
                if (_axisAlignedBoundingBox is null) { _calcAABBJobHandle.Complete(); _axisAlignedBoundingBox = _calcAABBBuffer[0]; }
                return _axisAlignedBoundingBox.Value;
            }
        }

        public void Dispose()
        {
            _jobHandle.Complete();
            _destroyJobHandle.Complete();
            Vertices.Dispose();
            VertexUV.Dispose();
            _calcAABBBuffer.Dispose();
            foreach (var triangle in TriangleIndex) { triangle.Dispose(); }
        }

        internal MeshData(Renderer renderer, Mesh mesh, Matrix4x4 worldSpaceTransform, UVChannel atlasTargetUVChannel = UVChannel.UV0)
        {
            ReferenceRenderer = renderer;
            using var meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh);

            var mainMesh = meshDataArray[0];

            var vertexCount = mainMesh.vertexCount;
            _vertices = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            VertexUV = new NativeArray<Vector2>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            mainMesh.GetVertices(_vertices);
            mainMesh.GetUVs((int)atlasTargetUVChannel - 1, VertexUV);

            SubMeshCount = mainMesh.subMeshCount;
            TriangleIndex = new NativeArray<TriangleIndex>[SubMeshCount];

            var worldSpaceTransformJob = new WorldSpaceTransformJob()
            {
                PositionBuffer = _vertices,
                WorldSpaceTransform = worldSpaceTransform
            }.Schedule(vertexCount, 64);

            _calcAABBBuffer = new NativeArray<TexTransUnityAABB>(1, Allocator.TempJob);
            _calcAABBJobHandle = new CalculateAABB()
            {
                PositionBuffer = _vertices,
                AABB = _calcAABBBuffer,
            }.Schedule(worldSpaceTransformJob);

            for (int subMeshIndex = 0; SubMeshCount > subMeshIndex; subMeshIndex += 1)
            {
                var desc = mainMesh.GetSubMesh(subMeshIndex);
                TriangleIndex[subMeshIndex] = UnsafeNativeArrayUtility.GetTriangleIndices(mainMesh, subMeshIndex);
            }


            _jobHandle = worldSpaceTransformJob;
            _destroyJobHandle = JobHandle.CombineDependencies(worldSpaceTransformJob, _calcAABBJobHandle);
        }
        internal MeshData(Renderer renderer) : this(renderer, GetMeshWithBaked(renderer), GetMatrix(renderer)) { }

        [BurstCompile]
        struct WorldSpaceTransformJob : IJobParallelFor
        {
            public NativeArray<Vector3> PositionBuffer;
            public Matrix4x4 WorldSpaceTransform;

            public void Execute(int index)
            {
                PositionBuffer[index] = WorldSpaceTransform.MultiplyPoint3x4(PositionBuffer[index]);
            }
        }
        [BurstCompile]
        struct CalculateAABB : IJob
        {
            [ReadOnly] public NativeArray<Vector3> PositionBuffer;
            [WriteOnly] public NativeArray<TexTransUnityAABB> AABB;
            public void Execute()
            {
                TexTransUnityAABB aabb = default;
                if (PositionBuffer.Length > 0) { aabb = new(PositionBuffer[0]); }
                for (var i = 0; PositionBuffer.Length > i; i += 1)
                { aabb.AddVertex(PositionBuffer[i]); }
                AABB[0] = aabb;
            }
        }
        public void AddJobDependency(JobHandle jobHandle)
        {
            _destroyJobHandle = JobHandle.CombineDependencies(_destroyJobHandle, jobHandle);
        }
        internal static Mesh GetMeshWithBaked(Renderer target)
        {
            switch (target)
            {
                case SkinnedMeshRenderer smr:
                    {
                        if (smr.sharedMesh == null) { throw new System.ArgumentException("Mesh が存在しません！"); }
                        Mesh mesh = new Mesh();
                        smr.BakeMesh(mesh);
                        return mesh;
                    }
                case MeshRenderer mr:
                    {
                        var mf = mr.GetComponent<MeshFilter>();
                        if (mf == null || mf.sharedMesh == null) { throw new System.ArgumentException("Mesh が存在しません！"); }
                        return mf.sharedMesh;
                    }
                default:
                    {
                        throw new System.ArgumentException("Rendererが対応したタイプではないか、TargetRendererが存在しません。");
                    }
            }
        }
        internal static Matrix4x4 GetMatrix(Renderer target)
        {
            switch (target)
            {
                case SkinnedMeshRenderer smr:
                    {
                        Matrix4x4 matrix;
                        if (smr.bones.Any())
                        {
                            matrix = Matrix4x4.TRS(smr.transform.position, smr.transform.rotation, Vector3.one);
                        }
                        else if (smr.rootBone == null)
                        {
                            matrix = smr.localToWorldMatrix;
                        }
                        else
                        {
                            matrix = smr.rootBone.localToWorldMatrix;
                        }

                        return matrix;
                    }
                case MeshRenderer mr:
                    {
                        return mr.localToWorldMatrix;
                    }
                default:
                    {
                        throw new System.ArgumentException("Rendererが対応したタイプではないか、TargetRendererが存在しません。");
                    }
            }
        }
        internal static MeshData GetMeshData(Renderer renderer) => new MeshData(renderer);
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct TexTransUnityAABB
    {
        public Vector3 Min;
        public Vector3 Max;

        public TexTransUnityAABB(Vector3 init)
        {
            Max = Min = init;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsIntersect(TexTransUnityAABB other)
        {
            var v = Min.x <= other.Max.x && Max.x >= other.Min.x;
            v &= Min.y <= other.Max.y && Max.y >= other.Min.y;
            v &= Min.z <= other.Max.z && Max.z >= other.Min.z;
            return v;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TexTransUnityAABB AddVertex(Vector3 vector3)
        {
            Min = Vector3.Min(Min, vector3);
            Max = Vector3.Max(Max, vector3);
            return this;
        }
    }
}
