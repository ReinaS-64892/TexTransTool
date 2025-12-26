using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.UVIsland;
using net.rs64.TexTransTool.IslandSelector;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.IslandSizePriorityTuner
{
    // これでできることは IslandSelector の方でもできる。
    // マイグレーションの都合と シンタックスシュガー のようなもの
    [Serializable]
    public class SetFromMaterial : IIslandSizePriorityTuner
    {
        [Range(0, 1)] public float PriorityValue = 1f;
        public List<Material> Materials;

        void LookAtCalling(IUnityObjectObserver lookingObject) { }
        void Tuning(float[] sizePriority, Island[] islands, IslandDescription[] islandDescriptions, IDomainReferenceViewer targeting)
        {
            if (Materials == null) { return; }
            UnityObjectEqualityComparison originEqual = targeting.OriginalObjectEquals;
            var selectMaterialsHash = originEqual.GetDomainsMaterialsHashSet(islandDescriptions.SelectMany(i => i.Materials).Distinct().SkipDestroyed(), Materials);
            var setValue = TTMath.Saturate(PriorityValue);

            for (int i = 0; i < sizePriority.Length; i += 1)
            {
                var mat = islandDescriptions[i].Materials[islandDescriptions[i].MaterialSlot];
                if (mat == null) { continue; }
                if (selectMaterialsHash.Contains(mat))
                {
                    sizePriority[i] = setValue;
                }
            }

        }

        void IIslandSizePriorityTuner.Tuning(float[] sizePriority, Island[] islands, IslandDescription[] islandDescriptions, IDomainReferenceViewer targeting)
        {
            Tuning(sizePriority, islands, islandDescriptions, targeting);
        }

        void IIslandSizePriorityTuner.LookAtCalling(IUnityObjectObserver looker)
        {
            LookAtCalling(looker);
        }
    }
}
