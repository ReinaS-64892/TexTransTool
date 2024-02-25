using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/Group/TTT PreviewGroup")]
    public sealed class PreviewGroup : TexTransCallEditorBehavior
    {
        internal override List<Renderer> GetRenderers => null;
        internal override bool IsPossibleApply => true;
        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        /*
        これは、プレビューをトリガーするためだけの物、後順序とかをルートにつけておけば視覚的にわかるようにするための物でもある。
        */
    }
}
