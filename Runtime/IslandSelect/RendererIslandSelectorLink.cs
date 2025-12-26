#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.UVIsland;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class RendererIslandSelectorLink : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT " + nameof(RendererIslandSelectorLink);
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        internal override void LookAtCalling(IUnityObjectObserver looker) { GetIslandSelector()?.LookAtCalling(looker); }
        internal AbstractIslandSelector? GetIslandSelector() { return IslandSelectorNOT.GetFirstChilde(transform); }
        internal override BitArray IslandSelectImpl(IslandSelectorContext ctx)
        {
            var islandSelector = GetIslandSelector();
            if (islandSelector == null) { return new BitArray(ctx.Islands.Length); }
            var linkFrom = islandSelector.IslandSelect(ctx);
            var linkRendererHash = new HashSet<Renderer>();

            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                if (linkFrom[i] is false) { continue; }
                linkRendererHash.Add(ctx.IslandDescription[i].Renderer);
            }

            var linkTo = new BitArray(ctx.Islands.Length);
            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                linkTo[i] = linkRendererHash.Contains(ctx.IslandDescription[i].Renderer);
            }

            return linkTo;
        }
        internal override void OnDrawGizmosSelected() { GetIslandSelector()?.OnDrawGizmosSelected(); }
    }
}
