#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.Utils;
using System;

namespace net.rs64.TexTransTool.MatAndTexUtils
{
    [AddComponentMenu("TexTransTool/MatAndTexUtils/TTT MatAndTexRelativeSeparator")]
    internal class MatAndTexRelativeSeparator : TexTransCallEditorBehavior
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public bool MultiRendererMode = false;
        public override List<Renderer> GetRenderers => TargetRenderers;
        public override bool IsPossibleApply => SeparateTarget.Any();
        public List<MatSlotBool> SeparateTarget = new List<MatSlotBool>();
        public bool IsTextureSeparate;
        public PropertyName PropertyName = PropertyName.DefaultValue;

        public override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        public void Apply(IEditorCallDomain domain)
        {
            var separatedMaterials = new Dictionary<Material, Material>();
            var separatedTextures = new Dictionary<Texture2D, Texture2D>();

            var rendererIndex = 0;
            foreach (var renderer in TargetRenderers)
            {
                var slotIndex = 0;

                if (SeparateTarget.Count <= rendererIndex) { break; }
                var separateTargetRenderer = SeparateTarget[rendererIndex].BoolList;

                if (renderer == null) { continue; }
                using (var serialized = new SerializedObject(renderer))
                {
                    foreach (SerializedProperty property in serialized.FindProperty("m_Materials"))
                    {
                        if (property.objectReferenceValue is Material material && material != null && separateTargetRenderer.Count > slotIndex && separateTargetRenderer[slotIndex])
                        {

                            if (!separatedMaterials.TryGetValue(material, out var separatedMaterial))
                            {
                                separatedMaterial = Instantiate(material);
                                separatedMaterials.Add(material, separatedMaterial);
                            }
                            domain.SetSerializedProperty(property, separatedMaterial);

                        }

                        slotIndex += 1;
                    }

                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                rendererIndex += 1;
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
    }
    [Serializable]
    internal class MatSlotBool
    {
        public List<bool> BoolList;

        public MatSlotBool(List<bool> boolList)
        {
            BoolList = boolList;
        }

    }
}



#endif
