using System;
using System.Collections.Generic;
using net.rs64.TexTransUnityCore.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    [AddTypeMenu("(Experimental) Merge Texture")]
    public class MergeTexture : ITextureFineTuning
    {
        public PropertyName MargeParent;
        public List<PropertyName> MargeChildren;
        public MergeTexture() { }
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
        public string MargeParent;
    }

    internal class MergeTextureApplicant : ITuningApplicant
    {
        public int Order => 30;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets, IDeferTextureCompress compress)
        {
            var mergeDict = new Dictionary<string, List<string>>();
            foreach (var kv in texFineTuningTargets)
            {
                var mtData = kv.Value.Find<MergeTextureData>();
                if (mtData is null) { continue; }

                if (mergeDict.ContainsKey(mtData.MargeParent) is false) { mergeDict[mtData.MargeParent] = new(); }
                mergeDict[mtData.MargeParent].Add(kv.Key);
            }

            foreach (var ftMarge in mergeDict)
            {
                if (texFineTuningTargets.ContainsKey(ftMarge.Key) is false) { continue; }
                var parentFTHolder = texFineTuningTargets[ftMarge.Key];

                using (var taxNa = new NativeArray<Color32>(parentFTHolder.OriginTexture2D.GetPixelData<Color32>(0), Allocator.Temp))
                {
                    var writeSpan = taxNa.AsSpan();
                    foreach (var c in ftMarge.Value)
                    {
                        var tfHolder = texFineTuningTargets[c];
                        var cNa = tfHolder.OriginTexture2D.GetPixelData<Color32>(0);
                        MergePixels(writeSpan, cNa);
                    }


                    var mergedTex2D = new Texture2D(parentFTHolder.OriginTexture2D.width, parentFTHolder.OriginTexture2D.height, parentFTHolder.OriginTexture2D.format, parentFTHolder.Texture2D.mipmapCount > 1, !parentFTHolder.Texture2D.isDataSRGB);
                    mergedTex2D.SetPixelData(taxNa, 0);
                    mergedTex2D.Apply();
                    if (parentFTHolder.Texture2D.width != parentFTHolder.OriginTexture2D.width)
                    {
                        var resized = TextureUtility.ResizeTexture(mergedTex2D, new Vector2Int(parentFTHolder.Texture2D.width, parentFTHolder.Texture2D.height), true);
                        UnityEngine.Object.DestroyImmediate(mergedTex2D);
                        mergedTex2D = resized;
                    }
                    mergedTex2D.name = $"Merged-AtRoot-{ftMarge.Key}-Children-{string.Join("and", ftMarge.Value)}";
                    parentFTHolder.Texture2D = mergedTex2D;

                    foreach (var c in ftMarge.Value)
                    {
                        var tfHolder = texFineTuningTargets[c];
                        tfHolder.Texture2D = parentFTHolder.Texture2D;
                    }

                    var compressSetting = parentFTHolder.Find<TextureCompressionData>();
                    if (compressSetting is not null) compress.DeferredTextureCompress(compressSetting, parentFTHolder.Texture2D);
                }
            }

        }

        public static void MergePixels(Span<Color32> write, Span<Color32> c)
        {
            if (write.Length != c.Length) { throw new ArgumentException("is write.Length != c.Length"); }
            for (var index = 0; write.Length > index; index += 1)
            {
                if (write[index].a <= c[index].a)
                {
                    write[index] = c[index];
                }
            }
        }
    }
}
