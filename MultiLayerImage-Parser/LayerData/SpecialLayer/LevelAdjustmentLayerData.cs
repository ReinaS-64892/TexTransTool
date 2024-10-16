using System;

namespace net.rs64.MultiLayerImage.LayerData
{
    internal class LevelAdjustmentLayerData : AbstractLayerData , IGrabTag
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
