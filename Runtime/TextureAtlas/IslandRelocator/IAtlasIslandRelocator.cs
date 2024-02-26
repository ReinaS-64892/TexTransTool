using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransTool.TextureAtlas.IslandRelocator
{
    public interface IAtlasIslandRelocator
    {
        bool RectTangleMove { get; }

        bool UseUpScaling { set; }
        float Padding { set; }

        Dictionary<AtlasIslandID, IslandRect> Relocation(Dictionary<AtlasIslandID, IslandRect> atlasIslands, IReadOnlyDictionary<AtlasIslandID, AtlasIsland> atlasIslandReference);
    }


}
