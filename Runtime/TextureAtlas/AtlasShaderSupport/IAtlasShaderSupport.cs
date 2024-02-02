using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    /// <summary>
    /// これらinterfaceは非常に実験的なAPIで予告なく変更や削除される可能性があります。
    ///
    /// この interface は AtlasTexture でアトラス化できるシェーダーのサポートを追加するAPIで
    /// 自身が対象とするマテリアルかどうかを判別する関数
    /// アトラス化対象のマテリアルの情報を貯める関数
    /// 貯めた情報の解放
    /// 貯めた情報を用いて、設定に応じてベイクなどを行いつつ マテリアルからテクスチャを引き出す関数
    /// 生成されたマテリアルに対して後処理(最適化など)を行う関数
    /// があり、<see cref="liltoonAtlasSupport"/> や <see cref="AtlasShaderRecorder"/> , <see cref="TextureBaker"/> などを参考にしてください。
    /// </summary>
    public interface IAtlasShaderSupport
    {
        bool IsThisShader(Material material);
        void AddRecord(Material material);
        void ClearRecord();

        List<PropAndTexture> GetPropertyAndTextures(IOriginTexture textureManager, Material material, PropertyBakeSetting bakeSetting);
        void MaterialCustomSetting(Material material);
    }
}
