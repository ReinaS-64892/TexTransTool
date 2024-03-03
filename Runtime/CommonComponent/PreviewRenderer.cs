using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/PreviewUtility/TTT PreviewRenderer")]
    [RequireComponent(typeof(Renderer))]
    public sealed class PreviewRenderer : TexTransCallEditorBehavior
    {
        internal override List<Renderer> GetRenderers => null;
        internal override bool IsPossibleApply => true;
        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        /*
        そのレンダラーを対象とする者をプレビューできる仕組み
        */
    }
}
