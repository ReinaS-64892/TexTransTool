#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool.TextureAtlas.FineSetting
{
    public struct MipMapRemove : IAddFineTuning
    {
        public string PropertyNames;
        public PropertySelect Select;

        public MipMapRemove(string propertyNames, PropertySelect select)
        {
            PropertyNames = propertyNames;
            Select = select;

        }

        public void AddSetting(List<TexFineTuningTarget> propAndTextures)
        {
            foreach (var target in FineSettingUtil.FilteredTarget(PropertyNames, Select, propAndTextures))
            {
                var MipMapData = target.TuningDataList.Find(I => I is MipMapData) as MipMapData;
                if (MipMapData != null)
                {
                    MipMapData.UseMipMap = false;
                }
                else
                {
                    target.TuningDataList.Add(new MipMapData() { UseMipMap = false });
                }
            }

        }
    }

    public class MipMapData : ITuningData
    {
        public bool UseMipMap = true;
    }

    public class MipMapApplicant : ITuningApplicant
    {
        public int Order => -32;

        public void ApplyTuning(List<TexFineTuningTarget> texFineTuningTargets)
        {
            foreach (var texf in texFineTuningTargets)
            {
                var mipMapData = texf.TuningDataList.Find(I => I is MipMapData) as MipMapData;
                if (mipMapData == null) { continue; }
                if (mipMapData.UseMipMap == texf.Texture2D.mipmapCount > 1) { continue; }

                var newTex = new Texture2D(texf.Texture2D.width, texf.Texture2D.height, TextureFormat.RGBA32, mipMapData.UseMipMap);
                newTex.SetPixels32(texf.Texture2D.GetPixels32());
                newTex.Apply();
                newTex.name = texf.Texture2D.name;
                texf.Texture2D = newTex;
            }
        }
    }

}
#endif
