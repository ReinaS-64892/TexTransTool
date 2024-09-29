using System;
using System.Collections.Generic;
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
    [Serializable]
    public class NotComparer : ISupportedShaderComparer
    {
        [SerializeReference, SubclassSelector] public ISupportedShaderComparer Comparer;
        public bool ThisSupported(Material material)
        {
            return !Comparer.ThisSupported(material);
        }
    }
    [Serializable]
    public class AndComparer : ISupportedShaderComparer
    {
        [SerializeReference, SubclassSelector] public List<ISupportedShaderComparer> Comparers;
        public bool ThisSupported(Material material)
        {
            foreach (var comparer in Comparers)
            { if (comparer.ThisSupported(material) is false) { return false; } }

            return true;
        }
    }
    [Serializable]
    public class OrComparer : ISupportedShaderComparer
    {
        [SerializeReference, SubclassSelector] public List<ISupportedShaderComparer> Comparers;
        public bool ThisSupported(Material material)
        {
            foreach (var comparer in Comparers)
            { if (comparer.ThisSupported(material)) { return true; } }

            return false;
        }
    }

    internal class AnythingShader : ISupportedShaderComparer
    {
        public bool ThisSupported(Material material) => true;
    }
}
