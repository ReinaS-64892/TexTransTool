#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEngine;


namespace net.rs64.TexTransTool
{
    [Serializable]
    public class RenderersBackup : IDisposable
    {
        [SerializeField] List<Renderer> _renderers;

        [SerializeField] List<Material> _initMaterial;
        [SerializeField] List<Mesh> _initMesh;

        public RenderersBackup(List<Renderer> renderers)
        {
            _renderers = new List<Renderer>(renderers);
            _initMaterial = RendererUtility.GetMaterials(_renderers);
            _initMesh = RendererUtility.GetMeshes(_renderers);
        }

        public void Dispose()
        {
            RendererUtility.SetMaterials(_renderers, _initMaterial);
            RendererUtility.SetMeshes(_renderers, _initMesh);
        }

    }
}
#endif