
namespace net.rs64.TexTransTool.TextureAtlas.IslandRelocator
{
    public interface IAtlasIslandRelocator
    {
        bool RectTangleMove { get; }
        float Padding { set; }
        bool Relocation(IslandRect[] atlasIslands);
    }


}
