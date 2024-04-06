using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject
{
    public interface IAtlasDefineConstraints
    {
        bool Constraints(Material material);
    }

    [Serializable]
    public class AndConstraints : IAtlasDefineConstraints
    {
        [SerializeReference] public List<IAtlasDefineConstraints> AtlasDefineConstraints;
        public bool Constraints(Material material)
        {
            foreach (var adc in AtlasDefineConstraints) { if (!adc.Constraints(material)) { return false; } }
            return true;
        }
    }
    [Serializable]
    public class OrConstraints : IAtlasDefineConstraints
    {
        [SerializeReference] public List<IAtlasDefineConstraints> AtlasDefineConstraints;
        public bool Constraints(Material material)
        {
            foreach (var adc in AtlasDefineConstraints) { if (!adc.Constraints(material)) { return true; } }
            return false;
        }
    }
    [Serializable]
    public class FloatPropertyValueGreater : IAtlasDefineConstraints
    {
        public string PropertyName;
        public float ComparerValue = 0.5f;
        public bool Less;
        public bool Constraints(Material material)
        {
                return !Less ? material.GetFloat(PropertyName) > ComparerValue : material.GetFloat(PropertyName) < ComparerValue;
        }
    }
    [Serializable]
    public class FloatPropertyValueEqual : IAtlasDefineConstraints
    {
        public string PropertyName;
        public float Value;
        public bool NotEqual;
        public bool Constraints(Material material)
        {
            return Mathf.Approximately(material.GetFloat(PropertyName), Value) == !NotEqual;
        }
    }
    [Serializable]
    public class IntPropertyValueGreater : IAtlasDefineConstraints
    {
        public string PropertyName;
        public int ComparerValue = 0;
        public bool Less;
        public bool Constraints(Material material)
        {
            return !Less ? material.GetInt(PropertyName) > ComparerValue : material.GetInt(PropertyName) < ComparerValue;
        }
    }
    [Serializable]
    public class IntPropertyValueEqual : IAtlasDefineConstraints
    {
        public string PropertyName;
        public int Value;
        public bool NotEqual;
        public bool Constraints(Material material)
        {
            return (material.GetInt(PropertyName) == Value) == !NotEqual;
        }
    }
    [Serializable]
    public class ShaderNameContains : IAtlasDefineConstraints
    {
        public string Name;
        public bool Constraints(Material material)
        {
            return material.shader.name.Contains(Name);
        }
    }
    [Serializable]
    public class Anything : IAtlasDefineConstraints
    {
        public bool Constraints(Material material) { return true; }
    }

}
