
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using net.rs64.TexTransCore.TransTextureCore;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Pool;
using net.rs64.TexTransCore.TransTextureCore.Utils;

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

    }
    internal static class IslandCulling
    {

        public static List<TriangleIndex> Culling(
            List<IslandSelectorRay> islandSelectors,
            List<Vector3> positions,
            List<Vector2> uv,
            List<TriangleIndex> triangles,
            List<TriangleIndex> output = null
        )
        {
            var iIslands = IslandUtility.UVtoIsland(triangles, uv);
            var rayCastHitTriangle = ListPool<TriangleIndex>.Get();
            foreach (var i in islandSelectors)
            {
                var hits = RayCast(i.Ray, positions, triangles);
                FilteredBackTriangle(hits);
                FilteredRangeTriangle(hits, i.RayRange);

                if (hits.Any())
                {
                    foreach (var hit in hits)
                        rayCastHitTriangle.Add(hit.Triangle);
                }
            }
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
            output?.Clear(); output ??= new ();
            output.AddRange(hitSelectIsland.SelectMany(I => I.triangles));

            ListPool<TriangleIndex>.Release(rayCastHitTriangle);
            HashSetPool<Island>.Release(hitSelectIsland);

            return output;
        }

        public static List<RayCastHitTriangle> RayCast(Ray ray, List<Vector3> positions, List<TriangleIndex> triangleIs, List<RayCastHitTriangle> output = null)
        {
            var rot = Quaternion.LookRotation(ray.direction);
            var rayMatrix = Matrix4x4.TRS(ray.origin, rot, Vector3.one).inverse;

            var triangle = ListPool<Triangle>.Get();
            triangle.AddRange(triangleIs.ConvertAll(I => new Triangle(I, positions)));
            var nativeTriangleArray = CollectionsUtility.ListToNativeArray(triangle, Allocator.TempJob); ListPool<Triangle>.Release(triangle);

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

            output?.Clear(); output ??= new ();
            for (int i = 0; nativeTriangleArray.Length > i; i += 1)
            {
                if (!hitResult[i]) { continue; }
                output.Add(new RayCastHitTriangle(triangleIs[i], distance[i]));
            }

            output.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            nativeTriangleArray.Dispose();
            hitResult.Dispose();
            distance.Dispose();
            return output;
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
