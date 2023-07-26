#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace Rs64.TexTransTool.Island
{
    public static class IslandCulling
    {

        public static List<TraiangleIndex> Culling(List<Ray> IslandSelectors, IReadOnlyList<Vector3> Positions, IReadOnlyList<Vector2> UV, List<TraiangleIndex> Traiangles)
        {
            var Islands = IslandUtils.CachengUVtoIsland(Traiangles, UV);
            var RayCastHitTraiangle = new List<TraiangleIndex>();
            foreach (var i in IslandSelectors)
            {
                var Hits = HitRay(i, Positions, Traiangles, out var RayMatrixPoss);
                FiltedBackTraiangle(Hits);
                if (Hits.Any()) RayCastHitTraiangle.Add(Hits[0].Traiangle);
            }
            var HitSelectIsland = new HashSet<Island>();
            foreach (var Hittri in RayCastHitTraiangle)
            {
                foreach (var island in Islands)
                {
                    if (island.trainagels.Any(I => I == Hittri))
                    {
                        HitSelectIsland.Add(island);
                        break;
                    }
                }
            }

            return HitSelectIsland.SelectMany(I => I.trainagels).ToList();

        }

        public static List<RayCastHitTraiangle> HitRay(Ray Ray, IReadOnlyList<Vector3> Positions, IReadOnlyList<TraiangleIndex> Traiangles, out List<Vector3> RayMatrixPoss)
        {
            var Rot = Quaternion.LookRotation(Ray.direction);
            var RayMatrix = Matrix4x4.TRS(Ray.origin, Rot, Vector3.one).inverse;

            RayMatrixPoss = new List<Vector3>();
            foreach (var i in Positions)
            {
                RayMatrixPoss.Add(RayMatrix.MultiplyPoint3x4(i));
            }

            var Hits = new List<(int, float, Vector4)>();
            for (int i = 0; i < Traiangles.Count; i++)
            {
                var Traiangle = Traiangles[i];
                var A = RayMatrixPoss[Traiangle.zero];
                var B = RayMatrixPoss[Traiangle.one];
                var C = RayMatrixPoss[Traiangle.two];

                var Closs = TransMapper.ClossTraiangle(new Vector2[] { A, B, C }, Vector2.zero);
                var TBC = TransMapper.ToBCS(Closs);
                if (float.IsNaN(TBC.x) || float.IsNaN(TBC.y) || float.IsNaN(TBC.z)) { continue; }
                var IsIn = TransMapper.IsInCal(Closs.x, Closs.y, Closs.z);
                if (IsIn)
                {
                    var Distans = TransMapper.FromBCS(new Vector3[] { A, B, C }, TBC).z;

                    Hits.Add((i, Distans, Closs));
                }
            }
            Hits.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            var Out = new List<RayCastHitTraiangle>();
            foreach (var i in Hits)
            {
                Out.Add(new RayCastHitTraiangle(Traiangles[i.Item1], i.Item3, i.Item2));
            }
            return Out;
        }
        public static void FiltedBackTraiangle(List<RayCastHitTraiangle> RCHTaris)
        {
            RCHTaris.RemoveAll(I => I.Distans < 0);
        }
        public struct RayCastHitTraiangle
        {
            public TraiangleIndex Traiangle;
            public Vector4 Closs;
            public float Distans;
            public RayCastHitTraiangle(TraiangleIndex traiangle, Vector4 closs, float distans)
            {
                this.Traiangle = traiangle;
                this.Closs = closs;
                this.Distans = distans;
            }
        }

    }
}



#endif