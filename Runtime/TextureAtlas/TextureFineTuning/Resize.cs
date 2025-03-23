#nullable enable
using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class Resize : ITextureFineTuning
    {
        [PowerOfTwo] public int Size = 512;
        [Obsolete("V4SaveData", true)] public PropertyName PropertyNames = PropertyName.DefaultValue;
        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.NotEqual;
        public string DownScaleAlgorithm = ITexTransToolForUnity.DS_ALGORITHM_DEFAULT;

        public Resize() { }
        [Obsolete("V4SaveData", true)]
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

    internal class ResizeApplicant : ITuningProcessor
    {
        public int Order => 129;

        public void ProcessingTuning(TexFineTuningProcessingContext ctx)
        {
            foreach (var tuning in ctx.TuningHolder)
            {
                var tuningHolder = tuning.Value;
                var sizeData = tuningHolder.Find<SizeData>();
                if (sizeData == null) { continue; }

                var pHolder = ctx.ProcessingHolder[tuning.Key];
                if (pHolder.RTOwned is false) { continue; }

                var targetProperty = pHolder.RenderTextureProperty!;
                var rt = ctx.RenderTextures[targetProperty];
                if (sizeData.TextureSize >= rt.Width) { continue; }

                var newRt = ctx.Engine.CreateRenderTexture(
                    sizeData.TextureSize
                    , (int)((rt.Hight / (float)rt.Width) * sizeData.TextureSize)
                    );
                ctx.Engine.DefaultResizing(newRt, rt);

                ctx.RenderTextures[targetProperty] = newRt;
                ctx.NewRenderTextures.Add(newRt);
            }
        }
    }

}
