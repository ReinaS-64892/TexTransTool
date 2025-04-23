#nullable enable
using System.Collections;
using net.rs64.TexTransTool.UVIsland;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class IslandSelectorNOT : AbstractIslandSelector, ITexTransToolStableComponent
    {
        internal const string ComponentName = "TTT IslandSelectorNOT";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        public int StabilizeSaveDataVersion => TTTDataVersion_0_10_X;

        internal override void LookAtCalling(ILookingObject looker) { GetIslandSelector()?.LookAtCalling(looker); }
        internal AbstractIslandSelector? GetIslandSelector() { return GetFirstChilde(transform); }
        internal static AbstractIslandSelector? GetFirstChilde(Transform tf)
        {
            var childeCount = tf.childCount;
            if (childeCount <= 0) { return null; }
            return tf.GetChild(0).GetComponent<AbstractIslandSelector>();
        }

        internal override BitArray IslandSelect(IslandSelectorContext ctx)
        {
            var islandSelector = GetIslandSelector();
            return islandSelector != null ? islandSelector.IslandSelect(ctx).Not() : new BitArray(ctx.Islands.Length, true);
        }

        internal override void OnDrawGizmosSelected() { GetIslandSelector()?.OnDrawGizmosSelected(); }
        internal override bool IsExperimental => false;
    }
}
