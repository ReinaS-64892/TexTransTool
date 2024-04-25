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
        HashSet<Renderer> _domainRenderers = new();
        ITextureManager _textureManager = new TextureManager(true);
        PreviewStackManager _stackManager = new();
        Dictionary<Material, Material> _previewMaterialMap = new();
        public RealTimePreviewDomain(GameObject domainRoot)
        {
            _domainRoot = domainRoot;
            _stackManager.NewPreviewTexture += NewPreviewTextureRegister;
            DomainRenderersUpdate();
        }

        public GameObject DomainRoot => _domainRoot;
        public PreviewStackManager PreviewStackManager => _stackManager;
        public int NowPriority { get; set; }
        HashSet<RenderTexture> needUpdate = new();

        public void AddTextureStack<BlendTex>(Texture dist, BlendTex setTex) where BlendTex : IBlendTexturePair
        {
            _stackManager.AddTextureStack(NowPriority, dist, setTex);

            if (dist is Texture2D texture2D) { needUpdate.Add(_stackManager.GetPreviewTexture(texture2D)); }
            if (dist is RenderTexture rt) { needUpdate.Add(rt); }
        }

        public void UpdateNeeded()
        {
            foreach (var rt in needUpdate) { _stackManager.UpdateStack(rt); }
            needUpdate.Clear();
        }


        public void DomainRenderersUpdate()
        {
            _domainRenderers.Clear();
            _domainRenderers.UnionWith(_domainRoot.GetComponentsInChildren<Renderer>(true));
            SwapPreviewMaterial();
        }
        public void SwapPreviewMaterial()
        {
            foreach (var renderer in _domainRenderers)
            {
                using (var serialized = new SerializedObject(renderer))
                {
                    foreach (SerializedProperty property in serialized.FindProperty("m_Materials"))
                    {
                        if (property.objectReferenceValue is not Material material) { continue; }
                        if (_previewMaterialMap.TryGetValue(material, out var replacement))
                        {
                            SetAsAnimation(property, replacement);
                        }
                        else
                        {
                            var prevMat = _previewMaterialMap[material] = UnityEngine.Object.Instantiate(material);
                            SetAsAnimation(property, prevMat);
                            ApplyPreviewTexture(prevMat);
                        }
                    }
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            static void SetAsAnimation(SerializedProperty property, Material replacement)
            {
                AnimationMode.AddPropertyModification(
                   EditorCurveBinding.PPtrCurve("", property.serializedObject.targetObject.GetType(), property.propertyPath),
                   new PropertyModification
                   {
                       target = property.serializedObject.targetObject,
                       propertyPath = property.propertyPath,
                       objectReference = property.objectReferenceValue,
                   },
                   true);
                property.objectReferenceValue = replacement;
            }
        }




        public void NewPreviewTextureRegister(Texture2D texture2D, RenderTexture previewTexture)
        {
            foreach (var m in _previewMaterialMap.Values)
            {
                var shader = m.shader;
                for (int i = 0; shader.GetPropertyCount() > i; i += 1)
                {
                    if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture) { continue; }
                    var nameID = shader.GetPropertyNameId(i);
                    if (m.GetTexture(nameID) == texture2D) { m.SetTexture(nameID, previewTexture); }
                }
            }
        }
        public void ApplyPreviewTexture(Material material)
        {
            var shader = material.shader;
            for (int i = 0; shader.GetPropertyCount() > i; i += 1)
            {
                if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture) { continue; }
                var nameID = shader.GetPropertyNameId(i);
                var prevTex = _stackManager.GetPreviewTexture(material.GetTexture(nameID) as Texture2D);
                if (prevTex != null) { material.SetTexture(nameID, prevTex); }
            }
        }

        public void PreviewExit()
        {
            foreach (var m in _previewMaterialMap.Values) { UnityEngine.Object.DestroyImmediate(m); }
            _previewMaterialMap.Clear();
            _stackManager.ReleaseStackAll();
        }

        public IEnumerable<Renderer> EnumerateRenderer() { return _domainRenderers; }

        public ITextureManager GetTextureManager() => _textureManager;
        public bool IsPreview() => true;

        public bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r) => l == r;
        public void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject) { }

        public void ReplaceMaterials(Dictionary<Material, Material> mapping, bool rendererOnly = false) { throw new NotImplementedException(); }
        public void SetMesh(Renderer renderer, Mesh mesh) { throw new NotImplementedException(); }
        public void TransferAsset(UnityEngine.Object asset) { throw new NotImplementedException(); }
    }
}
