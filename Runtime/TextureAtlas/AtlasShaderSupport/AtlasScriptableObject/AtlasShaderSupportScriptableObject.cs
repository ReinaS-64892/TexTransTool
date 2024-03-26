using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject
{
    [CreateAssetMenu(fileName = "AtlasShaderSupportScriptedObject", menuName = "TexTransTool/AtlasShaderSupportScriptedObject")]
    public class AtlasShaderSupportScriptableObject : ScriptableObject
    {
        [SerializeReference] public ISupportedShaderComparer SupportedShaderComparer = new ContainsName();
        public List<AtlasTargetDefine> AtlasTargetDefines;
        public Shader BakeShader;
        [SerializeReference] public List<IAtlasMaterialPostProses> AtlasMaterialPostProses = new();
    }
    [Serializable]
    public class AtlasTargetDefine
    {
        public string TexturePropertyName;
        [SerializeReference] public IAtlasDefineConstraints AtlasDefineConstraints = new FloatPropertyValueGreater();

        public List<string> BakePropertyNames;
    }
}
