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
        internal static IEnumerable<TexTransBehavior> TextureTransformerFilter(IEnumerable<TexTransBehavior> targets) => targets.Where(tf => tf != null && tf.ThisEnable);
        internal static IEnumerable<Behavior> GetChildeComponent<Behavior>(Transform transform)
        {
            return transform.GetChildren().Select(x => x.GetComponent<Behavior>()).Where(x => x != null);
        }

    }
}
