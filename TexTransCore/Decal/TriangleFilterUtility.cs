using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransCore.Decal
{
    internal static class TriangleFilterUtility
    {
        public interface ITriangleFiltering<InterObject>
        {
            bool Filtering(TriangleIndex TargetTri, InterObject Vertex);//対象の三角形を通せない場合True
        }
        public static List<TriangleIndex> FilteringTriangle<InterSpace, Filter>(List<TriangleIndex> Target, InterSpace InterObjects, IReadOnlyList<Filter> Filters)
        where Filter : ITriangleFiltering<InterSpace>
        {
            var FilteringTriangles = new List<TriangleIndex>(Target.Count);
            var Filtered = new bool[Target.Count];
            foreach (var filter in Filters)
            {
                for (int i = 0; i < Target.Count; i++)
                {
                    if (Filtered[i] == false)
                    {
                        var Triangle = Target[i];
                        Filtered[i] = filter.Filtering(Triangle, InterObjects);
                    }
                }
            }

            for (int i = 0; i < Target.Count; i++)
            {
                if (Filtered[i] == false)
                {
                    FilteringTriangles.Add(Target[i]);
                }
            }

            return FilteringTriangles;
        }

        public struct SideStruct : ITriangleFiltering<List<Vector3>>
        {
            public bool IsReverse;

            public SideStruct(bool isReverse)
            {
                IsReverse = isReverse;
            }

            public bool Filtering(TriangleIndex TargetTri, List<Vector3> Vertex)
            {
                return SideCheck(TargetTri, Vertex, IsReverse);
            }

            public static bool SideCheck(TriangleIndex TargetTri, List<Vector3> Vertex, bool IsReverse = false)
            {
                var ba = Vertex[TargetTri[1]] - Vertex[TargetTri[0]];
                var ac = Vertex[TargetTri[0]] - Vertex[TargetTri[2]];
                var TriangleSide = Vector3.Cross(ba, ac).z;
                if (!IsReverse) return TriangleSide < 0;
                else return TriangleSide > 0;
            }


        }

        public struct FarStruct : ITriangleFiltering<List<Vector3>>
        {
            public float Far;
            public bool IsAllVertex;

            public FarStruct(float far, bool isAllVertex)
            {
                Far = far;
                IsAllVertex = isAllVertex;
            }

            public bool Filtering(TriangleIndex TargetTri, List<Vector3> Vertex)
            {
                return FarClip(TargetTri, Vertex, Far, IsAllVertex);
            }
            public static bool FarClip(TriangleIndex TargetTri, List<Vector3> Vertex, float Far, bool IsAllVertex)//IsAllVertexは排除されるのにすべてが条件に外れてる場合と一つでも条件に外れてる場合の選択
            {
                if (IsAllVertex)
                {
                    return Vertex[TargetTri[0]].z > Far && Vertex[TargetTri[1]].z > Far && Vertex[TargetTri[2]].z > Far;
                }
                else
                {
                    return Vertex[TargetTri[0]].z > Far || Vertex[TargetTri[1]].z > Far || Vertex[TargetTri[2]].z > Far;
                }
            }
        }

        public struct NearStruct : TriangleFilterUtility.ITriangleFiltering<List<Vector3>>
        {
            public float Near;
            public bool IsAllVertex;

            public NearStruct(float near, bool isAllVertex)
            {
                Near = near;
                IsAllVertex = isAllVertex;
            }

            public bool Filtering(TriangleIndex TargetTri, List<Vector3> Vertex)
            {
                return NearClip(TargetTri, Vertex, Near, IsAllVertex);
            }
            public static bool NearClip(TriangleIndex TargetTri, List<Vector3> Vertex, float Near, bool IsAllVertex)
            {
                if (IsAllVertex)
                {
                    return Vertex[TargetTri[0]].z < Near && Vertex[TargetTri[1]].z < Near && Vertex[TargetTri[2]].z < Near;
                }
                else
                {
                    return Vertex[TargetTri[0]].z < Near || Vertex[TargetTri[1]].z < Near || Vertex[TargetTri[2]].z < Near;
                }
            }
        }

        public struct OutOfPolygonStruct : ITriangleFiltering<List<Vector3>>
        {
            public PolygonCulling PolygonCulling;
            public float MinRange;
            public float MaxRange;
            public bool IsAllVertex;

            public OutOfPolygonStruct(PolygonCulling polygonCulling, float minRange, float maxRange, bool isAllVertex)
            {
                PolygonCulling = polygonCulling;
                MinRange = minRange;
                MaxRange = maxRange;
                IsAllVertex = isAllVertex;
            }

            public bool Filtering(TriangleIndex TargetTri, List<Vector3> Vertex)
            {
                switch (PolygonCulling)
                {
                    default:
                    case PolygonCulling.Vertex:
                        return OutOfPolygonVertexBase(TargetTri, Vertex, MaxRange, MinRange, IsAllVertex);
                    case PolygonCulling.Edge:
                        return OutOfPolygonEdgeBase(TargetTri, Vertex, MaxRange, MinRange, IsAllVertex);
                    case PolygonCulling.EdgeAndCenterRay:
                        return OutOfPolygonEdgeEdgeAndCenterRayCast(TargetTri, Vertex, MaxRange, MinRange, IsAllVertex);
                }

            }

            public static bool OutOfPolygonVertexBase(TriangleIndex TargetTri, List<Vector3> Vertex, float MaxRange, float MinRange, bool IsAllVertex)
            {
                bool[] outOfPolygon = new bool[3] { false, false, false };
                foreach (var index in Enumerable.Range(0, 3))
                {
                    var targetVertex = Vertex[TargetTri[index]];
                    outOfPolygon[index] = !(targetVertex.x < MaxRange && targetVertex.x > MinRange && targetVertex.y < MaxRange && targetVertex.y > MinRange);
                }
                if (IsAllVertex) return outOfPolygon[0] && outOfPolygon[1] && outOfPolygon[2];
                else return outOfPolygon[0] || outOfPolygon[1] || outOfPolygon[2];
            }
            public static bool OutOfPolygonEdgeBase(TriangleIndex TargetTri, List<Vector3> Vertex, float MaxRange, float MinRange, bool IsAllVertex)
            {
                float centerPos = Mathf.Lerp(MaxRange, MinRange, 0.5f);
                var centerPosVec2 = new Vector2(centerPos, centerPos);
                bool[] outOfPolygon = new bool[3] { false, false, false };
                foreach (var index in new Vector2Int[3] { new Vector2Int(0, 1), new Vector2Int(1, 2), new Vector2Int(2, 1) })
                {

                    var a = Vertex[TargetTri[index.x]];
                    var b = Vertex[TargetTri[index.y]];
                    var nerPoint = VectorUtility.NearPointOnLine(a, b, centerPosVec2);
                    outOfPolygon[index.x] = !( MinRange < nerPoint.x && nerPoint.x < MaxRange  &&  MinRange < nerPoint.y &&  nerPoint.y < MaxRange );
                }
                if (IsAllVertex) return outOfPolygon[0] && outOfPolygon[1] && outOfPolygon[2];
                else return outOfPolygon[0] || outOfPolygon[1] || outOfPolygon[2];
            }
            public static bool OutOfPolygonEdgeEdgeAndCenterRayCast(TriangleIndex TargetTri, List<Vector3> Vertex, float MaxRange, float MinRange, bool IsAllVertex)
            {
                float centerPos = Mathf.Lerp(MaxRange, MinRange, 0.5f);
                var centerPosVec2 = new Vector2(centerPos, centerPos);
                if (!OutOfPolygonEdgeBase(TargetTri, Vertex, MaxRange, MinRange, IsAllVertex))
                {
                    return false;
                }
                else
                {
                    var crossT = VectorUtility.CrossTriangle(new List<Vector2>(3) { Vertex[TargetTri[0]], Vertex[TargetTri[1]], Vertex[TargetTri[2]] }, centerPosVec2);
                    return VectorUtility.IsInCal(crossT.x,crossT.y,crossT.z);
                }
            }
        }


    }
}