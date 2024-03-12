using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    [RequireComponent(typeof(Renderer))]
    public sealed class PreviewRenderer : TexTransCallEditorBehavior
    {
        internal const string ComponentName = "TTT PreviewRenderer";
        internal const string MenuPath = PreviewGroup.FoldoutName + "/" + ComponentName;

        internal override List<Renderer> GetRenderers => null;
        internal override bool IsPossibleApply => true;
        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        /*
        そのレンダラーを対象とする者をプレビューできる仕組み
        */
    }
}
