using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using JetBrains.Annotations;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using net.rs64.TexTransTool.MultiLayerImage;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal class TTCE4UnityWithTTT4Unity : TTCEForUnity, ITexTransToolForUnity
    {
        private bool _isPreview;
        public TTCE4UnityWithTTT4Unity(bool isPreview, IOriginTexture iOrigin) : base(iOrigin.LoadTexture)
        {
            _isPreview = isPreview;
        }


        public ITTRenderTexture UploadTexture(RenderTexture renderTexture)
        {
            var tex2D = renderTexture.CopyTexture2D();
            var data = tex2D.GetPixelData<byte>(0);//後々ここら辺はコアに持っていく必要がある
            var rt = UploadTexture(tex2D.width, tex2D.height, ToTTCTextureFormat(tex2D.format), false, data);

            Texture2D.DestroyImmediate(tex2D);
            return rt;
        }
        public ITTRenderTexture UploadTexture(int width, int height, TexTransCoreTextureFormat format, bool isLinear, ReadOnlySpan<byte> bytes)
        {
            var tex = new Texture2D(width, height, ToUnityTextureFormat(format), false, isLinear);
            using (var na = new NativeArray<byte>(bytes.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            {
                bytes.CopyTo(na);
                tex.LoadRawTextureData(na);
                tex.Apply();

                var rt = CreateRenderTexture(width, height, false, false);

                Graphics.Blit(tex, rt.Unwrap());
                return rt;
            }
        }

        internal static TextureFormat ToUnityTextureFormat(TexTransCoreTextureFormat format)
        {
            switch (format)
            {
                default: throw new ArgumentOutOfRangeException(format.ToString());
                case TexTransCoreTextureFormat.RGBA_Byte: return TextureFormat.RGBA32;
                case TexTransCoreTextureFormat.RGBA_UShort: return TextureFormat.RGBA64;
                case TexTransCoreTextureFormat.RGBA_Float: return TextureFormat.RGBAFloat;
            }
        }
        internal static TexTransCoreTextureFormat ToTTCTextureFormat(TextureFormat format)
        {
            switch (format)
            {
                default: throw new ArgumentOutOfRangeException(format.ToString());
                case TextureFormat.RGBA32: return TexTransCoreTextureFormat.RGBA_Byte;
                case TextureFormat.RGBA64: return TexTransCoreTextureFormat.RGBA_UShort;
                case TextureFormat.RGBAFloat: return TexTransCoreTextureFormat.RGBA_Float;
            }
        }

        public ITTDiskTexture Wrapping(Texture2D texture2D)
        {
            return new UnityDiskTexture(texture2D);
        }

        public ITTDiskTexture Wrapping(TTTImportedImage texture2D)
        {
            return new UnityImportedDiskTexture(texture2D, _isPreview);
        }
        internal class UnityImportedDiskTexture : ITTDiskTexture
        {
            internal TTTImportedImage Texture;
            private bool _isPreview;

            public UnityImportedDiskTexture(TTTImportedImage texture, bool isPreview)
            {
                Texture = texture;
                _isPreview = isPreview;
            }
            public int Width => _isPreview ? Texture.PreviewTexture.width : Texture.CanvasDescription.Width;

            public int Hight => _isPreview ? Texture.PreviewTexture.height : Texture.CanvasDescription.Height;

            public bool MipMap => false;

            public string Name { get => Texture.name; set => Texture.name = value; }


            public void Dispose() { }
        }
        ITTBlendKey ITexTransToolForUnity.QueryBlendKey(string blendKeyName)
        {
            return rs64.TexTransCoreEngineForUnity.TextureBlend.BlendObjects[blendKeyName];
        }

    }
}
