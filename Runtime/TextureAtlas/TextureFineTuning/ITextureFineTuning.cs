#nullable enable
using System.Collections.Generic;
using System.Linq;
using System;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    public interface ITextureFineTuning
    {
        void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets);
    }
    internal interface ITuningProcessor
    {
        int Order { get; }
        void ProcessingTuning(TexFineTuningProcessingContext ctx);
    }
    internal interface ITuningData
    {
    }
    public enum PropertySelect
    {
        Equal,
        NotEqual,
    }

    internal static class FineTuningUtil
    {
        public static IEnumerable<KeyValuePair<string,TexFineTuningHolder>> FilteredTarget(List<PropertyName> propertyNames, PropertySelect select, Dictionary<string,TexFineTuningHolder> targets)
        {
            var propertyNameList = propertyNames.Select(i => i.ToString()).ToHashSet();
            switch (select)
            {
                default:
                case PropertySelect.Equal:
                    {
                        return targets.Where(x => propertyNameList.Contains(x.Key));

                    }
                case PropertySelect.NotEqual:
                    {
                        return targets.Where(x => !propertyNameList.Contains(x.Key));

                    }
            }
        }
    }


}
