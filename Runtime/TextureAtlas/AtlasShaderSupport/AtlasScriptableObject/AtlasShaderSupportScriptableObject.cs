using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject
{
    [CreateAssetMenu(fileName = "AtlasShaderSupportScriptedObject", menuName = "TexTransTool/AtlasShaderSupportScriptedObject")]
    public class AtlasShaderSupportScriptableObject : ScriptableObject
    {
        [SerializeReference, SubclassSelector] public ISupportedShaderComparer SupportedShaderComparer = new ContainsName();
        public int Priority;
        public List<AtlasTargetDefine> AtlasTargetDefines;
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
                asTex.BakeProperties = atlasTargetDefine.BakePropertyNames.Select(s => BakeProperty.GetBakeProperty(material, s)).ToList();
                atlasTex.Add(asTex);
            }
            return atlasTex;
        }

        public bool IsConstraintValid(Material material, string propertyName)
        {
            var define = AtlasTargetDefines.Find(i => i.TexturePropertyName == propertyName);
            if (define is null) { return false; }

            return define.AtlasDefineConstraints.Constraints(material);
        }
    }
    [Serializable]
    public class AtlasTargetDefine
    {
        public string TexturePropertyName;
        [SerializeReference, SubclassSelector] public IAtlasDefineConstraints AtlasDefineConstraints = new Anything();
        public bool IsNormalMap;
        public List<string> BakePropertyNames;
    }
}
