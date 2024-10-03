using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static net.rs64.TexTransUnityCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool.Preview.RealTime
{
    internal class RealTimePreviewDomain : IDomain
    {
        GameObject _domainRoot;
        HashSet<Renderer> _domainRenderers = new();
        ITextureManager _textureManager = new TextureManager(true);
        PreviewStackManager _stackManager = new();
        Dictionary<Material, Material> _previewMaterialMap = new();
        Action<TexTransRuntimeBehavior, int> _lookAtCallBack;

        public RealTimePreviewDomain(GameObject domainRoot, Action<TexTransRuntimeBehavior, int> lookAtCallBack)
        {
            _domainRoot = domainRoot;
            _lookAtCallBack = lookAtCallBack;
            _stackManager.NewPreviewTexture += NewPreviewTextureRegister;

            _domainRenderers.Clear();
            _domainRenderers.UnionWith(_domainRoot.GetComponentsInChildren<Renderer>(true));
            SwapPreviewMaterial();
        }

        public GameObject DomainRoot => _domainRoot;
        int _nowPriority;
        TexTransRuntimeBehavior _texTransRuntimeBehavior;

        public void SetNowBehavior(TexTransRuntimeBehavior texTransRuntimeBehavior, int priority)
        {
            _nowPriority = priority;
            _texTransRuntimeBehavior = texTransRuntimeBehavior;
            _stackManager.ReleaseStackOfPriority(_nowPriority);
        }

        public void AddTextureStack<BlendTex>(Texture dist, BlendTex setTex) where BlendTex : IBlendTexturePair
        {
            _stackManager.AddTextureStack(_nowPriority, dist, setTex);
        }

        public void UpdateNeeded()
        {
            _stackManager.UpdateNeededStack();
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

        public bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r)
        {
            if (l == r) { return true; }
            if (l is Material lm && r is Material rm)
            {
                if (_previewMaterialMap.ContainsKey(lm)) lm = _previewMaterialMap[lm];
                if (_previewMaterialMap.ContainsKey(rm)) rm = _previewMaterialMap[rm];
                return lm == rm;
            }
            return false;
        }
        public void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject) { }

        public void ReplaceMaterials(Dictionary<Material, Material> mapping, bool one2one = true) { throw new NotImplementedException(); }
        public void SetMesh(Renderer renderer, Mesh mesh) { throw new NotImplementedException(); }
        public void TransferAsset(UnityEngine.Object asset) { throw new NotImplementedException(); }
        public void LookAt(UnityEngine.Object obj) { _lookAtCallBack(_texTransRuntimeBehavior, obj.GetInstanceID()); }
    }
}
