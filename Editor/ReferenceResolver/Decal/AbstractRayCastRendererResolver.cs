#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEditor;

namespace net.rs64.TexTransTool.ReferenceResolver.ATResolver
{
    [DisallowMultipleComponent]
    public abstract class AbstractRayCastRendererResolver : AbstractResolver
    {
        public float RayCastRange = 1f;

        public List<Renderer> FindRayCast(GameObject FindRoot)
        {
            var hits = new List<Renderer>();

            foreach (var renderer in FindRoot.GetComponentsInChildren<Renderer>())
            {
                var souseMesh = renderer.GetMesh();
                if (souseMesh == null) { continue; }
                var vertex = DecalUtility.GetWorldSpaceVertices(renderer);
                var triangle = souseMesh.GetTriangleIndex();

                var ray = new Ray(transform.position, transform.forward);

                var hitTriangles = IslandCulling.RayCast(ray, vertex, triangle);

                var count = 0;
                foreach (var hitTriangle in hitTriangles)
                {
                    if (hitTriangle.Distance < 0) { continue; }
                    if (hitTriangle.Distance > RayCastRange) { continue; }
                    count += 1;
                }

                if (count > 0) { hits.Add(renderer); }
            }

            return hits;
        }

        public void AddToDecal(AbstractDecal abstractDecal, List<Renderer> renderers)
        {
            var rendererHash = new HashSet<Renderer>(abstractDecal.TargetRenderers);
            foreach (var renderer in renderers)
            {
                if (rendererHash.Contains(renderer)) { continue; }

                abstractDecal.TargetRenderers.Add(renderer);
                rendererHash.Add(renderer);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawLine(transform.position, transform.position + (transform.forward * RayCastRange));
        }
    }
}
#endif