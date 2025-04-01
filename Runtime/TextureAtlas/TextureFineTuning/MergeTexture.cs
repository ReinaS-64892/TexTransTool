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

        void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(MargeChildren, PropertySelect.Equal, texFineTuningTargets))
            {
                target.Value.Get<MergeTextureData>().MargeParent = MargeParent;
            }
        }
        void ITextureFineTuning.AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            AddSetting(texFineTuningTargets);
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
            var alreadyMerge = new HashSet<string>();
            foreach (var kv in ctx.TuningHolder)
            {
                var mtData = kv.Value.Find<MergeTextureData>();
                if (mtData is null) { continue; }
                if (mtData.MargeParent is null) { continue; }
                if (alreadyMerge.Contains(kv.Key)) { continue; }//TODO :waning or info

                if (mergeDict.ContainsKey(mtData.MargeParent) is false)
                {
                    if (alreadyMerge.Contains(mtData.MargeParent)) { continue; }//TODO :waning or info
                    if (ctx.ProcessingHolder.ContainsKey(mtData.MargeParent) is false) { continue; } //マージ親がない場合は何もできないこと
                    alreadyMerge.Add(mtData.MargeParent);
                    mergeDict[mtData.MargeParent] = new();
                }
                mergeDict[mtData.MargeParent].Add(kv.Key);
                alreadyMerge.Add(kv.Key);
            }
            foreach (var ftMarge in mergeDict)
            {
                if (ctx.ProcessingHolder.ContainsKey(ftMarge.Key) is false) { continue; }
                var parentFTHolder = ctx.ProcessingHolder[ftMarge.Key];

                Debug.Assert(parentFTHolder.RTOwned);//順序的に常に Owned なやつしか存在しない。

                var parentRT = ctx.RenderTextures[parentFTHolder.RenderTextureProperty!];
                var newRT = ctx.Engine.CloneRenderTexture(parentRT);
                var children = string.Join(":", ftMarge.Value);
                newRT.Name += $"-ParentOf({ftMarge.Key})-ChildrenOf({children})";

                foreach (var childe in ftMarge.Value)
                {
                    var tfHolder = ctx.ProcessingHolder[childe];
                    Debug.Assert(tfHolder.RTOwned);

                    var cRt = ctx.RenderTextures[tfHolder.RenderTextureProperty!];

                    MergePixels(ctx.Engine, newRT, cRt);

                    tfHolder.RTOwned = false;
                    tfHolder.RenderTextureProperty = parentFTHolder.RenderTextureProperty!;
                }

                ctx.RenderTextures[parentFTHolder.RenderTextureProperty!] = newRT;
                ctx.NewRenderTextures.Add(newRT);
            }
        }
        public static void MergePixels(ITexTransToolForUnity engine, ITTRenderTexture parent, ITTRenderTexture childe)
        {
            using var ch = engine.GetComputeHandler(engine.GetExKeyQuery<IAtlasComputeKey>().MergeAtlasedTextures);
            var distTexID = ch.NameToID("DistTex");
            var addTexID = ch.NameToID("AddTex");
            ch.SetTexture(distTexID, parent);
            ch.SetTexture(addTexID, childe);

            ch.DispatchWithTextureSize(parent);
        }

    }
}
