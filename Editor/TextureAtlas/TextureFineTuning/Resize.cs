#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineSetting
{
    public struct Resize : IAddFineTuning
    {
        public int Size;
        public string PropertyNames;
        public PropertySelect Select;

        public Resize(int size, string propertyNames, PropertySelect select)
        {
            Size = size;
            PropertyNames = propertyNames;
            Select = select;

        }

        public void AddSetting(List<TexFineTuningTarget> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FilteredTarget(PropertyNames, Select, propAndTextures))
            {
                var SizeData = target.TuningDataList.Find(I => I is SizeData) as SizeData;
                if (SizeData != null)
                {
                    SizeData.TextureSize = Size;
                }
                else
                {
                    target.TuningDataList.Add(new SizeData() { TextureSize = Size });
                }
            }
        }
    }

    public class SizeData : ITuningData
    {
        public int TextureSize = 2048;
    }

    public class ResizeApplicant : ITuningApplicant
    {
        public int Order => -64;

        public void ApplyTuning(List<TexFineTuningTarget> texFineTuningTargets)
        {
            foreach (var texf in texFineTuningTargets)
            {
                var sizeData = texf.TuningDataList.Find(I => I is SizeData) as SizeData;
                if (sizeData == null) { continue; }
                if (sizeData.TextureSize == texf.Texture2D.width) { continue; }
                texf.Texture2D = TextureBlendUtils.ResizeTexture(texf.Texture2D, new Vector2Int(sizeData.TextureSize, sizeData.TextureSize));
            }
        }
    }

}
#endif
