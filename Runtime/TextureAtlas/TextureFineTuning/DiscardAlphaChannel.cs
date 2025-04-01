#nullable enable
using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    [AddTypeMenu("(Experimental) Discard Alpha Channel")]
    public class DiscardAlphaChannel : ITextureFineTuning
    {
        public bool IsDiscard = true;

        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.Equal;
        public DiscardAlphaChannel() { }
        public DiscardAlphaChannel(List<PropertyName> propertyNames, PropertySelect propertySelect)
        {
            PropertyNameList = propertyNames;
            Select = propertySelect;
        }

        void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                target.Value.Get<DiscardAlphaChannelData>().DiscardFrag = IsDiscard;
            }
        }
        void ITextureFineTuning.AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            AddSetting(texFineTuningTargets);
        }
    }

    internal class DiscardAlphaChannelData : ITuningData
    {
        public bool DiscardFrag;
    }

    internal class DiscardAlphaChannelApplicant : ITuningProcessor
    {
        public int Order => 128;

        public void ProcessingTuning(TexFineTuningProcessingContext ctx)
        {
            foreach (var tuning in ctx.TuningHolder)
            {
                var tuningHolder = tuning.Value;
                var discardFragData = tuningHolder.Find<DiscardAlphaChannelData>();
                if (discardFragData == null) { continue; }
                if (discardFragData.DiscardFrag is false) { continue; }

                if (ctx.ProcessingHolder[tuning.Key].RTOwned is false) { continue; }

                var targetProperty = ctx.ProcessingHolder[tuning.Key].RenderTextureProperty!;
                var rt = ctx.RenderTextures[targetProperty];

                var newRt = ctx.Engine.CloneRenderTexture(rt);
                ctx.Engine.AlphaFill(newRt, 1.0f);

                ctx.RenderTextures[targetProperty] = newRt;
                ctx.NewRenderTextures.Add(newRt);
            }
        }
    }
}
