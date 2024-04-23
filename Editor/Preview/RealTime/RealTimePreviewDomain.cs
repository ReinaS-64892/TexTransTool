using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Decal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Profiling;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool.Preview.RealTime
{
    internal class RealTimePreviewDomain : IDomain
    {
        GameObject _domainRoot;
        HashSet<Renderer> domainRenderers;
        ITextureManager _textureManager = new TextureManager(true);
        PreviewStackManager _stackManager;
        public void AddTextureStack<BlendTex>(Texture dist, BlendTex setTex) where BlendTex : IBlendTexturePair
        {
            // _stackManager
        }












        public IEnumerable<Renderer> EnumerateRenderer() { return domainRenderers; }

        public ITextureManager GetTextureManager() => _textureManager;
        public bool IsPreview() => true;

        public bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r) => l == r;
        public void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject) { }

        public void ReplaceMaterials(Dictionary<Material, Material> mapping, bool rendererOnly = false) { throw new NotImplementedException(); }
        public void SetMesh(Renderer renderer, Mesh mesh) { throw new NotImplementedException(); }
        public void TransferAsset(UnityEngine.Object asset) { throw new NotImplementedException(); }
    }
}
