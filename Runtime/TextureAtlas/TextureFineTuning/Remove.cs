using System;
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class Remove : ITextureFineTuning
    {
        public PropertyName PropertyNames;
        public PropertySelect Select;

        public Remove(PropertyName propertyNames, PropertySelect select)
        {
            PropertyNames = propertyNames;
            Select = select;
        }

        public static Remove Default => new(PropertyName.DefaultValue, PropertySelect.NotEqual);

        public void AddSetting(List<TexFineTuningTarget> propAndTextures)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNames, Select, propAndTextures))
            {
                var referenceCopyData = target.TuningDataList.Find(I => I is RemoveData) as RemoveData;
                if (referenceCopyData == null)
                {
                    target.TuningDataList.Add(new RemoveData());
                }
            }
        }

    }

    internal class RemoveData : ITuningData
    {

    }

    internal class RemoveApplicant : ITuningApplicant
    {

        public int Order => 64;

        public void ApplyTuning(List<TexFineTuningTarget> texFineTuningTargets)
        {
            texFineTuningTargets.RemoveAll(I => I.TuningDataList.Any(T => T is RemoveData));
        }
    }

}
