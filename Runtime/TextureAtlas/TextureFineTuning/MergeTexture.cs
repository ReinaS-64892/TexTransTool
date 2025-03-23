#nullable enable
using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    [AddTypeMenu("(Experimental) Merge Texture")]
    public class MergeTexture : ITextureFineTuning
    {
        public PropertyName MargeParent;
        public List<PropertyName> MargeChildren;
        public MergeTexture() { MargeParent = new(); MargeChildren = new(); }
        public MergeTexture(PropertyName margeParent, List<PropertyName> margeChildren)
        {
            MargeParent = margeParent;
            MargeChildren = margeChildren;
        }

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(MargeChildren, PropertySelect.Equal, texFineTuningTargets))
            {
                target.Value.Get<MergeTextureData>().MargeParent = MargeParent;
            }
        }
    }

    internal class MergeTextureData : ITuningData
    {
        public string? MargeParent;
    }

    internal class MergeTextureApplicant : ITuningProcessor
    {
        public int Order => 16;
        public void ProcessingTuning(TexFineTuningProcessingContext ctx)
        {
            var mergeDict = new Dictionary<string, List<string>>();
            foreach (var kv in ctx.TuningHolder)
            {
                var mtData = kv.Value.Find<MergeTextureData>();
                if (mtData is null) { continue; }
                if (mtData.MargeParent is null) { continue; }

                if (mergeDict.ContainsKey(mtData.MargeParent) is false) { mergeDict[mtData.MargeParent] = new(); }
                mergeDict[mtData.MargeParent].Add(kv.Key);
            }
            foreach (var ftMarge in mergeDict)
            {
                if (ctx.ProcessingHolder.ContainsKey(ftMarge.Key) is false) { continue; }
                var parentFTHolder = ctx.ProcessingHolder[ftMarge.Key];

                Debug.Assert(parentFTHolder.RTOwned);//順序的に常に Owned なやつしか存在しない。

                var parentRT = ctx.RenderTextures[parentFTHolder.RenderTextureProperty!];
                var newRT = ctx.Engine.CloneRenderTexture(parentRT);

                foreach (var childe in ftMarge.Value)
                {
                    var tfHolder = ctx.ProcessingHolder[childe];
                    Debug.Assert(tfHolder.RTOwned);

                    var cRt = ctx.RenderTextures[tfHolder.RenderTextureProperty!];

                    MergePixels(ctx.Engine, newRT, cRt);

                    tfHolder.RTOwned = false;
                    tfHolder.RenderTextureProperty = ftMarge.Key;
                }
            }
        }
        public static void MergePixels(ITexTransToolForUnity engine, ITTRenderTexture parent, ITTRenderTexture childe)
        {
            throw new NotImplementedException();
        }

    }
}
