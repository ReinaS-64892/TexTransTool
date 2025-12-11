#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MaterialModifier : TexTransBehavior, IDomainReferenceModifier
    {
        internal const string ComponentName = "TTT MaterialModifier";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.MaterialModification;

        [AffectVRAM] [MaterialSelector(popup: MaterialSelectorAttribute.Side.Left)] 
        public Material? TargetMaterial;

        [AffectVRAM] public bool IsOverrideShader = false;
        [AffectVRAM] public Shader? OverrideShader = null;
        public bool IsOverrideRenderQueue = false;
        public int OverrideRenderQueue = 2000;
        //[AffectVRAM]
        public List<MaterialProperty> OverrideProperties = new();

        internal override void Apply(IDomain domain)
        {
            domain.Observe(this);

            if (TargetMaterial == null) { TTLog.Info("MaterialModifier:info:TargetNotSet"); return; }

            var mats = GetTargetMaterials(domain, TargetMaterial);
            if (mats.Any() is false) { TTLog.Info("MaterialModifier:info:TargetNotFound"); return; }

            foreach (var mat in mats)
            {
                var mutableMat = domain.ToMutable(mat);
                ConfigureMaterial(mutableMat, this);
            }
        }


        public static void ConfigureMaterial(Material editableMat, MaterialModifier config)
        {
            ConfigureMaterial(editableMat, config.IsOverrideShader, config.OverrideShader, config.IsOverrideRenderQueue, config.OverrideRenderQueue, config.OverrideProperties);
        }

        public static void ConfigureMaterial(Material editableMat, bool isOverrideShader, Shader? overrideShader, bool isOverrideRenderQueue, int overrideRenderQueue, IEnumerable<MaterialProperty> overrideProperties)
        {
            if (isOverrideShader)
            {
                if (overrideShader == null) { TTLog.Info("MaterialModifier:info:NullShader"); }
                else { editableMat.shader = overrideShader; }
            }
            if (isOverrideRenderQueue)
            {
                editableMat.renderQueue = overrideRenderQueue;
            }
            foreach (var overrideProperty in overrideProperties)
            {
                overrideProperty.TrySet(editableMat);
            }
        }

        public static void GetAllOverridesAndApply(Material originalMaterial, Material overrideMaterial, Material editableTargetMaterial)
        {
            var (isOverideShader, overrideShader) = GetOverrideShader(originalMaterial, overrideMaterial);
            var (isOverrideRenderQueue, overrideRenderQueue) = GetOverrideRenderQueue(originalMaterial, overrideMaterial);
            var overrideProperties = GetOverrideProperties(originalMaterial, overrideMaterial).ToList();
            ConfigureMaterial(editableTargetMaterial, isOverideShader, overrideShader, isOverrideRenderQueue, overrideRenderQueue, overrideProperties);
        }

        public static IEnumerable<MaterialProperty> GetOverrideProperties(Material originalMaterial, Material overrideMaterial)
        {
            if (overrideMaterial == null) yield break;
            if (originalMaterial == null) yield break;

            var shader = overrideMaterial.shader;
            var propertyCount = shader.GetPropertyCount();
            for (var i = 0; propertyCount > i; i += 1)
            {
                var propertyIndex = i;

                if (!MaterialProperty.TryGet(overrideMaterial, propertyIndex, out var overrideProperty)) continue;
                if (MaterialProperty.TryGet(originalMaterial, propertyIndex, out var originalProperty))
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

        public static (bool, Shader?) GetOverrideShader(Material originalMaterial, Material overrideMaterial)
        {
            if (overrideMaterial == null) return (false, null);
            if (originalMaterial == null) return (false, null);
            if (originalMaterial.shader == overrideMaterial.shader) return (false, null);
            return (true, overrideMaterial.shader);
        }

        public static (bool, int) GetOverrideRenderQueue(Material originalMaterial, Material overrideMaterial)
        {
            if (overrideMaterial == null) return (false, 0);
            if (originalMaterial == null) return (false, 0);
            if (originalMaterial.renderQueue == overrideMaterial.renderQueue) return (false, 0);
            return (true, overrideMaterial.renderQueue);
        }

        public static IEnumerable<MaterialProperty> GetProperties(Material material)
        {
            if (material == null) yield break;

            var shader = material.shader;
            var propertyCount = shader.GetPropertyCount();
            for (var i = 0; propertyCount > i; i += 1)
            {
                var propertyIndex = i;

                if (!MaterialProperty.TryGet(material, propertyIndex, out var overrideProperty)) continue;

                yield return overrideProperty;
            }
        }

        private static IEnumerable<Material> GetTargetMaterials(IDomainReferenceViewer rendererTargeting, Material? target)
        { return rendererTargeting.GetDomainsMaterialsHashSet(target); }
        internal override IEnumerable<Renderer> TargetRenderers(IDomainReferenceViewer rendererTargeting)
        { return rendererTargeting.RendererFilterForMaterial(rendererTargeting.ObserveToGet(this, i => i.TargetMaterial)); }

        void IDomainReferenceModifier.RegisterDomainReference(IDomainReferenceViewer domainReferenceViewer, IDomainReferenceRegistry registry)
        {
            var mats = GetTargetMaterials(domainReferenceViewer, domainReferenceViewer.ObserveToGet(this, mm => mm.TargetMaterial));
            var addTextures = domainReferenceViewer.ObserveToGet(this,
                    c => c.OverrideProperties
                        .Where(op => op.PropertyType is UnityEngine.Rendering.ShaderPropertyType.Texture)
                        .Select(op => op.TextureValue)
                        .ToArray(),
                    (l, r) => l.SequenceEqual(r)
                );

            foreach (var mat in mats)
            {
                registry.RegisterAddTextures(mat, addTextures);
            }
        }
    }

}
