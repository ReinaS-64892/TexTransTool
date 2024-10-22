using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public interface ITexTransToolForUnity
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
        ITTRenderTexture UploadTexture(int width, int height, TexTransCoreTextureFormat format, bool isLinear, ReadOnlySpan<byte> bytes);
    }

}
