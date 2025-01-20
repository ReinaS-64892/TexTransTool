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
            }
            else
            {
                foreach (var ttb in list.Where(i => i.PhaseDefine == TexTransPhase.BeforeUVModification).Where(b => AvatarBuildUtils.CheckIsActiveBehavior(b, domainRoot))) { ttb.Apply(domain); }
                domain.MergeStack();
                foreach (var ttb in list.Where(i => i.PhaseDefine == TexTransPhase.UVModification).Where(b => AvatarBuildUtils.CheckIsActiveBehavior(b, domainRoot))) { ttb.Apply(domain); }
                domain.MergeStack();
                foreach (var ttb in list.Where(i => i.PhaseDefine == TexTransPhase.AfterUVModification).Where(b => AvatarBuildUtils.CheckIsActiveBehavior(b, domainRoot))) { ttb.Apply(domain); }
                domain.MergeStack();
                foreach (var ttb in list.Where(i => i.PhaseDefine == TexTransPhase.UnDefined).Where(b => AvatarBuildUtils.CheckIsActiveBehavior(b, domainRoot))) { ttb.Apply(domain); }
                domain.MergeStack();
                foreach (var ttb in list.Where(i => i.PhaseDefine == TexTransPhase.Optimizing).Where(b => AvatarBuildUtils.CheckIsActiveBehavior(b, domainRoot))) { ttb.Apply(domain); }
            }
        }
    }
}
