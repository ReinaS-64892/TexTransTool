#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class ReferenceCopy : ITextureFineTuning
    {

        public PropertyName SourcePropertyName = PropertyName.DefaultValue;
        [Obsolete("V4SaveData", true)] public PropertyName TargetPropertyName = PropertyName.DefaultValue;
        public List<PropertyName> TargetPropertyNameList = new() { PropertyName.DefaultValue };


        public ReferenceCopy() { }
        public ReferenceCopy(PropertyName sourcePropertyName, List<PropertyName> targetPropertyNameList)
        {
            SourcePropertyName = sourcePropertyName;
            TargetPropertyNameList = targetPropertyNameList;
        }

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var tpn in TargetPropertyNameList)
            {
                if (string.IsNullOrWhiteSpace(tpn)) { continue; }
                // コピーする先がない場合は新しく作る
                texFineTuningTargets.TryGetValue(tpn, out var copyTargetTextureHolder);
                if (copyTargetTextureHolder == null)
                {
                    copyTargetTextureHolder = new();
                    texFineTuningTargets.Add(tpn, copyTargetTextureHolder);
                }

                copyTargetTextureHolder.Get<ReferenceCopyData>().CopySource = SourcePropertyName;
            }
        }
    }

    internal class ReferenceCopyData : ITuningData
    {
        public string? CopySource;
    }

    internal class ReferenceCopyApplicant : ITuningProcessor
    {

        public int Order => 32;

        public void ProcessingTuning(TexFineTuningProcessingContext ctx)
        {
            foreach (var tuning in ctx.TuningHolder)
            {
                var tuningHolder = tuning.Value;
                var referenceCopyData = tuningHolder.Find<ReferenceCopyData>();
                if (referenceCopyData == null) { continue; }

                var thisTuning = ctx.ProcessingHolder[tuning.Key];

                //  Copy する元がない場合は何もしない
                if (referenceCopyData.CopySource is null || string.IsNullOrWhiteSpace(referenceCopyData.CopySource)) { continue; }
                if (ctx.ProcessingHolder.TryGetValue(referenceCopyData.CopySource, out var sourceTextureHolder))
                {
                    if (thisTuning.RenderTextureProperty == sourceTextureHolder.RenderTextureProperty) { continue; }// 同一だった場合何もできないから何もしない

                    thisTuning.RenderTextureProperty = sourceTextureHolder.RenderTextureProperty;
                    thisTuning.RTOwned = false;
                }
            }
        }
    }

}
