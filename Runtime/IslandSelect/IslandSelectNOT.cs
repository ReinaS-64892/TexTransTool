using System.Collections;
using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class IslandSelectNOT : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT IslandSelectNOT";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        public AbstractIslandSelector IslandSelector;
        internal override BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription)
        {
            return IslandSelector != null ? IslandSelector.IslandSelect(islands, islandDescription).Not() : new BitArray(islands.Length, true);
        }
    }
}
