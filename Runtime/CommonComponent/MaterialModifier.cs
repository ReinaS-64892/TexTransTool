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

        [AffectVRAM] public Material? TargetMaterial;

        [AffectVRAM] public bool IsOverrideShader = false;
        [AffectVRAM] public Shader? OverrideShader = null;
        public bool IsOverrideRenderQueue = false;
        public int OverrideRenderQueue = 2000;
        //[AffectVRAM]
        public List<MaterialProperty> OverrideProperties = new();

        internal override void Apply(IDomain domain)
        {
            domain.Observe(this);

            if (TargetMaterial == null) { TTLog.Info("MaterialModifier:info:TargetNotSet", this); return; }

            var mats = GetTargetMaterials(domain, TargetMaterial);
            if (mats.Count == 0) { TTLog.Info("MaterialModifier:info:TargetNotFound", this); return; }

            foreach (var mat in mats)
            {
                var mutableMat = domain.ToMutable(mat);
                ConfigureMaterial(mutableMat, this);
            }
        }


        internal static void ConfigureMaterial(Material editableMat, MaterialModifier config)
        {
            ConfigureMaterial(editableMat, config.IsOverrideShader, config.OverrideShader, config.IsOverrideRenderQueue, config.OverrideRenderQueue, config.OverrideProperties);
        }

        internal static void ConfigureMaterial(Material editableMat, bool isOverrideShader, Shader? overrideShader, bool isOverrideRenderQueue, int overrideRenderQueue, IEnumerable<MaterialProperty> overrideProperties)
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

        internal static void GetAllOverridesAndApply(Material originalMaterial, Material overrideMaterial, Material editableTargetMaterial)
        {
            var (isOverideShader, overrideShader) = GetOverrideShader(originalMaterial, overrideMaterial);
            var (isOverrideRenderQueue, overrideRenderQueue) = GetOverrideRenderQueue(originalMaterial, overrideMaterial);
            var overrideProperties = GetOverrideProperties(originalMaterial, overrideMaterial).ToList();
            ConfigureMaterial(editableTargetMaterial, isOverideShader, overrideShader, isOverrideRenderQueue, overrideRenderQueue, overrideProperties);
        }

        internal static IEnumerable<MaterialProperty> GetOverrideProperties(Material originalMaterial, Material overrideMaterial)
        {
            var shader = overrideMaterial.shader;
            var propertyCount = shader.GetPropertyCount();
            for (var i = 0; propertyCount > i; i += 1)
            {
                if (!MaterialProperty.TryGet(overrideMaterial, i, out var overrideProperty)) continue;
                // 同一名のプロパティが存在し、同値の場合無視
                if (MaterialProperty.TryGet(originalMaterial, overrideProperty.PropertyName, out var originalProperty))
                {
                    // 元のマテリアルから値を転送したりすると編集せずともなんか浮動小数点誤差が生じてfalseを返すっぽい？ので厳密な比較を行わない
                    if (overrideProperty.Equals(originalProperty, false))
                    {
                        continue;
                    }
                }

                yield return overrideProperty;
            }
        }

        internal static (bool, Shader?) GetOverrideShader(Material originalMaterial, Material overrideMaterial)
        {
            if (originalMaterial.shader == overrideMaterial.shader) return (false, null);
            return (true, overrideMaterial.shader);
        }

        internal static (bool, int) GetOverrideRenderQueue(Material originalMaterial, Material overrideMaterial)
        {
            if (originalMaterial.renderQueue == overrideMaterial.renderQueue) return (false, 0);
            return (true, overrideMaterial.renderQueue);
        }

        internal static IEnumerable<MaterialProperty> GetProperties(Material material)
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

        private static HashSet<Material> GetTargetMaterials(IDomainReferenceViewer rendererTargeting, Material? target)
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
