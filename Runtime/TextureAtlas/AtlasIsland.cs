using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransTool.TextureAtlas
{

    public class AtlasIsland : Island
    {
        public List<Vector2> UV;

        public AtlasIsland(AtlasIsland souse) : base(souse)
        {
            UV = souse.UV;
        }

        public AtlasIsland(Island souse, List<Vector2> uv) : base(souse)
        {
            UV = uv;
        }
    }
}