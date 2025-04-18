using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Build;
using UnityEngine;

namespace net.rs64.TexTransTool.Preview.Custom

{
    [TTTCustomPreview(typeof(PhaseDefinition))]
    internal class TexTransGroupPreview : ITTTCustomPreview
    {
        public void Preview(TexTransMonoBase texTransBehavior, GameObject domainRoot, UnityAnimationPreviewDomain domain)
        {
            if (texTransBehavior is not PhaseDefinition _) { return; }

            var list = new List<TexTransBehavior>();
            AvatarBuildUtils.GroupedComponentsCorrect(list, texTransBehavior.gameObject, new AvatarBuildUtils.DefaultGameObjectWakingTool());

            foreach (var ttb in list)
                ttb.Apply(domain);

            domain.MergeStack();
            domain.ReadBackToTexture2D();
        }
    }
}
