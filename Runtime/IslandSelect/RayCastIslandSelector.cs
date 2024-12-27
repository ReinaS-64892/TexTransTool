using UnityEngine;
using Unity.Collections;
using System.Collections;
using Unity.Jobs;
using net.rs64.TexTransTool.Utils;
using System.Linq;
using net.rs64.TexTransTool.UVIsland;
using net.rs64.TexTransCore;
using System;
using net.rs64.TexTransTool.TTMathUtil;
using net.rs64.TexTransCoreEngineForUnity;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class RayCastIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT " + nameof(RayCastIslandSelector);
        internal const string MenuPath = FoldoutName + "/" + ComponentName;


        internal override void LookAtCalling(ILookingObject looker)
        {
            looker.LookAt(transform.GetParents().Append(transform));
            looker.LookAt(this);
        }
        internal override BitArray IslandSelect(IslandSelectorContext ctx) { return RayBaseIslandSelect.RayCastIslandSelect(GetIslandSelectorRay(), ctx.Islands, ctx.IslandDescription); }
        internal RayIntersect.Ray GetIslandSelectorRay() { return new Ray(transform.position, transform.forward).ToTTCore(); }


        internal override void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawRay(Vector3.zero, new Vector3(0, 0, 1f));
        }
    }

}
