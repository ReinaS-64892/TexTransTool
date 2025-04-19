using net.rs64.TexTransCore.UVIsland;
using net.rs64.TexTransTool.IslandSelector;

namespace net.rs64.TexTransTool.TextureAtlas.IslandSizePriorityTuner
{
    public interface IIslandSizePriorityTuner
    {
        //islands と islandDescriptions は書き込みしないように、
        //sizePriority を調整してね！
        internal void Tuning(float[] sizePriority, Island[] islands, IslandDescription[] islandDescriptions, IRendererTargeting targeting);

        internal void LookAtCalling(ILookingObject looker);
    }
}
