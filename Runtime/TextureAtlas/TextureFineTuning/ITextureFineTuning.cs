using System.Collections.Generic;
using System.Linq;
using System;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    public interface ITextureFineTuning
    {
        void AddSetting(List<TexFineTuningTarget> propAndTextures);
    }
    public interface ITuningApplicant
    {
        int Order { get; }
        void ApplyTuning(List<TexFineTuningTarget> texFineTuningTargets);
    }
    public interface ITuningData
    {
    }
    public enum PropertySelect
    {
        Equal,
        NotEqual,
    }

    internal static class FineTuningUtil
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
