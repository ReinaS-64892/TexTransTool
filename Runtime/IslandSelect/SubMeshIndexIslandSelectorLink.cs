#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.UVIsland;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class SubMeshIndexIslandSelectorLink : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT " + nameof(SubMeshIndexIslandSelectorLink);
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        internal override void LookAtCalling(ILookingObject looker) { GetIslandSelector()?.LookAtCalling(looker); }
        internal AbstractIslandSelector? GetIslandSelector() { return IslandSelectorNOT.GetFirstChilde(transform); }
        internal override BitArray IslandSelect(IslandSelectorContext ctx)
        {
            var islandSelector = GetIslandSelector();
            if (islandSelector == null) { return new BitArray(ctx.Islands.Length); }
            var linkFrom = islandSelector.IslandSelect(ctx);
            var linkIndexHash = new HashSet<int>();

            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                if (linkFrom[i] is false) { continue; }
                linkIndexHash.Add(ctx.IslandDescription[i].MaterialSlot);
            }

            var linkTo = new BitArray(ctx.Islands.Length);
            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                linkTo[i] = linkIndexHash.Contains(ctx.IslandDescription[i].MaterialSlot);
            }

            return linkTo;
        }
        internal override void OnDrawGizmosSelected() { GetIslandSelector()?.OnDrawGizmosSelected(); }
    }
}
