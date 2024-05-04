using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.Utils;
using net.rs64.TexTransTool.TextureStack;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
using Object = UnityEngine.Object;

namespace net.rs64.TexTransTool
{
    /// <summary>
    /// This is an IDomain implementation that applies to specified renderers.
    ///
    /// If <see cref="Previewing"/> is true, This will call <see cref="AnimationMode.AddPropertyModification"/>
    /// everytime modifies some property so you can revert those changes with <see cref="AnimationMode.StopAnimationMode"/>.
    /// This class doesn't call <see cref="AnimationMode.BeginSampling"/> and <see cref="AnimationMode.EndSampling"/>
    /// so user must call those if needed.
    /// </summary>
    internal class RenderersDomain : IEditorCallDomain
    {
        List<Renderer> _renderers;
        public readonly bool Previewing;

        [CanBeNull] private readonly IAssetSaver _saver;
        private readonly ITextureManager _textureManager;
        private readonly StackManager<ImmediateTextureStack> _textureStacks;

        protected Dictionary<UnityEngine.Object, UnityEngine.Object> _replaceMap = new();//New Old

        public RenderersDomain(List<Renderer> previewRenderers, bool previewing, bool saveAsset = false)
        : this(previewRenderers, previewing, saveAsset ? new AssetSaver() : null) { }
        public RenderersDomain(List<Renderer> previewRenderers, bool previewing, IAssetSaver assetSaver)
        {
            _renderers = previewRenderers;
            Previewing = previewing;
            _saver = assetSaver;
            _textureManager = new TextureManager(Previewing);
            _textureStacks = new StackManager<ImmediateTextureStack>(_textureManager);
        }

        public void AddTextureStack<BlendTex>(Texture dist, BlendTex setTex) where BlendTex : IBlendTexturePair
        {
            _textureStacks.AddTextureStack(dist as Texture2D, setTex);
        }

        private void AddPropertyModification(Object component, string property, Object value)
        {
            if (!Previewing) return;
            AnimationMode.AddPropertyModification(
                EditorCurveBinding.PPtrCurve("", component.GetType(), property),
                new PropertyModification
                {
                    target = component,
                    propertyPath = property,
                    objectReference = value,
                },
                true);
        }

        public void SetSerializedProperty(SerializedProperty property, Object value)
        {
            AddPropertyModification(property.serializedObject.targetObject, property.propertyPath,
                property.objectReferenceValue);
            property.objectReferenceValue = value;
        }

        public virtual void ReplaceMaterials(Dictionary<Material, Material> mapping, bool rendererOnly = false)
        {
            foreach (var replacement in mapping.Values)
                TransferAsset(replacement);

            foreach (var renderer in _renderers)
            {
                if (renderer == null) { continue; }
                if (!renderer.sharedMaterials.Any()) { continue; }
                using (var serialized = new SerializedObject(renderer))
                {
                    foreach (SerializedProperty property in serialized.FindProperty("m_Materials"))
                        if (property.objectReferenceValue is Material material &&
                            mapping.TryGetValue(material, out var replacement))
                            SetSerializedProperty(property, replacement);

                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            if (!rendererOnly)
            {
                foreach (var keyValuePair in mapping)
                {
                    RegisterReplace(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        public virtual void SetMesh(Renderer renderer, Mesh mesh)
        {
            var preMesh = renderer.GetMesh();
            switch (renderer)
            {
                case SkinnedMeshRenderer skinnedRenderer:
                    {
                        AddPropertyModification(renderer, "m_Mesh", skinnedRenderer.sharedMesh);
                        skinnedRenderer.sharedMesh = mesh;
                        break;
                    }
                case MeshRenderer meshRenderer:
                    {
                        var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                        AddPropertyModification(meshFilter, "m_Mesh", meshFilter.sharedMesh);
                        meshFilter.sharedMesh = mesh;
                        break;
                    }
                default:
                    throw new ArgumentException($"Unexpected Renderer Type: {renderer.GetType()}", nameof(renderer));
            }

            RegisterReplace(preMesh, mesh);
        }
        public virtual void SetTexture(Texture2D target, Texture2D setTex)
        {
            var mats = ListPool<Material>.Get(); RendererUtility.GetFilteredMaterials(_renderers, mats);
            ReplaceMaterials(MaterialUtility.ReplaceTextureAll(mats, target, setTex));
            ListPool<Material>.Release(mats);

            RegisterReplace(target, setTex);
        }

        public void TransferAsset(Object Asset) => _saver?.TransferAsset(Asset);


        public virtual bool OriginEqual(Object l, Object r)
        {
            if (l == r) { return true; }

            return GetOrigin(l) == GetOrigin(r);

            Object GetOrigin(Object obj)
            {
                if(obj == null) { return null; }
                while (_replaceMap.ContainsKey(obj)) { obj = _replaceMap[obj]; }
                return obj;
            }

        }
        public virtual void RegisterReplace(Object oldObject, Object nowObject)
        {
            _replaceMap[nowObject] = oldObject;
        }
        public virtual void EditFinish()
        {
            MergeStack();
            _textureManager.DestroyDeferred();
            _textureManager.CompressDeferred();
        }

        public virtual void MergeStack()
        {
            var MergedStacks = _textureStacks.MergeStacks();

            foreach (var mergeResult in MergedStacks)
            {
                if (mergeResult.FirstTexture == null || mergeResult.MergeTexture == null) continue;
                SetTexture(mergeResult.FirstTexture, mergeResult.MergeTexture);
                TransferAsset(mergeResult.MergeTexture);
            }
        }


        public IEnumerable<Renderer> EnumerateRenderer() { return _renderers; }
        public bool IsPreview() => Previewing;
        public ITextureManager GetTextureManager() => _textureManager;

    }
}
