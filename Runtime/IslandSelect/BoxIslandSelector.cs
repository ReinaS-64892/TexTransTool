using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Island;
using UnityEngine;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class BoxIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT BoxIslandSelector";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
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
                        var vert = matrix.MultiplyPoint3x4(description.Position[tri[vi]]);

                        var isIn = true;
                        isIn &= Mathf.Abs(vert.x) < 0.5f;
                        isIn &= Mathf.Abs(vert.x) < 0.5f;
                        isIn &= Mathf.Abs(vert.x) < 0.5f;
                        if (isIn) { bitArray[islandIndex] = true; break; }
                    }
                    if (bitArray[islandIndex]) { break; }
                }
            }


            return bitArray;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
