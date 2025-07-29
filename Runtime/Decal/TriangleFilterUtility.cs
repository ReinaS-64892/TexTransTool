#nullable enable
using System;
using net.rs64.TexTransCore;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.Decal
{
    internal struct FilterTriangleJobInput<InterSpace>
    {
        public NativeArray<TriangleVertexIndices> Triangle;
        public NativeArray<bool> FilteredBit;
        public InterSpace InternalSpace;

        public FilterTriangleJobInput(NativeArray<TriangleVertexIndices> item1, NativeArray<bool> item2, InterSpace item3)
        {
            Triangle = item1;
            FilteredBit = item2;
            InternalSpace = item3;
        }
    }

    public delegate JobHandle JobChain<Input>(Input input, JobHandle jobHandle);
    internal static class TriangleFilterUtility
    {
        public static JobResult<NativeArray<bool>> FilteringTriangle<InterSpace>(NativeArray<TriangleVertexIndices> target, InterSpace interObjects, JobChain<FilterTriangleJobInput<InterSpace>>[] filtersJobs, JobHandle jobHandle = default)
        {
            Profiler.BeginSample("FilteringTriangle");
            var filteredBit = new NativeArray<bool>(target.Length, Allocator.TempJob);
            var input = new FilterTriangleJobInput<InterSpace>(target, filteredBit, interObjects);

            for (var i = 0; filtersJobs.Length > i; i += 1)
            {
                var filterJob = filtersJobs[i];
                jobHandle = filtersJobs[i].Invoke(input, jobHandle);
            }

            Profiler.EndSample();
            return new JobResult<NativeArray<bool>>(filteredBit, jobHandle);
        }

        [BurstCompile]
        public struct SideStruct : IJobParallelFor
        {
            [ReadOnly] public bool IsReverse;
            [ReadOnly] public NativeArray<TriangleVertexIndices> Triangle;
            [ReadOnly] public NativeArray<Vector3> WorldVerticals;
            public NativeArray<bool> FilteringBit;
            public void Execute(int index)
            {
                if (FilteringBit[index]) { return; }

                var tri = Triangle[index];
                var ba = WorldVerticals[tri[1]] - WorldVerticals[tri[0]];
                var ac = WorldVerticals[tri[0]] - WorldVerticals[tri[2]];
                var TriangleSide = Vector3.Cross(ba, ac).z;
                if (!IsReverse) { FilteringBit[index] = TriangleSide < 0; }
                else { FilteringBit[index] = TriangleSide > 0; }
            }

            internal static JobChain<FilterTriangleJobInput<NativeArray<Vector3>>> GetJobChain(bool isReverse)
            {
                return (input, jobHandle) =>
                {
                    var job = new SideStruct()
                    {
                        IsReverse = isReverse,
                        Triangle = input.Triangle,
                        FilteringBit = input.FilteredBit,
                        WorldVerticals = input.InternalSpace
                    };
                    return job.Schedule(input.FilteredBit.Length, 32, jobHandle);
                };
            }
        }
        [BurstCompile]
        public struct FarStruct : IJobParallelFor
        {
            [ReadOnly] public float Far;
            [ReadOnly] public bool IsAllVertex;

            [ReadOnly] public NativeArray<TriangleVertexIndices> Triangle;
            [ReadOnly] public NativeArray<Vector3> WorldVerticals;
            public NativeArray<bool> FilteringBit;

            public void Execute(int index)
            {
                if (FilteringBit[index]) { return; }
                var targetTri = Triangle[index];

                bool result;
                if (IsAllVertex) { result = WorldVerticals[targetTri[0]].z > Far && WorldVerticals[targetTri[1]].z > Far && WorldVerticals[targetTri[2]].z > Far; }
                else { result = WorldVerticals[targetTri[0]].z > Far || WorldVerticals[targetTri[1]].z > Far || WorldVerticals[targetTri[2]].z > Far; }

                FilteringBit[index] = result;
            }
            internal static JobChain<FilterTriangleJobInput<NativeArray<Vector3>>> GetJobChain(float far, bool isAllVertex)
            {
                return (input, jobHandle) =>
                {
                    var job = new FarStruct()
                    {
                        Far = far,
                        IsAllVertex = isAllVertex,
                        Triangle = input.Triangle,
                        FilteringBit = input.FilteredBit,
                        WorldVerticals = input.InternalSpace
                    };
                    return job.Schedule(input.FilteredBit.Length, 32, jobHandle);
                };
            }
        }
        [BurstCompile]
        public struct NearStruct : IJobParallelFor
        {
            [ReadOnly] public float Near;
            [ReadOnly] public bool IsAllVertex;

            [ReadOnly] public NativeArray<TriangleVertexIndices> Triangle;
            [ReadOnly] public NativeArray<Vector3> WorldVerticals;
            public NativeArray<bool> FilteringBit;

            public void Execute(int index)
            {
                if (FilteringBit[index]) { return; }
                var targetTri = Triangle[index];

                bool result;
                if (IsAllVertex) { result = WorldVerticals[targetTri[0]].z < Near && WorldVerticals[targetTri[1]].z < Near && WorldVerticals[targetTri[2]].z < Near; }
                else { result = WorldVerticals[targetTri[0]].z < Near || WorldVerticals[targetTri[1]].z < Near || WorldVerticals[targetTri[2]].z < Near; }

                FilteringBit[index] = result;
            }
            internal static JobChain<FilterTriangleJobInput<NativeArray<Vector3>>> GetJobChain(float near, bool isAllVertex)
            {
                return (input, jobHandle) =>
                {
                    var job = new NearStruct()
                    {
                        Near = near,
                        IsAllVertex = isAllVertex,
                        Triangle = input.Triangle,
                        FilteringBit = input.FilteredBit,
                        WorldVerticals = input.InternalSpace
                    };
                    return job.Schedule(input.FilteredBit.Length, 32, jobHandle);
                };
            }
        }
        [BurstCompile]
        public struct OutOfPolygonStruct : IJobParallelFor
        {
            private TexTransUnityAABB AABB;

            [ReadOnly] public NativeArray<TriangleVertexIndices> Triangle;
            [ReadOnly] public NativeArray<Vector3> PolygonVerticals;
            public NativeArray<bool> FilteringBit;

            public void Execute(int index)
            {
                if (FilteringBit[index]) { return; }
                FilteringBit[index] = OutOfPolygonVertexBase(Triangle[index], PolygonVerticals, AABB);
            }

            internal static JobChain<FilterTriangleJobInput<NativeArray<Vector3>>> GetJobChain(TexTransUnityAABB aabb)
            {
                return (input, jobHandle) =>
                {
                    var job = new OutOfPolygonStruct()
                    {
                        AABB = aabb,
                        Triangle = input.Triangle,
                        FilteringBit = input.FilteredBit,
                        PolygonVerticals = input.InternalSpace
                    };
                    return job.Schedule(input.FilteredBit.Length, 32, jobHandle);
                };
            }
            public static bool OutOfPolygonVertexBase(TriangleVertexIndices targetTri, NativeArray<Vector3> vertex, TexTransUnityAABB aabb)
            {
                var triAABB = new TexTransUnityAABB(vertex[targetTri.zero])
                .AddVertex(vertex[targetTri.one])
                .AddVertex(vertex[targetTri.two]);
                return aabb.IsIntersect(triAABB) is false;
            }

        }
    }

}
