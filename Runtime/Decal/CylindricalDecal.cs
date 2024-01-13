using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransTool.Decal.Cylindrical;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Serialization;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu("TexTransTool/OtherDecal/Cylindrical/TTT CylindricalDecal")]
    internal sealed class CylindricalDecal : AbstractSingleDecal<CCSSpace, Vector2>
    {
        [FormerlySerializedAs("cylindricalCoordinatesSystem")] public CylindricalCoordinatesSystem CylindricalCoordinatesSystem;
        public bool FixedAspect = true;
        public Vector2 Scale = Vector2.one;
        public bool SideCulling = true;
        public float InDistanceCulling = 1f;
        public float OutDistanceCulling = 1f;
        public float OutOfRangeOffset = 1f;

        internal override CCSSpace GetSpaceConverter(IIslandCache islandCacheManager) => new CCSSpace(CylindricalCoordinatesSystem, GetQuad());
        internal override DecalUtility.ITrianglesFilter<CCSSpace> GetTriangleFilter(IIslandCache islandCacheManager) => new CCSFilter(GetFilters());

        private List<TriangleFilterUtility.ITriangleFiltering<CCSSpace>> GetFilters()
        {
            var filters = new List<TriangleFilterUtility.ITriangleFiltering<CCSSpace>>
            {
                new CCSFilter.BorderOnPolygonStruct(),
                new CCSFilter.OutOfPerigonStruct(PolygonCulling.Edge, OutOfRangeOffset, false)
            };

            var thisCCSZ = CylindricalCoordinatesSystem.GetCCSPoint(transform.position).z;

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
            if (CylindricalCoordinatesSystem == null) { return; }
            Gizmos.color = Color.black;
            var Matrix = Matrix4x4.identity;
            Gizmos.matrix = Matrix;


            var quad = GetQuad();

            foreach (var fromPoint in quad)
            {
                var ccsPoint = CylindricalCoordinatesSystem.GetCCSPoint(fromPoint);
                ccsPoint.z = Mathf.Max(ccsPoint.z - InDistanceCulling, 0f);
                var offSetToPoint = CylindricalCoordinatesSystem.GetWorldPoint(ccsPoint);

                var ccsFromPoint = CylindricalCoordinatesSystem.GetCCSPoint(fromPoint);
                ccsFromPoint.z += OutDistanceCulling;
                var OffSetFromPoint = CylindricalCoordinatesSystem.GetWorldPoint(ccsFromPoint);

                Gizmos.DrawLine(OffSetFromPoint, offSetToPoint);
            }

            for (int count = 0; 4 > count; count += 1)
            {
                var (From, To) = GetEdge(quad, count);
                Gizmos.DrawLine(From, To);
            }

            DecalGizmoUtility.DrawGizmoQuad(DecalTexture, Color, transform.localToWorldMatrix);

        }
        public static (Vector3, Vector3) GetEdge(IReadOnlyList<Vector3> quad, int Count)
        {
            switch (Count)
            {
                default:
                case 0:
                    {
                        return (quad[0], quad[1]);
                    }
                case 1:
                    {
                        return (quad[0], quad[2]);
                    }
                case 2:
                    {
                        return (quad[2], quad[3]);
                    }
                case 3:
                    {
                        return (quad[1], quad[3]);
                    }
            }
        }

    }
}
