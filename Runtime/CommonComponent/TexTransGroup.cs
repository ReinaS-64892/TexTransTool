using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/Group/TTT TexTransGroup")]
    internal class TexTransGroup : TexTransCallEditorBehavior
    {
        public override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;
        public IEnumerable<TexTransBehavior> Targets => transform.GetChildren().Select(x => x.GetComponent<TexTransBehavior>()).Where(x => x != null);

        public override List<Renderer> GetRenderers => TextureTransformerFilter(Targets).SelectMany(I => I.GetRenderers).ToList();

        public override bool IsPossibleApply => PossibleApplyCheck();

        public static IEnumerable<TexTransBehavior> TextureTransformerFilter(IEnumerable<TexTransBehavior> targets) => targets.Where(tf => tf != null && tf.ThisEnable);

        bool PossibleApplyCheck()
        {
            bool possibleFlag = true;
            foreach (var tf in Targets)
            {
                if (tf == null) continue;
                if (tf.ThisEnable == false) continue;
                if (!tf.IsPossibleApply)
                {
                    possibleFlag = false;
                }
            }
            return possibleFlag;
        }

    }
}
