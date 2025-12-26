#nullable enable
using UnityEngine;
using Unity.Collections;
using System.Collections;
using Unity.Jobs;
using net.rs64.TexTransTool.Utils;
using System.Linq;
using net.rs64.TexTransTool.UVIsland;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class PinIslandSelector : AbstractIslandSelector , ITexTransToolStableComponent
    {
        internal const string ComponentName = "TTT " + nameof(PinIslandSelector);
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
                public int StabilizeSaveDataVersion => TTTDataVersion_0_10_X;
                
        public float IslandSelectorRange = 0.1f;
        internal override void LookAtCalling(IUnityObjectObserver looker)
        {
            looker.Observe(transform.GetParents().Append(transform));
            looker.Observe(this);
        }
        internal override BitArray IslandSelectImpl(IslandSelectorContext ctx) { return RayBaseIslandSelect.PinIslandSelect(GetIslandSelectorPin(), ctx.Islands, ctx.IslandDescription); }
        internal IslandSelectorPin GetIslandSelectorPin() { return new IslandSelectorPin(new Ray(transform.position, transform.forward).ToTTCore(), transform.lossyScale.z * IslandSelectorRange); }


        internal override void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, IslandSelectorRange));
        }
        internal override bool IsExperimental => false;
    }

    internal class PinIslandSelectorClass : IIslandSelector
    {
        public IslandSelectorPin IslandSelectorRay;

        public PinIslandSelectorClass(IslandSelectorPin islandSelectorRay)
        {
            IslandSelectorRay = islandSelectorRay;
        }

        BitArray IIslandSelector.IslandSelect(IslandSelectorContext ctx)
        {
            return RayBaseIslandSelect.PinIslandSelect(IslandSelectorRay, ctx.Islands, ctx.IslandDescription);
        }
    }
}
