#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.Decal.Cylindrical
{
    [AddComponentMenu("TexTransTool/CylindricalCoordinatesSystem")]
    public class CylindricalCoordinatesSystem : MonoBehaviour, ITexTransToolTag
    {
        public float GizmoRadius = 0.25f;
        public float GizmoHeight = 1f;

        [HideInInspector,SerializeField] int _saveDataVersion = Utils.ThiSaveDataVersion;
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
            var LoaclaPoint = transform.worldToLocalMatrix.MultiplyPoint(Point);
            var Height = LoaclaPoint.y;
            var Angle = Mathf.Atan2(LoaclaPoint.x, LoaclaPoint.z) * Mathf.Rad2Deg;
            var Distance = new Vector2(LoaclaPoint.x, LoaclaPoint.z).magnitude;

            return new Vector3(Height, Angle, Distance);
        }

        public Vector3 GetWorldPoint(Vector3 CCSPoint)
        {
            var Height = CCSPoint.x;
            var Angle = CCSPoint.y * Mathf.Deg2Rad;
            var Distance = CCSPoint.z;

            var x = Mathf.Sin(Angle) * Distance;
            var z = Mathf.Cos(Angle) * Distance;

            return transform.localToWorldMatrix.MultiplyPoint(new Vector3(x, Height, z));
        }

        public static float OffsetAngle(float angle, float offset)
        {
            var a = angle + offset;
            if (a > 180) a -= 360;
            if (a < -180) a += 360;
            return a;
        }

        public static float InversPosiNega(float angle)
        {
            return angle + (Mathf.Sign(angle) * -360);
        }

        public List<Vector3> VartexsConvertCCS(IReadOnlyList<Vector3> Vartexs, float OffSetAngle = 0)
        {
            List<Vector3> Convertvarts = new List<Vector3>();
            foreach (var i in Vartexs)
            {
                var CCSPoint = GetCCSPoint(i);
                Convertvarts.Add(new Vector3(CCSPoint.x, OffsetAngle(CCSPoint.y, OffSetAngle), CCSPoint.z));
            }

            return Convertvarts;
        }

        public static void OffSetApply(List<Vector3> Varts, float Offset)
        {
            for (int i = 0; i < Varts.Count; i++)
            {
                Varts[i] = new Vector3(Varts[i].x, OffsetAngle(Varts[i].y, Offset), Varts[i].z);
            }
        }


        public static int GetPositiveCount(TriangleIndex Tri, List<Vector3> Varts)
        {
            var PositiveCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (Varts[Tri[i]].y > 0) PositiveCount += 1;
            }

            return PositiveCount;
        }

        public static void HeightScaleFactor(List<Vector3> CCSVarts, float factor = 100)
        {
            for (int i = 0; i < CCSVarts.Count; i++)
            {
                CCSVarts[i] = new Vector3(CCSVarts[i].x * factor, CCSVarts[i].y, CCSVarts[i].z);
            }
        }

    }


    public class CCSSpace : DecalUtil.IConvertSpace
    {
        public CylindricalCoordinatesSystem CCS;
        public IReadOnlyList<Vector3> Quad;
        public List<Vector3> CCSvarts;
        public List<Vector3> CCSQuad;
        public float Offset;
        public List<Vector3> QuadNormalizedVarts;

        public CCSSpace(CylindricalCoordinatesSystem CCS, IReadOnlyList<Vector3> Quad)
        {
            this.CCS = CCS;
            this.Quad = Quad;
        }

        public void Input(DecalUtil.MeshDatas Meshdatas)
        {
            var cCSQuad = CCS.VartexsConvertCCS(Quad);
            var cCSVartexs = CCS.VartexsConvertCCS(Meshdatas.Varticals);
            var offset = cCSQuad.Min(I => I.y) * -1;

            CylindricalCoordinatesSystem.OffSetApply(cCSQuad, offset);
            CylindricalCoordinatesSystem.OffSetApply(cCSVartexs, offset);

            //円柱座標系での高さの値を大きくするとQuadNormalaizeの精度が上がる。
            CylindricalCoordinatesSystem.HeightScaleFactor(cCSVartexs);
            CylindricalCoordinatesSystem.HeightScaleFactor(cCSQuad);

            Offset = offset;
            var Normalized = DecalUtil.QuadNormaliz(cCSQuad.ConvertAll(i => (Vector2)i), cCSVartexs.ConvertAll(i => (Vector2)i));
            QuadNormalizedVarts = Utils.ZipListVector3(Normalized, cCSVartexs.ConvertAll(i => i.z));

            CCSvarts = cCSVartexs;
            CCSQuad = cCSQuad;
        }

        public List<Vector2> OutPutUV()
        {
            var UV = new List<Vector2>(QuadNormalizedVarts.Capacity);
            foreach (var i in QuadNormalizedVarts)
            {
                UV.Add(i);
            }
            return UV;
        }
    }

    public class CCSFilter : DecalUtil.ITrianglesFilter<CCSSpace>
    {
        public IReadOnlyList<TriangleFilterUtils.ITriangleFiltaring<CCSSpace>> Filters;

        public CCSFilter(IReadOnlyList<TriangleFilterUtils.ITriangleFiltaring<CCSSpace>> filters)
        {
            Filters = filters;
        }
        public List<TriangleIndex> Filtering(CCSSpace Spase, List<TriangleIndex> Trianglees)
        {
            return TriangleFilterUtils.FiltaringTriangle(Trianglees, Spase, Filters);
        }


        public struct InDistansStruct : TriangleFilterUtils.ITriangleFiltaring<CCSSpace>
        {
            public float Near;
            public bool IsAllVartex;

            public InDistansStruct(float Near, bool IsAllVartex)
            {
                this.Near = Near;
                this.IsAllVartex = IsAllVartex;
            }


            public bool Filtering(TriangleIndex tri, CCSSpace Space)
            {
                return TriangleFilterUtils.NearStruct.NearClip(tri, Space.QuadNormalizedVarts, Near, IsAllVartex);
            }
        }

        public struct OutDistansStruct : TriangleFilterUtils.ITriangleFiltaring<CCSSpace>
        {
            public float Far;
            public bool IsAllVartex;

            public OutDistansStruct(float Far, bool IsAllVartex)
            {
                this.Far = Far;
                this.IsAllVartex = IsAllVartex;
            }

            public bool Filtering(TriangleIndex tri, CCSSpace Space)
            {
                return TriangleFilterUtils.FarStruct.FarClip(tri, Space.QuadNormalizedVarts, Far, IsAllVartex);
            }
        }

        public struct SideStruct : TriangleFilterUtils.ITriangleFiltaring<CCSSpace>
        {
            public bool IsRevaers;

            public SideStruct(bool isRevaers)
            {
                IsRevaers = isRevaers;
            }

            public bool Filtering(TriangleIndex tri, CCSSpace Space)
            {
                return TriangleFilterUtils.SideStruct.SideChek(tri, Space.QuadNormalizedVarts, IsRevaers);
            }
        }

        public struct BorderOnPorygonStruct : TriangleFilterUtils.ITriangleFiltaring<CCSSpace>
        {
            public float Threshold /*= 150f */;

            public BorderOnPorygonStruct(float threshold)
            {
                Threshold = threshold;
            }

            public bool Filtering(TriangleIndex tri, CCSSpace Space)
            {
                return BorderOnPorygon(tri, Space.CCSvarts, Threshold);
            }

            public static bool BorderOnPorygon(TriangleIndex tri, List<Vector3> CCSvarts, float threshold = 150)
            {
                var CCStri = tri.GetTriangle(CCSvarts);

                var PosiCount = CylindricalCoordinatesSystem.GetPositiveCount(tri, CCSvarts);

                if (PosiCount == 0 || PosiCount == 3) return false;

                var min = CCStri.Min(I => I.y);
                var max = CCStri.Max(I => I.y);

                return (max - min) > threshold;
            }
        }
        public struct OutOfPorigonStruct : TriangleFilterUtils.ITriangleFiltaring<CCSSpace>
        {
            public PolygonCulling PolygonCaling;
            public float MinRange;
            public float MaxRange;
            public bool IsAllVartex;

            public OutOfPorigonStruct(PolygonCulling polygonCaling, float OutOfRangeOffset, bool isAllVartex)
            {
                PolygonCaling = polygonCaling;
                MinRange = 0 - OutOfRangeOffset;
                MaxRange = 1 + OutOfRangeOffset;
                IsAllVartex = isAllVartex;

            }

            public bool Filtering(TriangleIndex TargetTri, CCSSpace CCSSpase)
            {
                switch (PolygonCaling)
                {
                    default:
                    case PolygonCulling.Vartex:
                        return TriangleFilterUtils.OutOfPorigonStruct.OutOfPorigonVartexBase(TargetTri, CCSSpase.QuadNormalizedVarts, MaxRange, MinRange, IsAllVartex);
                    case PolygonCulling.Edge:
                        return TriangleFilterUtils.OutOfPorigonStruct.OutOfPorigonEdgeBase(TargetTri, CCSSpase.QuadNormalizedVarts, MaxRange, MinRange, IsAllVartex);
                    case PolygonCulling.EdgeAndCenterRay:
                        return TriangleFilterUtils.OutOfPorigonStruct.OutOfPorigonEdgeEdgeAndCenterRayCast(TargetTri, CCSSpase.QuadNormalizedVarts, MaxRange, MinRange, IsAllVartex);
                }

            }


        }
    }
}
#endif
