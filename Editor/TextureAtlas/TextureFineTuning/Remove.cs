#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.TextureAtlas.FineSetting
{
    public class Remove : IAddFineTuning
    {
        public string PropertyNames;
        public PropertySelect Select;

        public Remove(string propertyNames, PropertySelect select)
        {
            PropertyNames = propertyNames;
            Select = select;
        }

        public void AddSetting(List<TexFineTuningTarget> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FilteredTarget(PropertyNames, Select, propAndTextures))
            {
                var referenceCopyData = target.TuningDataList.Find(I => I is RemoveData) as RemoveData;
                if (referenceCopyData == null)
                {
                    target.TuningDataList.Add(new RemoveData());
                }
            }
        }

    }

    public class RemoveData : ITuningData
    {

    }

    public class RemoveApplicant : ITuningApplicant
    {

        public int Order => 64;

        public void ApplyTuning(List<TexFineTuningTarget> texFineTuningTargets)
        {
            foreach (var removeTarget in texFineTuningTargets.Where(I => I.TuningDataList.Any(T => T is RemoveData)))
            {
                texFineTuningTargets.Remove(removeTarget);
            }

        }
    }

}
#endif
