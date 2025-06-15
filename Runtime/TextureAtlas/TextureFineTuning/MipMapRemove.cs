#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    [Obsolete]
    [AddTypeMenu("(this is Obsolete, place use MipMap) Mip Map Remove")]
    public class MipMapRemove : ITextureFineTuning
    {
        public bool IsRemove = true;

        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.Equal;

        public MipMapRemove() { }
        public MipMapRemove(List<PropertyName> propertyNames, PropertySelect select)
        {
            PropertyNameList = propertyNames;
            Select = select;

        }

        void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                target.Value.Get<MipMapData>().UseMipMap = IsRemove is false;
            }

        }
        void ITextureFineTuning.AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            AddSetting(texFineTuningTargets);
        }
    }



}
