#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System;
using net.rs64.TexTransTool.ShaderSupport;

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
    internal enum PropertySelect
    {
        Equal,
        NotEqual,
    }

    internal static class FineSettingUtil
    {
        public static IEnumerable<TexFineTuningTarget> FilteredTarget(string PropertyNames, PropertySelect select, List<TexFineTuningTarget> propAndTextures)
        {
            var PropertyNameList = PropertyNames.Split(' ');
            switch (select)
            {
                default:
                case PropertySelect.Equal:
                    {
                        return propAndTextures.Where(x => PropertyNameList.Contains(x.PropertyName));

                    }
                case PropertySelect.NotEqual:
                    {
                        return propAndTextures.Where(x => !PropertyNameList.Contains(x.PropertyName));

                    }
            }
        }
    }


}
#endif
