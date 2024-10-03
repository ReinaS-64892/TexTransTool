using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransUnityCore.Island;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class IslandSelectorXOR : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT IslandSelectorXOR";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        internal override void LookAtCalling(ILookingObject looker) { LookAtChildren(this, looker); }
        internal override BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription)
        {
            BitArray bitArray = null;
            foreach (var islandSelector in TexTransGroup.GetChildeComponent<AbstractIslandSelector>(transform))
            {
                Profiler.BeginSample(islandSelector.GetType().Name);
                var selectBit = islandSelector.IslandSelect(islands, islandDescription);
                Profiler.EndSample();
                if (bitArray is null) { bitArray = selectBit; continue; }
                bitArray.Xor(selectBit);
            }
            return bitArray;
        }
        internal override void OnDrawGizmosSelected() { foreach (var islandSelector in TexTransGroup.GetChildeComponent<AbstractIslandSelector>(transform)) { islandSelector.OnDrawGizmosSelected(); } }
    }
}
