#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class MipMap : ITextureFineTuning
    {
        // これ .. MipMap の生成アルゴリズムと分けてもいいけど、まぁ分けるのは後でもできるからこれで行こう
        public bool UseMipMap = true;
        public string MipMapGenerateAlgorithm = ITexTransToolForUnity.DS_ALGORITHM_DEFAULT;
        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.Equal;

        public MipMap() { }
        public MipMap(List<PropertyName> propertyNames, PropertySelect select)
        {
            PropertyNameList = propertyNames;
            Select = select;

        }

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                var mipMapData = target.Value.Get<MipMapData>();
                mipMapData.UseMipMap = UseMipMap;
                mipMapData.MipMapGenerateAlgorithm = MipMapGenerateAlgorithm;
            }

        }
    }

    internal class MipMapData : ITuningData
    {
        public bool UseMipMap = true;
        public string MipMapGenerateAlgorithm = ITexTransToolForUnity.DS_ALGORITHM_DEFAULT;
    }

    internal class MipMapApplicant : ITuningProcessor
    {
        public int Order => 0;

        public void ProcessingTuning(TexFineTuningProcessingContext ctx)
        {
            foreach (var tuning in ctx.TuningHolder)
            {
                var tuningHolder = tuning.Value;
                var mipMapData = tuningHolder.Find<MipMapData>();
                if (mipMapData == null) { continue; }

                var texDesc = ctx.ProcessingHolder[tuning.Key].TextureDescriptor;
                texDesc.UseMipMap = mipMapData.UseMipMap;
                texDesc.MipMapGenerateAlgorithm = mipMapData.MipMapGenerateAlgorithm;
            }
        }
    }

}
