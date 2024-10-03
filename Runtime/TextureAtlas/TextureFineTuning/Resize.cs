using System;
using System.Collections.Generic;
using net.rs64.TexTransUnityCore.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class Resize : ITextureFineTuning
    {
        [PowerOfTwo] public int Size = 512;
        [Obsolete("V4SaveData",true)] public PropertyName PropertyNames = PropertyName.DefaultValue;
        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.NotEqual;

        public Resize() { }
        [Obsolete("V4SaveData",true)]
        public Resize(int size, PropertyName propertyNames, PropertySelect select)
        {
            Size = size;
            PropertyNames = propertyNames;
            Select = select;
        }
        public Resize(int size, List<PropertyName> propertyNames, PropertySelect select)
        {
            Size = size;
            PropertyNameList = propertyNames;
            Select = select;

        }

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                target.Value.Get<SizeData>().TextureSize = Size;
            }
        }
    }

    internal class SizeData : ITuningData
    {
        public int TextureSize = 2048;
    }

    internal class ResizeApplicant : ITuningApplicant
    {
        public int Order => -64;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets, IDeferTextureCompress compress)
        {
            foreach (var texKv in texFineTuningTargets)
            {
                var sizeData = texKv.Value.Find<SizeData>();
                if (sizeData == null) { continue; }
                if (sizeData.TextureSize >= texKv.Value.Texture2D.width) { continue; }
                texKv.Value.Texture2D = TextureUtility.ResizeTexture(texKv.Value.Texture2D, new Vector2Int(sizeData.TextureSize, (int)((texKv.Value.Texture2D.height / (float)texKv.Value.Texture2D.width) * sizeData.TextureSize)));
            }
        }
    }

}
