using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    public class TexFineTuningTarget
    {
        public string PropertyName;
        public Texture2D Texture2D;
        public List<ITuningData> TuningDataList;


        internal TexFineTuningTarget(PropAndTexture2D propAndTexture2D)
        {
            PropertyName = propAndTexture2D.PropertyName;
            Texture2D = propAndTexture2D.Texture2D;
            TuningDataList = new List<ITuningData>();
        }
    }

    internal class TexFineTuningUtility
    {

        public static void InitTexFineTuning(List<TexFineTuningTarget> texFineTuningTargets)
        {
            foreach (var texf in texFineTuningTargets)
            {
                texf.TuningDataList.Add(new MipMapData());
                texf.TuningDataList.Add(new CompressionQualityData());
            }
        }
        public static void FinalizeTexFineTuning(List<TexFineTuningTarget> texFineTuningTargets)
        {
            var applicantList = InterfaceUtility.GetInterfaceInstance<ITuningApplicant>().ToList();
            applicantList.Sort((L, R) => L.Order - R.Order);
            foreach (var applicant in applicantList)
            {
                applicant.ApplyTuning(texFineTuningTargets);
            }
        }


        public static List<TexFineTuningTarget> ConvertForTargets(Dictionary<string, Texture2D> propAndTexture2Ds)
        {
            var targets = new List<TexFineTuningTarget>(propAndTexture2Ds.Count);
            foreach (var pat in propAndTexture2Ds)
            {
                targets.Add(new TexFineTuningTarget(new(pat.Key, pat.Value)));
            }
            return targets;
        }
        public static List<PropAndTexture2D> ConvertForPropAndTexture2D(List<TexFineTuningTarget> texFineTuningTargets)
        {
            var targets = new List<PropAndTexture2D>(texFineTuningTargets.Capacity);
            foreach (var texftt in texFineTuningTargets)
            {
                targets.Add(new PropAndTexture2D(texftt.PropertyName, texftt.Texture2D));
            }
            return targets;
        }
    }

}
