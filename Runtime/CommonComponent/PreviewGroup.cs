using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class PreviewGroup : TexTransAnnotation
    {
        internal const string ComponentName = "TTT PreviewGroup";
        internal const string MenuPath = PhaseDefinition.FoldoutName + "/" + ComponentName;

        /*
        これは、プレビューをトリガーするためだけの物、後順序とかをルートにつけておけば視覚的にわかるようにするための物でもある。
        */
    }
}
