using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool.Preview.Custom

{
    [TTTCustomPreview(typeof(TexTransGroup))]
    internal class TexTransGroupPreview : ITTTCustomPreview
    {
        public void Preview(TexTransMonoBase texTransBehavior, RenderersDomain domain)
        {
            if (texTransBehavior is not TexTransGroup texTransGroup) { return; }


            var list = new List<TexTransBehavior>();
            AvatarBuildUtils.FindTreedBehavior(list, texTransBehavior.gameObject);

            if (texTransBehavior is PhaseDefinition)
            {
                foreach (var ttb in list) { ttb.Apply(domain); }
            }
            else
            {
                foreach (var ttb in list.Where(i => i.PhaseDefine == TexTransPhase.BeforeUVModification).Where(AvatarBuildUtils.CheckIsActiveBehavior)) { ttb.Apply(domain); }
                domain.MergeStack();
                foreach (var ttb in list.Where(i => i.PhaseDefine == TexTransPhase.UVModification).Where(AvatarBuildUtils.CheckIsActiveBehavior)) { ttb.Apply(domain); }
                domain.MergeStack();
                foreach (var ttb in list.Where(i => i.PhaseDefine == TexTransPhase.AfterUVModification).Where(AvatarBuildUtils.CheckIsActiveBehavior)) { ttb.Apply(domain); }
                domain.MergeStack();
                foreach (var ttb in list.Where(i => i.PhaseDefine == TexTransPhase.UnDefined).Where(AvatarBuildUtils.CheckIsActiveBehavior)) { ttb.Apply(domain); }
                domain.MergeStack();
                foreach (var ttb in list.Where(i => i.PhaseDefine == TexTransPhase.Optimizing).Where(AvatarBuildUtils.CheckIsActiveBehavior)) { ttb.Apply(domain); }
            }
        }
    }
}
