using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MaterialOverrideTransfer : TexTransBehavior//, IDomainReferenceViewer
    {
        internal const string ComponentName = "TTT MaterialOverrideTransfer";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.MaterialModification;

        [AffectVRAM] public Material TargetMaterial;
        [AffectVRAM] public Material MaterialVariantSource;

#if UNITY_EDITOR

        internal override void Apply(IDomain domain)
        {
            var materialOverrideTransfer = this;

            var isValid = materialOverrideTransfer.TargetMaterial != null && materialOverrideTransfer.MaterialVariantSource != null;
            if (materialOverrideTransfer.TargetMaterial == null) { TTLog.Info("MaterialOverrideTransfer:info:TargetNotSet"); }
            if (materialOverrideTransfer.MaterialVariantSource == null) { TTLog.Info("MaterialOverrideTransfer:info:VariantNotSet"); }
            if (isValid is false) { return; }

            var materialVariantSource = materialOverrideTransfer.MaterialVariantSource;
            var mats = GetTargetMaterials(domain, materialOverrideTransfer.TargetMaterial);

            if (mats.Any() is false) { TTLog.Info("MaterialOverrideTransfer:info:TargetNotFound"); return; }

            var overridePropertyDict = GetOverrides(materialVariantSource);

            foreach (var unEditableMat in mats)
            {
                var mutableMat = domain.ToMutable(unEditableMat);
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

        internal override IEnumerable<Renderer> TargetRenderers(IDomainReferenceViewer rendererTargeting)
        {
            var materialOverrideTransfer = this;
            return rendererTargeting.RendererFilterForMaterial(rendererTargeting.ObserveToGet(materialOverrideTransfer, mot => mot.TargetMaterial));
        }
        private static IEnumerable<Material> GetTargetMaterials(IDomainReferenceViewer rendererTargeting, Material target)
        {
            return rendererTargeting.GetDomainsMaterialsHashSet(target);
        }

        // public void AffectingRendererTargeting(IAffectingRendererTargeting rendererTargetingModification)
        // {
        //     var materialOverrideTransfer = this;
        //     var isValid = materialOverrideTransfer.TargetMaterial != null && materialOverrideTransfer.MaterialVariantSource != null;
        //     if (isValid is false) return;
        //     var materialVariantSource = rendererTargetingModification.LookAtGet(materialOverrideTransfer, mot => mot.MaterialVariantSource);
        //     _ = rendererTargetingModification.LookAtGet(materialVariantSource, mv => GetOverrides(mv).Where(kv => kv.Value is ShaderPropertyType.Texture), (l, r) => l.SequenceEqual(r));
        //     var overridePropertyDict = GetOverrides(materialVariantSource);
        //     foreach (var mutableMat in GetTargetMaterials(rendererTargetingModification, materialOverrideTransfer.TargetMaterial))
        //         TransferOverrides(mutableMat, materialVariantSource, overridePropertyDict);
        // }
#else
        internal override void Apply(IDomain domain)
        {
            throw new System.NotImplementedException();
        }

        internal override IEnumerable<Renderer> TargetRenderers(IDomainViewer rendererTargeting)
        {
            throw new System.NotImplementedException();
        }
#endif
    }
}
