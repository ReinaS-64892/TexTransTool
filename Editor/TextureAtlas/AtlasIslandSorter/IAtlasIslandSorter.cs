#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;
using Island = net.rs64.TexTransCore.Island.Island;
using static net.rs64.TexTransCore.TransTextureCore.TransTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.EditorIsland;
using net.rs64.TexTransTool.TextureAtlas.FineSetting;
using UnityEditor;

namespace net.rs64.TexTransTool.TextureAtlas
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
    ///
    /// ちなみに現時点で使用するのは <see cref="AtlasTexture"/> しか存在せず、その AtlasTexture では並び替えアルゴリズムを選択するUIは存在しないため
    /// DebugMode を使って書き換える日つよがあるためご注意ください。
    /// </summary>
    public interface IAtlasIslandSorter
    {
        string SorterName { get; }
        bool RectTangleMove { get; }
        Dictionary<AtlasIslandID, AtlasIsland> Sorting(Dictionary<AtlasIslandID, AtlasIsland> atlasIslands, float Padding);
    }

    internal static class AtlasIslandSorterUtility
    {
        static Dictionary<string, IAtlasIslandSorter> Sorters;
        static string[] SortersNames;
        [InitializeOnLoadMethod]
        static void Init()
        {
            var interfaces = InterfaceUtility.GetInterfaceInstance<IAtlasIslandSorter>();
            Sorters = new Dictionary<string, IAtlasIslandSorter>();
            foreach (var sorter in interfaces) { Sorters.Add(sorter.SorterName, sorter); }
            SortersNames = Sorters.Keys.ToArray();
        }
        public static IAtlasIslandSorter GetSorter(string SorterName)
        {
            if (Sorters == null) { Debug.LogError("Not Init"); return null; }
            if (!Sorters.ContainsKey(SorterName)) { Debug.LogError("Sorter Is not Exist"); return null; }
            return Sorters[SorterName];
        }

        public static string[] GetSorterName() => SortersNames;
    }
}
#endif