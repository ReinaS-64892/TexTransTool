
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using net.rs64.TexTransCore;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;
using net.rs64.TexTransCore.Decal;
using Debug = UnityEngine.Debug;
using UnityEngine.Pool;
using net.rs64.TexTransCore.Utils;
using UnityEngine.Profiling;

namespace net.rs64.TexTransCore.Island
{
    [Serializable]
    internal struct IslandSelectorRay
    {
        public Ray Ray;
        public float RayRange;
        public IslandSelectorRay(Ray ray, float rayRange)
        {
            this.Ray = ray;
            this.RayRange = rayRange;
        }

        public Matrix4x4 GetRayMatrix()
        {
            var rot = Quaternion.LookRotation(Ray.direction);
            var rayMatrix = Matrix4x4.TRS(Ray.origin, rot, new Vector3(1, 1, RayRange)).inverse;
            return rayMatrix;
        }

    }
    internal static class IslandCulling
    {

        public static List<TriangleIndex> Culling(
            List<IslandSelectorRay> islandSelectors,
            MeshData meshData,
            List<TriangleIndex> output = null
        )
        {
            var iIslands = meshData.Memo(IslandUtility.UVtoIsland);

            Profiler.BeginSample("IslandCulling raycast");
            var rayCastHitTriangle = ListPool<TriangleIndex>.Get();
            foreach (var i in islandSelectors)
            {
                var hits = RayCast(i.Ray, meshData);
                FilteredBackTriangle(hits);
                FilteredRangeTriangle(hits, i.RayRange);

                if (hits.Any())
                {
                    foreach (var hit in hits)
                        rayCastHitTriangle.Add(hit.Triangle);
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("IslandCulling map to island");
            var hitSelectIsland = HashSetPool<Island>.Get();
            foreach (var hitTriangle in rayCastHitTriangle)
            {
                foreach (var island in iIslands)
                {
                    if (island.triangles.Any(I => I == hitTriangle))
                    {
                        hitSelectIsland.Add(island);
                        break;
                    }
                }
            }
            output?.Clear(); output ??= new();
            output.AddRange(hitSelectIsland.SelectMany(I => I.triangles));
            Profiler.EndSample();

            ListPool<TriangleIndex>.Release(rayCastHitTriangle);
            HashSetPool<Island>.Release(hitSelectIsland);

            return output;
        }

        public static List<RayCastHitTriangle> RayCast(Ray ray, MeshData mesh)
        {
            var rayMatrix = ray.GetRayMatrix();

            var nativeTriangleArray = mesh.CombinedTriangles;

            var hitResult = new NativeArray<bool>(nativeTriangleArray.Length, Allocator.TempJob);
            var distance = new NativeArray<float>(nativeTriangleArray.Length, Allocator.TempJob);

            var rayCastJob = new RayCastJob
            {
                rayMatrix = rayMatrix,
                triangles = nativeTriangleArray,
                HitResult = hitResult,
                Distance = distance
            };

            var handle = rayCastJob.Schedule(nativeTriangleArray.Length, 1);
            handle.Complete();

            var output = new List<RayCastHitTriangle>();
            for (int i = 0; nativeTriangleArray.Length > i; i += 1)
            {
                if (!hitResult[i]) { continue; }
                output.Add(new RayCastHitTriangle(mesh.CombinedTriangleIndex[i], distance[i]));
            }

            output.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            hitResult.Dispose();
            distance.Dispose();
            return output;
        }

        public static Matrix4x4 GetRayMatrix(this Ray ray)
        {
            var rot = Quaternion.LookRotation(ray.direction);
            var rayMatrix = Matrix4x4.TRS(ray.origin, rot, Vector3.one).inverse;
            return rayMatrix;
        }

        public static void FilteredBackTriangle(List<RayCastHitTriangle> rayCastHitTriangles)
        {
            rayCastHitTriangles.RemoveAll(I => I.Distance < 0);
        }
        public static void FilteredRangeTriangle(List<RayCastHitTriangle> rayCastHitTriangles, float range)
        {
            rayCastHitTriangles.RemoveAll(I => I.Distance > range);
        }
        public struct RayCastHitTriangle
        {
            public TriangleIndex Triangle;
            public float Distance;
            public RayCastHitTriangle(TriangleIndex triangle, float distance)
            {
                this.Triangle = triangle;
                this.Distance = distance;
            }
        }


    }
}
