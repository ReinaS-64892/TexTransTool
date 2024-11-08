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
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool
{
    internal class TTCE4UnityWithTTT4Unity : TTCEUnity, ITexTransToolForUnity
    {
        private bool _isPreview;
        public TTCE4UnityWithTTT4Unity(bool isPreview, IOriginTexture iOrigin) : base(iOrigin.LoadTexture, t => { var size = iOrigin.GetOriginalTextureSize(t); return (size, size); })
        {
            _isPreview = isPreview;
        }

        public void UploadTexture<T>(ITTRenderTexture uploadTarget, ReadOnlySpan<T> bytes, TexTransCoreTextureFormat format) where T : unmanaged
        {
            var tex = new Texture2D(uploadTarget.Width, uploadTarget.Hight, format.ToUnityTextureFormat(uploadTarget.ContainsChannel), false);

            using var na = new NativeArray<T>(bytes.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            bytes.CopyTo(na);
            tex.LoadRawTextureData(na);

            tex.Apply();
            Graphics.Blit(tex, uploadTarget.Unwrap());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        public ITTRenderTexture UploadTexture(RenderTexture renderTexture)
        {
            var rt = CreateRenderTexture(renderTexture.width, renderTexture.height);
            Graphics.Blit(renderTexture, rt.Unwrap());
            return rt;
        }

        public void DownloadTexture<T>(ITTRenderTexture renderTexture, TexTransCoreTextureFormat format, Span<T> dataDist) where T : unmanaged
        {
            if (renderTexture.Unwrap().graphicsFormat == format.ToUnityGraphicsFormat(renderTexture.ContainsChannel))
            {
                renderTexture.Unwrap().DownloadFromRenderTexture(dataDist);
            }
            else
            {
                var cfRt = new RenderTexture(renderTexture.Width, renderTexture.Hight, 0, format.ToUnityGraphicsFormat(renderTexture.ContainsChannel));
                Graphics.Blit(renderTexture.Unwrap(), cfRt);
                cfRt.DownloadFromRenderTexture(dataDist);
            }
        }


        public ITTBlendKey QueryBlendKey(string blendKeyName)
        {
            return rs64.TexTransCoreEngineForUnity.TextureBlend.BlendObjects[blendKeyName];
        }

        public ITTDiskTexture Wrapping(Texture2D texture2D)
        {
            return new UnityDiskTexture(texture2D, _preloadAndTextureSize(texture2D));
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

    }
}
