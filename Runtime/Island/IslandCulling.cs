#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace net.rs64.TexTransTool.Island
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

        public static List<TriangleIndex> Culling(List<IslandSelector> IslandSelectors, IReadOnlyList<Vector3> Positions, IReadOnlyList<Vector2> UV, List<TriangleIndex> Triangles)
        {
            var iIslands = IslandUtils.CachingUVtoIsland(Triangles, UV);
            var rayCastHitTriangle = new List<TriangleIndex>();
            foreach (var i in IslandSelectors)
            {
                var hits = RayCast(i.Ray, Positions, Triangles, out var RayMatrixPoss);

                FilteredBackTriangle(hits);
                FilteredRangeTriangle(hits, i.RayRange);

                if (hits.Any())
                {
                    foreach(var hit in hits)
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
            var Rot = Quaternion.LookRotation(Ray.direction);
            var RayMatrix = Matrix4x4.TRS(Ray.origin, Rot, Vector3.one).inverse;

            RayMatrixPoss = new List<Vector3>();
            foreach (var i in Positions)
            {
                RayMatrixPoss.Add(RayMatrix.MultiplyPoint3x4(i));
            }

            var Hits = new List<(int, float, Vector4)>();
            for (int i = 0; i < Triangles.Count; i++)
            {
                var Triangle = Triangles[i];
                var A = RayMatrixPoss[Triangle.zero];
                var B = RayMatrixPoss[Triangle.one];
                var C = RayMatrixPoss[Triangle.two];

                var Close = TransMapper.CrossTriangle(new Vector2[] { A, B, C }, Vector2.zero);
                var TBC = TransMapper.ToBCS(Close);
                if (float.IsNaN(TBC.x) || float.IsNaN(TBC.y) || float.IsNaN(TBC.z)) { continue; }
                var IsIn = TransMapper.IsInCal(Close.x,Close.y,Close.z);
                if (IsIn)
                {
                    var Distans = TransMapper.FromBCS(new Vector3[] { A, B, C }, TBC).z;

                    Hits.Add((i, Distans, Close));
                }
            }
            Hits.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            var Out = new List<RayCastHitTriangle>(Hits.Capacity);
            foreach (var i in Hits)
            {
                Out.Add(new RayCastHitTriangle(Triangles[i.Item1], i.Item3, i.Item2));
            }
            return Out;
        }
        public static void FilteredBackTriangle(List<RayCastHitTriangle> RCHTaris)
        {
            RCHTaris.RemoveAll(I => I.Distans < 0);
        }
        public static void FilteredRangeTriangle(List<RayCastHitTriangle> RCHTaris, float Range)
        {
            RCHTaris.RemoveAll(I => I.Distans > Range);
        }
        public struct RayCastHitTriangle
        {
            public TriangleIndex Triangle;
            public Vector4 Close;
            public float Distans;
            public RayCastHitTriangle(TriangleIndex triangle, Vector4 close, float distans)
            {
                this.Triangle = triangle;
                this.Close = close;
                this.Distans = distans;
            }
        }

    }
}



#endif
