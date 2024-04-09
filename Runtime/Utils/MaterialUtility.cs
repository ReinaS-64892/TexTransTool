using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    internal static class MaterialUtility
    {
        public static Dictionary<Material, Material> ReplaceTextureAll(IEnumerable<Material> materials, Texture2D target, Texture2D setTex, Dictionary<Material, Material> outPut = null)
        {
            outPut?.Clear(); outPut ??= new();
            foreach (var mat in materials)
            {
                var textures = GetAllTexture2D(mat);

                if (textures.ContainsValue(target))
                {
                    var material = Object.Instantiate(mat);

                    foreach (var kvp in textures)
                    {
                        if (kvp.Value == target)
                        {
                            material.SetTexture(kvp.Key, setTex);
                        }
                    }

                    outPut.Add(mat, material);
                }
            }
            return outPut;
        }
        public static void SetTextures(this Material targetMat, List<PropAndTexture2D> propAndTextures, bool focusSetTexture = false)
        {
            foreach (var propAndTexture in propAndTextures)
            {
                if (!targetMat.HasProperty(propAndTexture.PropertyName)) { continue; }
                if (focusSetTexture || targetMat.GetTexture(propAndTexture.PropertyName) is Texture2D)
                {
                    targetMat.SetTexture(propAndTexture.PropertyName, propAndTexture.Texture2D);
                }
            }
        }

        public static Dictionary<string, Texture2D> GetAllTexture2D(this Material material, Dictionary<string, Texture2D> output = null)
        {
            output?.Clear(); output ??= new();
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
                    output.Add(propName, texture2D);
                }
            }
            return output;
        }
        public static IEnumerable<(string, Texture2D)> GetAllTexture2DProperty(this Material material)
        {
            if (material == null || material.shader == null) { yield break; }
            var shader = material.shader;
            var propCount = shader.GetPropertyCount();
            for (var i = 0; propCount > i; i += 1)
            {
                if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture) { continue; }
                var propName = shader.GetPropertyName(i);
                var texture = material.GetTexture(propName);
                var tex2D = texture as Texture2D;
                yield return (propName, tex2D);
            }
        }

        public static void AllPropertyReset(this Material material)
        {
            var shader = material.shader;
            var propCount = shader.GetPropertyCount();
            for (var i = 0; propCount > i; i += 1)
            {
                var pID = shader.GetPropertyNameId(i);
                switch (shader.GetPropertyType(i))
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                        {
                            material.SetColor(pID, shader.GetPropertyDefaultVectorValue(i));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
                        {
                            material.SetVector(pID, shader.GetPropertyDefaultVectorValue(i));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        {
                            material.SetFloat(pID, shader.GetPropertyDefaultFloatValue(i));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Int:
                        {
                            material.SetInt(pID, shader.GetPropertyDefaultIntValue(i));
                            break;
                        }
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        {
                            material.SetTexture(pID, null);
                            material.SetTextureScale(pID, Vector2.zero);
                            material.SetTextureOffset(pID, Vector2.zero);
                            break;

                        }
                }
            }
        }
    }
}
