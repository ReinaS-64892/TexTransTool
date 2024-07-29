using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransCore.Utils
{
    internal static class MaterialUtility
    {
        public static Dictionary<Material, Material> ReplaceTextureAll<Tex>(IEnumerable<Material> materials, Tex target, Tex setTex)
        where Tex : Texture
        {
            var outPut = new Dictionary<Material, Material>();
            foreach (var mat in materials)
            {
                var textures = GetAllTexture(mat);

                if (textures.ContainsValue(target))
                {
                    var material = UnityEngine.Object.Instantiate(mat);

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
        public static Dictionary<Material, Material> ReplaceTextureAll<Tex>(IEnumerable<Material> materials, Dictionary<Tex, Tex> texturePair)
        where Tex : Texture
        {
            var outPut = new Dictionary<Material, Material>();
            foreach (var mat in materials)
            {
                var textures = GetAllTexture(mat);

                bool replacedFlag = false;
                foreach (var tex in textures) { if (texturePair.ContainsKey(tex.Value as Tex)) { replacedFlag = true; break; } }
                if (replacedFlag == false) { continue; }

                var material = UnityEngine.Object.Instantiate(mat);
                foreach (var tex in textures) { if (texturePair.TryGetValue(tex.Value as Tex, out var swapTex)) { material.SetTexture(tex.Key, swapTex); } }
                outPut.Add(mat, material);
            }
            return outPut;
        }
        public static void SetTextures<Tex>(this Material targetMat, Dictionary<string, Tex> propAndTextures, Func<Texture, bool> setTargetComparer)
        where Tex : Texture
        {
            foreach (var propAndTexture in propAndTextures)
            {
                if (!targetMat.HasProperty(propAndTexture.Key)) { continue; }
                if (setTargetComparer(targetMat.GetTexture(propAndTexture.Key)))
                {
                    targetMat.SetTexture(propAndTexture.Key, propAndTexture.Value);
                }
            }
        }

        public static Dictionary<string, Texture2D> GetAllTexture2D(this Material material)
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
        public static Dictionary<string, Texture> GetAllTexture(this Material material)
        {
            var output = new Dictionary<string, Texture>();
            if (material == null || material.shader == null) { return output; }
            var shader = material.shader;
            var propCount = shader.GetPropertyCount();
            for (var i = 0; propCount > i; i += 1)
            {
                if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture) { continue; }
                var propName = shader.GetPropertyName(i);
                var texture = material.GetTexture(propName);
                if (texture != null) { output.TryAdd(propName, texture); }
            }
            return output;
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
