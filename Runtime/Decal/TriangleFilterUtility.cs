#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rs64.TexTransTool.Decal
{
    public static class TrainagelFilterUtility
    {
        public interface ITraiangleFiltaring<InterObject>
        {
            bool Filtering(TraiangleIndex TargetTri, InterObject Vartex);//対象の三角形を通せない場合True
        }
        public static List<TraiangleIndex> FiltaringTraiangle<InterSpace, Filter>(List<TraiangleIndex> Target, InterSpace InterObjects, IReadOnlyList<Filter> Filtars)
        where Filter : ITraiangleFiltaring<InterSpace>
        {
            var FiltalingTraingles = new List<TraiangleIndex>(Target.Count);
            var Filted = new bool[Target.Count];
            foreach (var filter in Filtars)
            {
                for (int i = 0; i < Target.Count; i++)
                {
                    if (Filted[i] == false)
                    {
                        var Traiangle = Target[i];
                        Filted[i] = filter.Filtering(Traiangle, InterObjects);
                    }
                }
            }

            for (int i = 0; i < Target.Count; i++)
            {
                if (Filted[i] == false)
                {
                    FiltalingTraingles.Add(Target[i]);
                }
            }

            return FiltalingTraingles;
        }

        public struct SideStruct : ITraiangleFiltaring<List<Vector3>>
        {
            public bool IsReverse;

            public SideStruct(bool isReverse)
            {
                IsReverse = isReverse;
            }

            public bool Filtering(TraiangleIndex TargetTri, List<Vector3> Vartex)
            {
                return SideChek(TargetTri, Vartex, IsReverse);
            }

            public static bool SideChek(TraiangleIndex TargetTri, List<Vector3> Vartex, bool IsReverse = false)
            {
                var ba = Vartex[TargetTri[1]] - Vartex[TargetTri[0]];
                var ac = Vartex[TargetTri[0]] - Vartex[TargetTri[2]];
                var TraiangleSide = Vector3.Cross(ba, ac).z;
                if (!IsReverse) return TraiangleSide < 0;
                else return TraiangleSide > 0;
            }


        }

        public struct FarStruct : ITraiangleFiltaring<List<Vector3>>
        {
            public float Far;
            public bool IsAllVartex;

            public FarStruct(float far, bool isAllVartex)
            {
                Far = far;
                IsAllVartex = isAllVartex;
            }

            public bool Filtering(TraiangleIndex TargetTri, List<Vector3> Vartex)
            {
                return FarClip(TargetTri, Vartex, Far, IsAllVartex);
            }
            public static bool FarClip(TraiangleIndex TargetTri, List<Vector3> Vartex, float Far, bool IsAllVartex)//IsAllVartexは排除されるのにすべてが条件に外れてる場合と一つでも条件に外れてる場合の選択
            {
                if (IsAllVartex)
                {
                    return Vartex[TargetTri[0]].z > Far && Vartex[TargetTri[1]].z > Far && Vartex[TargetTri[2]].z > Far;
                }
                else
                {
                    return Vartex[TargetTri[0]].z > Far || Vartex[TargetTri[1]].z > Far || Vartex[TargetTri[2]].z > Far;
                }
            }
        }

        public struct NearStruct : TrainagelFilterUtility.ITraiangleFiltaring<List<Vector3>>
        {
            public float Near;
            public bool IsAllVartex;

            public NearStruct(float near, bool isAllVartex)
            {
                Near = near;
                IsAllVartex = isAllVartex;
            }

            public bool Filtering(TraiangleIndex TargetTri, List<Vector3> Vartex)
            {
                return NearClip(TargetTri, Vartex, Near, IsAllVartex);
            }
            public static bool NearClip(TraiangleIndex TargetTri, List<Vector3> Vartex, float Near, bool IsAllVartex)
            {
                if (IsAllVartex)
                {
                    return Vartex[TargetTri[0]].z < Near && Vartex[TargetTri[1]].z < Near && Vartex[TargetTri[2]].z < Near;
                }
                else
                {
                    return Vartex[TargetTri[0]].z < Near || Vartex[TargetTri[1]].z < Near || Vartex[TargetTri[2]].z < Near;
                }
            }
        }

        public struct OutOfPorigonStruct : ITraiangleFiltaring<List<Vector3>>
        {
            public PolygonCaling PolygonCaling;
            public float MinRange;
            public float MaxRange;
            public bool IsAllVartex;

            public OutOfPorigonStruct(PolygonCaling polygonCaling, float minRange, float maxRange, bool isAllVartex)
            {
                PolygonCaling = polygonCaling;
                MinRange = minRange;
                MaxRange = maxRange;
                IsAllVartex = isAllVartex;
            }

            public bool Filtering(TraiangleIndex TargetTri, List<Vector3> Vartex)
            {
                switch (PolygonCaling)
                {
                    default:
                    case PolygonCaling.Vartex:
                        return OutOfPorigonVartexBase(TargetTri, Vartex, MaxRange, MinRange, IsAllVartex);
                    case PolygonCaling.Edge:
                        return OutOfPorigonEdgeBase(TargetTri, Vartex, MaxRange, MinRange, IsAllVartex);
                    case PolygonCaling.EdgeAndCenterRay:
                        return OutOfPorigonEdgeEdgeAndCenterRayCast(TargetTri, Vartex, MaxRange, MinRange, IsAllVartex);
                }

            }

            public static bool OutOfPorigonVartexBase(TraiangleIndex TargetTri, List<Vector3> Vartex, float MaxRange, float MinRange, bool IsAllVartex)
            {
                bool[] OutOfPrygon = new bool[3] { false, false, false };
                foreach (var Index in Enumerable.Range(0, 3))
                {

                    var Tvartex = Vartex[TargetTri[Index]];
                    OutOfPrygon[Index] = !(Tvartex.x < MaxRange && Tvartex.x > MinRange && Tvartex.y < MaxRange && Tvartex.y > MinRange);
                }
                if (IsAllVartex) return OutOfPrygon[0] && OutOfPrygon[1] && OutOfPrygon[2];
                else return OutOfPrygon[0] || OutOfPrygon[1] || OutOfPrygon[2];
            }
            public static bool OutOfPorigonEdgeBase(TraiangleIndex TargetTri, List<Vector3> Vartex, float MaxRange, float MinRange, bool IsAllVartex)
            {
                float CenterPos = Mathf.Lerp(MaxRange, MinRange, 0.5f);
                var ConterPosVec2 = new Vector2(CenterPos, CenterPos);
                bool[] OutOfPrygon = new bool[3] { false, false, false };
                foreach (var Index in new Vector2Int[3] { new Vector2Int(0, 1), new Vector2Int(1, 2), new Vector2Int(2, 1) })
                {

                    var a = Vartex[TargetTri[Index.x]];
                    var b = Vartex[TargetTri[Index.y]];
                    var NerPoint = TransMapper.NeaPointOnLine(a, b, ConterPosVec2);
                    OutOfPrygon[Index.x] = !( MinRange < NerPoint.x && NerPoint.x < MaxRange  &&  MinRange < NerPoint.y &&  NerPoint.y < MaxRange );
                }
                if (IsAllVartex) return OutOfPrygon[0] && OutOfPrygon[1] && OutOfPrygon[2];
                else return OutOfPrygon[0] || OutOfPrygon[1] || OutOfPrygon[2];
            }
            public static bool OutOfPorigonEdgeEdgeAndCenterRayCast(TraiangleIndex TargetTri, List<Vector3> Vartex, float MaxRange, float MinRange, bool IsAllVartex)
            {
                float CenterPos = Mathf.Lerp(MaxRange, MinRange, 0.5f);
                var ConterPosVec2 = new Vector2(CenterPos, CenterPos);
                if (!OutOfPorigonEdgeBase(TargetTri, Vartex, MaxRange, MinRange, IsAllVartex))
                {
                    return false;
                }
                else
                {
                    var ClossT = TransMapper.ClossTraiangle(new List<Vector2>(3) { Vartex[TargetTri[0]], Vartex[TargetTri[1]], Vartex[TargetTri[2]] }, ConterPosVec2);
                    return TransMapper.IsInCal(ClossT.x, ClossT.y, ClossT.z);
                }
            }
        }


    }
}
#endif