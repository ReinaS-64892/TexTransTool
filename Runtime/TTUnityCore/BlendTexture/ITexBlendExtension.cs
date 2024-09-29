using UnityEngine;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.BlendTexture
{
    /// <summary>
    /// これらinterfaceは非常に実験的なAPIで予告なく変更や削除される可能性があります。
    ///
    /// 任意の色合成を追加できるようにするAPIで、
    /// _DistTex を Base , _MainTex を Add として扱いその色合成の結果を描画するシェーダーが必要です。
    ///
    /// ShaderKeyword は EditorGUI.Popup() の仕様で / を使用した階層になり、拡張で追加するキーワードはすべて階層に入っていない場合は追加できません。
    /// つまり "hoge/fugaModeName" のような形にしてください。そして、それがシェーダーきーわどとして渡されるときは、"/" は "_" に置き換えられるので、
    /// シェーダーでは "hoge_fugaModeName" として、ShaderKeywordに渡されます。
    ///
    /// <see cref="TextureBlend.BlendBlit(RenderTexture, Texture, string, bool)"/>
    /// </summary>
    public interface ITexBlendExtension
    {
        (HashSet<string> ShaderKeywords, Shader shader) GetExtensionBlender();
    }
}