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

        public Shader OverrideShader;
        public List<MaterialProperty> OverrideProperties = new();

        // EditorにおけるRecording用
        public bool IsRecording = false;
        public Material RecordingMaterial;

        internal override void Apply([NotNull] IDomain domain)
        {
            domain.LookAt(this);

            if (TargetMaterial == null) { TTTRuntimeLog.Info("MaterialConfigurator:info:TargetNotSet"); return; }

            var mats = GetTargetMaterials(domain.EnumerateRenderer(), domain.OriginEqual, TargetMaterial);
            if (mats.Any() is false) { TTTRuntimeLog.Info("MaterialConfigurator:info:TargetNotFound"); return; }

            var materialSwapDict = new Dictionary<Material, Material>();
            foreach (var unEditableMat in mats)
            {
                Material mat;
                if (IsRecording && RecordingMaterial != null)
                {
                    // Recording中はRecordingMaterialをそのまま使用
                    mat = RecordingMaterial;
                }
                else 
                {
                    mat = Material.Instantiate(unEditableMat);
                    if(OverrideShader != null) {
                        mat.shader = OverrideShader;
                    }
                    SetProperties(mat, OverrideProperties);
                    mat.name += "_Configured";
                    domain.TransferAsset(mat);
                }
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
                // 元のマテリアルから取得できてかつ同値なプロパティは無視
                if (MaterialProperty.TryGet(originalMaterial, propertyName, propertyType, out var originalProperty) && overrideProperty.Equals(originalProperty)) continue;

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
