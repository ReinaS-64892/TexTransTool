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
        internal override void LookAtCalling(ILookingObject looker) { looker.LookAt(this); }
        public List<Material> Materials;
        internal override BitArray IslandSelect(IslandSelectorContext ctx)
        {
            var selectRendererHash = ctx.OriginEqual.GetDomainsMaterialsHashSet(ctx.IslandDescription.Select(i => i.Renderer).Distinct(), Materials);
            var bitArray = new BitArray(ctx.Islands.Length);
            var sharedMaterialsCache = new Dictionary<Renderer, Material[]>();

            for (int i = 0; i < ctx.Islands.Length; i += 1)
            {
                if (sharedMaterialsCache.TryGetValue(ctx.IslandDescription[i].Renderer, out var rMats) is false)
                { rMats = sharedMaterialsCache[ctx.IslandDescription[i].Renderer] = ctx.IslandDescription[i].Renderer.sharedMaterials; }
                bitArray[i] = selectRendererHash.Contains(rMats[ctx.IslandDescription[i].MaterialSlot]);
            }

            return bitArray;
        }
        internal override void OnDrawGizmosSelected() { }
    }
}
