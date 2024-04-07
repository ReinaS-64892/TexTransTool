using System;
using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.IslandSelector;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.IslandFineTuner
{
    [Serializable]
    internal class SizeOffset : IIslandFineTuner
    {
        [Range(0, 1)] public float OffsetValue = 1f;
        public AbstractIslandSelector IslandSelector;


        public void IslandFineTuning(float[] sizePriority, float[] setSize, Island[] islands, IslandDescription[] islandDescriptions, IReplaceTracking replaceTracking)
        {
            var targetBit = IslandSelector.IslandSelect(islands, islandDescriptions);

            for (var i = 0; setSize.Length > i; i += 1)
            {
                if (!targetBit[i]) { continue; }
                setSize[i] = OffsetValue;
            }
        }
    }
}
