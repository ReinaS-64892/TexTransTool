using System;
using UnityEngine;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static partial class AdditionalLayerInformationParser
    {
        [Serializable, AdditionalLayerInfoParser("SoCo")]
        internal class SoCo : AdditionalLayerInfo
        {
            public Color Color;
            public override void ParseAddLY(SubSpanStream stream)
            {
                var version = stream.ReadUInt32();

                Debug.Assert(version == 16);

                var descriptor = DescriptorStructureParser.ParseDescriptorStructure(ref stream);


                try
                {
                    var colorStructure = (DescriptorStructureParser.DescriptorStructure)descriptor.Structures["Clr "];

                    Color.r = (float)(double)colorStructure.Structures["Rd  "];
                    Color.g = (float)(double)colorStructure.Structures["Grn "];
                    Color.b = (float)(double)colorStructure.Structures["Bl  "];
                    Color.a = 1;//PSDのソリッドカラーにはアルファの値は含まれていないが、TTTのソリッドカラーは持っているため、適当な値を入れておく。
                }
                catch
                {
                    Debug.LogError("Unknown SoCo data format.");
                }

            }
        }

        /*
        クリスタはこのソリッドカラーの追加情報を出力できないらしい
        */

    }
}