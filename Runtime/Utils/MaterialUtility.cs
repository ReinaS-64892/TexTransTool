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

    }
}