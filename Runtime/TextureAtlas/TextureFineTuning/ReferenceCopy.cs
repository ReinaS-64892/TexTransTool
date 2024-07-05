using System;
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class ReferenceCopy : ITextureFineTuning
    {

        public PropertyName SourcePropertyName = PropertyName.DefaultValue;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;

        public ReferenceCopy() { }
        public ReferenceCopy(PropertyName sourcePropertyName, PropertyName targetPropertyName)
        {
            SourcePropertyName = sourcePropertyName;
            TargetPropertyName = targetPropertyName;
        }

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            if (!texFineTuningTargets.TryGetValue(TargetPropertyName, out var copyTargetTextureHolder)) { return; }
            if (copyTargetTextureHolder == null)
            {
                copyTargetTextureHolder = new TexFineTuningHolder(null);
                texFineTuningTargets.Add(TargetPropertyName, copyTargetTextureHolder);
            }

            copyTargetTextureHolder.Get<ReferenceCopyData>().CopySource = SourcePropertyName;

        }
    }

    internal class ReferenceCopyData : ITuningData
    {
        public string CopySource;
    }

    internal class ReferenceCopyApplicant : ITuningApplicant
    {

        public int Order => 32;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
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
