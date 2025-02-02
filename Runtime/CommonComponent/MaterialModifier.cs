#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MaterialModifier : TexTransRuntimeBehavior, IRendererTargetingAffecterWithRuntime
    {
        internal const string FoldoutName = "Other";
        internal const string ComponentName = "TTT MaterialModifier";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.MaterialModification;

        [AffectVRAM] public Material? TargetMaterial;

        [AffectVRAM] public bool IsOverrideShader = false;
        [AffectVRAM] public Shader? OverrideShader = null;
        //[AffectVRAM]
        public List<MaterialProperty> OverrideProperties = new();

        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);

            if (TargetMaterial == null) { TTTRuntimeLog.Info("MaterialModifier:info:TargetNotSet"); return; }

            var mats = GetTargetMaterials(domain, TargetMaterial);
            if (mats.Any() is false) { TTTRuntimeLog.Info("MaterialModifier:info:TargetNotFound"); return; }

            foreach (var mat in mats)
            {
                var mutableMat = mat;
                domain.GetMutable(ref mutableMat);
                ConfigureMaterial(mutableMat, this);
            }
        }


        public static void ConfigureMaterial(Material editableMat, MaterialModifier config)
        {
            ConfigureMaterial(editableMat, config.IsOverrideShader, config.OverrideShader, config.OverrideProperties);
        }

        public static void ConfigureMaterial(Material editableMat, bool isOverrideShader, Shader? overrideShader, IEnumerable<MaterialProperty> overrideProperties)
        {
            if (isOverrideShader)
            {
                if (overrideShader == null) { TTTRuntimeLog.Info("MaterialModifier:info:NullShader"); }
                else { editableMat.shader = overrideShader; }
            }
            foreach (var overrideProperty in overrideProperties)
            {
                overrideProperty.TrySet(editableMat);
            }
        }

        public static void TransferValues(Material source, Material target)
        {
            var properties = GetProperties(source);
            ConfigureMaterial(target, true, source.shader, properties);
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

        public static (bool, Shader?) GetOverrideShader(Material originalMaterial, Material overrideMaterial)
        {
            if (overrideMaterial == null) return (false, null);
            if (originalMaterial == null) return (false, null);
            if (originalMaterial.shader == overrideMaterial.shader) return (false, null);
            return (true, overrideMaterial.shader);
        }

        public static IEnumerable<MaterialProperty> GetProperties(Material material)
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

        private static IEnumerable<Material> GetTargetMaterials(IRendererTargeting rendererTargeting, Material? target)
        { return rendererTargeting.GetDomainsMaterialsHashSet(target); }
        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        { return rendererTargeting.RendererFilterForMaterial(rendererTargeting.LookAtGet(this, i => i.TargetMaterial)); }

        void IRendererTargetingAffecterWithRuntime.AffectingRendererTargeting(IAffectingRendererTargeting rendererTargeting)
        {
            var targetMat = rendererTargeting.LookAtGet(this, i => i.TargetMaterial);
            if (targetMat == null) { return; }

            _ = rendererTargeting.LookAtGet(this, c => c.IsOverrideShader);// looking
            _ = rendererTargeting.LookAtGet(this, c => c.OverrideShader);
            _ = rendererTargeting.LookAtGet(this,
                    c => c.OverrideProperties
                        .Where(op => op.PropertyType is UnityEngine.Rendering.ShaderPropertyType.Texture)
                        .Select(op => op.TextureValue)
                        .ToArray(),
                    (l, r) => l.SequenceEqual(r)
                );

            foreach (var mutableMat in GetTargetMaterials(rendererTargeting, targetMat))
                ConfigureMaterial(mutableMat, this);
        }
    }

}
