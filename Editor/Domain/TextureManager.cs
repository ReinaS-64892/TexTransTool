using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool
{
    internal readonly struct TextureManager : ITextureManager
    {
        private readonly bool Previewing;
        private readonly List<Texture2D> DestroyList;
        private readonly Dictionary<Texture2D, (TextureFormat CompressFormat, int Quality)> CompressDict;
        private readonly Dictionary<Texture2D, Texture2D> OriginDict;

        public TextureManager(bool previewing)
        {
            Previewing = previewing;
            if (!Previewing) { DestroyList = new List<Texture2D>(); }
            else { DestroyList = null; }
            if (!Previewing) { CompressDict = new Dictionary<Texture2D, (TextureFormat, int)>(); }
            else { CompressDict = null; }
            if (!Previewing) { OriginDict = new Dictionary<Texture2D, Texture2D>(); }
            else { OriginDict = null; }
        }

        public void DeferDestroyTexture2D(Texture2D texture2D)
        {
            DestroyList.Add(texture2D);
        }

        public void DestroyTextures()
        {
            if (DestroyList == null) { return; }
            foreach (var tex in DestroyList)
            {
                if (tex == null || AssetDatabase.Contains(tex)) { continue; }
                UnityEngine.Object.DestroyImmediate(tex);
            }
            DestroyList.Clear();
            if (OriginDict != null) { OriginDict.Clear(); }
        }

        public int GetOriginalTextureSize(Texture2D texture2D)
        {
            return NormalizePowerOfTwo(GetOriginalTexture(texture2D).width);
        }
        public void WriteOriginalTexture(Texture2D texture2D, RenderTexture writeTarget)
        {
            Graphics.Blit(GetOriginalTexture(texture2D), writeTarget);
        }
        public Texture2D GetOriginalTexture(Texture2D texture2D)
        {
            if (Previewing)
            {
                return texture2D;
            }
            else
            {
                if (OriginDict.ContainsKey(texture2D))
                {
                    return OriginDict[texture2D];
                }
                else
                {
                    var originTex = texture2D.TryGetUnCompress();
                    DeferDestroyTexture2D(originTex);
                    OriginDict.Add(texture2D, originTex);
                    return originTex;
                }
            }
        }
        public void TextureCompressDelegation((TextureFormat CompressFormat, int Quality) compressSetting, Texture2D target)
        {
            if (CompressDict == null) { return; }
            CompressDict[target] = compressSetting;
        }
        public void ReplaceTextureCompressDelegation(Texture2D Souse, Texture2D Target)
        {
            if (CompressDict == null) { return; }
            if (Target == Souse) { return; }
            if (CompressDict.ContainsKey(Souse))
            {
                CompressDict[Target] = CompressDict[Souse];
                CompressDict.Remove(Souse);
            }
            else
            {
                CompressDict[Target] = (Souse.format, 50);
            }
        }

        public void TextureFinalize()
        {
            if (CompressDict == null) { return; }
            foreach (var texAndFormat in CompressDict)
            {
                EditorUtility.CompressTexture(texAndFormat.Key, texAndFormat.Value.CompressFormat, texAndFormat.Value.Quality);
            }

            foreach (var tex in CompressDict.Keys)
            {
                tex.Apply(true, true);
            }

            foreach (var tex in CompressDict.Keys)
            {
                var sTexture = new SerializedObject(tex);

                var sStreamingMipmaps = sTexture.FindProperty("m_StreamingMipmaps");
                sStreamingMipmaps.boolValue = true;

                sTexture.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public static int NormalizePowerOfTwo(int v) => Mathf.IsPowerOfTwo(v) ? v : Mathf.NextPowerOfTwo(v);



    }
}
