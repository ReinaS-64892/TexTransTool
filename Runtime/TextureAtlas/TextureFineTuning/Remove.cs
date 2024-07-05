using System;
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class Remove : ITextureFineTuning
    {
        public PropertyName PropertyNames = PropertyName.DefaultValue;
        public PropertySelect Select = PropertySelect.NotEqual;

        public Remove() { }
        public Remove(PropertyName propertyNames, PropertySelect select)
        {
            PropertyNames = propertyNames;
            Select = select;
        }

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNames, Select, texFineTuningTargets))
            {
                target.Value.Get<RemoveData>();
            }
        }

    }

    internal class RemoveData : ITuningData
    {

    }

    internal class RemoveApplicant : ITuningApplicant
    {

        public int Order => 64;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var removeTarget in texFineTuningTargets.Where(i => i.Value.Find<RemoveData>() is not null).ToArray())
            {
                texFineTuningTargets.Remove(removeTarget.Key);
            }

        }
    }

}
