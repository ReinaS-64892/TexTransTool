#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    /*
        競合しないタイプは 0、参照破壊や変更系が後の方に行く


        MipMapRemove 0
        ColorSpace 0
        Compress 0

        MergeTexture 16
        ReferenceCopy 32

        Remove 64

        DiscardAlphaChannel 128
        Resize 129

    */

    public class TexFineTuningHolder
    {
        Dictionary<Type, ITuningData> _tuningDataDict;
        internal TexFineTuningHolder() { _tuningDataDict = new(); }

        internal TuningData? Find<TuningData>() where TuningData : class, ITuningData, new()
        {
            if (_tuningDataDict.TryGetValue(typeof(TuningData), out ITuningData t)) { return t as TuningData; }
            else { return null; }
        }
        internal TuningData Get<TuningData>() where TuningData : class, ITuningData, new()
        {
            if (_tuningDataDict.ContainsKey(typeof(TuningData))) { return (TuningData)_tuningDataDict[typeof(TuningData)]; }
            else
            {
                var d = _tuningDataDict[typeof(TuningData)] = new TuningData();
                return (TuningData)d;
            }
        }
        internal void Set<TuningData>(TuningData tuningData) where TuningData : class, ITuningData, new()
        {
            _tuningDataDict[typeof(TuningData)] = tuningData;
        }
    }
    internal class TexFineTuningProcessingContext
    {
        public ITexTransToolForUnity Engine;
        public Dictionary<string, TexFineTuningHolder> TuningHolder;
        public Dictionary<string, TextureProcessingHolder> ProcessingHolder;

        public Dictionary<string, ITTRenderTexture> RenderTextures;

        public HashSet<ITTRenderTexture> NewRenderTextures = new();

        public Dictionary<string, ITTRenderTexture> OriginRenderTextures;

        public TexFineTuningProcessingContext(ITexTransToolForUnity ttt4u, Dictionary<string, TexFineTuningHolder> tuningHolder, Dictionary<string, ITTRenderTexture> renderTextures, Dictionary<ITTRenderTexture, TexTransToolTextureDescriptor> textureDescriptors)
        {
            Engine = ttt4u;

            TuningHolder = tuningHolder;

            RenderTextures = new(renderTextures);
            ProcessingHolder = RenderTextures.ToDictionary(rt => rt.Key, rt => new TextureProcessingHolder(rt.Key, new TexTransToolTextureDescriptor()));
            foreach (var prop in TuningHolder.Keys)
            {
                if (ProcessingHolder.ContainsKey(prop)) { continue; }
                ProcessingHolder.Add(prop, new("", new()) { RTOwned = false, RenderTextureProperty = null });// ReferenceCopy で増えたやつ用に
            }

            OriginRenderTextures = renderTextures;
        }

        internal class TextureProcessingHolder
        {
            public bool RTOwned = true;
            public string? RenderTextureProperty;
            public TexTransToolTextureDescriptor TextureDescriptor;

            public TextureProcessingHolder(string renderTextureProperty, TexTransToolTextureDescriptor textureDescriptor)
            {
                RenderTextureProperty = renderTextureProperty;
                TextureDescriptor = textureDescriptor;
            }

        }
    }
    internal class TexFineTuningResult
    {
        public Dictionary<string, ITTRenderTexture> RenderTextures;
        public Dictionary<ITTRenderTexture, TexTransToolTextureDescriptor> TextureDescriptors;


        public TexFineTuningResult(Dictionary<string, ITTRenderTexture> renderTextures, Dictionary<ITTRenderTexture, TexTransToolTextureDescriptor> textureDescriptors)
        {
            RenderTextures = renderTextures;
            TextureDescriptors = textureDescriptors;
        }
    }
    internal class TexFineTuningUtility
    {

        public static Dictionary<string, TexFineTuningHolder> InitTexFineTuningHolders(IEnumerable<string> properties)
        {
            var texFineTuningTargets = properties.ToDictionary(i => i, i => new TexFineTuningHolder());
            foreach (var texKv in texFineTuningTargets)
            {
                texKv.Value.Set(new MipMapData());
                texKv.Value.Set(new TextureCompressionTuningData());
            }
            return texFineTuningTargets;
        }

        public static TexFineTuningResult ProcessingTextureFineTuning(ITexTransToolForUnity engine, Dictionary<string, TexFineTuningHolder> tuningHolder, Dictionary<string, ITTRenderTexture> atlasedTextures)
        {
            var pressers = InterfaceUtility.GetInterfaceInstance<ITuningProcessor>().ToArray();
            Array.Sort(pressers, (l, r) => l.Order - r.Order);

            var ctx = new TexFineTuningProcessingContext(
                engine
                , tuningHolder
                , atlasedTextures
                , new()
            );

            foreach (var p in pressers) { p.ProcessingTuning(ctx); }

            var resultTextures = new Dictionary<string, ITTRenderTexture>();
            var resultTextureDescriptor = new Dictionary<ITTRenderTexture, TexTransToolTextureDescriptor>();

            foreach (var pHolder in ctx.ProcessingHolder)
            {
                var rtProperty = pHolder.Value.RenderTextureProperty;
                if (rtProperty is null) { continue; }
                if (ctx.RenderTextures.TryGetValue(rtProperty, out var rt) is false) { continue; }
                resultTextures[pHolder.Key] = rt;

                if (pHolder.Value.RTOwned)
                {
                    resultTextureDescriptor[rt] = pHolder.Value.TextureDescriptor;
                }
            }

            foreach (var unusedRt in ctx.NewRenderTextures.Where(r => resultTextureDescriptor.ContainsKey(r) is false))
                unusedRt.Dispose();

            return new(resultTextures, resultTextureDescriptor);
        }

    }

}
