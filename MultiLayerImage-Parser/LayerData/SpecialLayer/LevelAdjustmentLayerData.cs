using System;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine;

namespace net.rs64.MultiLayerImage.LayerData
{
    internal class LevelAdjustmentLayerData : AbstractLayerData
    {
        public LevelData RGB;
        public LevelData Red;
        public LevelData Green;
        public LevelData Blue;

        [Serializable]
        public struct LevelData
        {
            public float InputFloor;
            public float InputCeiling;
            public float Gamma;
            public float OutputFloor;
            public float OutputCeiling;



        }
    }
}
