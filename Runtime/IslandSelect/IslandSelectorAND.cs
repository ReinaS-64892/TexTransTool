using System.Collections;
using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class IslandSelectorAND : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT IslandSelectorAND";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        internal override IEnumerable<UnityEngine.Object> GetDependency() { return TexTransGroup.GetChildeComponent<AbstractIslandSelector>(transform); }
        internal override BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription)
        {
            BitArray bitArray = null;
            foreach (var islandSelector in TexTransGroup.GetChildeComponent<AbstractIslandSelector>(transform))
            {
                Profiler.BeginSample(islandSelector.GetType().Name);
                var selectBit = islandSelector.IslandSelect(islands, islandDescription);
                Profiler.EndSample();
                if (bitArray is null) { bitArray = selectBit; continue; }
                bitArray.And(selectBit);
            }
            return bitArray;
        }
    }
}
