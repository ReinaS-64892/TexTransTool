using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.TextureStack;
using net.rs64.TexTransTool.Utils;
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
        private readonly IProgressHandling _progressHandler;
        private readonly ITextureManager _textureManager;
        private readonly IStackManager _textureStacks;
        private readonly IIslandCache _islandCache;

        [NotNull] protected FlatMapDict<UnityEngine.Object> _objectMap = new();

        public RenderersDomain(List<Renderer> previewRenderers, bool previewing, bool saveAsset = false, bool progressDisplay = false)
        : this(previewRenderers, previewing, saveAsset ? new AssetSaver() : null, progressDisplay) { }
        public RenderersDomain(List<Renderer> previewRenderers, bool previewing, IAssetSaver assetSaver, bool progressDisplay = false)
        {
            _renderers = previewRenderers;
            Previewing = previewing;
            _saver = assetSaver;
            _progressHandler = progressDisplay ? new ProgressHandler() : null;
            _textureManager = new TextureManager(Previewing);
            _textureStacks = new StackManager<ImmediateTextureStack>(_textureManager);
            _islandCache = TTTConfig.UseIslandCache ? new EditorIsland.EditorIslandCache() : null;

            _progressHandler?.ProgressStateEnter("ProsesAvatar");
        }

        public RenderersDomain(List<Renderer> previewRenderers,
                        bool previewing,
                        IAssetSaver saver,
                        IProgressHandling progressHandler,
                        ITextureManager textureManager,
                        IStackManager stackManager,
                        IIslandCache islandCache
                       )
        {
            _renderers = previewRenderers;
            Previewing = previewing;
            _saver = saver;
            _progressHandler = progressHandler;
            _textureManager = textureManager;
            _textureStacks = stackManager;
            _islandCache = islandCache;


            _progressHandler?.ProgressStateEnter("ProsesAvatar");
        }

        public void AddTextureStack(Texture2D Dist, BlendTexturePair SetTex)
        {
            _textureStacks.AddTextureStack(Dist, SetTex);
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
                    _objectMap.Add(keyValuePair.Key, keyValuePair.Value);
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

            _objectMap.Add(preMesh, mesh);
        }
        public virtual void SetTexture(Texture2D target, Texture2D setTex)
        {
            var mats = ListPool<Material>.Get(); RendererUtility.GetFilteredMaterials(_renderers, mats);
            ReplaceMaterials(MaterialUtility.ReplaceTextureAll(mats, target, setTex));
            ListPool<Material>.Release(mats);

            _objectMap.Add(target, setTex);
        }

        public void TransferAsset(Object Asset) => _saver?.TransferAsset(Asset);


        public bool TryReplaceQuery(Object oldObject, out Object nowObject)
        {
            return _objectMap.GetMapping.TryGetValue(oldObject, out nowObject);
        }
        public void RegisterReplace(Object oldObject, Object nowObject)
        {
            _objectMap.Add(oldObject, nowObject);
        }
        public virtual void EditFinish()
        {
            ProgressStateEnter("Finalize");
            ProgressUpdate("MergeStack", 0.0f);

            MergeStack();

            ProgressUpdate("DeferTexDestroy", 0.3f);

            _textureManager.DeferTexDestroy();

            ProgressUpdate("TexCompressDelegationInvoke", 0.6f);

            _textureManager.TextureFinalize();

            ProgressUpdate("End", 1f);
            ProgressStateExit();
            ProgressStateExit();
            _progressHandler?.ProgressFinalize();
        }

        public virtual void MergeStack()
        {
            ProgressUpdate("MergeStack", 0f);
            var MergedStacks = _textureStacks.MergeStacks();
            ProgressUpdate("MergeStack", 0.9f);
            foreach (var mergeResult in MergedStacks)
            {
                if (mergeResult.FirstTexture == null || mergeResult.MergeTexture == null) continue;
                SetTexture(mergeResult.FirstTexture, mergeResult.MergeTexture);
                TransferAsset(mergeResult.MergeTexture);
            }
            ProgressUpdate("MergeStack", 1);
        }


        public void ProgressStateEnter(string enterName) => _progressHandler?.ProgressStateEnter(enterName);
        public void ProgressUpdate(string state, float value) => _progressHandler?.ProgressUpdate(state, value);
        public void ProgressStateExit() => _progressHandler?.ProgressStateExit();
        public void ProgressFinalize() => _progressHandler?.ProgressFinalize();



        public ITextureManager GetTextureManager() => _textureManager;

        public IIslandCache GetIslandCacheManager()
        {
            return _islandCache;
        }
    }
}
