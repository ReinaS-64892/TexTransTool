using System;
using System.Collections;
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransCore.Decal
{
    internal struct FilterTriangleJobInput<InterSpace>
    {
        public NativeArray<TriangleIndex> Triangle;
        public NativeArray<bool> FilteredBit;
        public InterSpace InternalSpace;

        public FilterTriangleJobInput(NativeArray<TriangleIndex> item1, NativeArray<bool> item2, InterSpace item3)
        {
            Triangle = item1;
            FilteredBit = item2;
            InternalSpace = item3;
        }
    }

    public delegate JobHandle JobChain<Input>(Input input, JobHandle jobHandle);
    internal static class TriangleFilterUtility
    {
        public static JobResult<NativeArray<bool>> FilteringTriangle<InterSpace>(NativeArray<TriangleIndex> target, InterSpace interObjects, JobChain<FilterTriangleJobInput<InterSpace>>[] filtersJobs)
        {
            Profiler.BeginSample("FilteringTriangle");
            var filteredBit = new NativeArray<bool>(target.Length, Allocator.TempJob);
            var input = new FilterTriangleJobInput<InterSpace>(target, filteredBit, interObjects);

            var jobHandle = default(JobHandle);
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
            [ReadOnly] public NativeArray<TriangleIndex> Triangle;
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

            [ReadOnly] public NativeArray<TriangleIndex> Triangle;
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

            [ReadOnly] public NativeArray<TriangleIndex> Triangle;
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
            public PolygonCulling PolygonCulling;
            public float MinRange;
            public float MaxRange;
            public bool IsAllVertex;

            [ReadOnly] public NativeArray<TriangleIndex> Triangle;
            [ReadOnly] public NativeArray<Vector3> WorldVerticals;
            public NativeArray<bool> FilteringBit;

            public void Execute(int index)
            {
                if (FilteringBit[index]) { return; }
                var targetTri = Triangle[index];

                bool result;
                switch (PolygonCulling)
                {
                    default:
                    case PolygonCulling.Vertex:
                        result = OutOfPolygonVertexBase(targetTri, WorldVerticals, MaxRange, MinRange, IsAllVertex);
                        break;
                    case PolygonCulling.Edge:
                        result = OutOfPolygonEdgeBase(targetTri, WorldVerticals, MaxRange, MinRange, IsAllVertex);
                        break;

                    case PolygonCulling.EdgeAndCenterRay:
                        result = OutOfPolygonEdgeEdgeAndCenterRayCast(targetTri, WorldVerticals, MaxRange, MinRange, IsAllVertex);
                        break;
                }

                FilteringBit[index] = result;
            }

            internal static JobChain<FilterTriangleJobInput<NativeArray<Vector3>>> GetJobChain(PolygonCulling polygonCulling, float minRange, float maxRange, bool isAllVertex)
            {
                return (input, jobHandle) =>
                {
                    var job = new OutOfPolygonStruct()
                    {
                        PolygonCulling = polygonCulling,
                        MinRange = minRange,
                        MaxRange = maxRange,
                        IsAllVertex = isAllVertex,
                        Triangle = input.Triangle,
                        FilteringBit = input.FilteredBit,
                        WorldVerticals = input.InternalSpace
                    };
                    return job.Schedule(input.FilteredBit.Length, 32, jobHandle);
                };
            }
            public static bool OutOfPolygonVertexBase(TriangleIndex targetTri, NativeArray<Vector3> vertex, float maxRange, float minRange, bool isAllVertex)
            {
                Span<bool> outOfPolygon = stackalloc bool[3] { false, false, false };
                for (var index = 0; 3 > index; index += 1)
                {
                    var targetVertex = vertex[targetTri[index]];
                    outOfPolygon[index] = !(targetVertex.x < maxRange && targetVertex.x > minRange && targetVertex.y < maxRange && targetVertex.y > minRange);
                }
                if (isAllVertex) return outOfPolygon[0] && outOfPolygon[1] && outOfPolygon[2];
                else return outOfPolygon[0] || outOfPolygon[1] || outOfPolygon[2];
            }
            public static bool OutOfPolygonEdgeBase(TriangleIndex targetTri, NativeArray<Vector3> Vertex, float maxRange, float minRange, bool isAllVertex)
            {
                float centerPos = Mathf.Lerp(maxRange, minRange, 0.5f);
                var centerPosVec2 = new Vector2(centerPos, centerPos);
                Span<bool> outOfPolygon = stackalloc bool[3] { false, false, false };
                Span<Vector2Int> edgeIndexArray = stackalloc Vector2Int[3] { new Vector2Int(0, 1), new Vector2Int(1, 2), new Vector2Int(2, 1) };
                foreach (var index in edgeIndexArray)
                {

                    var a = Vertex[targetTri[index.x]];
                    var b = Vertex[targetTri[index.y]];
                    var nerPoint = VectorUtility.NearPointOnLine(a, b, centerPosVec2);
                    outOfPolygon[index.x] = !(minRange < nerPoint.x && nerPoint.x < maxRange && minRange < nerPoint.y && nerPoint.y < maxRange);
                }
                if (isAllVertex) return outOfPolygon[0] && outOfPolygon[1] && outOfPolygon[2];
                else return outOfPolygon[0] || outOfPolygon[1] || outOfPolygon[2];
            }
            public static bool OutOfPolygonEdgeEdgeAndCenterRayCast(TriangleIndex targetTri, NativeArray<Vector3> vertex, float maxRange, float minRange, bool isAllVertex)
            {
                float centerPos = Mathf.Lerp(maxRange, minRange, 0.5f);
                var centerPosVec2 = new Vector2(centerPos, centerPos);
                if (!OutOfPolygonEdgeBase(targetTri, vertex, maxRange, minRange, isAllVertex))
                {
                    return false;
                }
                else
                {
                    Span<Vector2> tri = stackalloc Vector2[3];
                    tri[0] = vertex[targetTri[0]]; tri[1] = vertex[targetTri[1]]; tri[2] = vertex[targetTri[2]];
                    var crossT = VectorUtility.CrossTriangle(tri, centerPosVec2);
                    return VectorUtility.IsInCal(crossT.x, crossT.y, crossT.z);
                }
            }

        }


    }

}
