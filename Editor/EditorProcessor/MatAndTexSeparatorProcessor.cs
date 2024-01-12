#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransTool.MatAndTexUtils;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.EditorProcessor
{
    [EditorProcessor(typeof(MatAndTexAbsoluteSeparator))]
    [EditorProcessor(typeof(MatAndTexRelativeSeparator))]
    internal class MatAndTexSeparatorProcessor : IEditorProcessor
    {
        public void Process(TexTransCallEditorBehavior texTransCallEditorBehavior, IEditorCallDomain editorCallDomain)
        {
            var separator = texTransCallEditorBehavior as IMatAndTexSeparator;
            var separatedMaterials = new Dictionary<Material, Material>();
            var separatedTextures = new Dictionary<Texture2D, Texture2D>();

            var rendererIndex = 0;
            foreach (var renderer in texTransCallEditorBehavior.GetRenderers)
            {
                var slotIndex = 0;

                var separateTargetRenderer = separator.GetSeparateTarget(editorCallDomain, rendererIndex);

                if (renderer == null) { continue; }
                using (var serialized = new SerializedObject(renderer))
                {
                    foreach (SerializedProperty property in serialized.FindProperty("m_Materials"))
                    {
                        if (property.objectReferenceValue is Material material && material != null && separateTargetRenderer.Count > slotIndex && separateTargetRenderer[slotIndex])
                        {

                            if (!separatedMaterials.TryGetValue(material, out var separatedMaterial))
                            {
                                separatedMaterial = UnityEngine.Object.Instantiate(material);
                                separatedMaterials.Add(material, separatedMaterial);
                            }
                            editorCallDomain.SetSerializedProperty(property, separatedMaterial);

                        }

                        slotIndex += 1;
                    }

                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                rendererIndex += 1;
            }

            if (separator.IsTextureSeparate)
            {
                foreach (var separatedMaterial in separatedMaterials.Values)
                {
                    var propertyName = separator.PropertyName.ToString();
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

            editorCallDomain.transferAssets(separatedMaterials.Values);
            editorCallDomain.transferAssets(separatedTextures.Values);

        }
    }
}
#endif