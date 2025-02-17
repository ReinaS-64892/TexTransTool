using net.rs64.TexTransTool.IslandSelector;
using net.rs64.TexTransTool.UVIsland;

namespace net.rs64.TexTransTool.TextureAtlas.IslandFineTuner
{
    internal interface IIslandFineTuner
    {
        //islands と islandDescriptions は書き込みしないように、
        //sizePriority と islandRect を操作して調整していく感じ
        void IslandFineTuning(float[] sizePriority, Island[] islands, IslandDescription[] islandDescriptions, IReplaceTracking replaceTracking);

        void LookAtCalling(ILookingObject looker);
    }
}
