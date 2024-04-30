using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Island;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class IslandSelectorRelay : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT IslandSelectorRelay";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        public AbstractIslandSelector IslandSelector = null;

        internal override IEnumerable<UnityEngine.Object> GetDependency() { return IslandSelector.GetDependency().Append(IslandSelector); }
        internal override int GetDependencyHash() { return (IslandSelector?.GetInstanceID() ?? 0) ^ (IslandSelector?.GetDependencyHash() ?? 0); }

        internal override BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription)
        {
            return IslandSelector != null ? IslandSelector.IslandSelect(islands, islandDescription) : new BitArray(islands.Length);
        }
        internal override void OnDrawGizmosSelected() { IslandSelector?.OnDrawGizmosSelected(); }
    }
}
