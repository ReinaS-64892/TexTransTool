#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;
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
    public class RenderersDomain : IDomain
    {
        List<Renderer> _renderers;
        TextureStacks _textureStacks = new TextureStacks();

        public readonly bool Previewing;
        [CanBeNull] private readonly IAssetSaver _saver;

        public RenderersDomain(List<Renderer> previewRenderers, bool previewing, [CanBeNull] IAssetSaver saver = null, ProgressHandler progressHandler = null)
        {
            _renderers = previewRenderers;
            Previewing = previewing;
            _saver = saver;
            _progressHandler = progressHandler;
            _progressHandler?.ProgressStateEnter("ProsesAvatar");
        }

        public void AddTextureStack(Texture2D Dist, BlendTextures SetTex)
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
        }

        public virtual void SetMesh(Renderer renderer, Mesh mesh)
        {
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
        }

        public void TransferAsset(Object Asset) => _saver?.TransferAsset(Asset);

        public virtual void SetTexture(Texture2D Target, Texture2D SetTex)
        {
            this.ReplaceMaterials(RendererEditorUtility.SetTexture(_renderers, Target, SetTex));
        }

        public virtual void EditFinish()
        {
            ProgressStateEnter("Finalize");
            MargeStack();
            ProgressStateExit();
            ProgressStateExit();
            _progressHandler?.ProgressFinalize();
        }

        public virtual void MargeStack()
        {
            ProgressUpdate("MargeStack", 0f);
            var mangedStack = _textureStacks.MargeStacks();
            ProgressUpdate("MargeStack", 0.9f);
            foreach (var mergeResult in mangedStack)
            {
                if (mergeResult.FirstTexture == null || mergeResult.MargeTexture == null) continue;
                SetTexture(mergeResult.FirstTexture, mergeResult.MargeTexture);
                TransferAsset(mergeResult.MargeTexture);
            }
            ProgressUpdate("MargeStack", 1);
        }

        ProgressHandler _progressHandler;
        public void ProgressStateEnter(string EnterName) => _progressHandler?.ProgressStateEnter(EnterName);
        public void ProgressUpdate(string State, float Value) => _progressHandler?.ProgressUpdate(State, Value);
        public void ProgressStateExit() => _progressHandler?.ProgressStateExit();

        public Texture2D GetOriginalTexture2D(Texture2D texture2D)
        {
            if (Previewing)
            {
                return texture2D;
            }
            else
            {
                return texture2D.TryGetUnCompress();
            }
        }
    }
}
#endif