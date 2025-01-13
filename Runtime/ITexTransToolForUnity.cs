#nullable enable
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.Utils;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public interface ITexTransToolForUnity : ITexTransCoreEngine
    {
        public const string BL_KEY_DEFAULT = "Normal";

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
            // TextureBlend.ToGamma(unityRt);
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
            var (format, channel) = renderTexture.graphicsFormat.ToTTCTextureFormat();
            using var na = new NativeArray<byte>(EnginUtil.GetPixelParByte(format, channel) * renderTexture.width * renderTexture.height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            renderTexture.DownloadFromRenderTexture(na.AsSpan());
            return UploadTexture<byte>(renderTexture.width, renderTexture.height, channel, na.AsSpan(), format);
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


    public static class TTT4UnityUtility
    {
        public static Texture2D DownloadToTexture2D<TTT4U>(this TTT4U ttt4u, ITTRenderTexture rt, TexTransCoreTextureFormat format = TexTransCoreTextureFormat.Byte)
        where TTT4U : ITexTransRenderTextureIO
        {
            var tex = new Texture2D(rt.Width, rt.Hight, format.ToUnityTextureFormat(rt.ContainsChannel), false);
            var texPtr = tex.GetRawTextureData<byte>();
            ttt4u.DownloadTexture<byte>(texPtr, format, rt);
            tex.Apply();
            return tex;
        }
    }

}
