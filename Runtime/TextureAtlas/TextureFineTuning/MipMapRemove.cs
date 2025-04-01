#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    [Obsolete]
    [AddTypeMenu("(this is Obsolete, place use MipMap) Mip Map Remove")]// TODO : Migration
    public class MipMapRemove : ITextureFineTuning
    {
        public bool IsRemove = true;

        [Obsolete("V4SaveData", true)] public PropertyName PropertyNames = PropertyName.DefaultValue;
        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.Equal;

        public MipMapRemove() { }
        [Obsolete("V4SaveData", true)]
        public MipMapRemove(PropertyName propertyNames, PropertySelect select)
        {
            PropertyNames = propertyNames;
            Select = select;

        }
        public MipMapRemove(List<PropertyName> propertyNames, PropertySelect select)
        {
            PropertyNameList = propertyNames;
            Select = select;

        }

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                target.Value.Get<MipMapData>().UseMipMap = IsRemove is false;
            }

        }
    }



}
