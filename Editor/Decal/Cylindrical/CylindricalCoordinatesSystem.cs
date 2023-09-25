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
    public class CylindricalCoordinatesSystem : MonoBehaviour, ITexTransToolTag
    {
        public float GizmoRadius = 0.25f;
        public float GizmoHeight = 1f;

        [HideInInspector,SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
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

        public Vector3 GetCCSPoint(Vector3 Point)
        {
            var localPoint = transform.worldToLocalMatrix.MultiplyPoint(Point);
            var height = localPoint.y;
            var angle = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg;
            var distance = new Vector2(localPoint.x, localPoint.z).magnitude;

            return new Vector3(height, angle, distance);
        }

        public Vector3 GetWorldPoint(Vector3 CCSPoint)
        {
            var height = CCSPoint.x;
            var angle = CCSPoint.y * Mathf.Deg2Rad;
            var distance = CCSPoint.z;

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

        public List<Vector3> VertexConvertCCS(IReadOnlyList<Vector3> Vertex, float OffSetAngle = 0)
        {
            List<Vector3> convertVertex = new List<Vector3>();
            foreach (var i in Vertex)
            {
                var CCSPoint = GetCCSPoint(i);
                convertVertex.Add(new Vector3(CCSPoint.x, OffsetAngle(CCSPoint.y, OffSetAngle), CCSPoint.z));
            }

            return convertVertex;
        }

        public static void OffSetApply(List<Vector3> vertex, float Offset)
        {
            for (int i = 0; i < vertex.Count; i++)
            {
                vertex[i] = new Vector3(vertex[i].x, OffsetAngle(vertex[i].y, Offset), vertex[i].z);
            }
        }


        public static int GetPositiveCount(TriangleIndex Tri, List<Vector3> vertex)
        {
            var positiveCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (vertex[Tri[i]].y > 0) positiveCount += 1;
            }

            return positiveCount;
        }

        public static void HeightScaleFactor(List<Vector3> CCSVertex, float factor = 100)
        {
            for (int i = 0; i < CCSVertex.Count; i++)
            {
                CCSVertex[i] = new Vector3(CCSVertex[i].x * factor, CCSVertex[i].y, CCSVertex[i].z);
            }
        }

    }


    public class CCSSpace : DecalUtility.IConvertSpace
    {
        public CylindricalCoordinatesSystem CCS;
        public IReadOnlyList<Vector3> Quad;
        public List<Vector3> CCSVertex;
        public List<Vector3> CCSQuad;
        public float Offset;
        public List<Vector3> QuadNormalizedVertex;

        public CCSSpace(CylindricalCoordinatesSystem CCS, IReadOnlyList<Vector3> Quad)
        {
            this.CCS = CCS;
            this.Quad = Quad;
        }

        public void Input(DecalUtility.MeshData MeshData)
        {
            var CCSQuad = CCS.VertexConvertCCS(Quad);
            var CCSVertex = CCS.VertexConvertCCS(MeshData.Vertex);
            var offset = CCSQuad.Min(I => I.y) * -1;

            CylindricalCoordinatesSystem.OffSetApply(CCSQuad, offset);
            CylindricalCoordinatesSystem.OffSetApply(CCSVertex, offset);

            //円柱座標系での高さの値を大きくするとQuadNormalizeの精度が上がる。
            CylindricalCoordinatesSystem.HeightScaleFactor(CCSVertex);
            CylindricalCoordinatesSystem.HeightScaleFactor(CCSQuad);

            Offset = offset;
            var Normalized = DecalUtility.QuadNormalize(CCSQuad.ConvertAll(i => (Vector2)i), CCSVertex.ConvertAll(i => (Vector2)i));
            QuadNormalizedVertex = CollectionsUtility.ZipListVector3(Normalized, CCSVertex.ConvertAll(i => i.z));

            this.CCSVertex = CCSVertex;
            this.CCSQuad = CCSQuad;
        }

        public List<Vector2> OutPutUV()
        {
            var UV = new List<Vector2>(QuadNormalizedVertex.Capacity);
            foreach (var i in QuadNormalizedVertex)
            {
                UV.Add(i);
            }
            return UV;
        }
    }

    public class CCSFilter : DecalUtility.ITrianglesFilter<CCSSpace>
    {
        public IReadOnlyList<TriangleFilterUtility.ITriangleFiltering<CCSSpace>> Filters;

        public CCSFilter(IReadOnlyList<TriangleFilterUtility.ITriangleFiltering<CCSSpace>> filters)
        {
            Filters = filters;
        }
        public List<TriangleIndex> Filtering(CCSSpace Space, List<TriangleIndex> Triangles)
        {
            return TriangleFilterUtility.FilteringTriangle(Triangles, Space, Filters);
        }


        public struct InDistanceStruct : TriangleFilterUtility.ITriangleFiltering<CCSSpace>
        {
            public float Near;
            public bool IsAllVertex;

            public InDistanceStruct(float Near, bool IsAllVertex)
            {
                this.Near = Near;
                this.IsAllVertex = IsAllVertex;
            }


            public bool Filtering(TriangleIndex tri, CCSSpace Space)
            {
                return TriangleFilterUtility.NearStruct.NearClip(tri, Space.QuadNormalizedVertex, Near, IsAllVertex);
            }
        }

        public struct OutDistanceStruct : TriangleFilterUtility.ITriangleFiltering<CCSSpace>
        {
            public float Far;
            public bool IsAllVertex;

            public OutDistanceStruct(float Far, bool IsAllVertex)
            {
                this.Far = Far;
                this.IsAllVertex = IsAllVertex;
            }

            public bool Filtering(TriangleIndex tri, CCSSpace Space)
            {
                return TriangleFilterUtility.FarStruct.FarClip(tri, Space.QuadNormalizedVertex, Far, IsAllVertex);
            }
        }

        public struct SideStruct : TriangleFilterUtility.ITriangleFiltering<CCSSpace>
        {
            public bool IsReverse;

            public SideStruct(bool isReverse)
            {
                IsReverse = isReverse;
            }

            public bool Filtering(TriangleIndex tri, CCSSpace Space)
            {
                return TriangleFilterUtility.SideStruct.SideCheck(tri, Space.QuadNormalizedVertex, IsReverse);
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

            public static bool BorderOnPolygon(TriangleIndex tri, List<Vector3> CCSVertex, float threshold = 150)
            {
                var CCStri = tri.GetTriangle(CCSVertex);

                var positiveCount = CylindricalCoordinatesSystem.GetPositiveCount(tri, CCSVertex);

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

            public OutOfPerigonStruct(PolygonCulling polygonCulling, float OutOfRangeOffset, bool isAllVertex)
            {
                PolygonCulling = polygonCulling;
                MinRange = 0 - OutOfRangeOffset;
                MaxRange = 1 + OutOfRangeOffset;
                IsAllVertex = isAllVertex;

            }

            public bool Filtering(TriangleIndex TargetTri, CCSSpace CCSSpace)
            {
                switch (PolygonCulling)
                {
                    default:
                    case PolygonCulling.Vertex:
                        return TriangleFilterUtility.OutOfPolygonStruct.OutOfPolygonVertexBase(TargetTri, CCSSpace.QuadNormalizedVertex, MaxRange, MinRange, IsAllVertex);
                    case PolygonCulling.Edge:
                        return TriangleFilterUtility.OutOfPolygonStruct.OutOfPolygonEdgeBase(TargetTri, CCSSpace.QuadNormalizedVertex, MaxRange, MinRange, IsAllVertex);
                    case PolygonCulling.EdgeAndCenterRay:
                        return TriangleFilterUtility.OutOfPolygonStruct.OutOfPolygonEdgeEdgeAndCenterRayCast(TargetTri, CCSSpace.QuadNormalizedVertex, MaxRange, MinRange, IsAllVertex);
                }

            }


        }
    }
}
#endif
