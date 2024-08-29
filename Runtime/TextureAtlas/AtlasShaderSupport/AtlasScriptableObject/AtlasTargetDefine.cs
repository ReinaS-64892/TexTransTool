using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject
{


    [Serializable]
    public class AtlasShaderTexture2D
    {
        public string PropertyName;
        public Texture Texture;

        public Vector2 TextureScale;
        public Vector2 TextureTranslation;

        public bool IsNormalMap;

        [SerializeReference] public List<BakeProperty> BakeProperties;
        public HashSet<string> BakeUseMaxValueProperties;
    }


    [Serializable]
    public abstract class BakeProperty
    {
        public string PropertyName;

        public abstract void WriteMaterial(Material material);

        public static bool PropertyListEqual(List<BakeProperty> l, List<BakeProperty> r)
        {
            var count = l.Count;
            if (count != r.Count) { return false; }

            for (var i = 0; count > i; i += 1)
            {
                var lProp = l[i];
                var rProp = r[i];

                var res = ValueComparer(lProp, rProp);
                if (res == false) { return false; }
            }
            return true;
        }

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
                case BakeVector lc:
                    {
                        var rc = r as BakeVector;
                        return lc.Vector == rc.Vector;
                    }
                case BakeTexture lc:
                    {
                        var rc = r as BakeTexture;
                        return lc.Texture2D == rc.Texture2D && lc.TextureScale == rc.TextureScale && lc.TextureTranslation == rc.TextureTranslation;
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
                case UnityEngine.Rendering.ShaderPropertyType.Color: { return new BakeColor() { PropertyName = propertyName, Color = material.GetColor(propertyName) }; }
                case UnityEngine.Rendering.ShaderPropertyType.Vector: { return new BakeVector() { PropertyName = propertyName, Vector = material.GetVector(propertyName) }; }
                case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    {
                        return new BakeTexture()
                        {
                            PropertyName = propertyName,
                            Texture2D = material.GetTexture(propertyName) as Texture2D,
                            TextureScale = material.GetTextureScale(propertyName),
                            TextureTranslation = material.GetTextureOffset(propertyName),
                        };
                    }
                default: { return null; }
            }
        }
    }
    [Serializable]
    public class BakeFloat : BakeProperty
    {
        public float Float;

        public override void WriteMaterial(Material material)
        {
            material.SetFloat(PropertyName, Float);
        }
    }
    [Serializable]
    public class BakeRange : BakeProperty
    {
        public float Float; public Vector2 MinMax;

        public override void WriteMaterial(Material material)
        {
            material.SetFloat(PropertyName, Float);
        }
    }
    [Serializable]
    public class BakeColor : BakeProperty
    {
        public Color Color;

        public override void WriteMaterial(Material material)
        {
            material.SetColor(PropertyName, Color);
        }
    }
    [Serializable]
    public class BakeVector : BakeProperty
    {
        public Vector4 Vector;

        public override void WriteMaterial(Material material)
        {
            material.SetVector(PropertyName, Vector);
        }
    }
    [Serializable]
    public class BakeTexture : BakeProperty
    {
        public Texture2D Texture2D;

        public Vector2 TextureScale;
        public Vector2 TextureTranslation;

        public override void WriteMaterial(Material material)
        {
            material.SetTexture(PropertyName, Texture2D);
            material.SetTextureScale(PropertyName, TextureScale);
            material.SetTextureOffset(PropertyName, TextureTranslation);
        }
    }
}
