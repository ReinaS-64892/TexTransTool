using UnityEngine;
using System;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [Serializable]
    [Obsolete("V0SaveData", true)]
    public class MatSelectorV0
    {
        public Material Material;
        public bool IsTarget = false;
        public int AtlasChannel = 0;
        public float TextureSizeOffSet = 1;
    }
}
