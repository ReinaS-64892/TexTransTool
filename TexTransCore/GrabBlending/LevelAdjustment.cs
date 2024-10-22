#nullable enable
using System;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class LevelAdjustment : ITTGrabBlending
    {
        public LevelData RGB;
        public LevelData R;
        public LevelData G;
        public LevelData B;
        public LevelAdjustment(LevelData rgb, LevelData r, LevelData g, LevelData b) 
        {
            RGB = rgb;
            R = r;
            G = g;
            B = b;
        }

    }
    [Serializable]
    public class LevelData
    {
        [Range(0, 0.99f)] public float InputFloor = 0;
        [Range(0.01f, 1)] public float InputCeiling = 1;

        [Range(0.1f, 9.9f)] public float Gamma = 1;

        [Range(0, 1)] public float OutputFloor = 0;
        [Range(0, 1)] public float OutputCeiling = 1;
    }
}
