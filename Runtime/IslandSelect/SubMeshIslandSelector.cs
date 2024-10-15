using System.Collections;
using System.Collections.Generic;
using net.rs64.TexTransCoreForUnity.Island;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class SubMeshIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT SubMeshIslandSelector";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        public int SelectSubMeshIndex = 0;
        internal override void LookAtCalling(ILookingObject looker) { looker.LookAt(this); }
        internal override BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription)
        {
            var bitArray = new BitArray(islands.Length);

            for (var islandIndex = 0; islands.Length > islandIndex; islandIndex += 1)
            {
                bitArray[islandIndex] = islandDescription[islandIndex].MaterialSlot == SelectSubMeshIndex;
            }

            return bitArray;
        }

        internal override void OnDrawGizmosSelected() { }
    }
}
