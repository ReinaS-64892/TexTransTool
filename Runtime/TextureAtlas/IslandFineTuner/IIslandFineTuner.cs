using System;
using System.Collections.Generic;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.IslandSelector;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.IslandFineTuner
{
    internal interface IIslandFineTuner
    {
        //islands と islandDescriptions は書き込みしないように、
        //sizePriority と islandRect を操作して調整していく感じ
        void IslandFineTuning(float[] sizePriority, Island[] islands, IslandDescription[] islandDescriptions, IReplaceTracking replaceTracking);

        IEnumerable<UnityEngine.Object> GetDependency();
    }
}
