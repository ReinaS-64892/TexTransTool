using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public struct MipMapRemove : ITextureFineTuning
    {
        public PropertyName PropertyNames;
        public PropertySelect Select;

        public MipMapRemove(PropertyName propertyNames, PropertySelect select)
        {
            PropertyNames = propertyNames;
            Select = select;

        }

        public static MipMapRemove Default => new(PropertyName.DefaultValue, PropertySelect.Equal);

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNames, Select, texFineTuningTargets))
            {
                target.Value.Get<MipMapData>().UseMipMap = false;
            }

        }
    }

    internal class MipMapData : ITuningData
    {
        public bool UseMipMap = true;
    }

    internal class MipMapApplicant : ITuningApplicant
    {
        public int Order => -32;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var texKv in texFineTuningTargets)
            {
                var mipMapData = texKv.Value.Find<MipMapData>();
                if (mipMapData == null) { continue; }
                if (mipMapData.UseMipMap == texKv.Value.Texture2D.mipmapCount > 1) { continue; }

                Profiler.BeginSample("MipMapApplicant");
                var newTex = new Texture2D(texKv.Value.Texture2D.width, texKv.Value.Texture2D.height, TextureFormat.RGBA32, mipMapData.UseMipMap, !texKv.Value.Texture2D.isDataSRGB);
                var pixelData = texKv.Value.Texture2D.GetPixelData<Color32>(0);
                newTex.SetPixelData(pixelData, 0); pixelData.Dispose();
                newTex.Apply();
                newTex.name = texKv.Value.Texture2D.name;
                texKv.Value.Texture2D = newTex;
                Profiler.EndSample();
            }
        }
    }

}
