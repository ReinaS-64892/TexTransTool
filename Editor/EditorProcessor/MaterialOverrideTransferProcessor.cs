using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool.EditorProcessor
{
    [EditorProcessor(typeof(MaterialOverrideTransfer))]
    internal class MaterialOverrideTransferProcessor : IEditorProcessor
    {
        public void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IEditorCallDomain editorCallDomain)
        {
            var materialOverrideTransfer = texTransCallEditorBehavior as MaterialOverrideTransfer;
            if (!materialOverrideTransfer.IsPossibleApply) { throw new TTTNotExecutable(); }

            var materialVariantSource = materialOverrideTransfer.MaterialVariantSource;
            var targetMaterial = materialOverrideTransfer.TargetMaterial;


            var mats = RendererUtility.GetFilteredMaterials(editorCallDomain.EnumerateRenderer()).Where(m => editorCallDomain.OriginEqual(m, targetMaterial));

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

            editorCallDomain.ReplaceMaterials(materialSwapDict);
        }
    }
}
