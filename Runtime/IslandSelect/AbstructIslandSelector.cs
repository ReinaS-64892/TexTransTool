using UnityEngine;
using net.rs64.TexTransTool;
using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using Unity.Collections;
using System.Collections;

namespace net.rs64.TexTransTool.IslandSelector
{
    public abstract class AbstractIslandSelector : MonoBehaviour, ITexTransToolTag, IIslandSelector
    {
        internal const string FoldoutName = "IslandSelector";

        [HideInInspector, SerializeField] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        int ITexTransToolTag.SaveDataVersion => _saveDataVersion;

        internal abstract BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription);
        BitArray IIslandSelector.IslandSelect(Island[] islands, IslandDescription[] islandDescription) => IslandSelect(islands, islandDescription);
        /*
        頂点の持つ情報
        レンダラー
        レンダラーから見た時のスロット
        マテリアルの参照を比較してはならない。
        */


    }
    internal interface IIslandSelector
    {
        internal abstract BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription);
    }
    internal readonly struct IslandDescription
    {
        public readonly NativeArray<Vector3> Position;//ワールドスペース
        public readonly NativeArray<Vector2> UV;
        public readonly Renderer Renderer;
        public readonly int MaterialSlot;

        public IslandDescription(NativeArray<Vector3> position, NativeArray<Vector2> uV, Renderer renderer, int materialSlot)
        {
            Position = position;
            UV = uV;
            Renderer = renderer;
            MaterialSlot = materialSlot;
        }

    }
}
