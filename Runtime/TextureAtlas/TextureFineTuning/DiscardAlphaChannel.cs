using System;
using System.Collections.Generic;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

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

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                target.Value.Get<DiscardAlphaChannelData>().DiscardFrag = IsDiscard;
            }
        }
    }

    internal class DiscardAlphaChannelData : ITuningData
    {
        public bool DiscardFrag;
    }

    internal class DiscardAlphaChannelApplicant : ITuningApplicant
    {
        public int Order => 0;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets, IDeferTextureCompress compress)
        {
            foreach (var texKv in texFineTuningTargets)
            {
                var discardFragData = texKv.Value.Find<DiscardAlphaChannelData>();
                if (discardFragData == null) { continue; }
                if (discardFragData.DiscardFrag is false) { continue; }

                var texDataNativeArray = texKv.Value.Texture2D.GetRawTextureData<Color32>();
                var texDataSpan = texDataNativeArray.AsSpan();
                for (var i = 0; texDataSpan.Length > i; i += 1)
                {
                    texDataSpan[i].a = byte.MaxValue;
                }
                texKv.Value.Texture2D.LoadRawTextureData(texDataNativeArray);
                texKv.Value.Texture2D.Apply(false);
            }
        }

    }
}
