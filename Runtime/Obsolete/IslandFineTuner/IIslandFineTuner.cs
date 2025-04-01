using System;
using net.rs64.TexTransCore.UVIsland;
using net.rs64.TexTransTool.IslandSelector;

namespace net.rs64.TexTransTool.TextureAtlas.IslandFineTuner
{
    [Obsolete("V6SaveData", true)]
    internal interface IIslandFineTuner
    {
        //islands と islandDescriptions は書き込みしないように、
        //sizePriority と islandRect を操作して調整していく感じ
        void IslandFineTuning(float[] sizePriority, Island[] islands, IslandDescription[] islandDescriptions, IReplaceTracking replaceTracking);

        void LookAtCalling(ILookingObject looker);
    }
}
