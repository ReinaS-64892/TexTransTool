using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject
{
    [CreateAssetMenu(fileName = "AtlasShaderSupportScriptedObject", menuName = "TexTransTool/AtlasShaderSupportScriptedObject")]
    public class AtlasShaderSupportScriptableObject : ScriptableObject
    {
        [SerializeReference] public ISupportedShaderComparer SupportedShaderComparer = new ContainsName();
        public int Priority;
        public List<AtlasTargetDefine> AtlasTargetDefines;
        public Shader BakeShader;
        [SerializeReference] public List<IAtlasMaterialPostProses> AtlasMaterialPostProses = new();


        public List<AtlasShaderTexture2D> GetAtlasShaderTexture2D(Material material)
        {
            var atlasTex = new List<AtlasShaderTexture2D>();
            foreach (var atlasTargetDefine in AtlasTargetDefines)
            {
                var constraint = atlasTargetDefine.AtlasDefineConstraints;
                if (!constraint.Constraints(material)) { continue; }
                var asTex = new AtlasShaderTexture2D();
                var tex = material.GetTexture(atlasTargetDefine.TexturePropertyName) as Texture2D;

                asTex.Texture2D = tex;
                asTex.TextureScale = material.GetTextureScale(atlasTargetDefine.TexturePropertyName);
                asTex.TextureTranslation = material.GetTextureOffset(atlasTargetDefine.TexturePropertyName);

                asTex.PropertyName = atlasTargetDefine.TexturePropertyName;
                asTex.BakeProperties = atlasTargetDefine.BakePropertyNames.Select(s => BakeProperty.GetBakeProperty(material, s)).ToList();
                atlasTex.Add(asTex);
            }
            return atlasTex;
        }
    }
    [Serializable]
    public class AtlasTargetDefine
    {
        public string TexturePropertyName;
        [SerializeReference] public IAtlasDefineConstraints AtlasDefineConstraints = new Anything();

        public List<string> BakePropertyNames;
    }
}
