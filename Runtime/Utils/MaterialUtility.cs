using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    internal static class MaterialUtility
    {
        public static bool ReferencesTexture(this Material material, Texture target)
        {
            if(material == null || target == null)
            {
                return false;
            }
            var shader = material.shader;

            if(shader == null)
            {
                return false;
            }

            var propertyCount = shader.GetPropertyCount();
            for (var i = 0; i < propertyCount; i++)
            {

                if(shader.GetPropertyType(i) is UnityEngine.Rendering.ShaderPropertyType.Texture)
                {
                    var propertyNameId = shader.GetPropertyNameId(i);
                    var texture = material.GetTexture(propertyNameId);
                    if(texture == target)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static void ReplaceTexture(this Material material, Texture oldTexture, Texture newTexture)
        {
            if (material == null || oldTexture == null)
            {
                return;
            }
            var shader = material.shader;

            if (shader == null)
            {
                return;
            }

            var propertyCount = shader.GetPropertyCount();
            for (var i = 0; i < propertyCount; i++)
            {

                if (shader.GetPropertyType(i) is UnityEngine.Rendering.ShaderPropertyType.Texture)
                {
                    var propertyNameId = shader.GetPropertyNameId(i);
                    var texture = material.GetTexture(propertyNameId);
                    if (texture == oldTexture)
                    {
                        material.SetTexture(propertyNameId, newTexture);
                    }
                }
            }
        }

        public static Dictionary<string, Texture> GetTextureReferences(this Material material) => material.GetTextureReferences<Texture>();
        public static Dictionary<string, T> GetTextureReferences<T>(this Material material)
            where T: Texture
        {
            if (material == null)
            {
                return new();
            }

            var shader = material.shader;
            if (shader == null)
            {
                return new();
            }

            var propertyCount = shader.GetPropertyCount();
            if (propertyCount == 0)
            {
                return new();
            }
            Dictionary<string, T> dictionary = new(propertyCount);

            for (var propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                if (shader.GetPropertyType(propertyIndex) is UnityEngine.Rendering.ShaderPropertyType.Texture)
                {
                    var propertyName = shader.GetPropertyName(propertyIndex);
                    var texture = material.GetTexture(propertyName);
                    if (texture is T typeTexture && typeTexture != null)
                    {
                        dictionary.Add(propertyName, typeTexture);
                    }
                }
            }
            return dictionary;
        }
        public static IEnumerable<Texture> EnumerateReferencedTextures(this Material material)
        {

            if (material == null)
            {
                yield break;
            }

            var shader = material.shader;
            if (shader == null)
            {
                yield break;
            }

            var propertyCount = shader.GetPropertyCount();
            if (propertyCount == 0)
            {
                yield break;
            }

            for (var propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                if (shader.GetPropertyType(propertyIndex) is UnityEngine.Rendering.ShaderPropertyType.Texture)
                {
                    var propertyNameId = shader.GetPropertyNameId(propertyIndex);
                    var texture = material.GetTexture(propertyNameId);
                    if (texture != null)
                    {
                        yield return texture;
                    }
                }
            }
        }
        public static IEnumerable<Texture> EnumerateTextures(this Material material, Func<Material, Shader> shaderSelector, Func<Material, int, Texture> textureSelector)
        {

            if (material == null)
            {
                yield break;
            }

            var shader = shaderSelector(material);
            if (shader == null)
            {
                yield break;
            }

            var propertyCount = shader.GetPropertyCount();
            if (propertyCount == 0)
            {
                yield break;
            }

            for (var propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                if (shader.GetPropertyType(propertyIndex) is UnityEngine.Rendering.ShaderPropertyType.Texture)
                {
                    var propertyNameId = shader.GetPropertyNameId(propertyIndex);
                    var texture = textureSelector(material, propertyNameId);
                    if (texture != null)
                    {
                        yield return texture;
                    }
                }
            }
        }

        public static void ResetAllProperties(this Material material)
        {
            var shader = material.shader;
            var propertyCount = shader.GetPropertyCount();

            for (var propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++) 
            {
                ResetProperty(material, shader, propertyIndex);
            }
        }
        public static void ResetProperty(this Material material, string propertyName)
        {
            var shader = material.shader;
            ResetProperty(material, shader, shader.FindPropertyIndex(propertyName));
        }
        private static void ResetProperty(Material material, Shader shader, int propertyIndex)
        {
            var propertyNameId = shader.GetPropertyNameId(propertyIndex);
            switch (shader.GetPropertyType(propertyIndex))
            {
                case UnityEngine.Rendering.ShaderPropertyType.Color:
                    {
                        material.SetColor(propertyNameId, shader.GetPropertyDefaultVectorValue(propertyIndex));
                        break;
                    }
                case UnityEngine.Rendering.ShaderPropertyType.Vector:
                    {
                        material.SetVector(propertyNameId, shader.GetPropertyDefaultVectorValue(propertyIndex));
                        break;
                    }
                case UnityEngine.Rendering.ShaderPropertyType.Float:
                case UnityEngine.Rendering.ShaderPropertyType.Range:
                    {
                        material.SetFloat(propertyNameId, shader.GetPropertyDefaultFloatValue(propertyIndex));
                        break;
                    }
                case UnityEngine.Rendering.ShaderPropertyType.Int:
                    {
                        material.SetInt(propertyNameId, shader.GetPropertyDefaultIntValue(propertyIndex));
                        break;
                    }
                case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    {
                        material.SetTexture(propertyNameId, null);
                        material.SetTextureScale(propertyNameId, Vector2.one);
                        material.SetTextureOffset(propertyNameId, Vector2.zero);
                        break;
                    }
            }
        }

        public static void CopyProperties(Material sMat, Material dMat)
        {
            var shader = sMat.shader;

            if (shader != dMat.shader) { throw new ArgumentException(); }

            var propertyCount = shader.GetPropertyCount();
            for (var propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                var propertyNameId = shader.GetPropertyNameId(propertyIndex);
                switch (shader.GetPropertyType(propertyIndex))
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                        {
                            dMat.SetColor(propertyNameId, sMat.GetColor(propertyNameId));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
                        {
                            dMat.SetVector(propertyNameId, sMat.GetVector(propertyNameId));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        {
                            dMat.SetFloat(propertyNameId, sMat.GetFloat(propertyNameId));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Int:
                        {
                            dMat.SetInt(propertyNameId, sMat.GetInt(propertyNameId));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        {
                            dMat.SetTexture(propertyNameId, sMat.GetTexture(propertyNameId));
                            dMat.SetTextureScale(propertyNameId, sMat.GetTextureScale(propertyNameId));
                            dMat.SetTextureOffset(propertyNameId, sMat.GetTextureOffset(propertyNameId));
                            break;
                        }
                }
            }
        }



    }
}
