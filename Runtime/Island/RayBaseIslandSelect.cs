using System;
using Unity.Jobs;
using Unity.Collections;
using net.rs64.TexTransCore;
using System.Collections;
using net.rs64.TexTransTool.IslandSelector;
using net.rs64.TexTransCore.TTMathUtil;
using net.rs64.TexTransCore.UVIsland;

namespace net.rs64.TexTransTool.UVIsland
{
    [Serializable]
    internal struct IslandSelectorPin
    {
        public RayIntersect.Ray Direction;
        public float PinRange;
        public IslandSelectorPin(RayIntersect.Ray ray, float rayRange)
        {
            this.Direction = ray;
            this.PinRange = rayRange;
        }
    }
    internal static class RayBaseIslandSelect
    {
        internal static BitArray PinIslandSelect(IslandSelectorPin islandSelectorPin, Island[] islands, IslandDescription[] islandDescription)
        {
            var bitArray = new BitArray(islands.Length);
            using var jobHolder = BuildRayCastJobs(islandSelectorPin.Direction, islands, islandDescription);

            for (var i = 0; jobHolder.jobs.Length > i; i += 1)
            {
                jobHolder.jobs[i].Complete();
                var hRes = jobHolder.hitResults[i];
                var distance = jobHolder.distances[i];

                for (var ti = 0; hRes.Length > ti; ti += 1)
                {
                    if (hRes[ti] is false) { continue; }
                    if (distance[ti] > islandSelectorPin.PinRange) { continue; }

                    bitArray[i] = true;
                    break;
                }
            }

            return bitArray;
        }
        internal static BitArray RayCastIslandSelect(RayIntersect.Ray islandSelectorRay, Island[] islands, IslandDescription[] islandDescription)
        {
            var bitArray = new BitArray(islands.Length);
            using var jobHolder = BuildRayCastJobs(islandSelectorRay, islands, islandDescription);

            (int index, float distance) minDistanceIslandIndex = (-1, float.MaxValue);

            for (var i = 0; jobHolder.jobs.Length > i; i += 1)
            {
                jobHolder.jobs[i].Complete();
                var hRes = jobHolder.hitResults[i];
                var distance = jobHolder.distances[i];

                for (var ti = 0; hRes.Length > ti; ti += 1)
                {
                    if (hRes[ti] is false) { continue; }
                    var triDistance = distance[ti];
                    if (triDistance >= minDistanceIslandIndex.distance) { continue; }
                    minDistanceIslandIndex = (i, triDistance);
                }
            }

            // No hit
            if (minDistanceIslandIndex.index is -1) { return bitArray; }

            bitArray[minDistanceIslandIndex.index] = true;
            return bitArray;
        }


        private static RaysCastJobHolder BuildRayCastJobs(RayIntersect.Ray islandSelectorRay, Island[] islands, IslandDescription[] islandDescription)
        {
            var jobs = new JobHandle[islands.Length];
            var hitResults = new NativeArray<bool>[islands.Length];
            var distances = new NativeArray<float>[islands.Length];
            for (var i = 0; jobs.Length > i; i += 1)
            {
                var triCount = islands[i].Triangles.Count;
                var nativeTriangleVertexIndices = new NativeArray<TriangleVertexIndices>(triCount, Allocator.TempJob);
                for (var triIndex = 0; triCount > triIndex; triIndex += 1) { nativeTriangleVertexIndices[triIndex] = islands[i].Triangles[triIndex]; }
                var hitResult = hitResults[i] = new NativeArray<bool>(triCount, Allocator.TempJob);
                var distance = distances[i] = new NativeArray<float>(triCount, Allocator.TempJob);

                var rayCastJob = new RayCastJob2()
                {
                    ray = islandSelectorRay,
                    Triangles = nativeTriangleVertexIndices,
                    Position = islandDescription[i].Position,
                    HitResult = hitResult,
                    Distance = distance,
                };
                jobs[i] = rayCastJob.Schedule(triCount, 64);
            }
            return new(jobs, hitResults, distances);
        }
        struct RaysCastJobHolder : IDisposable
        {
            public JobHandle[] jobs;
            public NativeArray<bool>[] hitResults;
            public NativeArray<float>[] distances;

            public RaysCastJobHolder(JobHandle[] jobs, NativeArray<bool>[] hitResults, NativeArray<float>[] distances)
            {
                this.jobs = jobs;
                this.hitResults = hitResults;
                this.distances = distances;
            }

            public void Dispose()
            {
                for (var i = 0; jobs.Length > i; i += 1) { jobs[i].Complete(); }
                for (var i = 0; hitResults.Length > i; i += 1) { hitResults[i].Dispose(); }
                for (var i = 0; distances.Length > i; i += 1) { distances[i].Dispose(); }
            }
        }

        public struct RayCastHitTriangle
        {
            public TriangleVertexIndices Triangle;
            public float Distance;
            public RayCastHitTriangle(TriangleVertexIndices triangle, float distance)
            {
                this.Triangle = triangle;
                this.Distance = distance;
            }
        }


    }
}
