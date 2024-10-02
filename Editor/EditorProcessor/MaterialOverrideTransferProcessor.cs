using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransUnityCore.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool.EditorProcessor
{
    [EditorProcessor(typeof(MaterialOverrideTransfer))]
    internal class MaterialOverrideTransferProcessor : IEditorProcessor
    {

        public void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IDomain domain)
        {
            var materialOverrideTransfer = texTransCallEditorBehavior as MaterialOverrideTransfer;

            var isValid = materialOverrideTransfer.TargetMaterial != null && materialOverrideTransfer.MaterialVariantSource != null;
            if (materialOverrideTransfer.TargetMaterial == null) { TTTRuntimeLog.Info("MaterialOverrideTransfer:info:TargetNotSet"); }
            if (materialOverrideTransfer.MaterialVariantSource == null) { TTTRuntimeLog.Info("MaterialOverrideTransfer:info:VariantNotSet"); }
            if (isValid is false) { return; }

            var materialVariantSource = materialOverrideTransfer.MaterialVariantSource;
            var mats = GetTargetMaterials(domain.EnumerateRenderer(), domain.OriginEqual, materialOverrideTransfer.TargetMaterial);

            if (mats.Any() is false) { TTTRuntimeLog.Info("MaterialOverrideTransfer:info:TargetNotFound"); return; }

            var overridePropertyDict = new Dictionary<string, ShaderPropertyType>();
            var shader = materialVariantSource.shader;
            var pCount = shader.GetPropertyCount();
            for (var i = 0; pCount > i; i += 1)
            {
                var propertyName = shader.GetPropertyName(i);
                if (materialVariantSource.IsPropertyOverriden(propertyName)) { overridePropertyDict.Add(propertyName, shader.GetPropertyType(i)); }
            }

            var materialSwapDict = new Dictionary<Material, Material>();
            foreach (var unEditableMat in mats)
            {
                var mat = materialSwapDict[unEditableMat] = Material.Instantiate(unEditableMat);
                foreach (var overrideProperty in overridePropertyDict)
                {
                    if (!mat.HasProperty(overrideProperty.Key)) { continue; }
                    if (mat.shader.GetPropertyType(mat.shader.FindPropertyIndex(overrideProperty.Key)) != overrideProperty.Value) { continue; }

                    switch (overrideProperty.Value)
                    {
                        case ShaderPropertyType.Texture:
                            {
                                mat.SetTexture(overrideProperty.Key, materialVariantSource.GetTexture(overrideProperty.Key));
                                mat.SetTextureOffset(overrideProperty.Key, materialVariantSource.GetTextureOffset(overrideProperty.Key));
                                mat.SetTextureScale(overrideProperty.Key, materialVariantSource.GetTextureScale(overrideProperty.Key));
                                break;
                            }
                        case ShaderPropertyType.Color:
                            {
                                mat.SetColor(overrideProperty.Key, materialVariantSource.GetColor(overrideProperty.Key));
                                break;
                            }
                        case ShaderPropertyType.Vector:
                            {
                                mat.SetVector(overrideProperty.Key, materialVariantSource.GetVector(overrideProperty.Key));
                                break;
                            }
                        case ShaderPropertyType.Int:
                            {
                                mat.SetInt(overrideProperty.Key, materialVariantSource.GetInt(overrideProperty.Key));
                                break;
                            }
                        case ShaderPropertyType.Float:
                        case ShaderPropertyType.Range:
                            {
                                mat.SetFloat(overrideProperty.Key, materialVariantSource.GetFloat(overrideProperty.Key));
                                break;
                            }
                    }
                }
            }

            domain.ReplaceMaterials(materialSwapDict);
        }

        private static IEnumerable<Material> GetTargetMaterials(IEnumerable<Renderer> domainRenderer, OriginEqual originEqual, Material target)
        {
            return RendererUtility.GetFilteredMaterials(domainRenderer).Where(m => originEqual(m, target));
        }

        public IEnumerable<Renderer> ModificationTargetRenderers(TexTransCallEditorBehavior texTransCallEditorBehavior, IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            var materialOverrideTransfer = texTransCallEditorBehavior as MaterialOverrideTransfer;
            if (materialOverrideTransfer.TargetMaterial == null) { return Array.Empty<Renderer>(); }

            var modTarget = GetTargetMaterials(domainRenderers, replaceTracking, materialOverrideTransfer.TargetMaterial).ToHashSet();
            return domainRenderers.Where(i => i.sharedMaterials.Any(mat => modTarget.Contains(mat)));
        }
    }
}
