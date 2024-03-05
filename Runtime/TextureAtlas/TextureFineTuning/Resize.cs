using System;
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public struct Resize : ITextureFineTuning
    {
        public int Size;
        public PropertyName PropertyNames;
        public PropertySelect Select;

        public Resize(int size, PropertyName propertyNames, PropertySelect select)
        {
            Size = size;
            PropertyNames = propertyNames;
            Select = select;

        }

        public void AddSetting(List<TexFineTuningTarget> propAndTextures)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNames, Select, propAndTextures))
            {
                var sizeData = target.TuningDataList.Find(I => I is SizeData) as SizeData;
                if (sizeData != null)
                {
                    sizeData.TextureSize = Size;
                }
                else
                {
                    target.TuningDataList.Add(new SizeData() { TextureSize = Size });
                }
            }
        }


        public static Resize Default => new(512, PropertyName.DefaultValue, PropertySelect.NotEqual);
        public ITextureFineTuning GetDefault => Default;
    }

    internal class SizeData : ITuningData
    {
        public int TextureSize = 2048;
    }

    internal class ResizeApplicant : ITuningApplicant
    {
        public int Order => -64;

        public void ApplyTuning(List<TexFineTuningTarget> texFineTuningTargets)
        {
            foreach (var texf in texFineTuningTargets)
            {
                var sizeData = texf.TuningDataList.Find(I => I is SizeData) as SizeData;
                if (sizeData == null) { continue; }
                if (sizeData.TextureSize == texf.Texture2D.width) { continue; }
                texf.Texture2D = TextureUtility.ResizeTexture(texf.Texture2D, new Vector2Int(sizeData.TextureSize, (int)((texf.Texture2D.height / (float)texf.Texture2D.width) * sizeData.TextureSize)));
            }
        }
    }

}
