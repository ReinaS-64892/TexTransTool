using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public interface ITexTransToolForUnity : ITexTransCoreEngine
    {
        /// <summary>
        /// キーを文字列ベースで取得してくるやつ、MLIC とかいろいろ便利なタイミングは多いと思う
        /// キーに合うものがなかった場合の取り回しは...場合によって変える方がいいね、例外はいてもいいし、デフォルトにフォールバックしてもいい。動作しないものにしてもよいね
        /// </summary>
        ITTBlendKey QueryBlendKey(string blendKeyName);

        ITTDiskTexture Wrapping(Texture2D texture2D);
        ITTDiskTexture Wrapping(TTTImportedImage texture2D);

        /// <summary>
        /// 基本的にパフォーマンスはカスであること前提なので使わない方向性で進めたいね
        /// </summary>
        void UploadTexture<T>(ITTRenderTexture uploadTarget, ReadOnlySpan<T> bytes, TexTransCoreTextureFormat format) where T : unmanaged;

        ITTRenderTexture UploadTexture<T>(int width, int height, TexTransCoreTextureChannel channel, ReadOnlySpan<T> bytes, TexTransCoreTextureFormat format) where T : unmanaged
        {
            var rt = CreateRenderTexture(width, height, channel);
            UploadTexture(rt, bytes, format);
            return rt;
        }
        ITTRenderTexture UploadTexture(RenderTexture renderTexture)
        {
            var texture2D = renderTexture.CopyTexture2D();
            var rt = CreateRenderTexture(texture2D.width, texture2D.height, TexTransCoreTextureChannel.RGBA);
            UploadTexture<byte>(rt, texture2D.GetRawTextureData<byte>().AsSpan(), TexTransCoreTextureFormat.Byte);
            UnityEngine.Object.DestroyImmediate(texture2D);
            return rt;
        }

        void DownloadTexture<T>(ITTRenderTexture renderTexture, TexTransCoreTextureFormat format, Span<T> dataDist) where T : unmanaged;
    }

}
