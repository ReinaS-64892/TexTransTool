#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;
using static net.rs64.TexTransTool.TextureLayerUtil;
using Object = UnityEngine.Object;

namespace net.rs64.TexTransTool
{
    [Serializable]
    public class PreviewDomain : IDomain, IDisposable
    {
        [SerializeField] List<Renderer> _renderers;
        [SerializeField] TextureStacks _textureStacks = new TextureStacks();

        public PreviewDomain(List<Renderer> previewRenderers)
        {
            _renderers = previewRenderers;
            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
        }
        public void AddTextureStack(Texture2D Dist, BlendTextures SetTex)
        {
            _textureStacks.AddTextureStack(Dist, SetTex);
        }

        private static void AddPropertyModification(Object component, string property, Object value)
        {
            AnimationMode.AddPropertyModification(
                EditorCurveBinding.PPtrCurve("", component.GetType(), ""),
                new PropertyModification
                {
                    target = component,
                    propertyPath = property,
                    objectReference = value,
                },
                true);

        }

        public void SetMaterial(Material target, Material set, bool isPaired)
        {
            foreach (var renderer in _renderers)
            {
                var materials = renderer.sharedMaterials;
                var modified = false;
                for (var index = 0; index < materials.Length; index++)
                {
                    var originalMaterial = materials[index];
                    if (target == originalMaterial)
                    {
                        materials[index] = set;

                        AddPropertyModification(renderer, $"m_Materials.Array.data[{index}]", originalMaterial);

                        modified = true;
                    }
                }
                if (modified)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }

        public void SetMesh(Renderer renderer, Mesh mesh)
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

        public void transferAsset(UnityEngine.Object Asset)
        {
            //なにもしなくていい
        }

        public void SetTexture(Texture2D Target, Texture2D SetTex)
        {
            var matPair = RendererUtility.SetTexture(_renderers, Target, SetTex);
            this.SetMaterials(matPair, true);
        }

        public void EditFinish()
        {
            foreach (var MargeResult in _textureStacks.MargeStacks())
            {
                SetTexture(MargeResult.FirstTexture, MargeResult.MargeTexture);
            }
            AnimationMode.EndSampling();
        }

        public void Dispose()
        {
            AnimationMode.StopAnimationMode();
        }
    }
}
#endif