using System;
using net.rs64.TexTransCore;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{

    [Serializable, AdditionalLayerInfoParser("SoCo")]
    internal class SoCo : AdditionalLayerInfoBase
    {
        public ColorWOAlpha Color;
        public override void ParseAddLY(SubSpanStream stream)
        {
            var version = stream.ReadUInt32();

            Debug.Assert(version == 16);

            var descriptor = DescriptorStructureParser.ParseDescriptorStructure(ref stream);


            try
            {
                var colorStructure = (DescriptorStructureParser.DescriptorStructure)descriptor.Structures["Clr "];

                Color.R = (float)((double)colorStructure.Structures["Rd  "] / (double)byte.MaxValue);
                Color.G = (float)((double)colorStructure.Structures["Grn "] / (double)byte.MaxValue);
                Color.B = (float)((double)colorStructure.Structures["Bl  "] / (double)byte.MaxValue);
            }
            catch
            {
                // どうすんのこれ...? いつかログ用の関数作らないとね...
                // Debug.LogError("Unknown SoCo data format.");
            }

        }


        /*
        クリスタはこのソリッドカラーの追加情報を出力できないらしい
        */

    }
}
