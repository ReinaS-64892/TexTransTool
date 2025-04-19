#nullable enable
using System.Collections;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class IslandSelectorOR : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT IslandSelectorOR";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        internal override void LookAtCalling(ILookingObject looker) { LookAtChildren(this, looker); }
        internal override BitArray IslandSelect(IslandSelectorContext ctx)
        {
            BitArray? bitArray = null;
            foreach (var islandSelector in transform.GetChildeComponent<AbstractIslandSelector>())
            {
                Profiler.BeginSample(islandSelector.GetType().Name);
                var selectBit = (islandSelector as IIslandSelector).IslandSelect(ctx);
                Profiler.EndSample();
                if (bitArray is null) { bitArray = selectBit; continue; }
                bitArray.Or(selectBit);
            }
            bitArray ??= new(ctx.Islands.Length);
            return bitArray;
        }
        internal override void OnDrawGizmosSelected() { foreach (var islandSelector in transform.GetChildeComponent<AbstractIslandSelector>()) { islandSelector.OnDrawGizmosSelected(); } }
        internal override bool IsExperimental => false;
    }
}
