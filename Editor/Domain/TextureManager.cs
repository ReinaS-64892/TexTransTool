using System.Collections.Generic;
using System.IO;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.Utils;
using Unity.Collections;
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
        private readonly Dictionary<TTTImportedCanvasDescription, byte[]> CanvasSouse;

        public TextureManager(bool previewing)
        {
            Previewing = previewing;
            DestroyList = !Previewing ? new() : null;
            CompressDict = !Previewing ? new() : null;
            OriginDict = !Previewing ? new() : null;
            CanvasSouse = !Previewing ? new() : null;
        }

        public void DeferDestroyOf(Texture2D texture2D)
        {
            DestroyList.Add(texture2D);
        }

        public void DestroyDeferred()
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
            return TexTransCore.TransTextureCore.Utils.TextureUtility.NormalizePowerOfTwo(GetOriginalTexture(texture2D).width);
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
                    DeferDestroyOf(originTex);
                    OriginDict.Add(texture2D, originTex);
                    return originTex;
                }
            }
        }
        public void WriteOriginalTexture(TTTImportedImage texture, RenderTexture writeTarget)
        {
            if (Previewing)
            {
                Graphics.Blit(texture.PreviewTexture, writeTarget);
            }
            else
            {
                if (!CanvasSouse.ContainsKey(texture.CanvasDescription)) { CanvasSouse[texture.CanvasDescription] = File.ReadAllBytes(AssetDatabase.GetAssetPath(texture.CanvasDescription)); }
                texture.LoadImage(CanvasSouse[texture.CanvasDescription], writeTarget);
            }
        }
        public void DeferTextureCompress((TextureFormat CompressFormat, int Quality) compressSetting, Texture2D target)
        {
            if (CompressDict == null) { return; }
            CompressDict[target] = compressSetting;
        }
        public void DeferInheritTextureCompress(Texture2D Souse, Texture2D Target)
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

        public void CompressDeferred()
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
    }
}
