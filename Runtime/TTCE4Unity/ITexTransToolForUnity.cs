#nullable enable
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.TransTexture;
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
        public const string BL_KEY_NOT_BLEND = "NotBlend";
        public const string DS_ALGORITHM_DEFAULT = "AverageSampling";

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
            var uploadTex = UploadTexture(unityRt);
            this.LinearToGamma(uploadTex);
            var discTex = new RenderTextureAsDiskTexture(uploadTex);
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

        // インターフェースから中身の RenderTexture を引き出す。
        // 何度も引き出しても同じものである必要がある、基本的に Unity ベッタリな場所でしか使用してはならず、使用できない実装があっても良い。
        RenderTexture GetReferenceRenderTexture(ITTRenderTexture renderTexture)
        {
            throw new NotSupportedException();
        }

        // できる場合はいい感じに 内部で使用されている物を雑に投げつけて
        public TexTransCoreTextureFormat PrimaryTextureFormat { get => TexTransCoreTextureFormat.Byte; }

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
        public static ITTRenderTexture WrappingOrUploadToLoadFullScale(this ITexTransToolForUnity engine, Texture texture)
        {
            switch (texture)
            {
                case Texture2D texture2D:
                    {
                        using var diskTex = engine.Wrapping(texture2D);
                        return engine.LoadTextureWidthFullScale(diskTex);
                    }
                case RenderTexture rt: { return engine.UploadTexture(rt); }
                default: { throw new InvalidOperationException(); }
            }
        }
        public static void WrappingOrUploadToLoad(this ITexTransToolForUnity engine, ITTRenderTexture write, Texture texture)
        {
            switch (texture)
            {
                case Texture2D texture2D:
                    {
                        using var diskTex = engine.Wrapping(texture2D);
                        engine.LoadTextureWidthAnySize(write, diskTex);
                        break;
                    }
                case RenderTexture rt:
                    {
                        using var uploaded = engine.UploadTexture(rt);
                        engine.CopyTextureWidthAnySize(write, uploaded);
                        break;
                    }
                default: { throw new InvalidOperationException(); }
            }
        }
        public static ITTTexture WrappingOrUpload(this ITexTransToolForUnity engine, Texture texture)
        {
            switch (texture)
            {
                case Texture2D texture2D: { return engine.Wrapping(texture2D); }
                case RenderTexture rt: { return engine.UploadTexture(rt); }
                default: { throw new InvalidOperationException(); }
            }
        }


        internal static Texture2D DownloadToTexture2D<TTT4U>(this TTT4U engine, ITTRenderTexture renderTexture, bool useMipMap, TexTransCoreTextureFormat? overrideFormat = null, bool isLiner = false)
        where TTT4U : ITexTransToolForUnity
        {
            var ttcFormat = overrideFormat ?? engine.PrimaryTextureFormat;
            var texFormat = ttcFormat.ToUnityTextureFormat();
            var bpp = EnginUtil.GetPixelParByte(ttcFormat, TexTransCoreTextureChannel.RGBA);

            var resultTex2D = new Texture2D(renderTexture.Width, renderTexture.Hight, texFormat, useMipMap, isLiner);
            var map = resultTex2D.GetRawTextureData<byte>().AsSpan().Slice(0, renderTexture.Width * renderTexture.Hight * bpp);
            engine.DownloadTexture(map, ttcFormat, renderTexture);

            return resultTex2D;
        }

    }

}
