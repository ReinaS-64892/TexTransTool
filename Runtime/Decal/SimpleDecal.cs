using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.IslandSelector;
using System;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class SimpleDecal : AbstractSingleDecal<ParallelProjectionSpace, Vector3>
    {
        internal const string ComponentName = "TTT SimpleDecal";
        internal const string MenuPath = ComponentName;
        public bool FixedAspect = true;
        [FormerlySerializedAs("SideChek")] public bool SideCulling = true;
        [FormerlySerializedAs("PolygonCaling")] public PolygonCulling PolygonCulling = PolygonCulling.Vertex;

        public AbstractIslandSelector IslandSelector;

        public bool UseDepth;
        public bool DepthInvert;
        internal override bool? GetUseDepthOrInvert => UseDepth ? new bool?(DepthInvert) : null;

        //次のマイナーで obsolete にする
        [SerializeField] internal bool IslandCulling = false;
        [SerializeField] internal Vector2 IslandSelectorPos = new Vector2(0.5f, 0.5f);
        [SerializeField] internal float IslandSelectorRange = 1;



        internal override ParallelProjectionSpace GetSpaceConverter() { return new ParallelProjectionSpace(transform.worldToLocalMatrix); }
        internal override DecalUtility.ITrianglesFilter<ParallelProjectionSpace> GetTriangleFilter()
        {
            if (IslandSelector != null) { return new IslandSelectToPPFilter(IslandSelector, GetFilter()); }
            return new ParallelProjectionFilter(GetFilter());

        }

        internal List<TriangleFilterUtility.ITriangleFiltering<IList<Vector3>>> GetFilter()
        {
            var filters = new List<TriangleFilterUtility.ITriangleFiltering<IList<Vector3>>>
            {
                new TriangleFilterUtility.FarStruct(1, true),
                new TriangleFilterUtility.NearStruct(0, true)
            };
            if (SideCulling) filters.Add(new TriangleFilterUtility.SideStruct());
            filters.Add(new TriangleFilterUtility.OutOfPolygonStruct(PolygonCulling, 0, 1, true));

            return filters;
        }

        internal void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.black;
            var matrix = transform.localToWorldMatrix;

            Gizmos.matrix = matrix;

            var centerPos = Vector3.zero;
            Gizmos.DrawWireCube(centerPos + new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));//基準となる四角形


            DecalGizmoUtility.DrawGizmoQuad(DecalTexture, Color, matrix);
        }
    }
}
