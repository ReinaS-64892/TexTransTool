using System;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{

    [Serializable]
    internal class AdditionalLayerInfoBase
    {
        public uint Length;
        public virtual void ParseAddLY(SubSpanStream stream) { }
    }
    [Serializable]
    internal class FallBackAdditionalLayerInfoParser : AdditionalLayerInfoBase
    {
        public string KeyCode;
        public long ByteIndex;
        public override void ParseAddLY(SubSpanStream stream)
        {
            ByteIndex = stream.FirstToPosition;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal class AdditionalLayerInfoParserAttribute : Attribute
    {
        public string Code;
        public AdditionalLayerInfoParserAttribute(string codeStr)
        {
            Code = codeStr;
        }
    }

}