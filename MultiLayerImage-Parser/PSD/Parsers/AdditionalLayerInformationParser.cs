using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{
    internal static class AdditionalLayerInformationParser
    {
        static Dictionary<string, (Type, bool)> s_additionalLayerInfoParsersTypes;
        public static Dictionary<string, (Type, bool)> AdditionalLayerInfoParsersTypes
        {
            get { s_additionalLayerInfoParsersTypes ??= GetAdditionalLayerInfoParsersTypes(); return s_additionalLayerInfoParsersTypes; }
        }
        static Dictionary<string, (Type, bool)> GetAdditionalLayerInfoParsersTypes()
        {
            var dict = new Dictionary<string, (Type, bool)>();

            foreach (var addLYType in AppDomain.CurrentDomain.GetAssemblies()
                 .SelectMany(I => I.GetTypes())
                 .Where(I => I.GetCustomAttribute<AdditionalLayerInfoParserAttribute>() != null))
            {
                var Instants = Activator.CreateInstance(addLYType) as AdditionalLayerInfoBase;

                var customAttribute = addLYType.GetCustomAttribute<AdditionalLayerInfoParserAttribute>();
                if (!dict.ContainsKey(customAttribute.Code))
                {
                    dict.Add(customAttribute.Code, (addLYType, customAttribute.MayULongLength));
                }
            }

            return dict;
        }
        public static AdditionalLayerInfoBase[] PaseAdditionalLayerInfos(bool isPSB, SubSpanStream stream, bool roundTo4 = false)
        {
            var addLayerInfoList = new List<AdditionalLayerInfoBase>();
            while (stream.Position < stream.Length)
            {
                if (!ParserUtility.Signature(ref stream, PSDLowLevelParser.OctBIMSignature)) { break; }
                var keyCode = stream.ReadSubStream(4).Span.ParseUTF8();
                // var length = isPSB is false ? stream.ReadUInt32() : stream.ReadUInt64();

                AdditionalLayerInfoBase info;
                if (AdditionalLayerInfoParsersTypes.ContainsKey(keyCode))
                {
                    var TypeAndMayLong = AdditionalLayerInfoParsersTypes[keyCode];
                    var parser = info = Activator.CreateInstance(TypeAndMayLong.Item1) as AdditionalLayerInfoBase;
                    parser.Length = isPSB is false ? stream.ReadUInt32() : TypeAndMayLong.Item2 ? stream.ReadUInt64() : stream.ReadUInt32();
                    parser.ParseAddLY(stream.ReadSubStream((int)parser.Length));
                    addLayerInfoList.Add(parser);
                }
                else
                {
                    var fallBack = new FallBackAdditionalLayerInfoParser();
                    info = fallBack;
                    fallBack.KeyCode = keyCode;
                    fallBack.Length = stream.ReadUInt32();
                    fallBack.ParseAddLY(stream.ReadSubStream((int)fallBack.Length));
                    addLayerInfoList.Add(fallBack);
                }

                if (roundTo4)
                {
                    if ((info.Length % 4) != 0)
                    {
                        stream.ReadSubStream((int)(4 - (info.Length % 4)));
                    }
                }
            }
            return addLayerInfoList.ToArray();
        }

    }
}
