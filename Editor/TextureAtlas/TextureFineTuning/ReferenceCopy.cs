#if UNITY_EDITOR
using System.Collections.Generic;

namespace net.rs64.TexTransTool.TextureAtlas.FineSetting
{
    internal class ReferenceCopy : IAddFineTuning
    {

        public string SourcePropertyName;
        public string TargetPropertyName;

        public ReferenceCopy(string sourcePropertyName, string targetPropertyName)
        {
            SourcePropertyName = sourcePropertyName;
            TargetPropertyName = targetPropertyName;
        }

        public void AddSetting(List<TexFineTuningTarget> propAndTextures)
        {
            var copyTargetTexture = propAndTextures.Find(x => x.PropertyName == TargetPropertyName);
            if (copyTargetTexture == null)
            {
                propAndTextures.Add(new(new(TargetPropertyName, null)) { TuningDataList = new() { new ReferenceCopyData(SourcePropertyName) } });
            }
            else
            {
                var referenceCopyData = copyTargetTexture.TuningDataList.Find(I => I is ReferenceCopyData) as ReferenceCopyData;
                if (referenceCopyData != null)
                {
                    referenceCopyData.CopySouse = SourcePropertyName;
                }
                else
                {
                    copyTargetTexture.TuningDataList.Add(new ReferenceCopyData(SourcePropertyName));
                }
            }

        }
    }

    internal class ReferenceCopyData : ITuningData
    {
        public string CopySouse;

        public ReferenceCopyData(string copySouse)
        {
            CopySouse = copySouse;
        }
    }

    internal class ReferenceCopyApplicant : ITuningApplicant
    {

        public int Order => 32;

        public void ApplyTuning(List<TexFineTuningTarget> texFineTuningTargets)
        {
            foreach (var texf in texFineTuningTargets)
            {
                var referenceCopyData = texf.TuningDataList.Find(I => I is ReferenceCopyData) as ReferenceCopyData;
                if (referenceCopyData == null) { continue; }

                var souse = texFineTuningTargets.Find(I => I.PropertyName == referenceCopyData.CopySouse);
                if (souse == null) { continue; }
                texf.Texture2D = souse.Texture2D;
            }
        }
    }

}
#endif
