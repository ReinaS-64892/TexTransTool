using System;

namespace net.rs64.MultiLayerImageParser.PSD
{
    internal static partial class AdditionalLayerInformationParser
    {
        [Serializable]
        internal class AdditionalLayerInfo
        {
            public uint Length;
            public virtual void ParseAddLY(SubSpanStream stream) { }
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