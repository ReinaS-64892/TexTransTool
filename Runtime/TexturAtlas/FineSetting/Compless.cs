#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool.TexturAtlas.FineSettng
{
    public struct Compless : IFineSetting
    {
        public int Order => 0;
        public FromatQuality fromatQuality;
        public TextureCompressionQuality compressionQuality;
        public string PropatyNames;
        public PropatySelect select;

        public Compless(FromatQuality compless_fromatQuality, TextureCompressionQuality compless_compressionQuality, string compless_PropatyNames, PropatySelect compless_select)
        {
            fromatQuality = compless_fromatQuality;
            compressionQuality = compless_compressionQuality;
            PropatyNames = compless_PropatyNames;
            select = compless_select;

        }

        public enum FromatQuality
        {
            None,
            Low,
            Normal,
            High,
        }
        public void FineSetting(List<PropAndTexture2D> propAndTextures)
        {
            TextureFormat textureFormat = GetTextureFormat(fromatQuality);
            foreach (var target in FineSettingUtil.FiltTarget(PropatyNames, select, propAndTextures))
            {
                if (target.Texture2D.format == textureFormat) { continue; }
                EditorUtility.CompressTexture(target.Texture2D, textureFormat, compressionQuality);
            }
        }

        public static TextureFormat GetTextureFormat(FromatQuality fromatQuality)
        {
            var textureFormat = TextureFormat.RGBA32;
#if UNITY_STANDALONE_WIN
            switch (fromatQuality)
            {
                case FromatQuality.None:
                    textureFormat = TextureFormat.RGBA32;
                    break;
                case FromatQuality.Low:
                    textureFormat = TextureFormat.DXT1;
                    break;
                default:
                case FromatQuality.Normal:
                    textureFormat = TextureFormat.DXT5;
                    break;
                case FromatQuality.High:
                    textureFormat = TextureFormat.BC7;
                    break;
            }
#elif UNITY_ANDROID
            switch (fromatQuality)
            {
                case FromatQuality.None:
                    textureFormat = TextureFormat.RGBA32;
                    break;
                case FromatQuality.Low:
                    textureFormat = TextureFormat.ASTC_8x8;
                    break;
                default:
                case FromatQuality.Normal:
                    textureFormat = TextureFormat.ASTC_6x6;
                    break;
                case FromatQuality.High:
                    textureFormat = TextureFormat.ASTC_4x4;
                    break;
            }
#endif
            return textureFormat;
        }
    }


}
#endif