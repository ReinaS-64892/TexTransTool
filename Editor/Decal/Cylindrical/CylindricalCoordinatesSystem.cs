#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.Decal.Cylindrical
{
    [AddComponentMenu("TexTransTool/TTT CylindricalCoordinatesSystem")]
    internal class CylindricalCoordinatesSystem : MonoBehaviour, ITexTransToolTag
    {
        public float GizmoRadius = 0.25f;
        public float GizmoHeight = 1f;

        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.black;

            var up = Vector3.up * GizmoHeight;

            Gizmos.DrawWireSphere(Vector3.zero, GizmoRadius);
            Gizmos.DrawLine(Vector3.zero, up);
            Gizmos.DrawWireSphere(up, GizmoRadius);
        }

        public Vector3 GetCCSPoint(Vector3 point)
        {
            var localPoint = transform.worldToLocalMatrix.MultiplyPoint(point);
            var height = localPoint.y;
            var angle = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg;
            var distance = new Vector2(localPoint.x, localPoint.z).magnitude;

            return new Vector3(height, angle, distance);
        }

        public Vector3 GetWorldPoint(Vector3 cssPoint)
        {
            var height = cssPoint.x;
            var angle = cssPoint.y * Mathf.Deg2Rad;
            var distance = cssPoint.z;

            var x = Mathf.Sin(angle) * distance;
            var z = Mathf.Cos(angle) * distance;

            return transform.localToWorldMatrix.MultiplyPoint(new Vector3(x, height, z));
        }

        public static float OffsetAngle(float angle, float offset)
        {
            var a = angle + offset;
            if (a > 180) a -= 360;
            if (a < -180) a += 360;
            return a;
        }

        public static float PositiveNegativeInversion(float angle)
        {
            return angle + (Mathf.Sign(angle) * -360);
        }

        public List<Vector3> VertexConvertCCS(IReadOnlyList<Vector3> vertex, float offSetAngle = 0)
        {
            List<Vector3> convertVertex = new List<Vector3>();
            foreach (var i in vertex)
            {
                var cssPoint = GetCCSPoint(i);
                convertVertex.Add(new Vector3(cssPoint.x, OffsetAngle(cssPoint.y, offSetAngle), cssPoint.z));
            }

            return convertVertex;
        }

        public static void OffSetApply(List<Vector3> vertex, float offset)
        {
            for (int i = 0; i < vertex.Count; i++)
            {
                vertex[i] = new Vector3(vertex[i].x, OffsetAngle(vertex[i].y, offset), vertex[i].z);
            }
        }


        public static int GetPositiveCount(TriangleIndex tri, List<Vector3> vertex)
        {
            var positiveCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (vertex[tri[i]].y > 0) positiveCount += 1;
            }

            return positiveCount;
        }

        public static void HeightScaleFactor(List<Vector3> ccsVertex, float factor = 100)
        {
            for (int i = 0; i < ccsVertex.Count; i++)
            {
                ccsVertex[i] = new Vector3(ccsVertex[i].x * factor, ccsVertex[i].y, ccsVertex[i].z);
            }
        }

    }


    internal class CCSSpace : DecalUtility.IConvertSpace<Vector2>
    {
        public CylindricalCoordinatesSystem CCS;
        public IReadOnlyList<Vector3> Quad;
        public List<Vector3> CCSVertex;
        public List<Vector3> CCSQuad;
        public float Offset;
        public List<Vector3> QuadNormalizedVertex;

        public CCSSpace(CylindricalCoordinatesSystem ccs, IReadOnlyList<Vector3> quad)
        {
            CCS = ccs;
            Quad = quad;
        }

        public void Input(DecalUtility.MeshData MeshData)
        {
            var ccsQuad = CCS.VertexConvertCCS(Quad);
            var ccsVertex = CCS.VertexConvertCCS(MeshData.Vertex);
            var offset = ccsQuad.Min(I => I.y) * -1;

            CylindricalCoordinatesSystem.OffSetApply(ccsQuad, offset);
            CylindricalCoordinatesSystem.OffSetApply(ccsVertex, offset);

            //円柱座標系での高さの値を大きくするとQuadNormalizeの精度が上がる。
            CylindricalCoordinatesSystem.HeightScaleFactor(ccsVertex);
            CylindricalCoordinatesSystem.HeightScaleFactor(ccsQuad);

            Offset = offset;
            var Normalized = DecalUtility.QuadNormalize(ccsQuad.ConvertAll(i => (Vector2)i), ccsVertex.ConvertAll(i => (Vector2)i));
            QuadNormalizedVertex = CollectionsUtility.ZipListVector3(Normalized, ccsVertex.ConvertAll(i => i.z));

            CCSVertex = ccsVertex;
            CCSQuad = ccsQuad;
        }

        public List<Vector2> OutPutUV(List<Vector2> output = null)
        {
            output?.Clear(); output ??= new List<Vector2>(QuadNormalizedVertex.Capacity);
            foreach (var i in QuadNormalizedVertex)
            {
                output.Add(i);
            }
            return output;
        }
    }

    internal class CCSFilter : DecalUtility.ITrianglesFilter<CCSSpace>
    {
        public IReadOnlyList<TriangleFilterUtility.ITriangleFiltering<CCSSpace>> Filters;

        public CCSFilter(IReadOnlyList<TriangleFilterUtility.ITriangleFiltering<CCSSpace>> filters)
        {
            Filters = filters;
        }
        public List<TriangleIndex> Filtering(CCSSpace space, List<TriangleIndex> triangles, List<TriangleIndex> output = null)
        {
            return TriangleFilterUtility.FilteringTriangle(triangles, space, Filters, output);
        }


        public struct InDistanceStruct : TriangleFilterUtility.ITriangleFiltering<CCSSpace>
        {
            public float Near;
            public bool IsAllVertex;

            public InDistanceStruct(float near, bool isAllVertex)
            {
                Near = near;
                IsAllVertex = isAllVertex;
            }


            public bool Filtering(TriangleIndex tri, CCSSpace space)
            {
                return TriangleFilterUtility.NearStruct.NearClip(tri, space.QuadNormalizedVertex, Near, IsAllVertex);
            }
        }

        public struct OutDistanceStruct : TriangleFilterUtility.ITriangleFiltering<CCSSpace>
        {
            public float Far;
            public bool IsAllVertex;

            public OutDistanceStruct(float far, bool isAllVertex)
            {
                Far = far;
                IsAllVertex = isAllVertex;
            }

            public bool Filtering(TriangleIndex tri, CCSSpace space)
            {
                return TriangleFilterUtility.FarStruct.FarClip(tri, space.QuadNormalizedVertex, Far, IsAllVertex);
            }
        }

        public struct SideStruct : TriangleFilterUtility.ITriangleFiltering<CCSSpace>
        {
            public bool IsReverse;

            public SideStruct(bool isReverse)
            {
                IsReverse = isReverse;
            }

            public bool Filtering(TriangleIndex tri, CCSSpace space)
            {
                return TriangleFilterUtility.SideStruct.SideCheck(tri, space.QuadNormalizedVertex, IsReverse);
            }
        }

        public struct BorderOnPolygonStruct : TriangleFilterUtility.ITriangleFiltering<CCSSpace>
        {
            public float Threshold /*= 150f */;

            public BorderOnPolygonStruct(float threshold)
            {
                Threshold = threshold;
            }

            public bool Filtering(TriangleIndex tri, CCSSpace Space)
            {
                return BorderOnPolygon(tri, Space.CCSVertex, Threshold);
            }

            public static bool BorderOnPolygon(TriangleIndex tri, List<Vector3> ccsVertex, float threshold = 150)
            {
                var CCStri = tri.GetTriangle(ccsVertex);

                var positiveCount = CylindricalCoordinatesSystem.GetPositiveCount(tri, ccsVertex);

                if (positiveCount == 0 || positiveCount == 3) return false;

                var min = CCStri.Min(I => I.y);
                var max = CCStri.Max(I => I.y);

                return (max - min) > threshold;
            }
        }
        public struct OutOfPerigonStruct : TriangleFilterUtility.ITriangleFiltering<CCSSpace>
        {
            public PolygonCulling PolygonCulling;
            public float MinRange;
            public float MaxRange;
            public bool IsAllVertex;

            public OutOfPerigonStruct(PolygonCulling polygonCulling, float outOfRangeOffset, bool isAllVertex)
            {
                PolygonCulling = polygonCulling;
                MinRange = 0 - outOfRangeOffset;
                MaxRange = 1 + outOfRangeOffset;
                IsAllVertex = isAllVertex;

            }

            public bool Filtering(TriangleIndex targetTri, CCSSpace ccsSpace)
            {
                switch (PolygonCulling)
                {
                    default:
                    case PolygonCulling.Vertex:
                        return TriangleFilterUtility.OutOfPolygonStruct.OutOfPolygonVertexBase(targetTri, ccsSpace.QuadNormalizedVertex, MaxRange, MinRange, IsAllVertex);
                    case PolygonCulling.Edge:
                        return TriangleFilterUtility.OutOfPolygonStruct.OutOfPolygonEdgeBase(targetTri, ccsSpace.QuadNormalizedVertex, MaxRange, MinRange, IsAllVertex);
                    case PolygonCulling.EdgeAndCenterRay:
                        return TriangleFilterUtility.OutOfPolygonStruct.OutOfPolygonEdgeEdgeAndCenterRayCast(targetTri, ccsSpace.QuadNormalizedVertex, MaxRange, MinRange, IsAllVertex);
                }

            }


        }
    }
}
#endif
