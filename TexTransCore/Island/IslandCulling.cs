
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using net.rs64.TexTransCore.TransTextureCore;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransCore.Island
{
    [Serializable]
    internal struct IslandSelector
    {
        public Ray Ray;
        public float RayRange;
        public IslandSelector(Ray ray, float rayRange)
        {
            this.Ray = ray;
            this.RayRange = rayRange;
        }

    }
    internal static class IslandCulling
    {

        public static List<TriangleIndex> Culling(
            List<IslandSelector> IslandSelectors,
            List<Vector3> Positions,
            List<Vector2> UV,
            List<TriangleIndex> Triangles,
            IIslandCache Caches = null
        )
        {
            var iIslands = IslandUtility.UVtoIsland(Triangles, UV, Caches);
            var rayCastHitTriangle = new List<TriangleIndex>();
            foreach (var i in IslandSelectors)
            {
                var hits = RayCast(i.Ray, Positions, Triangles);
                FilteredBackTriangle(hits);
                FilteredRangeTriangle(hits, i.RayRange);

                if (hits.Any())
                {
                    foreach (var hit in hits)
                        rayCastHitTriangle.Add(hit.Triangle);
                }
            }
            var hitSelectIsland = new HashSet<Island>();
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

            return hitSelectIsland.SelectMany(I => I.triangles).ToList();

        }

        public static List<RayCastHitTriangle> RayCast(Ray Ray, List<Vector3> Positions, List<TriangleIndex> TriangleIs)
        {
            var rot = Quaternion.LookRotation(Ray.direction);
            var rayMatrix = Matrix4x4.TRS(Ray.origin, rot, Vector3.one).inverse;

            var Triangle = TriangleIs.ConvertAll(I => new Triangle(I, Positions)).ToArray();

            var triangle = new NativeArray<Triangle>(Triangle, Allocator.TempJob);
            var hitResult = new NativeArray<bool>(Triangle.Length, Allocator.TempJob);
            var distance = new NativeArray<float>(Triangle.Length, Allocator.TempJob);

            var rayCastJob = new RayCastJob
            {
                rayMatrix = rayMatrix,
                triangles = triangle,
                HitResult = hitResult,
                Distance = distance
            };

            var handle = rayCastJob.Schedule(triangle.Length, 1);
            handle.Complete();

            var Out = new List<RayCastHitTriangle>();
            for (int i = 0; triangle.Length > i; i += 1)
            {
                if (!hitResult[i]) { continue; }
                Out.Add(new RayCastHitTriangle(TriangleIs[i], distance[i]));
            }

            Out.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            triangle.Dispose();
            hitResult.Dispose();
            distance.Dispose();
            return Out;
        }
        public static void FilteredBackTriangle(List<RayCastHitTriangle> RCHTri)
        {
            RCHTri.RemoveAll(I => I.Distance < 0);
        }
        public static void FilteredRangeTriangle(List<RayCastHitTriangle> RCHTri, float Range)
        {
            RCHTri.RemoveAll(I => I.Distance > Range);
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
