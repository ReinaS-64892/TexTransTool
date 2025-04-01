using System.Linq;
using net.rs64.TexTransTool.Build;
using UnityEngine;

namespace net.rs64.TexTransTool.Preview.Custom
{
    [TTTCustomPreview(typeof(PreviewGroup))]
    internal class PreviewGroupPreview : ITTTCustomPreview
    {
        public void Preview(TexTransMonoBase texTransBehavior, GameObject domainRoot, UnityAnimationPreviewDomain domain)
        {
            if (texTransBehavior is not PreviewGroup previewGroup) { return; }

            var phaseOnTf = AvatarBuildUtils.FindAtPhase(previewGroup.gameObject);

            foreach (var phase in TexTransPhaseUtility.EnumerateAllPhase())
            {
                foreach (var ttb in phaseOnTf[phase].Where(i => i.PhaseDefine == phase).Where(b => AvatarBuildUtils.CheckIsActiveBehavior(b, domainRoot)))
                {
                    ttb.Apply(domain);
                }
                domain.MergeStack();
                domain.ReadBackToTexture2D();
            }
        }


    }
}
