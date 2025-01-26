using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool.EditorProcessor
{
    [EditorProcessor(typeof(MaterialOverrideTransfer))]
    internal class MaterialOverrideTransferProcessor : IEditorProcessor, IRendererTargetingAffecterWithEditorCall
    {

        public void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IDomain domain)
        {
            var materialOverrideTransfer = texTransCallEditorBehavior as MaterialOverrideTransfer;

            var isValid = materialOverrideTransfer.TargetMaterial != null && materialOverrideTransfer.MaterialVariantSource != null;
            if (materialOverrideTransfer.TargetMaterial == null) { TTTRuntimeLog.Info("MaterialOverrideTransfer:info:TargetNotSet"); }
            if (materialOverrideTransfer.MaterialVariantSource == null) { TTTRuntimeLog.Info("MaterialOverrideTransfer:info:VariantNotSet"); }
            if (isValid is false) { return; }

            var materialVariantSource = materialOverrideTransfer.MaterialVariantSource;
            var mats = GetTargetMaterials(domain, materialOverrideTransfer.TargetMaterial);

            if (mats.Any() is false) { TTTRuntimeLog.Info("MaterialOverrideTransfer:info:TargetNotFound"); return; }

            var overridePropertyDict = GetOverrides(materialVariantSource);

            foreach (var unEditableMat in mats)
            {
                var mutableMat = unEditableMat;
                domain.GetMutable(ref mutableMat);
                TransferOverrides(mutableMat, materialVariantSource, overridePropertyDict);
            }
        }

        private static Dictionary<string, ShaderPropertyType> GetOverrides(Material materialVariantSource)
        {
            var overridePropertyDict = new Dictionary<string, ShaderPropertyType>();
            var shader = materialVariantSource.shader;
            var pCount = shader.GetPropertyCount();
            for (var i = 0; pCount > i; i += 1)
            {
                var propertyName = shader.GetPropertyName(i);
                if (materialVariantSource.IsPropertyOverriden(propertyName)) { overridePropertyDict.Add(propertyName, shader.GetPropertyType(i)); }
            }

            return overridePropertyDict;
        }

        private static void TransferOverrides(Material mutableMat, Material materialVariantSource, Dictionary<string, ShaderPropertyType> overridePropertyDict)
        {
            foreach (var overrideProperty in overridePropertyDict)
            {
                if (!mutableMat.HasProperty(overrideProperty.Key)) { continue; }
                if (mutableMat.shader.GetPropertyType(mutableMat.shader.FindPropertyIndex(overrideProperty.Key)) != overrideProperty.Value) { continue; }

                switch (overrideProperty.Value)
                {
                    case ShaderPropertyType.Texture:
                        {
                            mutableMat.SetTexture(overrideProperty.Key, materialVariantSource.GetTexture(overrideProperty.Key));
                            mutableMat.SetTextureOffset(overrideProperty.Key, materialVariantSource.GetTextureOffset(overrideProperty.Key));
                            mutableMat.SetTextureScale(overrideProperty.Key, materialVariantSource.GetTextureScale(overrideProperty.Key));
                            break;
                        }
                    case ShaderPropertyType.Color:
                        {
                            mutableMat.SetColor(overrideProperty.Key, materialVariantSource.GetColor(overrideProperty.Key));
                            break;
                        }
                    case ShaderPropertyType.Vector:
                        {
                            mutableMat.SetVector(overrideProperty.Key, materialVariantSource.GetVector(overrideProperty.Key));
                            break;
                        }
                    case ShaderPropertyType.Int:
                        {
                            mutableMat.SetInt(overrideProperty.Key, materialVariantSource.GetInt(overrideProperty.Key));
                            break;
                        }
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        {
                            mutableMat.SetFloat(overrideProperty.Key, materialVariantSource.GetFloat(overrideProperty.Key));
                            break;
                        }
                }
            }
        }

        public IEnumerable<Renderer> ModificationTargetRenderers(TexTransCallEditorBehavior texTransCallEditorBehavior, IRendererTargeting rendererTargeting)
        {
            var materialOverrideTransfer = texTransCallEditorBehavior as MaterialOverrideTransfer;
            return rendererTargeting.RendererFilterForMaterial(rendererTargeting.LookAtGet(materialOverrideTransfer, mot => mot.TargetMaterial));
        }
        private static IEnumerable<Material> GetTargetMaterials(IRendererTargeting rendererTargeting, Material target)
        {
            return rendererTargeting.GetDomainsMaterialsHashSet(target);
        }

        public void AffectingRendererTargeting(TexTransCallEditorBehavior texTransCallEditorBehavior, IAffectingRendererTargeting rendererTargetingModification)
        {
            var materialOverrideTransfer = texTransCallEditorBehavior as MaterialOverrideTransfer;
            var isValid = materialOverrideTransfer.TargetMaterial != null && materialOverrideTransfer.MaterialVariantSource != null;
            if (isValid is false) return;
            var materialVariantSource = rendererTargetingModification.LookAtGet(materialOverrideTransfer, mot => mot.MaterialVariantSource);
            _ = rendererTargetingModification.LookAtGet(materialVariantSource, mv => GetOverrides(mv).Where(kv => kv.Value is ShaderPropertyType.Texture), (l, r) => l.SequenceEqual(r));
            var overridePropertyDict = GetOverrides(materialVariantSource);
            foreach (var mutableMat in GetTargetMaterials(rendererTargetingModification, materialOverrideTransfer.TargetMaterial))
                TransferOverrides(mutableMat, materialVariantSource, overridePropertyDict);
        }
    }
}
