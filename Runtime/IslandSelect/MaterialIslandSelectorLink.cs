#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.UVIsland;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class MaterialIslandSelectorLink : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT " + nameof(MaterialIslandSelectorLink);
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        internal override void LookAtCalling(ILookingObject looker) { GetIslandSelector()?.LookAtCalling(looker); }
        internal AbstractIslandSelector? GetIslandSelector() { return IslandSelectorNOT.GetFirstChilde(transform); }
        internal override BitArray IslandSelect(IslandSelectorContext ctx)
        {
            var islandSelector = GetIslandSelector();
            if (islandSelector == null) { return new BitArray(ctx.Islands.Length); }
            var linkFrom = islandSelector.IslandSelect(ctx);
            var linkMaterialHash = new HashSet<Material>();
            var sharedMaterialsCache = new Dictionary<Renderer, Material[]>();

            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                if (linkFrom[i] is false) { continue; }
                if (sharedMaterialsCache.TryGetValue(ctx.IslandDescription[i].Renderer, out var rMats) is false)
                { rMats = sharedMaterialsCache[ctx.IslandDescription[i].Renderer] = ctx.IslandDescription[i].Renderer.sharedMaterials; }
                linkMaterialHash.Add(rMats[ctx.IslandDescription[i].MaterialSlot]);
            }

            var linkTo = new BitArray(ctx.Islands.Length);
            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                if (sharedMaterialsCache.TryGetValue(ctx.IslandDescription[i].Renderer, out var rMats) is false)
                { rMats = sharedMaterialsCache[ctx.IslandDescription[i].Renderer] = ctx.IslandDescription[i].Renderer.sharedMaterials; }
                linkTo[i] = linkMaterialHash.Contains(rMats[ctx.IslandDescription[i].MaterialSlot]);
            }

            return linkTo;
        }
        internal override void OnDrawGizmosSelected() { GetIslandSelector()?.OnDrawGizmosSelected(); }
    }
}
