
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;

namespace net.rs64.TexTransCore.Island
{
    [Serializable]
    public struct IslandSelector
    {
        public Ray Ray;
        public float RayRange;
        public IslandSelector(Ray ray, float rayRange)
        {
            this.Ray = ray;
            this.RayRange = rayRange;
        }

    }
    public static class IslandCulling
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
                var hits = RayCast(i.Ray, Positions, Triangles, out var RayMatrixPoss);

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

        public static List<RayCastHitTriangle> RayCast(Ray Ray, IReadOnlyList<Vector3> Positions, IReadOnlyList<TriangleIndex> Triangles, out List<Vector3> RayMatrixPoss)
        {
            var rot = Quaternion.LookRotation(Ray.direction);
            var rayMatrix = Matrix4x4.TRS(Ray.origin, rot, Vector3.one).inverse;

            RayMatrixPoss = new List<Vector3>();
            foreach (var i in Positions)
            {
                RayMatrixPoss.Add(rayMatrix.MultiplyPoint3x4(i));
            }

            var hits = new List<(int, float, Vector4)>();
            for (int i = 0; i < Triangles.Count; i++)
            {
                var triangle = Triangles[i];
                var A = RayMatrixPoss[triangle.zero];
                var B = RayMatrixPoss[triangle.one];
                var C = RayMatrixPoss[triangle.two];

                var CrossT = VectorUtility.CrossTriangle(new List<Vector2> { A, B, C }, Vector2.zero);
                var TBC = VectorUtility.ToBarycentricCoordinateSystem(CrossT);
                if (float.IsNaN(TBC.x) || float.IsNaN(TBC.y) || float.IsNaN(TBC.z)) { continue; }
                var IsIn = VectorUtility.IsInCal(CrossT.x, CrossT.y, CrossT.z);
                if (IsIn)
                {
                    var Distance = VectorUtility.FromBarycentricCoordinateSystem(new List<Vector3> { A, B, C }, TBC).z;

                    hits.Add((i, Distance, CrossT));
                }
            }
            hits.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            var Out = new List<RayCastHitTriangle>(hits.Capacity);
            foreach (var i in hits)
            {
                Out.Add(new RayCastHitTriangle(Triangles[i.Item1], i.Item3, i.Item2));
            }
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
            public Vector4 Close;
            public float Distance;
            public RayCastHitTriangle(TriangleIndex triangle, Vector4 close, float distance)
            {
                this.Triangle = triangle;
                this.Close = close;
                this.Distance = distance;
            }
        }

    }
}
