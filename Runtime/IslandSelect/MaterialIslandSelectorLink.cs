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
        internal override BitArray IslandSelectImpl(IslandSelectorContext ctx)
        {
            var islandSelector = GetIslandSelector();
            if (islandSelector == null) { return new BitArray(ctx.Islands.Length); }
            var linkFrom = islandSelector.IslandSelect(ctx);
            var linkMaterialHash = new HashSet<Material>();

            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                if (linkFrom[i] is false) { continue; }

                var mat = ctx.IslandDescription[i].Materials[ctx.IslandDescription[i].MaterialSlot];
                if (mat == null) { continue; }
                linkMaterialHash.Add(mat);
            }

            var linkTo = new BitArray(ctx.Islands.Length);
            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                var mat = ctx.IslandDescription[i].Materials[ctx.IslandDescription[i].MaterialSlot];
                linkTo[i] = mat != null ? linkMaterialHash.Contains(mat) : false;
            }

            return linkTo;
        }
        internal override void OnDrawGizmosSelected() { GetIslandSelector()?.OnDrawGizmosSelected(); }
    }
}
