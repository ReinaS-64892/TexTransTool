using System.Collections.Generic;
using System.Linq;
using System;

namespace net.rs64.TexTransTool.TextureAtlas.FineSetting
{
    internal interface IAddFineTuning
    {
        void AddSetting(List<TexFineTuningTarget> propAndTextures);

    }
    internal interface ITuningApplicant
    {
        int Order { get; }
        void ApplyTuning(List<TexFineTuningTarget> texFineTuningTargets);
    }
    internal interface ITuningData
    {
    }
    public enum PropertySelect
    {
        Equal,
        NotEqual,
    }

    internal static class FineSettingUtil
    {
        public static IEnumerable<TexFineTuningTarget> FilteredTarget(string propertyNames, PropertySelect select, List<TexFineTuningTarget> propAndTextures)
        {
            var propertyNameList = propertyNames.Split(' ');
            switch (select)
            {
                default:
                case PropertySelect.Equal:
                    {
                        return propAndTextures.Where(x => propertyNameList.Contains(x.PropertyName));

                    }
                case PropertySelect.NotEqual:
                    {
                        return propAndTextures.Where(x => !propertyNameList.Contains(x.PropertyName));

                    }
            }
        }
    }


}