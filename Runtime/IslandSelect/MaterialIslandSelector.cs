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
        internal override void LookAtCalling(ILookingObject looker) { looker.LookAt(this); }
        internal override BitArray IslandSelect(IslandSelectorContext ctx)
        {
            var selectMaterialsHash = ctx.OriginEqual.GetDomainsMaterialsHashSet(ctx.IslandDescription.SelectMany(i => i.Materials).Distinct().Where(m => m != null).Cast<Material>(), Materials);
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
