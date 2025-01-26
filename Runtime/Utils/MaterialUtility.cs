using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    internal static class MaterialUtility
    {
        public static Dictionary<Material, Material> ReplaceTextureAll<Tex>(IEnumerable<Material> materials, Dictionary<Tex, Tex> texturePair)
        where Tex : Texture
        {
            var outPut = new Dictionary<Material, Material>();
            foreach (var mat in materials)
            {
                var textures = GetAllTextureWithDictionary(mat);

                bool replacedFlag = false;
                foreach (var tex in textures) { if (texturePair.ContainsKey(tex.Value as Tex)) { replacedFlag = true; break; } }
                if (replacedFlag == false) { continue; }

                var material = UnityEngine.Object.Instantiate(mat);
                foreach (var tex in textures) { if (texturePair.TryGetValue(tex.Value as Tex, out var swapTex)) { material.SetTexture(tex.Key, swapTex); } }
                outPut.Add(mat, material);
            }
            return outPut;
        }

        public static bool ContainsTexture<Tex>(this Material mat, Tex target)
        where Tex : Texture
        {
            var shader = mat.shader;
            var pc = shader.GetPropertyCount();
            for (var i = 0; pc > i; i += 1)
            {
                var NameID = shader.GetPropertyNameId(i);
                switch (shader.GetPropertyType(i))
                {
                    default: break;
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        {
                            var dTex = mat.GetTexture(NameID) as Tex;
                            if (dTex == null) { break; }
                            if (dTex != target) { break; }
                            return true;
                        }
                }
            }
            return false;
        }
        public static void ReplaceTextureInPlace<Tex>(this Material mat, Tex target, Tex set)
        where Tex : Texture
        {
            var shader = mat.shader;
            var pc = shader.GetPropertyCount();
            for (var i = 0; pc > i; i += 1)
            {
                var NameID = shader.GetPropertyNameId(i);
                switch (shader.GetPropertyType(i))
                {
                    default: break;
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        {
                            var dTex = mat.GetTexture(NameID) as Tex;
                            if (dTex == null) { break; }
                            if (dTex != target) { break; }
                            mat.SetTexture(NameID, set);
                            break;
                        }
                }
            }
        }

        public static Dictionary<string, Texture2D> GetAllTexture2DWithDictionary(this Material material)
        {
            var output = new Dictionary<string, Texture2D>();
            if (material == null || material.shader == null) { return output; }
            var shader = material.shader;
            var propCount = shader.GetPropertyCount();
            for (var i = 0; propCount > i; i += 1)
            {
                if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture) { continue; }
                var propName = shader.GetPropertyName(i);
                var texture = material.GetTexture(propName);
                if (texture != null && texture is Texture2D texture2D)
                {
                    output.TryAdd(propName, texture2D);
                }
            }
            return output;
        }
        public static Dictionary<string, Texture> GetAllTextureWithDictionary(this Material material)
        {
            return GetAllTextureWithDictionary<Texture>(material);
        }
        public static Dictionary<string, Tex> GetAllTextureWithDictionary<Tex>(this Material material) where Tex : Texture
        {
            var output = new Dictionary<string, Tex>();
            if (material == null || material.shader == null) { return output; }
            var shader = material.shader;
            var propCount = shader.GetPropertyCount();
            for (var i = 0; propCount > i; i += 1)
            {
                if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture) { continue; }
                var propName = shader.GetPropertyName(i);
                var texture = material.GetTexture(propName) as Tex;
                if (texture != null) { output.TryAdd(propName, texture); }
            }
            return output;
        }

        public static IEnumerable<Tex> GetAllTexture<Tex>(this Material material) where Tex : Texture
        {
            if (material == null || material.shader == null) { yield break; }
            var shader = material.shader;
            var propCount = shader.GetPropertyCount();
            for (var i = 0; propCount > i; i += 1)
            {
                if (shader.GetPropertyType(i) is not UnityEngine.Rendering.ShaderPropertyType.Texture) { continue; }

                var texture = material.GetTexture(shader.GetPropertyNameId(i)) as Tex;
                if (texture != null)
                {
                    yield return texture;
                }
            }
        }
        public static IEnumerable<Tex> GetAllTexture<Tex>(this Material material, Func<Material,Shader> GetShader,Func<Material, int, Tex> GetTex) where Tex : Texture
        {
            if (material == null || material.shader == null) { yield break; }
            var shader = GetShader(material);
            var propCount = shader.GetPropertyCount();
            for (var i = 0; propCount > i; i += 1)
            {
                if (shader.GetPropertyType(i) is not UnityEngine.Rendering.ShaderPropertyType.Texture) { continue; }

                var texture = GetTex(material, shader.GetPropertyNameId(i));
                if (texture != null)
                {
                    yield return texture;
                }
            }
        }
        public static void AllPropertyReset(this Material material)
        {
            var shader = material.shader;
            var propCount = shader.GetPropertyCount();
            for (var propIndex = 0; propCount > propIndex; propIndex += 1)
            {
                ResetPropertyFromNameID(material, shader, propIndex);
            }
        }
        public static void PropertyReset(this Material material, string propertyName)
        {
            var shader = material.shader;
            ResetPropertyFromNameID(material, shader, shader.FindPropertyIndex(propertyName));
        }
        private static void ResetPropertyFromNameID(Material material, Shader shader, int propertyIndex)
        {
            var NameID = shader.GetPropertyNameId(propertyIndex);
            switch (shader.GetPropertyType(propertyIndex))
            {
                case UnityEngine.Rendering.ShaderPropertyType.Color:
                    {
                        material.SetColor(NameID, shader.GetPropertyDefaultVectorValue(propertyIndex));
                        break;
                    }
                case UnityEngine.Rendering.ShaderPropertyType.Vector:
                    {
                        material.SetVector(NameID, shader.GetPropertyDefaultVectorValue(propertyIndex));
                        break;
                    }
                case UnityEngine.Rendering.ShaderPropertyType.Float:
                case UnityEngine.Rendering.ShaderPropertyType.Range:
                    {
                        material.SetFloat(NameID, shader.GetPropertyDefaultFloatValue(propertyIndex));
                        break;
                    }
                case UnityEngine.Rendering.ShaderPropertyType.Int:
                    {
                        material.SetInt(NameID, shader.GetPropertyDefaultIntValue(propertyIndex));
                        break;
                    }
                case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    {
                        material.SetTexture(NameID, null);
                        material.SetTextureScale(NameID, Vector2.one);
                        material.SetTextureOffset(NameID, Vector2.zero);
                        break;
                    }
            }
        }

        public static void PropertyCopy(Material sMat, Material dMat)
        {
            if (sMat.shader != dMat.shader) { throw new ArgumentException(); }
            var shader = sMat.shader;
            var pc = shader.GetPropertyCount();
            for (var i = 0; pc > i; i += 1)
            {
                var NameID = shader.GetPropertyNameId(i);
                switch (shader.GetPropertyType(i))
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                        {
                            dMat.SetColor(NameID, sMat.GetColor(NameID));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
                        {
                            dMat.SetVector(NameID, sMat.GetVector(NameID));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        {
                            dMat.SetFloat(NameID, sMat.GetFloat(NameID));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Int:
                        {
                            dMat.SetInt(NameID, sMat.GetInt(NameID));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        {
                            dMat.SetTexture(NameID, sMat.GetTexture(NameID));
                            dMat.SetTextureScale(NameID, sMat.GetTextureScale(NameID));
                            dMat.SetTextureOffset(NameID, sMat.GetTextureOffset(NameID));
                            break;
                        }
                }
            }
        }



    }
}
