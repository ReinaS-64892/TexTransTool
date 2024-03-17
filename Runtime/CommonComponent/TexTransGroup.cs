using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class TexTransGroup : TexTransCallEditorBehavior
    {
        internal const string FoldoutName = "Group";
        internal const string ComponentName = "TTT TexTransGroup";
        internal const string MenuPath = TexTransGroup.FoldoutName + "/" + ComponentName;

        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;
        internal IEnumerable<TexTransBehavior> Targets => GetChildeComponent<TexTransBehavior>(transform);

        internal override List<Renderer> GetRenderers => TextureTransformerFilter(Targets).SelectMany(I => I.GetRenderers).ToList();

        internal override bool IsPossibleApply => PossibleApplyCheck();

        internal static IEnumerable<TexTransBehavior> TextureTransformerFilter(IEnumerable<TexTransBehavior> targets) => targets.Where(tf => tf != null && tf.ThisEnable);

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

        internal static IEnumerable<Behavior> GetChildeComponent<Behavior>(Transform transform)
        {
            return transform.GetChildren().Select(x => x.GetComponent<Behavior>()).Where(x => x != null);
        }

    }
}
