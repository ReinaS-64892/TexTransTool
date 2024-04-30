using System;
using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.IslandSelector;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.IslandFineTuner
{
    [Serializable]
    internal class SizePriority : IIslandFineTuner
    {
        [Range(0, 1)] public float PriorityValue = 1f;
        public AbstractIslandSelector IslandSelector;

        public IEnumerable<UnityEngine.Object> GetDependency() { return IslandSelector.GetDependency(); }

        public int GetDependencyHash() { return IslandSelector.GetDependencyHash(); }
        public void IslandFineTuning(float[] sizePriority, Island[] islands, IslandDescription[] islandDescriptions, IReplaceTracking replaceTracking)
        {
            var targetBit = IslandSelector.IslandSelect(islands, islandDescriptions);

            for (var i = 0; sizePriority.Length > i; i += 1)
            {
                if (!targetBit[i]) { continue; }
                sizePriority[i] = PriorityValue;
            }
        }

    }
}
