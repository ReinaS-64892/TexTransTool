using System;
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class Remove : ITextureFineTuning
    {
        public bool IsRemove = true;

        [Obsolete("V4SaveData", true)] public PropertyName PropertyNames = PropertyName.DefaultValue;
        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.NotEqual;
        public Remove() { }
        [Obsolete("V4SaveData", true)]
        public Remove(PropertyName propertyNames, PropertySelect select)
        {
            PropertyNames = propertyNames;
            Select = select;
        }
        public Remove(List<PropertyName> propertyNames, PropertySelect select)
        {
            PropertyNameList = propertyNames;
            Select = select;
        }

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                target.Value.Get<RemoveData>().IsRemove = IsRemove;
            }
        }

    }

    internal class RemoveData : ITuningData
    {
        public bool IsRemove = true;
    }

    internal class RemoveApplicant : ITuningApplicant
    {

        public int Order => 64;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets, IDeferTextureCompress compress)
        {
            foreach (var removeTarget in texFineTuningTargets.Where(i => i.Value.Find<RemoveData>() is not null).ToArray())
            {
                if (removeTarget.Value.Find<RemoveData>().IsRemove) { texFineTuningTargets.Remove(removeTarget.Key); }
            }

        }
    }

}
