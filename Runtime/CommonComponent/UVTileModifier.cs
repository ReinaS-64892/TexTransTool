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

namespace net.rs64.TexTransTool
{
    public class UVTileModifier : TexTransRuntimeBehavior
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

                var ray = new Ray(transform.position, transform.forward);
                var hitTriangles = IslandCulling.RayCast(ray, vertex, triangle, ListPool<IslandCulling.RayCastHitTriangle>.Get());

                hitTriangles.RemoveAll(FilterTriangle);

                if (hitTriangles.Any())
                {
                    var editableMesh = UnityEngine.Object.Instantiate(souseMesh);

                    var uv = editableMesh.GetUVList(0, ListPool<Vector2>.Get());
                    var mvUV = ListPool<Vector2>.Get();
                    mvUV.AddRange(uv);

                    foreach (var tri in hitTriangles)
                    {
                        foreach (var index in tri.Triangle)
                        {
                            mvUV[index] = TileSet(uv[index], Tile);
                        }
                    }

                    editableMesh.SetUVs(0, mvUV);

                    ListPool<Vector2>.Release(uv);
                    ListPool<Vector2>.Release(mvUV);

                    domain.SetMesh(renderer, editableMesh);
                }
                ListPool<IslandCulling.RayCastHitTriangle>.Release(hitTriangles);


                bool FilterTriangle(IslandCulling.RayCastHitTriangle rayCastHitTriangle)
                {
                    if (rayCastHitTriangle.Distance < 0) { return true; }
                    if (rayCastHitTriangle.Distance > RayCastRange) { return true; }
                    return false;
                }
            }

        }

        private Vector2 TileSet(Vector2 vector2, Vector2Int tile)
        {
            vector2.x = vector2.x - Mathf.Floor(vector2.x) + tile.x;
            vector2.y = vector2.y - Mathf.Floor(vector2.y) + tile.x;
            return vector2;
        }

        internal static bool RendererFilter(Renderer renderer)
        {
            if (renderer.tag == "EditorOnly") { return true; }
            if (renderer.GetMesh() == null) { return true; }
            if (renderer.GetMesh().uv.Any() == false) { return true; }
            return false;
        }
    }
}