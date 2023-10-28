#if UNITY_EDITOR
using System.Diagnostics.SymbolStore;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu("TexTransTool/TTT SimpleDecal")]
    public class SimpleDecal : AbstractSingleDecal<ParallelProjectionSpace>
    {
        public bool FixedAspect = true;
        [FormerlySerializedAs("SideChek")] public bool SideCulling = true;
        [FormerlySerializedAs("PolygonCaling")] public PolygonCulling PolygonCulling = PolygonCulling.Vertex;

        public override ParallelProjectionSpace GetSpaceConverter => new ParallelProjectionSpace(transform.worldToLocalMatrix);
        public override DecalUtility.ITrianglesFilter<ParallelProjectionSpace> GetTriangleFilter
        {
            get
            {
                if (IslandCulling) { return new IslandCullingPPFilter(GetFilter(), GetIslandSelector(), new EditorIsland.EditorIslandCache()); }
                else { return new ParallelProjectionFilter(GetFilter()); }
            }
        }

        public bool IslandCulling = false;
        public Vector2 IslandSelectorPos = new Vector2(0.5f, 0.5f);
        public float IslandSelectorRange = 1;
        public List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> GetFilter()
        {
            var filters = new List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>>
            {
                new TriangleFilterUtility.FarStruct(1, false),
                new TriangleFilterUtility.NearStruct(0, true)
            };
            if (SideCulling) filters.Add(new TriangleFilterUtility.SideStruct());
            filters.Add(new TriangleFilterUtility.OutOfPolygonStruct(PolygonCulling, 0, 1, true));

            return filters;
        }

        public List<IslandSelector> GetIslandSelector()
        {
            if (!IslandCulling) return null;
            return new List<IslandSelector>() {
                new IslandSelector(new Ray(transform.localToWorldMatrix.MultiplyPoint3x4(IslandSelectorPos - new Vector2(0.5f, 0.5f)), transform.forward), transform.localScale.z * IslandSelectorRange)
                };
        }

        protected virtual void OnDrawGizmosSelected()
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



#endif
