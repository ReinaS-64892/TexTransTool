using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class SphereIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT SphereIslandSelector";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        internal override IEnumerable<UnityEngine.Object> GetDependency() { return transform.GetParents().Append(transform); }
        internal override int GetDependencyHash() { return 0; }
        internal override BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription)
        {
            var bitArray = new BitArray(islands.Length);
            var matrix = transform.worldToLocalMatrix;

            for (var islandIndex = 0; islands.Length > islandIndex; islandIndex += 1)
            {
                var description = islandDescription[islandIndex];

                foreach (var tri in islands[islandIndex].triangles)
                {
                    for (var vi = 0; 3 > vi; vi += 1)
                    {
                        if (matrix.MultiplyPoint3x4(description.Position[tri[vi]]).sqrMagnitude < 1) { bitArray[islandIndex] = true; break; }
                    }
                    if (bitArray[islandIndex]) { break; }
                }
            }

            return bitArray;
        }

        internal override void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, 1);
        }
    }
}
