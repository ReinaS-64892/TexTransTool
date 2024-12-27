using System.Collections;
using net.rs64.TexTransTool.UVIsland;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class SubMeshIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT SubMeshIslandSelector";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        public int SelectSubMeshIndex = 0;
        internal override void LookAtCalling(ILookingObject looker) { looker.LookAt(this); }
        internal override BitArray IslandSelect(IslandSelectorContext ctx)
        {
            var bitArray = new BitArray(ctx.Islands.Length);

            for (var islandIndex = 0; ctx.Islands.Length > islandIndex; islandIndex += 1)
            {
                bitArray[islandIndex] = ctx.IslandDescription[islandIndex].MaterialSlot == SelectSubMeshIndex;
            }

            return bitArray;
        }

        internal override void OnDrawGizmosSelected() { }
    }
}
