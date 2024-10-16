using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject
{
    [CreateAssetMenu(fileName = "AtlasShaderSupportScriptedObject", menuName = "TexTransTool/AtlasShaderSupportScriptedObject")]
    public class AtlasShaderSupportScriptableObject : ScriptableObject
    {
        [HideInInspector, SerializeField] internal int TTTSaveDataVersion = TexTransRuntimeBehavior.TTTDataVersion;
        [SerializeReference, SubclassSelector] public ISupportedShaderComparer SupportedShaderComparer = new ContainsName();
        public int Priority;
        public List<AtlasTargetDefine> AtlasTargetDefines = new();
        public Shader BakeShader;
        [SerializeReference, SubclassSelector] public List<IAtlasMaterialPostProses> AtlasMaterialPostProses = new();


        public List<AtlasShaderTexture2D> GetAtlasShaderTexture2D(Material material)
        {
            var atlasTex = new List<AtlasShaderTexture2D>();
            foreach (var atlasTargetDefine in AtlasTargetDefines)
            {
                var constraint = atlasTargetDefine.AtlasDefineConstraints;
                if (!constraint.Constraints(material)) { continue; }
                var asTex = new AtlasShaderTexture2D();

                var tex = material.GetTexture(atlasTargetDefine.TexturePropertyName);
                if (tex != null && tex.dimension != TextureDimension.Tex2D)
                {
                    switch (tex)
                    {
                        default: { tex = null; break; }
                        case Texture2D: { break; }
                        case RenderTexture rt:
                            {
                                if (TTRt.IsTemp(rt) is false) { tex = null; }
                                break;
                            }
                    }
                }

                asTex.Texture = tex;
                asTex.TextureScale = material.GetTextureScale(atlasTargetDefine.TexturePropertyName);
                asTex.TextureTranslation = material.GetTextureOffset(atlasTargetDefine.TexturePropertyName);
                asTex.IsNormalMap = atlasTargetDefine.IsNormalMap;

                asTex.PropertyName = atlasTargetDefine.TexturePropertyName;
                asTex.BakeProperties = atlasTargetDefine.BakePropertyDescriptions.Select(s => BakeProperty.GetBakeProperty(material, s.PropertyName)).ToList();
                asTex.BakeUseMaxValueProperties = atlasTargetDefine.BakePropertyDescriptions.Where(i => i.UseMaxValue).Select(i => i.PropertyName).ToHashSet();
                atlasTex.Add(asTex);
            }
            return atlasTex;
        }

        AtlasTargetDefine GetDefine(string propertyName)
        {
            return AtlasTargetDefines.Find(i => i.TexturePropertyName == propertyName);
        }
        public bool IsConstraintValid(Material material, string propertyName)
        {
            var define = GetDefine(propertyName);
            if (define is null) { return false; }

            return define.AtlasDefineConstraints.Constraints(material);
        }
        public IReadOnlyList<BakePropertyDescription> GetBakePropertyNames(string propertyName)
        {
            var define = GetDefine(propertyName);
            if (define is null) { return null; }

            return define.BakePropertyDescriptions;
        }
    }
    [Serializable]
    public class AtlasTargetDefine
    {
        public string TexturePropertyName;
        [SerializeReference, SubclassSelector] public IAtlasDefineConstraints AtlasDefineConstraints = new Anything();
        public bool IsNormalMap;
        [Obsolete("V4SaveData", true)][HideInInspector] public List<string> BakePropertyNames = new();
        public List<BakePropertyDescription> BakePropertyDescriptions = new();
    }
    [Serializable]
    public class BakePropertyDescription
    {
        public string PropertyName;
        public bool UseMaxValue;
    }
}
