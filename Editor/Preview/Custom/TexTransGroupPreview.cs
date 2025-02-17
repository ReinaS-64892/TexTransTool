using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Build;
using UnityEngine;

namespace net.rs64.TexTransTool.Preview.Custom

{
    [TTTCustomPreview(typeof(TexTransGroup))]
    [TTTCustomPreview(typeof(PhaseDefinition))]
    internal class TexTransGroupPreview : ITTTCustomPreview
    {
        public void Preview(TexTransMonoBase texTransBehavior, GameObject domainRoot, RenderersDomain domain)
        {
            if (texTransBehavior is not TexTransGroup texTransGroup) { return; }


            var list = new List<TexTransBehavior>();
            AvatarBuildUtils.GroupedComponentsCorrect(list, texTransBehavior.gameObject, new AvatarBuildUtils.DefaultGameObjectWakingTool());

            if (texTransBehavior is PhaseDefinition)
            {
                foreach (var ttb in list) { ttb.Apply(domain); }
                domain.MergeStack();
            }
            else
            {
                foreach (var phase in TexTransPhaseUtility.EnumerateAllPhase())
                {
                    foreach (var ttb in list.Where(i => i.PhaseDefine == phase).Where(b => AvatarBuildUtils.CheckIsActiveBehavior(b, domainRoot))) { ttb.Apply(domain); }
                    domain.MergeStack();
                }
            }
        }
    }
}
