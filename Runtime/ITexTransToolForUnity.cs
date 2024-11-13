#nullable enable
using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using net.rs64.TexTransTool.MultiLayerImage;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public interface ITexTransToolForUnity : ITexTransCoreEngine, ITexTransRenderTextureUploadToCreate
    {
        /// <summary>
        /// キーを文字列ベースで取得してくるやつ、MLIC とかいろいろ便利なタイミングは多いと思う
        /// キーに合うものがなかった場合の取り回しは...場合によって変える方がいいね、例外はいてもいいし、デフォルトにフォールバックしてもいい。動作しないものにしてもよいね
        /// </summary>
        ITTBlendKey QueryBlendKey(string blendKeyName);

        // デフォルト実装は極力使わないように、パフォーマンスがごみカスだし画質もカスなので...
        ITTDiskTexture Wrapping(Texture2D texture2D)
        {
            var unityRt = TTRt2.Get(texture2D.width, texture2D.height);
            Graphics.Blit(texture2D, unityRt);
            TextureBlend.ToGamma(unityRt);
            var discTex = new RenderTextureAsDiskTexture(UploadTexture(unityRt));
            TTRt2.Rel(unityRt);
            return discTex;
        }
        ITTDiskTexture Wrapping(TTTImportedImage imported)
        {
            throw new NotImplementedException();
        }

        ITTRenderTexture UploadTexture(RenderTexture renderTexture)
        {
            using var na = new NativeArray<byte>(renderTexture.width * renderTexture.height * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            renderTexture.DownloadFromRenderTexture(na.AsSpan());
            var rt = CreateRenderTexture(renderTexture.width, renderTexture.height, TexTransCoreTextureChannel.RGBA);
            UploadTexture<byte>(rt, na.AsSpan(), TexTransCoreTextureFormat.Byte);
            return rt;
        }

    }
    public interface ITexTransUnityDiskUtil : ITexTransUnityDiskWrapper, ITexTransLoadTextureWithDiskUtil
    { }
    public interface ITexTransUnityDiskWrapper
    {
        ITTDiskTexture Wrapping(Texture2D texture2D);
        ITTDiskTexture Wrapping(TTTImportedImage texture2D);
    }
    public interface ITexTransLoadTextureWithDiskUtil
    {
        void LoadTexture(ITexTransToolForUnity ttce4u, ITTRenderTexture writeTarget, ITTDiskTexture diskTexture);
    }

    public interface ITexTransRenderTextureUploadToCreate : ITexTransRenderTextureIO, ITexTransCreateTexture
    {
        ITTRenderTexture UploadTexture<T>(int width, int height, TexTransCoreTextureChannel channel, ReadOnlySpan<T> bytes, TexTransCoreTextureFormat format) where T : unmanaged
        {
            var rt = CreateRenderTexture(width, height, channel);
            UploadTexture(rt, bytes, format);
            return rt;
        }
    }
    public class RenderTextureAsDiskTexture : ITTDiskTexture
    {
        private ITTRenderTexture _renderTexture;

        public ITTRenderTexture TTRenderTexture => _renderTexture;

        public RenderTextureAsDiskTexture(ITTRenderTexture renderTexture)
        {
            _renderTexture = renderTexture;
        }
        public int Width => _renderTexture.Width;

        public int Hight => _renderTexture.Hight;

        public string Name { get => _renderTexture.Name; set => _renderTexture.Name = value; }

        public void Dispose()
        {
            _renderTexture.Dispose();
        }
    }

}
