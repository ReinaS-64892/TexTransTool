using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MaterialConfigurator : TexTransRuntimeBehavior
    {
        internal const string FoldoutName = "Other";
        internal const string ComponentName = "TTT MaterialConfigurator";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        public Material TargetMaterial;

        public bool IsOverrideShader = false;        
        public Shader OverrideShader = null;
        public List<MaterialProperty> OverrideProperties = new();

        internal override void Apply([NotNull] IDomain domain)
        {
            domain.LookAt(this);

            if (TargetMaterial == null) { TTTRuntimeLog.Info("MaterialConfigurator:info:TargetNotSet"); return; }

            var mats = GetTargetMaterials(domain.EnumerateRenderer(), domain.OriginEqual, TargetMaterial);
            if (mats.Any() is false) { TTTRuntimeLog.Info("MaterialConfigurator:info:TargetNotFound"); return; }

            var materialSwapDict = new Dictionary<Material, Material>();
            foreach (var unEditableMat in mats)
            {
                var mat = Material.Instantiate(unEditableMat);
                ConfigureMaterial(mat, this);
                materialSwapDict[unEditableMat] = mat;
            }
            domain.ReplaceMaterials(materialSwapDict);
        }

        
        public static void ConfigureMaterial(Material editableMat, MaterialConfigurator config)
        {
            ConfigureMaterial(editableMat, config.IsOverrideShader, config.OverrideShader, config.OverrideProperties);
        }

        public static void ConfigureMaterial(Material editableMat, bool isOverrideShader, Shader overrideShader, IEnumerable<MaterialProperty> overrideProperties)
        {
            if (isOverrideShader) {
                editableMat.shader = overrideShader;
            }
            foreach (var overrideProperty in overrideProperties)
            {
                overrideProperty.TrySet(editableMat);
            }
        }

        public static void TransferValues(Material source, Material target)
        {
            var shader = source.shader;
            var properties = GetProperties(source);
            ConfigureMaterial(target, true, shader, properties);
        }

        public static IEnumerable<MaterialProperty> GetOverrideProperties(Material originalMaterial, Material overrideMaterial)
        {
            if (overrideMaterial == null) yield break;
            if (originalMaterial == null) yield break;

            var shader = overrideMaterial.shader;
            var propertyCount = shader.GetPropertyCount();
            for (var i = 0; propertyCount > i; i += 1)
            {
                var propertyName = shader.GetPropertyName(i);
                var propertyType = shader.GetPropertyType(i);

                if (!MaterialProperty.TryGet(overrideMaterial, propertyName, propertyType, out var overrideProperty)) continue;
                if (MaterialProperty.TryGet(originalMaterial, propertyName, propertyType, out var originalProperty))
                {
                     // 元のマテリアルから値を転送したりすると編集せずともなんか浮動小数点誤差が生じてfalseを返すっぽい？ので厳密な比較を行わない
                    if (overrideProperty.Equals(originalProperty, false))
                    {
                        // 元のマテリアルから取得できてかつ同値なプロパティは無視
                        continue;
                    }
                }

                yield return overrideProperty;
            }
        }

        private static IEnumerable<MaterialProperty> GetProperties(Material material)
        {
            if (material == null) yield break;

            var shader = material.shader;
            var propertyCount = shader.GetPropertyCount();
            for (var i = 0; propertyCount > i; i += 1)
            {
                var propertyName = shader.GetPropertyName(i);
                var propertyType = shader.GetPropertyType(i);

                if (!MaterialProperty.TryGet(material, propertyName, propertyType, out var overrideProperty)) continue;

                yield return overrideProperty;
            }
        }

        private static IEnumerable<Material> GetTargetMaterials(IEnumerable<Renderer> domainRenderer, OriginEqual originEqual, Material target)
        {
            return RendererUtility.GetFilteredMaterials(domainRenderer).Where(m => originEqual(m, target));
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            if (TargetMaterial == null) { return Array.Empty<Renderer>(); }

            var modTarget = GetTargetMaterials(domainRenderers, replaceTracking, TargetMaterial).ToHashSet();
            return domainRenderers.Where(i => i.sharedMaterials.Any(mat => modTarget.Contains(mat)));
        }

    }

}
