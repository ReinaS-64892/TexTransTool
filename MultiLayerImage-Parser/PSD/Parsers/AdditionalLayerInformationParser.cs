using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static partial class AdditionalLayerInformationParser
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
                    parser.ParseAddLY(stream.ReadSubStream((int)length));
                    addLayerInfoList.Add(parser);
                }
            }
            return addLayerInfoList.ToArray();
        }

    }
}