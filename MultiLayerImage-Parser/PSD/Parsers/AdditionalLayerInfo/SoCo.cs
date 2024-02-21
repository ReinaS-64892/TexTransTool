using System;
using UnityEngine;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{

    [Serializable, AdditionalLayerInfoParser("SoCo")]
    internal class SoCo : AdditionalLayerInfoBase
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

                Color.r = (float)((double)colorStructure.Structures["Rd  "] / (double)byte.MaxValue);
                Color.g = (float)((double)colorStructure.Structures["Grn "] / (double)byte.MaxValue);
                Color.b = (float)((double)colorStructure.Structures["Bl  "] / (double)byte.MaxValue);
                Color.a = 1;//PSDのソリッドカラーにはアルファの値は含まれていないが、TTTのソリッドカラーは持っているため、適当な値を入れておく。
            }
            catch
            {
                Debug.LogError("Unknown SoCo data format.");
            }

        }


        /*
        クリスタはこのソリッドカラーの追加情報を出力できないらしい
        */

    }
}