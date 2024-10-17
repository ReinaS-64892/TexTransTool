using System;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{

    [Serializable]
    internal class AdditionalLayerInfoBase
    {
        public BinaryAddress Address;
        public virtual void ParseAddLY(bool isPSB, BinarySectionStream stream) { }
    }
    [Serializable]
    internal class FallBackAdditionalLayerInfoParser : AdditionalLayerInfoBase
    {
        public string KeyCode;
        public override void ParseAddLY(bool isPSB, BinarySectionStream stream) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal class AdditionalLayerInfoParserAttribute : Attribute
    {
        public string Code;
        public bool MayULongLength;
        public AdditionalLayerInfoParserAttribute(string codeStr, bool mayULongLength = false)
        {
            Code = codeStr;
            MayULongLength = mayULongLength;
        }
    }

}
