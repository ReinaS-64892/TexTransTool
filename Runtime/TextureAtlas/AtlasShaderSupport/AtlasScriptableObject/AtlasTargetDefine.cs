using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject
{


    [Serializable]
    public class AtlasShaderTexture2D
    {
        public string PropertyName;
        public Texture2D Texture2D;

        [SerializeReference] public List<BakeProperty> BakeProperties;
    }


    [Serializable]
    public abstract class BakeProperty
    {
        public string PropertyName;

        public static bool ValueComparer(BakeProperty l, BakeProperty r)
        {
            if (l.GetType() != r.GetType()) { return false; }

            switch (l)
            {
                case BakeFloat lf:
                    {
                        var rf = r as BakeFloat;
                        return Mathf.Approximately(lf.Float, rf.Float);
                    }
                case BakeRange lr:
                    {
                        var rr = r as BakeRange;
                        if (rr.MinMax != lr.MinMax) { return false; }
                        return Mathf.Approximately(lr.Float, rr.Float);
                    }
                case BakeColor lc:
                    {
                        var rc = r as BakeColor;
                        return lc.Color == rc.Color;
                    }
                default:
                    return false;
            }
        }

        public static BakeProperty GetBakeProperty(Material material, string propertyName)
        {
            var propIndex = material.shader.FindPropertyIndex(propertyName);
            switch (material.shader.GetPropertyType(propIndex))
            {
                case UnityEngine.Rendering.ShaderPropertyType.Float: { return new BakeFloat() { PropertyName = propertyName, Float = material.GetFloat(propertyName) }; }
                case UnityEngine.Rendering.ShaderPropertyType.Range: { return new BakeRange() { PropertyName = propertyName, Float = material.GetFloat(propertyName), MinMax = material.shader.GetPropertyRangeLimits(propIndex) }; }
                case UnityEngine.Rendering.ShaderPropertyType.Color: { return new BakeColor() { PropertyName = propertyName, Color = material.GetColor(propertyName)}; }
                case UnityEngine.Rendering.ShaderPropertyType.Vector: { return new BakeVector() { PropertyName = propertyName, Vector = material.GetVector(propertyName)}; }
                default: { return null; }
            }
        }
    }
    [Serializable]
    public class BakeFloat : BakeProperty { public float Float; }
    [Serializable]
    public class BakeRange : BakeProperty { public float Float; public Vector2 MinMax; }
    [Serializable]
    public class BakeColor : BakeProperty { public Color Color; }
    public class BakeVector : BakeProperty { public Vector4 Vector; }
}
