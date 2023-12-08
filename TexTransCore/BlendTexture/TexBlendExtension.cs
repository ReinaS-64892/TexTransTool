using UnityEngine;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.BlendTexture
{
    /// <summary>
    /// これらinterfaceは非常に実験的なAPIで予告なく変更や削除される可能性があります。
    ///
    /// 任意の色合成を追加できるようにするAPIで、
    /// _DistTex を Base , _MainTex を Add として扱いその色合成の結果を描画するシェーダーが必要で、
    /// 複数の色合成を持つ場合 ShaderKeyword で変更できるようにしたものが必要です。
    /// そして ShaderKeyword はセーブデータに保存されるので他のものと被らないようにご注意ください。
    /// <see cref="TextureBlend.BlendBlit(RenderTexture, Texture, string, bool)"/>
    /// </summary>
    public interface TexBlendExtension
    {
        (HashSet<string> ShaderKeywords, Shader shader) GetExtensionBlender();
    }
}