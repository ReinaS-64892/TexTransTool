using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser.AdditionalLayerInfo
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
        public static AdditionalLayerInfoBase[] PaseAdditionalLayerInfos(bool isPSB, BinarySectionStream stream, bool roundTo4 = false)
        {
            var addLayerInfoList = new List<AdditionalLayerInfoBase>();
            while (stream.Position < stream.Length)
            {
                if (stream.Signature(PSDLowLevelParser.OctBIMSignature) is false) { break; }

                Span<byte> keyBuf = stackalloc byte[4];
                stream.ReadToSpan(keyBuf);
                var keyCode = keyBuf.ParseASCII();

                AdditionalLayerInfoBase info;
                if (AdditionalLayerInfoParsersTypes.ContainsKey(keyCode))
                {
                    var TypeAndMayLong = AdditionalLayerInfoParsersTypes[keyCode];
                    var parser = info = Activator.CreateInstance(TypeAndMayLong.Item1) as AdditionalLayerInfoBase;
                    var length = isPSB is false ? stream.ReadUInt32() : TypeAndMayLong.Item2 ? stream.ReadUInt64() : stream.ReadUInt32();
                    parser.Address = stream.PeekToAddress((long)length);
                    parser.ParseAddLY(isPSB, stream.ReadSubSection(parser.Address.Length));
                    addLayerInfoList.Add(parser);
                }
                else
                {
                    var fallBack = new FallBackAdditionalLayerInfoParser();
                    info = fallBack;
                    fallBack.KeyCode = keyCode;
                    var length = stream.ReadUInt32();
                    fallBack.Address = stream.PeekToAddress(length);
                    fallBack.ParseAddLY(isPSB, stream.ReadSubSection(fallBack.Address.Length));
                    addLayerInfoList.Add(fallBack);
                }

                if (roundTo4)
                {
                    if ((info.Address.Length % 4) != 0)
                    {
                        stream.ReadSubSection((int)(4 - (info.Address.Length % 4)));
                    }
                }
            }
            return addLayerInfoList.ToArray();
        }

    }
}
