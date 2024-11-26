using System.Collections;
using net.rs64.TexTransTool.UVIsland;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class IslandSelectorNOT : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT IslandSelectorNOT";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        internal override void LookAtCalling(ILookingObject looker) { GetIslandSelector().LookAtCalling(looker); }
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
