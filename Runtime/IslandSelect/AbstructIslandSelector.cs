#nullable enable
using UnityEngine;
using Unity.Collections;
using System.Collections;
using net.rs64.TexTransTool.UVIsland;
using System;
using net.rs64.TexTransCore.UVIsland;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.IslandSelector
{
    [DisallowMultipleComponent]
    public abstract class AbstractIslandSelector : TexTransMonoBaseGameObjectOwned, IIslandSelector, IEquatable<AbstractIslandSelector>
    {
        internal const string FoldoutName = "IslandSelector";

        internal abstract void LookAtCalling(IUnityObjectObserver looker);


        BitArray IIslandSelector.IslandSelect(IslandSelectorContext ctx) { return IslandSelect(ctx); }
        internal BitArray IslandSelect(IslandSelectorContext ctx)
        {
            if (ctx.Targeting.IsActive(gameObject) is false) { return new BitArray(ctx.Islands.Length); }
            return IslandSelectImpl(ctx);
        }
        internal abstract BitArray IslandSelectImpl(IslandSelectorContext ctx);

        internal abstract void OnDrawGizmosSelected();

        internal static int LookAtChildren(AbstractIslandSelector abstractIslandSelector, IUnityObjectObserver lookingObject)
        {
            var hash = 0;
            var chiles = abstractIslandSelector.transform.GetChildeComponent<AbstractIslandSelector>();
            foreach (var chile in chiles) { chile.LookAtCalling(lookingObject); }
            lookingObject.ObserveToChildeComponents<AbstractIslandSelector>(abstractIslandSelector.gameObject);
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
        public IDomainReferenceViewer Targeting;

        public IslandSelectorContext(Island[] islands, IslandDescription[] islandDescription, IDomainReferenceViewer targeting)
        {
            Islands = islands;
            IslandDescription = islandDescription;
            Targeting = targeting;
        }
    }
    internal readonly struct IslandDescription
    {
        public readonly NativeArray<Vector3> Position;//ワールドスペース
        public readonly NativeArray<Vector2> UV;
        public readonly Renderer Renderer;
        public readonly Material?[] Materials;
        public readonly int MaterialSlot;// SubMeshIndex でもある

        public IslandDescription(NativeArray<Vector3> position, NativeArray<Vector2> uV, Renderer renderer, Material?[] materials, int materialSlot)
        {
            Position = position;
            UV = uV;
            Renderer = renderer;
            Materials = materials;
            MaterialSlot = materialSlot;
        }

    }
}
