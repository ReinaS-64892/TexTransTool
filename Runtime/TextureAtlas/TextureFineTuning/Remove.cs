#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class Remove : ITextureFineTuning
    {
        public bool IsRemove = true;

        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.NotEqual;
        public Remove() { }
        public Remove(List<PropertyName> propertyNames, PropertySelect select)
        {
            PropertyNameList = propertyNames;
            Select = select;
        }

        void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                target.Value.Get<RemoveData>().IsRemove = IsRemove;
            }
        }
        void ITextureFineTuning.AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            AddSetting(texFineTuningTargets);
        }
    }

    internal class RemoveData : ITuningData
    {
        public bool IsRemove = true;
    }

    internal class RemoveApplicant : ITuningProcessor
    {

        public int Order => 64;

        public void ProcessingTuning(TexFineTuningProcessingContext ctx)
        {
            foreach (var removeTarget in ctx.TuningHolder.Where(i => i.Value.Find<RemoveData>() is not null).ToArray())
            {
                if (removeTarget.Value.Find<RemoveData>()!.IsRemove)
                {
                    var pHolder = ctx.ProcessingHolder[removeTarget.Key];
                    pHolder.RTOwned = false;
                    pHolder.RenderTextureProperty = null;
                }
            }
        }
    }

}
