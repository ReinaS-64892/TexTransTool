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
                if (!texFineTuningTargets.TryGetValue(tpn, out var copyTargetTextureHolder)) { continue; }
                if (copyTargetTextureHolder == null)
                {
                    copyTargetTextureHolder = new TexFineTuningHolder(null);
                    texFineTuningTargets.Add(tpn, copyTargetTextureHolder);
                }

                copyTargetTextureHolder.Get<ReferenceCopyData>().CopySource = SourcePropertyName;
            }
        }
    }

    internal class ReferenceCopyData : ITuningData
    {
        public string CopySource;
    }

    internal class ReferenceCopyApplicant : ITuningApplicant
    {

        public int Order => 32;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets, IDeferTextureCompress compress)
        {
            foreach (var texKv in texFineTuningTargets.ToArray())
            {
                var referenceCopyData = texKv.Value.Find<ReferenceCopyData>();
                if (referenceCopyData == null) { continue; }

                if (texFineTuningTargets.TryGetValue(referenceCopyData.CopySource, out var sourceTextureHolder))
                {
                    texKv.Value.Texture2D = sourceTextureHolder.Texture2D;
                }
            }
        }
    }

}
