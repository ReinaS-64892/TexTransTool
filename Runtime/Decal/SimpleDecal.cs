using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu("TexTransTool/TTT SimpleDecal")]
    public sealed class SimpleDecal : AbstractSingleDecal<ParallelProjectionSpace, Vector3>
    {
        public bool FixedAspect = true;
        [FormerlySerializedAs("SideChek")] public bool SideCulling = true;
        [FormerlySerializedAs("PolygonCaling")] public PolygonCulling PolygonCulling = PolygonCulling.Vertex;

        public bool IslandCulling = false;
        public Vector2 IslandSelectorPos = new Vector2(0.5f, 0.5f);
        public float IslandSelectorRange = 1;

        public bool UseDepth;
        public bool DepthInvert;
        internal override bool? GetUseDepthOrInvert => UseDepth ? new bool?(DepthInvert) : null;
        internal override ParallelProjectionSpace GetSpaceConverter(IIslandCache islandCacheManager) { return new ParallelProjectionSpace(transform.worldToLocalMatrix); }
        internal override DecalUtility.ITrianglesFilter<ParallelProjectionSpace> GetTriangleFilter(IIslandCache islandCacheManager)
        {
            if (IslandCulling) { return new IslandCullingPPFilter<Vector2>(GetFilter(), GetIslandSelector(), islandCacheManager); }
            else { return new ParallelProjectionFilter<Vector2>(GetFilter()); }
        }

        internal List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> GetFilter()
        {
            var filters = new List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>>
            {
                new TriangleFilterUtility.FarStruct(1, true),
                new TriangleFilterUtility.NearStruct(0, true)
            };
            if (SideCulling) filters.Add(new TriangleFilterUtility.SideStruct());
            filters.Add(new TriangleFilterUtility.OutOfPolygonStruct(PolygonCulling, 0, 1, true));

            return filters;
        }

        internal List<IslandSelector> GetIslandSelector()
        {
            if (!IslandCulling) return null;
            return new List<IslandSelector>() {
                new IslandSelector(new Ray(transform.localToWorldMatrix.MultiplyPoint3x4(IslandSelectorPos - new Vector2(0.5f, 0.5f)), transform.forward), transform.localScale.z * IslandSelectorRange)
                };
        }

        internal void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.black;
            var matrix = transform.localToWorldMatrix;

            Gizmos.matrix = matrix;

            var centerPos = Vector3.zero;
            Gizmos.DrawWireCube(centerPos + new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));//基準となる四角形


            DecalGizmoUtility.DrawGizmoQuad(DecalTexture, Color, matrix);

            if (IslandCulling)
            {
                Vector3 selectorOrigin = new Vector2(IslandSelectorPos.x - 0.5f, IslandSelectorPos.y - 0.5f);
                var selectorTail = (Vector3.forward * IslandSelectorRange) + selectorOrigin;
                Gizmos.DrawLine(selectorOrigin, selectorTail);
            }
        }
    }
}
