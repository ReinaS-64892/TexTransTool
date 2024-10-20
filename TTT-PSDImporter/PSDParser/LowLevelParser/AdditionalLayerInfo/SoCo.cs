using System;
using System.Diagnostics;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser.AdditionalLayerInfo
{

    [Serializable, AdditionalLayerInfoParser("SoCo")]
    public class SoCo : AdditionalLayerInfoBase
    {
        public double R;// 0 ~ 255
        public double G;// 0 ~ 255
        public double B;// 0 ~ 255
        public override void ParseAddLY(bool isPSB,BinarySectionStream stream)
        {
            var version = stream.ReadUInt32();

            Debug.Assert(version == 16);

            var descriptor = DescriptorStructureParser.ParseDescriptorStructure(ref stream);


            try
            {
                var colorStructure = (DescriptorStructureParser.DescriptorStructure)descriptor.Structures["Clr "];

                R = (double)colorStructure.Structures["Rd  "];
                G = (double)colorStructure.Structures["Grn "];
                B = (double)colorStructure.Structures["Bl  "];
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
