using System;
using net.rs64.TexTransCore.UVIsland;
using net.rs64.TexTransTool.IslandSelector;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.IslandSizePriorityTuner
{
    [Serializable]
    public class SetFromIslandSelector : IIslandSizePriorityTuner
    {
        [Range(0, 1)] public float PriorityValue = 1f;
        public AbstractIslandSelector IslandSelector;

        void LookAtCalling(ILookingObject lookingObject) { if (IslandSelector != null) { IslandSelector.LookAtCalling(lookingObject); } }
        void Tuning(float[] sizePriority, Island[] islands, IslandDescription[] islandDescriptions, IRendererTargeting targeting)
        {
            if (IslandSelector == null) { return; }

            var targetBit = IslandSelector.IslandSelect(new(islands, islandDescriptions, targeting));

            for (var i = 0; sizePriority.Length > i; i += 1)
            {
                if (!targetBit[i]) { continue; }
                sizePriority[i] = PriorityValue;
            }
        }

        void IIslandSizePriorityTuner.Tuning(float[] sizePriority, Island[] islands, IslandDescription[] islandDescriptions, IRendererTargeting targeting)
        {
            Tuning(sizePriority, islands, islandDescriptions, targeting);
        }

        void IIslandSizePriorityTuner.LookAtCalling(ILookingObject looker)
        {
            LookAtCalling(looker);
        }
    }
}
