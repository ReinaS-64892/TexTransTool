using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool
{
    [Serializable]
    public struct MaterialProperty
    {
        public string PropertyName;
        public ShaderPropertyType PropertyType;
        
        // ShaderPropertyType.Texture
        public Texture TextureValue;
        public Vector2 TextureOffsetValue;
        public Vector2 TextureScaleValue; 
        // ShaderPropertyType.Color
        public Color ColorValue;
        // ShaderPropertyType.Vector
        public Vector4 VectorValue;
        // ShaderPropertyType.Int
        public int IntValue;
        // ShaderPropertyType.Float, ShaderPropertyType.Range
        public float FloatValue;

        public bool Equals(MaterialProperty other, bool strict = true)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (PropertyType != other.PropertyType) return false;
            if (PropertyName != other.PropertyName) return false;

            switch (PropertyType)
            {
                case ShaderPropertyType.Texture:
                    if (strict) {
                        return TextureValue.Equals(other.TextureValue) && TextureOffsetValue.Equals(other.TextureOffsetValue) && TextureScaleValue.Equals(other.TextureScaleValue);
                    }
                    else {
                        return TextureValue == other.TextureValue && TextureOffsetValue == other.TextureOffsetValue && TextureScaleValue == other.TextureScaleValue;
                    }
                case ShaderPropertyType.Color:
                    return strict ? ColorValue.Equals(other.ColorValue) : ColorValue == other.ColorValue;
                case ShaderPropertyType.Vector:
                    return strict ? VectorValue.Equals(other.VectorValue) : VectorValue == other.VectorValue;
                case ShaderPropertyType.Int:
                    return IntValue == other.IntValue;
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    return strict ? Math.Abs(FloatValue - other.FloatValue) < Mathf.Epsilon : FloatValue == other.FloatValue;
                default:
                    return false;
            }
        }

        public bool TrySet(Material mat)
        {
            if (!Validiate(mat, PropertyName, PropertyType)) return false;

            switch (PropertyType)
            {
                case ShaderPropertyType.Texture:
                    {
                        mat.SetTexture(PropertyName, TextureValue);
                        mat.SetTextureOffset(PropertyName, TextureOffsetValue);
                        mat.SetTextureScale(PropertyName, TextureScaleValue);
                        break;
                    }
                case ShaderPropertyType.Color:
                    {
                        mat.SetColor(PropertyName, ColorValue);
                        break;
                    }
                case ShaderPropertyType.Vector:
                    {
                        mat.SetVector(PropertyName, VectorValue);
                        break;
                    }
                case ShaderPropertyType.Int:
                    {
                        mat.SetInt(PropertyName, IntValue);
                        break;
                    }
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    {
                        mat.SetFloat(PropertyName, FloatValue);
                        break;
                    }
            }
            return true;
        }

        public static bool TryGet(Material mat, string propertyName, ShaderPropertyType propertyType, out MaterialProperty materialProperty)
        {
            materialProperty = default;

            if (!Validiate(mat, propertyName, propertyType)) return false;

            materialProperty = new MaterialProperty
            {
                PropertyName = propertyName,
                PropertyType = propertyType
            };

            switch (propertyType)
            {
                case ShaderPropertyType.Texture:
                    {
                        materialProperty.TextureValue = mat.GetTexture(propertyName);
                        materialProperty.TextureOffsetValue = mat.GetTextureOffset(propertyName);
                        materialProperty.TextureScaleValue = mat.GetTextureScale(propertyName);
                        break;
                    }
                case ShaderPropertyType.Color:
                    {
                        materialProperty.ColorValue = mat.GetColor(propertyName);
                        break;
                    }
                case ShaderPropertyType.Vector:
                    {
                        materialProperty.VectorValue = mat.GetVector(propertyName);
                        break;
                    }
                case ShaderPropertyType.Int:
                    {
                        materialProperty.IntValue = mat.GetInt(propertyName);
                        break;
                    }
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    {
                        materialProperty.FloatValue = mat.GetFloat(propertyName);
                        break;
                    }
            }
            return true;
        }

        private static bool Validiate(Material mat, string propertyName, ShaderPropertyType propertyType)
        {
            if (!mat.HasProperty(propertyName))
            {
                return false;
            }
            var propertyIndex = mat.shader.FindPropertyIndex(propertyName);
            if(propertyIndex == -1 || mat.shader.GetPropertyType(propertyIndex) != propertyType)
            {
                return false;
            }

            return true;
        }
    }
}
