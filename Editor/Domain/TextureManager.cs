#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public struct TextureManager : ITextureManager, IDisposable
    {
        private readonly bool Previewing;
        private readonly List<Texture2D> DestroyList;

        public TextureManager(bool previewing)
        {
            Previewing = previewing;
            if (!Previewing) { DestroyList = new List<Texture2D>(); }
            else { DestroyList = null; }
        }

        public void DeferDestroyTexture2D(Texture2D texture2D)
        {
            DestroyList.Add(texture2D);
        }

        public void Dispose()
        {
            if (DestroyList == null) { return; }
            foreach (var tex in DestroyList)
            {
                if (tex == null) { continue; }
                UnityEngine.Object.DestroyImmediate(tex);
            }
            DestroyList.Clear();
        }

        public Texture2D GetOriginalTexture2D(Texture2D texture2D)
        {
            if (Previewing)
            {
                return texture2D;
            }
            else
            {
                var originTex = texture2D.TryGetUnCompress();
                DeferDestroyTexture2D(originTex);
                return originTex;
            }
        }
    }
}
#endif