#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool
{
    [Serializable]
    public struct MaterialProperty : IEquatable<MaterialProperty>
    {
        public string PropertyName;
        [AffectVRAM] public ShaderPropertyType PropertyType;

        // ShaderPropertyType.Texture
        [AffectVRAM] public Texture TextureValue;
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
                    if (strict)
                    {
                        return TextureValue.Equals(other.TextureValue) && TextureOffsetValue.Equals(other.TextureOffsetValue) && TextureScaleValue.Equals(other.TextureScaleValue);
                    }
                    else
                    {
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
                    return strict ? FloatValue == other.FloatValue : Mathf.Approximately(FloatValue, other.FloatValue);
                default:
                    return false;
            }
        }

        public bool TrySet(Material mat)
        {
            if (!Validate(mat, PropertyName, PropertyType)) return false;

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

        public static bool TryGet(Material mat, string propertyName, out MaterialProperty materialProperty)
        {
            return TryGet(mat, mat.shader.FindPropertyIndex(propertyName), out materialProperty);
        }
        public static bool TryGet(Material mat, int propertyIndex, out MaterialProperty materialProperty)
        {
            materialProperty = default;
            var shader = mat.shader;

            if (!ValidateIndex(shader, propertyIndex)) return false;
            var propertyNameID = shader.GetPropertyNameId(propertyIndex);
            var propertyName = shader.GetPropertyName(propertyIndex);
            var propertyType = shader.GetPropertyType(propertyIndex);

            materialProperty = new MaterialProperty
            {
                PropertyName = propertyName,
                PropertyType = propertyType
            };

            switch (propertyType)
            {
                case ShaderPropertyType.Texture:
                    {
                        materialProperty.TextureValue = mat.GetTexture(propertyNameID);
                        materialProperty.TextureOffsetValue = mat.GetTextureOffset(propertyNameID);
                        materialProperty.TextureScaleValue = mat.GetTextureScale(propertyNameID);
                        break;
                    }
                case ShaderPropertyType.Color:
                    {
                        materialProperty.ColorValue = mat.GetColor(propertyNameID);
                        break;
                    }
                case ShaderPropertyType.Vector:
                    {
                        materialProperty.VectorValue = mat.GetVector(propertyNameID);
                        break;
                    }
                case ShaderPropertyType.Int:
                    {
                        materialProperty.IntValue = mat.GetInt(propertyNameID);
                        break;
                    }
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    {
                        materialProperty.FloatValue = mat.GetFloat(propertyNameID);
                        break;
                    }
            }
            return true;
        }

        private static bool Validate(Material mat, string propertyName, ShaderPropertyType propertyType)
        {
            if (!mat.HasProperty(propertyName))
            {
                return false;
            }
            var propertyIndex = mat.shader.FindPropertyIndex(propertyName);
            if (propertyIndex == -1 || mat.shader.GetPropertyType(propertyIndex) != propertyType)
            {
                return false;
            }

            return true;
        }
        private static bool ValidateIndex(Shader shader, int propertyIndex)
        {
            return 0 <= propertyIndex && propertyIndex < shader.GetPropertyCount();
        }
        public bool Equals(MaterialProperty other) { return Equals(other, true); }
        public override bool Equals(object other)
        {
            if (other is MaterialProperty materialProperty) { return Equals(materialProperty, true); }
            return false;
        }

        public override int GetHashCode()
        {
            switch (PropertyType)
            {
                default: return HashCode.Combine(PropertyName, PropertyType);
                case ShaderPropertyType.Texture:
                    {
                        return HashCode.Combine(PropertyName, PropertyType, TextureValue, TextureOffsetValue, TextureScaleValue);
                    }
                case ShaderPropertyType.Color:
                    {
                        return HashCode.Combine(PropertyName, PropertyType, ColorValue);
                    }
                case ShaderPropertyType.Vector:
                    {
                        return HashCode.Combine(PropertyName, PropertyType, VectorValue);
                    }
                case ShaderPropertyType.Int:
                    {
                        return HashCode.Combine(PropertyName, PropertyType, IntValue);
                    }
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    {
                        return HashCode.Combine(PropertyName, PropertyType, FloatValue);
                    }
            }
        }

        public class NotStrictComparer : IEqualityComparer<MaterialProperty>
        {
            public bool Equals(MaterialProperty x, MaterialProperty y)
            {
                return x.Equals(y, false);
            }

            public int GetHashCode(MaterialProperty obj)
            {
                return obj.GetHashCode();
            }
        }

    }
}
