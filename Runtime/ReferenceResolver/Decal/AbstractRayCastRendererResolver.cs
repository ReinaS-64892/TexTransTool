using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.Decal;
using UnityEngine;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.Utils;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.ReferenceResolver.ATResolver
{
    [DisallowMultipleComponent]
    internal abstract class AbstractRayCastRendererResolver : AbstractResolver
    {
        public float RayCastRange = 1f;

        public List<Renderer> FindRayCast(GameObject findRoot)
        {
            var hits = new List<Renderer>();

            foreach (var renderer in findRoot.GetComponentsInChildren<Renderer>())
            {
                var souseMesh = renderer.GetMesh();
                if (souseMesh == null) { continue; }

                var meshdata = renderer.Memo(MeshData.GetMeshData);

                var ray = new Ray(transform.position, transform.forward);

                var hitTriangles = IslandCulling.RayCast(ray, meshdata);

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

        public void AddToDecal(SimpleDecal abstractDecal, List<Renderer> renderers)
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
