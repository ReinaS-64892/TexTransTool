using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using net.rs64.MultiLayerImage.LayerData;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser.ChannelInformation;
using static net.rs64.MultiLayerImage.Parser.PSD.LayerRecordParser;
using static net.rs64.MultiLayerImage.Parser.PSD.PSDHighLevelParser;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal interface ISpecialLayerParser
    {
        AbstractLayerData Perse(HighLevelParserContext ctx, LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal class SpecialInfoOfAttribute : Attribute
    {
        public Type Type;
        public SpecialInfoOfAttribute(Type type)
        {
            Type = type;
        }
    }

    internal static class SpecialLayerParserUtil
    {

        static Dictionary<Type, ISpecialLayerParser> s_specialLayerParser;
        public static Dictionary<Type, ISpecialLayerParser> SpecialLayerParser
        {
            get { s_specialLayerParser ??= GetAdditionalLayerInfoParsersTypes(); return s_specialLayerParser; }
        }
        static Dictionary<Type, ISpecialLayerParser> GetAdditionalLayerInfoParsersTypes()
        {
            var dict = new Dictionary<Type, ISpecialLayerParser>();

            foreach (var addLYType in AppDomain.CurrentDomain.GetAssemblies()
                 .SelectMany(I => I.GetTypes())
                 .Where(I => I.GetCustomAttributes<SpecialInfoOfAttribute>().Any()))
            {
                var instants = Activator.CreateInstance(addLYType) as ISpecialLayerParser;
                foreach (var attr in addLYType.GetCustomAttributes<SpecialInfoOfAttribute>())
                    if (dict.ContainsKey(attr.Type) is false) { dict.Add(attr.Type, instants); }
            }

            return dict;
        }
    }
}
