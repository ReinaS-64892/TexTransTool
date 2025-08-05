#nullable enable
using UnityEngine;
using System.Collections;
using net.rs64.TexTransTool.Utils;
using System.Linq;
using net.rs64.TexTransTool.UVIsland;
using System;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCore.TTMathUtil;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class AimIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT " + nameof(AimIslandSelector);
        internal const string MenuPath = FoldoutName + "/" + ComponentName;

        internal override void LookAtCalling(IUnityObjectObserver looker)
        {
            looker.Observe(transform.GetParents().Append(transform));
            looker.Observe(this);
        }
        internal override BitArray IslandSelectImpl(IslandSelectorContext ctx) { return RayBaseIslandSelect.RayCastIslandSelect(GetIslandSelectorRay(), ctx.Islands, ctx.IslandDescription); }
        internal RayIntersect.Ray GetIslandSelectorRay() { return new Ray(transform.position, transform.forward).ToTTCore(); }

        static readonly Vector3[] lineList = new Vector3[]{

            new (0f,0f,0f),
            new (0f,0f,1f),


            new (-0.1f,0f,0.9f),
            new (0f,0f,1f),

            new (0f,0f,1f),
            new (0.1f,0f,0.9f),


            new (0f,-0.1f,0.9f),
            new (0f,0f,1f),

            new (0f,0f,1f),
            new (0f,0.1f,0.9f),
        };


        internal override void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position,transform.rotation,transform.lossyScale * 0.25f);
            Gizmos.DrawLineList(lineList.AsSpan());
        }
    }

}
