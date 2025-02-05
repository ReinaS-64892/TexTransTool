#nullable enable
using UnityEngine;
using Unity.Collections;
using System.Collections;
using net.rs64.TexTransTool.UVIsland;
using System;

namespace net.rs64.TexTransTool.IslandSelector
{
    [DisallowMultipleComponent]
    public abstract class AbstractIslandSelector : TexTransMonoBaseGameObjectOwned, IIslandSelector, IEquatable<AbstractIslandSelector>
    {
        internal const string FoldoutName = "IslandSelector";

        internal abstract void LookAtCalling(ILookingObject looker);


        internal abstract BitArray IslandSelect(IslandSelectorContext ctx);
        BitArray IIslandSelector.IslandSelect(IslandSelectorContext ctx) => IslandSelect(ctx);

        internal abstract void OnDrawGizmosSelected();

        internal static int LookAtChildren(AbstractIslandSelector abstractIslandSelector, ILookingObject lookingObject)
        {
            var hash = 0;
            var chiles = TexTransGroup.GetChildeComponent<AbstractIslandSelector>(abstractIslandSelector.transform);
            foreach (var chile in chiles) { chile.LookAtCalling(lookingObject); }
            lookingObject.LookAtChildeComponents<AbstractIslandSelector>(abstractIslandSelector.gameObject);
            return hash;
        }

        public bool Equals(AbstractIslandSelector other)
        {
            return this == other;
        }

        internal virtual bool IsExperimental => true;
    }
    internal interface IIslandSelector
    {
        internal abstract BitArray IslandSelect(IslandSelectorContext ctx);
    }
    internal class IslandSelectorContext
    {
        public Island[] Islands;
        public IslandDescription[] IslandDescription;
        public OriginEqual OriginEqual;

        public IslandSelectorContext(Island[] islands, IslandDescription[] islandDescription, OriginEqual originEqual)
        {
            Islands = islands;
            IslandDescription = islandDescription;
            OriginEqual = originEqual;
        }
    }
    internal readonly struct IslandDescription
    {
        public readonly NativeArray<Vector3> Position;//ワールドスペース
        public readonly NativeArray<Vector2> UV;
        public readonly Renderer Renderer;
        public readonly int MaterialSlot;// SubMeshIndex でもある

        public IslandDescription(NativeArray<Vector3> position, NativeArray<Vector2> uV, Renderer renderer, int materialSlot)
        {
            Position = position;
            UV = uV;
            Renderer = renderer;
            MaterialSlot = materialSlot;
        }

    }
}
