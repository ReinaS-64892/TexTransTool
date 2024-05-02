using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.IslandRelocator
{
    /// <summary>
    /// これは非常に実験的なAPIで予告なく変更や削除される可能性があります。
    ///
    /// AtlasTexture や今後追加されるUVの再配置を行うコンポーネントのアルゴリズムを拡張することのできるAPIで
    /// 設定として、矩形移動かどうかが必要で、
    /// <see cref="RectTangleMove"/> true であれば　矩形でテクスチャの転写が行われ、そうでなければポリゴン単位でテクスチャの転写が行われます。
    ///
    /// そして並び替え <see cref="Sorting"/>
    /// これは 引数 atlasIslands の Pivot や Size を書き換え返すことでそれら並び替えができ、
    /// AtlasIslandID が同じであれば、戻り値の物はクローンの物でも問題はない。
    /// atlasIslandReference は ReadOnly 想定で中身を書き換えると正常に動作しなくなるよ。
    /// </summary>


    public abstract class AtlasIslandRelocatorObject : ScriptableObject, IAtlasIslandRelocator
    {
        public abstract bool RectTangleMove { get; }
        protected float Padding { get; private set; }
        float IAtlasIslandRelocator.Padding { set => Padding = value; }
        public abstract bool Relocation(IslandRect[] atlasIslands);
    }


}
