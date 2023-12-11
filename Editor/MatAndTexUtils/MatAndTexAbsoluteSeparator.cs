#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.Utils;
using System;
using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool.MatAndTexUtils
{
    [AddComponentMenu("TexTransTool/MatAndTexUtils/TTT MatAndTexAbsoluteSeparator")]
    internal class MatAndTexAbsoluteSeparator : TextureTransformer , IMaterialReplaceEventLiner
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public bool MultiRendererMode = false;
        public override List<Renderer> GetRenderers => TargetRenderers;

        public override bool IsPossibleApply => SeparateTarget.Any();

        public override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        public List<Material> SeparateTarget = new List<Material>();
        public bool IsTextureSeparate;
        public PropertyName PropertyName = PropertyName.DefaultValue;


        public override void Apply(IDomain domain)
        {
            var separatedMaterials = new Dictionary<Material, Material>();
            var separatedTextures = new Dictionary<Texture2D, Texture2D>();

            foreach (var renderer in TargetRenderers)
            {
                if (renderer == null) { continue; }
                using (var serialized = new SerializedObject(renderer))
                {
                    foreach (SerializedProperty property in serialized.FindProperty("m_Materials"))
                        if (property.objectReferenceValue is Material material && SeparateTarget.Contains(material))
                        {

                            if (!separatedMaterials.TryGetValue(material, out var separatedMaterial))
                            {
                                separatedMaterial = Instantiate(material);
                                separatedMaterials.Add(material, separatedMaterial);
                            }
                            domain.SetSerializedProperty(property, separatedMaterial);
                        }

                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            if (IsTextureSeparate)
            {
                foreach (var separatedMaterial in separatedMaterials.Values)
                {
                    var propertyName = PropertyName.ToString();
                    if (
                        separatedMaterial.HasProperty(propertyName) &&
                        separatedMaterial.GetTexture(propertyName) is Texture2D texture2D &&
                        texture2D != null
                    )
                    {
                        if (separatedTextures.ContainsKey(texture2D))
                        {
                            separatedMaterial.SetTexture(propertyName, separatedTextures[texture2D]);
                        }
                        else
                        {
                            var separatedTexture = texture2D.CloneTexture2D();
                            separatedMaterial.SetTexture(propertyName, separatedTexture);
                            separatedTextures.Add(texture2D, separatedTexture);
                        }
                    }
                }
            }

            domain.transferAssets(separatedMaterials.Values);
            domain.transferAssets(separatedTextures.Values);
        }
        public void MaterialReplace(Material souse, Material target)
        {
            var index = SeparateTarget.IndexOf(souse);
            if (index == -1) { return; }
            SeparateTarget[index] = target;
        }
    }
}



#endif
