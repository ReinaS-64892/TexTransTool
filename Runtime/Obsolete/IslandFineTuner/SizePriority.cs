using System;
using net.rs64.TexTransCore.UVIsland;
using net.rs64.TexTransTool.IslandSelector;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.IslandFineTuner
{
    [Serializable]
    [Obsolete("V6SaveData", true)]
    internal class SizePriority : IIslandFineTuner
    {
        [Range(0, 1)] public float PriorityValue = 1f;
        public AbstractIslandSelector IslandSelector;

        public void LookAtCalling(ILookingObject lookingObject) { if (IslandSelector != null) { IslandSelector.LookAtCalling(lookingObject); } }
        public void IslandFineTuning(float[] sizePriority, Island[] islands, IslandDescription[] islandDescriptions, IReplaceTracking replaceTracking)
        {
            if (IslandSelector == null) { return; }

            var targetBit = IslandSelector.IslandSelect(new(islands, islandDescriptions, replaceTracking.OriginEqual));

            for (var i = 0; sizePriority.Length > i; i += 1)
            {
                if (!targetBit[i]) { continue; }
                sizePriority[i] = PriorityValue;
            }
        }

    }
}
