#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.UVIsland;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class MaterialIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT " + nameof(MaterialIslandSelector);
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        public List<Material> Materials = new();
        internal override void LookAtCalling(IUnityObjectObserver looker) { looker.Observe(this); }
        internal override BitArray IslandSelectImpl(IslandSelectorContext ctx)
        {
            UnityObjectEqualityComparison oe = ctx.Targeting.OriginalObjectEquals;
            var selectMaterialsHash = oe.GetDomainsMaterialsHashSet(ctx.IslandDescription.SelectMany(i => i.Materials).Distinct().SkipDestroyed(), Materials);
            var bitArray = new BitArray(ctx.Islands.Length);

            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                var mat = ctx.IslandDescription[i].Materials[ctx.IslandDescription[i].MaterialSlot];
                if (mat == null) { continue; }
                bitArray[i] = selectMaterialsHash.Contains(mat);
            }

            return bitArray;
        }
        internal override void OnDrawGizmosSelected() { }
    }
}
