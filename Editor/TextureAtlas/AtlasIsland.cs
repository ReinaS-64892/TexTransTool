#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;
using static net.rs64.TexTransCore.TransTextureCore.TransTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.EditorIsland;
using net.rs64.TexTransTool.TextureAtlas.FineSetting;

namespace net.rs64.TexTransTool.TextureAtlas
{

    internal class AtlasIsland : Island
    {
        public List<Vector2> UV;

        public AtlasIsland(AtlasIsland Souse) : base(Souse)
        {
            UV = Souse.UV;
        }

        public AtlasIsland(Island Souse, List<Vector2> uv) : base(Souse)
        {
            UV = uv;
        }
    }
}
#endif