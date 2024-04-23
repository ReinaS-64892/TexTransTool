using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Island;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.IslandSelector
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class RendererIslandSelector : AbstractIslandSelector
    {
        internal const string ComponentName = "TTT RendererIslandSelector";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
                internal override IEnumerable<UnityEngine.Object> GetDependency() { yield break; }
        public List<Renderer> RendererList;
        internal override BitArray IslandSelect(Island[] islands, IslandDescription[] islandDescription)
        {
            var bitArray = new BitArray(islands.Length);
            var hash = RendererList.ToHashSet();

            for (int i = 0; i < islands.Length; i += 1) { bitArray[i] = hash.Contains(islandDescription[i].Renderer); }

            return bitArray;
        }
    }
}
