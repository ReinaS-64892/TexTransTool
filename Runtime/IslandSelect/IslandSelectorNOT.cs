using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Island;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class IslandSelectorNOT : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT IslandSelectorNOT";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        internal override IEnumerable<UnityEngine.Object> GetDependency()
        {
            var islandSelector = GetIslandSelector();
            if (islandSelector != null)
            {
                yield return islandSelector;
                foreach (var depend in islandSelector.GetDependency()) { yield return depend; }
            }
        }

        internal override int GetDependencyHash()
        {
            var islandSelector = GetIslandSelector();
            return (islandSelector?.GetInstanceID() ?? 0) ^ (islandSelector?.GetDependencyHash() ?? 0);
        }

        internal AbstractIslandSelector GetIslandSelector()
        {
            var childeCount = transform.childCount;
            if (childeCount <= 0) { return null; }
            return transform.GetChild(0).GetComponent<AbstractIslandSelector>();
        }

        internal override BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription)
        {
            var islandSelector = GetIslandSelector();
            return islandSelector != null ? islandSelector.IslandSelect(islands, islandDescription).Not() : new BitArray(islands.Length, true);
        }

        internal override void OnDrawGizmosSelected() { GetIslandSelector()?.OnDrawGizmosSelected(); }
    }
}
