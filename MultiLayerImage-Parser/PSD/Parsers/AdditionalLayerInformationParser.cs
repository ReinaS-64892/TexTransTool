using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace net.rs64.MultiLayerImageParser.PSD
{
    internal static class AdditionalLayerInformationParser
    {
        public static Dictionary<string, Type> AdditionalLayerInfoParsersTypes;
        public static Dictionary<string, Type> GetAdditionalLayerInfoParsersTypes()
        {
            var dict = new Dictionary<string, Type>();

            foreach (var addLYType in AppDomain.CurrentDomain.GetAssemblies()
                 .SelectMany(I => I.GetTypes())
                 .Where(I => I.GetCustomAttribute<AdditionalLayerInfoParserAttribute>() != null))
            {
                var Instants = Activator.CreateInstance(addLYType) as AdditionalLayerInfo;

                var customAttribute = addLYType.GetCustomAttribute<AdditionalLayerInfoParserAttribute>();
                if (!dict.ContainsKey(customAttribute.Code))
                {
                    dict.Add(customAttribute.Code, addLYType);
                }
            }

            return dict;
        }
        public static AdditionalLayerInfo[] PaseAdditionalLayerInfos(SubSpanStream stream)
        {
            var addLayerInfoList = new List<AdditionalLayerInfo>();
            if (AdditionalLayerInfoParsersTypes == null) AdditionalLayerInfoParsersTypes = GetAdditionalLayerInfoParsersTypes();
            var addLayerInfoParsers = AdditionalLayerInfoParsersTypes;
            while (stream.Position < stream.Length)
            {
                if (!ParserUtility.Signature(ref stream, PSDLowLevelParser.OctBIMSignature)) { break; }
                var keyCode = stream.ReadSubStream(4).Span.ParseUTF8();
                uint length = stream.ReadUInt32();

                if (addLayerInfoParsers.ContainsKey(keyCode))
                {
                    var parser = Activator.CreateInstance(addLayerInfoParsers[keyCode]) as AdditionalLayerInfo;
                    parser.Length = length;
                    parser.ParseAddLY(ref stream);
                    addLayerInfoList.Add(parser);
                }
            }
            return addLayerInfoList.ToArray();
        }

        [Serializable]
        internal class AdditionalLayerInfo
        {
            public uint Length;
            public virtual void ParseAddLY(ref SubSpanStream stream) { }
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

        [Serializable, AdditionalLayerInfoParser("luni")]
        internal class luni : AdditionalLayerInfo
        {
            public string LayerName;

            public override void ParseAddLY(ref SubSpanStream stream)
            {
                LayerName = stream.ReadSubStream((int)Length).Span.ParseUTF16();
            }
        }
        [Serializable, AdditionalLayerInfoParser("lnsr")]
        internal class lnsr : AdditionalLayerInfo
        {
            public int IDForLayerName;

            public override void ParseAddLY(ref SubSpanStream stream)
            {
                IDForLayerName = stream.ReadInt32();
            }
        }
        [Serializable, AdditionalLayerInfoParser("lyid")]
        internal class lyid : AdditionalLayerInfo
        {
            public int ChannelID;

            public override void ParseAddLY(ref SubSpanStream stream)
            {
                ChannelID = stream.ReadInt32();
            }
        }
        [Serializable, AdditionalLayerInfoParser("lsct")]
        internal class lsct : AdditionalLayerInfo
        {
            public SelectionDividerTypeEnum SelectionDividerType;
            public string BlendModeKey;
            public int SubType;

            public enum SelectionDividerTypeEnum
            {
                AnyOther = 0,
                OpenFolder = 1,
                ClosedFolder = 2,
                BoundingSectionDivider = 3,
            }

            public override void ParseAddLY(ref SubSpanStream stream)
            {
                SelectionDividerType = (lsct.SelectionDividerTypeEnum)stream.ReadUInt32();
                if (Length >= 12)
                {
                    stream.ReadSubStream(4);
                    BlendModeKey = stream.ReadSubStream(4).Span.ParseUTF8();
                }
                if (Length >= 16)
                {
                    SubType = stream.ReadInt32();
                }
            }
        }

    }
}