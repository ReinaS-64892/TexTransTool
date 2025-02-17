using System;
using System.Diagnostics;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser.AdditionalLayerInfo
{
    public abstract class hue : AdditionalLayerInfoBase
    {
        public bool Colorization;
        public short HueColorization;
        public short SaturationColorization;
        public short LightnessColorization;
        public short Hue;
        public short Saturation;
        public short Lightness;
        public abstract bool IsOld { get; }
        public override void ParseAddLY(bool isPSB, BinarySectionStream stream)
        {
            var version = stream.ReadUInt16();

            Debug.Assert(version == 2);

            Colorization = stream.ReadByte() != 0;

            _ = stream.ReadByte();//Padding

            HueColorization = stream.ReadInt16();
            SaturationColorization = stream.ReadInt16();
            LightnessColorization = stream.ReadInt16();
            Hue = stream.ReadInt16();
            Saturation = stream.ReadInt16();
            Lightness = stream.ReadInt16();

            //レンジとかなんたらは、現状対応する実装がないので読み取らない。
        }
    }
    /*
    色相、彩度の調整レイヤーのセーブデータ
    2がついていないほうは古い方で Hueが -100 ~ 100 になっている (新しいほうは -180 ~ 180)
    */
    [Serializable, AdditionalLayerInfoParser("hue2")]
    public class hue2 : hue
    {
        public override bool IsOld => false;
    }
    [Serializable, AdditionalLayerInfoParser("hue ")]
    public class hueOld : hue
    {
        public override bool IsOld => true;
    }

}
