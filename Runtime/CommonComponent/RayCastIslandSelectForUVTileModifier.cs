using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransCore.Island;
using UnityEngine;
using UnityEngine.Pool;
using net.rs64.TexTransTool.TextureAtlas;
using Unity.Collections;
using System;
using net.rs64.TexTransCore.TransTextureCore;

namespace net.rs64.TexTransTool
{

    [AddComponentMenu("TexTransTool/Other/TTT RayCastIslandSelectForUVTileModifier")]
    public class RayCastIslandSelectForUVTileModifier : TexTransRuntimeBehavior
    {
        public GameObject TargetRoot;
        public List<Renderer> Renderers => TargetRoot.GetComponentsInChildren<Renderer>(true).Where(RendererFilter).ToList();

        internal override bool IsPossibleApply => TargetRoot != null;
        internal override List<Renderer> GetRenderers => Renderers;
        internal override TexTransPhase PhaseDefine => TexTransPhase.UVModification;

        public Vector2Int Tile;
        public float RayCastRange = 1f;


        internal override void Apply([NotNull] IDomain domain)
        {

            foreach (var renderer in Renderers)
            {
                var souseMesh = renderer.GetMesh();
                if (souseMesh == null) { continue; }
                var vertex = DecalUtility.GetWorldSpaceVertices(renderer);
                var triangle = souseMesh.GetTriangleIndex();
                var uv = souseMesh.GetUVList(0, ListPool<Vector2>.Get());

                var ray = new Ray(transform.position, transform.forward);
                var targetCullied = ListPool<TriangleIndex>.Get();
                targetCullied = IslandCulling.Culling(new List<IslandSelector>() { new(ray, RayCastRange) }, vertex, uv, triangle, domain.GetIslandCacheManager(), targetCullied);



                if (targetCullied.Any())
                {
                    var editableMesh = UnityEngine.Object.Instantiate(souseMesh);
                    editableMesh.name += "_TileModified";

                    var mvUV = ListPool<Vector2>.Get();
                    mvUV.AddRange(uv);

                    foreach (var tri in targetCullied)
                    {
                        foreach (var index in tri)
                        {
                            mvUV[index] = TileSet(uv[index], Tile);
                        }
                    }

                    editableMesh.SetUVs(0, mvUV);
                    domain.SetMesh(renderer, editableMesh);

                    ListPool<Vector2>.Release(mvUV);
                }
                ListPool<TriangleIndex>.Release(targetCullied);
                ListPool<Vector2>.Release(uv);
            }

        }

        private Vector2 TileSet(Vector2 vector2, Vector2Int tile)
        {
            vector2.x = vector2.x - Mathf.Floor(vector2.x) + tile.x;
            vector2.y = vector2.y - Mathf.Floor(vector2.y) + tile.y;
            return vector2;
        }

        internal static bool RendererFilter(Renderer renderer)
        {
            if (renderer.tag == "EditorOnly") { return false; }
            if (renderer.GetMesh() == null) { return false; }
            if (renderer.GetMesh().uv.Any() == false) { return false; }
            return true;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, transform.position + (transform.forward * RayCastRange));
        }
    }
}