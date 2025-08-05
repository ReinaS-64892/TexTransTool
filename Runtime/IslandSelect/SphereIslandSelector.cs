#nullable enable
using System.Collections;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.UVIsland;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class SphereIslandSelector : AbstractIslandSelector, ITexTransToolStableComponent
    {
        internal const string ComponentName = "TTT SphereIslandSelector";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        public int StabilizeSaveDataVersion => TTTDataVersion_0_10_X;

        public bool IsAll;
        internal override void LookAtCalling(IUnityObjectObserver looker)
        {
            looker.Observe(transform.GetParents().Append(transform));
            looker.Observe(this);
        }
        internal override BitArray IslandSelectImpl(IslandSelectorContext ctx)
        {
            var bitArray = new BitArray(ctx.Islands.Length);
            var matrix = transform.worldToLocalMatrix;

            for (var islandIndex = 0; ctx.Islands.Length > islandIndex; islandIndex += 1)
            {
                var description = ctx.IslandDescription[islandIndex];
                var island = ctx.Islands[islandIndex];

                bitArray[islandIndex] = IsAll ? All(island, description) : Some(island, description);
            }

            return bitArray;
            bool All(Island island, IslandDescription description)
            {
                foreach (var tri in island.Triangles)
                {
                    for (var vi = 0; 3 > vi; vi += 1)
                    {
                        if (matrix.MultiplyPoint3x4(description.Position[tri[vi]]).sqrMagnitude >= 1) { return false; }
                    }
                }
                return true;
            }
            bool Some(Island island, IslandDescription description)
            {
                foreach (var tri in island.Triangles)
                {
                    for (var vi = 0; 3 > vi; vi += 1)
                    {
                        if (matrix.MultiplyPoint3x4(description.Position[tri[vi]]).sqrMagnitude < 1) { return true; }
                    }
                }
                return false;
            }
        }

        internal override void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, 1);
        }
        internal override bool IsExperimental => false;
    }
}
