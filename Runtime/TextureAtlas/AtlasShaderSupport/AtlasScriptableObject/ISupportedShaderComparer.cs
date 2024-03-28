using System;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject
{
    public interface ISupportedShaderComparer
    {
        bool ThisSupported(Material material);
    }
    [Serializable]
    public class ContainsName : ISupportedShaderComparer
    {
        public string Name;
        public bool ThisSupported(Material material) { return material.shader.name.Contains(Name); }
    }
    [Serializable]
    public class ShaderReference : ISupportedShaderComparer
    {
        public Shader Shader;
        public bool ThisSupported(Material material) { return material.shader == Shader; }
    }

    internal class AnythingShader : ISupportedShaderComparer
    {
        public bool ThisSupported(Material material) => true;
    }
}
