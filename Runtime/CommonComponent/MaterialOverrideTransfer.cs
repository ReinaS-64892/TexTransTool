using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MaterialOverrideTransfer : TexTransRuntimeBehavior
    {
        internal const string Name = "TTT MaterialOverrideTransfer";
        internal const string FoldoutName = "Other";
        internal const string MenuPath = FoldoutName + "/" + Name;

        internal override List<Renderer> GetRenderers => null;

        internal override bool IsPossibleApply => TargetMaterial != null && MaterialVariantSource != null && MaterialVariantSource.isVariant;

        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        public Material TargetMaterial;
        public Material MaterialVariantSource;

        internal override void Apply([NotNull] IDomain domain)
        {
            if (!IsPossibleApply) { throw new TTTNotExecutable(); }

            var mats = RendererUtility.GetFilteredMaterials(domain.EnumerateRenderer()).Where(m => domain.OriginEqual(m, TargetMaterial));

            var overridePropertyDict = new Dictionary<string, ShaderPropertyType>();
            var shader = MaterialVariantSource.shader;
            var pCount = shader.GetPropertyCount();
            for (var i = 0; pCount > i; i += 1)
            {
                var propertyName = shader.GetPropertyName(i);
                if (MaterialVariantSource.IsPropertyOverriden(propertyName)) { overridePropertyDict.Add(propertyName, shader.GetPropertyType(i)); }
            }

            var materialSwapDict = new Dictionary<Material, Material>();
            foreach (var unEditableMat in mats)
            {
                var mat = materialSwapDict[unEditableMat] = Instantiate(unEditableMat);
                foreach (var overrideProperty in overridePropertyDict)
                {
                    if (!mat.HasProperty(overrideProperty.Key)) { continue; }
                    if (mat.shader.GetPropertyType(mat.shader.FindPropertyIndex(overrideProperty.Key)) != overrideProperty.Value) { continue; }

                    switch (overrideProperty.Value)
                    {
                        case ShaderPropertyType.Texture:
                            {
                                mat.SetTexture(overrideProperty.Key, MaterialVariantSource.GetTexture(overrideProperty.Key));
                                mat.SetTextureOffset(overrideProperty.Key, MaterialVariantSource.GetTextureOffset(overrideProperty.Key));
                                mat.SetTextureScale(overrideProperty.Key, MaterialVariantSource.GetTextureScale(overrideProperty.Key));
                                break;
                            }
                        case ShaderPropertyType.Color:
                            {
                                mat.SetColor(overrideProperty.Key, MaterialVariantSource.GetColor(overrideProperty.Key));
                                break;
                            }
                        case ShaderPropertyType.Vector:
                            {
                                mat.SetVector(overrideProperty.Key, MaterialVariantSource.GetVector(overrideProperty.Key));
                                break;
                            }
                        case ShaderPropertyType.Int:
                            {
                                mat.SetInt(overrideProperty.Key, MaterialVariantSource.GetInt(overrideProperty.Key));
                                break;
                            }
                        case ShaderPropertyType.Float:
                        case ShaderPropertyType.Range:
                            {
                                mat.SetFloat(overrideProperty.Key, MaterialVariantSource.GetFloat(overrideProperty.Key));
                                break;
                            }
                    }
                }
            }

            domain.ReplaceMaterials(materialSwapDict);

        }

        internal override IEnumerable<Object> GetDependency(IDomain domain)
        {
            yield return TargetMaterial;
            yield return MaterialVariantSource;
        }

        internal override int GetDependencyHash(IDomain domain)
        {
            var hash = TargetMaterial?.GetInstanceID() ?? 0;
            hash ^= MaterialVariantSource?.GetInstanceID() ?? 0;
            return hash;
        }
    }
}
