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
        float Padding { set; }
        bool Relocation(IslandRect[] atlasIslands);
    }


}
