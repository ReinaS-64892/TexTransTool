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
    public class PinIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT " + nameof(PinIslandSelector);
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        public float IslandSelectorRange = 0.1f;
        internal override void LookAtCalling(ILookingObject looker)
        {
            looker.LookAt(transform.GetParents().Append(transform));
            looker.LookAt(this);
        }
        internal override BitArray IslandSelect(IslandSelectorContext ctx) { return RayBaseIslandSelect.PinIslandSelect(GetIslandSelectorPin(), ctx.Islands, ctx.IslandDescription); }
        internal IslandSelectorPin GetIslandSelectorPin() { return new IslandSelectorPin(new Ray(transform.position, transform.forward).ToTTCore(), transform.lossyScale.z * IslandSelectorRange); }


        internal override void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, IslandSelectorRange));
        }
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
