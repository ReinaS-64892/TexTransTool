#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.UVIsland;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class SubMeshIslandSelectorLink : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT " + nameof(SubMeshIslandSelectorLink);
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        internal override void LookAtCalling(ILookingObject looker) { GetIslandSelector()?.LookAtCalling(looker); }
        internal AbstractIslandSelector? GetIslandSelector() { return IslandSelectorNOT.GetFirstChilde(transform); }
        internal override BitArray IslandSelect(IslandSelectorContext ctx)
        {
            var islandSelector = GetIslandSelector();
            if (islandSelector == null) { return new BitArray(ctx.Islands.Length); }
            var linkFrom = islandSelector.IslandSelect(ctx);
            var linkSubMeshHash = new HashSet<(Renderer, int)>();

            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                if (linkFrom[i] is false) { continue; }
                linkSubMeshHash.Add((ctx.IslandDescription[i].Renderer, ctx.IslandDescription[i].MaterialSlot));
            }

            var linkTo = new BitArray(ctx.Islands.Length);
            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                linkTo[i] = linkSubMeshHash.Contains((ctx.IslandDescription[i].Renderer, ctx.IslandDescription[i].MaterialSlot));
            }

            return linkTo;
        }
        internal override void OnDrawGizmosSelected() { GetIslandSelector()?.OnDrawGizmosSelected(); }
    }
}
