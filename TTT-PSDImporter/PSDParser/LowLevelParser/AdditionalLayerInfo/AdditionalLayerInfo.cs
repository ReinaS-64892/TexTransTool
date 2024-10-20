using System;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser.AdditionalLayerInfo
{

    [Serializable]
    public class AdditionalLayerInfoBase
    {
        public BinaryAddress Address;
        public virtual void ParseAddLY(bool isPSB, BinarySectionStream stream) { }
    }
    [Serializable]
    public class FallBackAdditionalLayerInfoParser : AdditionalLayerInfoBase
    {
        public string KeyCode;
        public override void ParseAddLY(bool isPSB, BinarySectionStream stream) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AdditionalLayerInfoParserAttribute : Attribute
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
