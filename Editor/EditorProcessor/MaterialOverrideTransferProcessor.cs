using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.EditorProcessor
{
    [EditorProcessor(typeof(MaterialOverrideTransfer))]
    internal class MaterialOverrideTransferProcessor : IEditorProcessor
    {

        public void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IDomain domain)
        {
            var materialOverrideTransfer = texTransCallEditorBehavior as MaterialOverrideTransfer;
            
            if (materialOverrideTransfer.TargetMaterial == null) { TTTRuntimeLog.Info("MaterialOverrideTransfer:info:TargetNotSet"); return; }
            if (materialOverrideTransfer.MaterialVariantSource == null) { TTTRuntimeLog.Info("MaterialOverrideTransfer:info:VariantNotSet"); return;}

            var mats = GetTargetMaterials(domain.EnumerateRenderer(), domain.OriginEqual, materialOverrideTransfer.TargetMaterial);
            if (mats.Any() is false) { TTTRuntimeLog.Info("MaterialOverrideTransfer:info:TargetNotFound"); return; }

            var materialSwapDict = new Dictionary<Material, Material>();
            foreach (var unEditableMat in mats)
            {
                var mat = Material.Instantiate(unEditableMat);
                var overrideProperties = GetOverrideProperties(materialOverrideTransfer.MaterialVariantSource);
                SetProperties(mat, overrideProperties);
                materialSwapDict[unEditableMat] = mat;
            }
            domain.ReplaceMaterials(materialSwapDict);
        }

        public static void SetProperties(Material mat, IEnumerable<MaterialProperty> materialProperties)
        {
            foreach (var materialProperty in materialProperties)
            {
                materialProperty.Set(mat);
            }
        }

        public static IEnumerable<MaterialProperty> GetOverrideProperties(Material variant)
        {
            if (variant == null) yield break;

            var shader = variant.shader;
            var pCount = shader.GetPropertyCount();
            for (var i = 0; pCount > i; i += 1)
            {
                var propertyName = shader.GetPropertyName(i);
                var propertyType = shader.GetPropertyType(i);

                if (!variant.IsPropertyOverriden(propertyName)) continue;

                if (!MaterialProperty.TryGet(variant, propertyName, propertyType, out var property)) continue;

                yield return property;
            }
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
