#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransTool.Decal.Cylindrical;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu("TexTransTool/TTT CylindricalDecal")]
    internal class CylindricalDecal : AbstractSingleDecal<CCSSpace,Vector2>
    {
        public CylindricalCoordinatesSystem cylindricalCoordinatesSystem;
        public bool FixedAspect = true;
        public Vector2 Scale = Vector2.one;
        public bool SideCulling = true;
        public float InDistanceCulling = 1f;
        public float OutDistanceCulling = 1f;
        public float OutOfRangeOffset = 1f;

        public override CCSSpace GetSpaceConverter => new CCSSpace(cylindricalCoordinatesSystem, GetQuad());
        public override DecalUtility.ITrianglesFilter<CCSSpace> GetTriangleFilter => new CCSFilter(GetFilters());

        private List<TriangleFilterUtility.ITriangleFiltering<CCSSpace>> GetFilters()
        {
            var filters = new List<TriangleFilterUtility.ITriangleFiltering<CCSSpace>>
            {
                new CCSFilter.BorderOnPolygonStruct(),
                new CCSFilter.OutOfPerigonStruct(PolygonCulling.Edge, OutOfRangeOffset, false)
            };

            var thisCCSZ = cylindricalCoordinatesSystem.GetCCSPoint(transform.position).z;

            filters.Add(new CCSFilter.OutDistanceStruct(OutDistanceCulling + thisCCSZ, false));
            filters.Add(new CCSFilter.InDistanceStruct(Mathf.Max(thisCCSZ - InDistanceCulling, 0f), false));
            if (SideCulling) filters.Add(new CCSFilter.SideStruct());

            return filters;
        }

        public static readonly Vector3[] LocalQuad = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
        };


        public List<Vector3> GetQuad()
        {
            var matrix = transform.localToWorldMatrix;
            var worldSpaceQuad = new List<Vector3>(4);
            foreach (var i in LocalQuad)
            {
                worldSpaceQuad.Add(matrix.MultiplyPoint(i));
            }
            return worldSpaceQuad;
        }

        void OnDrawGizmosSelected()
        {
            if (cylindricalCoordinatesSystem == null) { return; }
            Gizmos.color = Color.black;
            var Matrix = Matrix4x4.identity;
            Gizmos.matrix = Matrix;


            var quad = GetQuad();

            foreach (var fromPoint in quad)
            {
                var CCSPoint = cylindricalCoordinatesSystem.GetCCSPoint(fromPoint);
                CCSPoint.z = Mathf.Max(CCSPoint.z - InDistanceCulling, 0f);
                var offSetToPoint = cylindricalCoordinatesSystem.GetWorldPoint(CCSPoint);

                var CCSFromPoint = cylindricalCoordinatesSystem.GetCCSPoint(fromPoint);
                CCSFromPoint.z += OutDistanceCulling;
                var OffSetFromPoint = cylindricalCoordinatesSystem.GetWorldPoint(CCSFromPoint);

                Gizmos.DrawLine(OffSetFromPoint, offSetToPoint);
            }

            for (int count = 0; 4 > count; count += 1)
            {
                var (From, To) = GetEdge(quad, count);
                Gizmos.DrawLine(From, To);
            }

            DecalGizmoUtility.DrawGizmoQuad(DecalTexture, Color, transform.localToWorldMatrix);

        }
        public static (Vector3, Vector3) GetEdge(IReadOnlyList<Vector3> Quad, int Count)
        {
            switch (Count)
            {
                default:
                case 0:
                    {
                        return (Quad[0], Quad[1]);
                    }
                case 1:
                    {
                        return (Quad[0], Quad[2]);
                    }
                case 2:
                    {
                        return (Quad[2], Quad[3]);
                    }
                case 3:
                    {
                        return (Quad[1], Quad[3]);
                    }
            }
        }

    }
}
#endif
