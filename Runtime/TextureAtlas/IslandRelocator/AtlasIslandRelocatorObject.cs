using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransTool.TextureAtlas.IslandRelocator
{
    /// <summary>
    /// これらinterfaceは非常に実験的なAPIで予告なく変更や削除される可能性があります。
    ///
    /// AtlasTexture や今後追加されるUVの再配置を行うコンポーネントのアルゴリズムを拡張することのできるAPIで
    /// 設定として、並び替えアルゴリズムの名前と矩形移動かどうかが必要で、
    /// <see cref="SorterName"/> 名前はセーブデータに入るものなのでほかの物と被らないようにご注意ください。
    /// <see cref="RectTangleMove"/> true であれば　矩形でテクスチャの転写が行われ、そうでなければポリゴン単位でテクスチャの転写が行われます。
    ///
    /// そして並び替え <see cref="Sorting"/>
    /// これは 引数 atlasIslands の Pivot や Size を書き換え返すことでそれら並び替えができ、
    /// AtlasIslandID が同じであれば、戻り値の物はクローンの物でも問題はない。
    /// atlasIslandReference は ReadOnly 想定で中身を書き換えると正常に動作しなくなるよ。
    ///
    /// ちなみに現時点で使用するのは <see cref="NFDHPlasFC"/> しか存在せず、今の AtlasTexture では並び替えアルゴリズムを選択するUIは存在しないため
    /// DebugMode を使って書き換える必要があるためご注意ください。
    /// </summary>


    public abstract class AtlasIslandRelocatorObject : ScriptableObject, IAtlasIslandRelocator
    {
        public abstract bool RectTangleMove { get; }
        protected bool UseUpScaling { get; private set; }
        protected float Padding { get; private set; }
        bool IAtlasIslandRelocator.UseUpScaling { set => UseUpScaling = value; }
        float IAtlasIslandRelocator.Padding { set => Padding = value; }
        public abstract Dictionary<AtlasIslandID, IslandRect> Relocation(Dictionary<AtlasIslandID, IslandRect> atlasIslands, IReadOnlyDictionary<AtlasIslandID, AtlasIsland> atlasIslandReference);
    }


}
