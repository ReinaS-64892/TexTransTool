using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;
using UnityEngine.Pool;

namespace net.rs64.TexTransCore.Decal
{
    internal static class TriangleFilterUtility
    {
        public interface ITriangleFiltering<InterObject>
        {
            bool Filtering(TriangleIndex targetTri, InterObject vertex);//対象の三角形を通せない場合True
        }
        public static List<TriangleIndex> FilteringTriangle<InterSpace, Filter>(List<TriangleIndex> target, InterSpace interObjects, IReadOnlyList<Filter> filters, List<TriangleIndex> outPut = null)
        where Filter : ITriangleFiltering<InterSpace>
        {
            var targetCount = target.Count;
            var filtered = ArrayPool<bool>.Shared.Rent(targetCount);
            for (var i = 0; targetCount > i; i += 1) { filtered[i] = false; }

            foreach (var filter in filters)
            {
                for (int i = 0; targetCount > i; i++)
                {
                    if (filtered[i] == false)
                    {
                        var Triangle = target[i];
                        filtered[i] = filter.Filtering(Triangle, interObjects);
                    }
                }
            }

            outPut?.Clear(); outPut ??= new (target.Count);
            for (int i = 0; i < target.Count; i++)
            {
                if (filtered[i] == false)
                {
                    outPut.Add(target[i]);
                }
            }
            ArrayPool<bool>.Shared.Return(filtered);

            return outPut;
        }

        public struct SideStruct : ITriangleFiltering<IList<Vector3>>
        {
            public bool IsReverse;

            public SideStruct(bool isReverse)
            {
                IsReverse = isReverse;
            }

            public bool Filtering(TriangleIndex targetTri, IList<Vector3> vertex)
            {
                return SideCheck(targetTri, vertex, IsReverse);
            }

            public static bool SideCheck(TriangleIndex targetTri, IList<Vector3> vertex, bool isReverse = false)
            {
                var ba = vertex[targetTri[1]] - vertex[targetTri[0]];
                var ac = vertex[targetTri[0]] - vertex[targetTri[2]];
                var TriangleSide = Vector3.Cross(ba, ac).z;
                if (!isReverse) return TriangleSide < 0;
                else return TriangleSide > 0;
            }


        }

        public struct FarStruct : ITriangleFiltering<IList<Vector3>>
        {
            public float Far;
            public bool IsAllVertex;

            public FarStruct(float far, bool isAllVertex)
            {
                Far = far;
                IsAllVertex = isAllVertex;
            }

            public bool Filtering(TriangleIndex targetTri, IList<Vector3> vertex)
            {
                return FarClip(targetTri, vertex, Far, IsAllVertex);
            }
            public static bool FarClip(TriangleIndex targetTri, IList<Vector3> vertex, float far, bool isAllVertex)//IsAllVertexは排除されるのにすべてが条件に外れてる場合と一つでも条件に外れてる場合の選択
            {
                if (isAllVertex)
                {
                    return vertex[targetTri[0]].z > far && vertex[targetTri[1]].z > far && vertex[targetTri[2]].z > far;
                }
                else
                {
                    return vertex[targetTri[0]].z > far || vertex[targetTri[1]].z > far || vertex[targetTri[2]].z > far;
                }
            }
        }

        public struct NearStruct : TriangleFilterUtility.ITriangleFiltering<IList<Vector3>>
        {
            public float Near;
            public bool IsAllVertex;

            public NearStruct(float near, bool isAllVertex)
            {
                Near = near;
                IsAllVertex = isAllVertex;
            }

            public bool Filtering(TriangleIndex targetTri, IList<Vector3> vertex)
            {
                return NearClip(targetTri, vertex, Near, IsAllVertex);
            }
            public static bool NearClip(TriangleIndex targetTri, IList<Vector3> vertex, float near, bool isAllVertex)
            {
                if (isAllVertex)
                {
                    return vertex[targetTri[0]].z < near && vertex[targetTri[1]].z < near && vertex[targetTri[2]].z < near;
                }
                else
                {
                    return vertex[targetTri[0]].z < near || vertex[targetTri[1]].z < near || vertex[targetTri[2]].z < near;
                }
            }
        }

        public struct OutOfPolygonStruct : ITriangleFiltering<IList<Vector3>>
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

            public bool Filtering(TriangleIndex targetTri, IList<Vector3> vertex)
            {
                switch (PolygonCulling)
                {
                    default:
                    case PolygonCulling.Vertex:
                        return OutOfPolygonVertexBase(targetTri, vertex, MaxRange, MinRange, IsAllVertex);
                    case PolygonCulling.Edge:
                        return OutOfPolygonEdgeBase(targetTri, vertex, MaxRange, MinRange, IsAllVertex);
                    case PolygonCulling.EdgeAndCenterRay:
                        return OutOfPolygonEdgeEdgeAndCenterRayCast(targetTri, vertex, MaxRange, MinRange, IsAllVertex);
                }

            }

            public static bool OutOfPolygonVertexBase(TriangleIndex targetTri, IList<Vector3> vertex, float maxRange, float minRange, bool isAllVertex)
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
            public static bool OutOfPolygonEdgeBase(TriangleIndex targetTri, IList<Vector3> Vertex, float maxRange, float minRange, bool isAllVertex)
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
            public static bool OutOfPolygonEdgeEdgeAndCenterRayCast(TriangleIndex targetTri, IList<Vector3> vertex, float maxRange, float minRange, bool isAllVertex)
            {
                float centerPos = Mathf.Lerp(maxRange, minRange, 0.5f);
                var centerPosVec2 = new Vector2(centerPos, centerPos);
                if (!OutOfPolygonEdgeBase(targetTri, vertex, maxRange, minRange, isAllVertex))
                {
                    return false;
                }
                else
                {
                    var tri = ListPool<Vector2>.Get();
                    tri.Add(vertex[targetTri[0]]); tri.Add(vertex[targetTri[1]]); tri.Add(vertex[targetTri[2]]);
                    var crossT = VectorUtility.CrossTriangle(tri, centerPosVec2);
                    ListPool<Vector2>.Release(tri);
                    return VectorUtility.IsInCal(crossT.x, crossT.y, crossT.z);
                }
            }
        }


    }
}
