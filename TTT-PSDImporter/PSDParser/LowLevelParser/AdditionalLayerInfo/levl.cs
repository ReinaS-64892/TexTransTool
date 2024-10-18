using System;
using System.Diagnostics;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{
    [Serializable, AdditionalLayerInfoParser("levl")]
    public class levl : AdditionalLayerInfoBase
    {
        public LevelData RGB;
        public LevelData Red;
        public LevelData Green;
        public LevelData Blue;
        public override void ParseAddLY(bool isPSB, BinarySectionStream stream)
        {
            Debug.Assert(stream.ReadInt16() == 2);

            RGB = ParseLevelData(stream);
            Red = ParseLevelData(stream);
            Green = ParseLevelData(stream);
            Blue = ParseLevelData(stream);

            static LevelData ParseLevelData(BinarySectionStream subSpanStream)
            {
                var data = new LevelData();

                data.InputFloor = subSpanStream.ReadInt16();
                data.InputCeiling = subSpanStream.ReadInt16();
                data.OutputFloor = subSpanStream.ReadInt16();
                data.OutputCeiling = subSpanStream.ReadInt16();
                data.Gamma = subSpanStream.ReadInt16();

                return data;
            }

        }

        [Serializable]
        public struct LevelData //大体0...255 の物だが、ガンマだけ違うから注意
        {
            public short InputFloor;
            public short InputCeiling;
            public short OutputFloor;
            public short OutputCeiling;
            public short Gamma;

        }
    }
    /*
    レベル補正レイヤーの追加情報、最初の四つ分のやつ以外の情報は謎。
    */

}
