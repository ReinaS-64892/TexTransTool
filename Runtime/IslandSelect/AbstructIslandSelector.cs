using UnityEngine;
using net.rs64.TexTransTool;
using System.Collections.Generic;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransTool.IslandSelector
{
    public abstract class AbstractIslandSelector : MonoBehaviour, ITexTransToolTag
    {
        [HideInInspector, SerializeField] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        int ITexTransToolTag.SaveDataVersion => _saveDataVersion;

        internal abstract HashSet<Key> IslandSelect<Key>(Dictionary<Key, Island> islands, Dictionary<Key,IslandDescription> islandDescription);

        internal readonly struct IslandDescription
        {
            public readonly Vector3[] Position;//ワールドスペース
            public readonly Vector2[] UV;
            public readonly Renderer Renderer;
            public readonly int MaterialSlot;

            public IslandDescription(Vector3[] position, Vector2[] uV, Renderer renderer, int materialSlot) : this()
            {
                Position = position;
                UV = uV;
                Renderer = renderer;
                MaterialSlot = materialSlot;
            }

        }

        /*
        頂点の持つ情報
        レンダラー
        レンダラーから見た時のスロット (マテリアルはここから調べるように)
        */


    }
}
