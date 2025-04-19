#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.UVIsland;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class RendererIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT RendererIslandSelector";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        internal override void LookAtCalling(ILookingObject looker) { looker.LookAt(this); }
        public List<Renderer> RendererList = new();
        internal override BitArray IslandSelect(IslandSelectorContext ctx)
        {
            OriginEqual oe = ctx.Targeting.OriginEqual;
            var selectRendererHash = oe.GetDomainsRenderers(ctx.IslandDescription.Select(i => i.Renderer).Distinct(), RendererList).ToHashSet();
            var bitArray = new BitArray(ctx.Islands.Length);

            for (int i = 0; i < ctx.Islands.Length; i += 1) { bitArray[i] = selectRendererHash.Contains(ctx.IslandDescription[i].Renderer); }

            return bitArray;
        }
        internal override void OnDrawGizmosSelected() { }
    }
}
