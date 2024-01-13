using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace net.rs64.TexTransTool.MatAndTexUtils
{
    [AddComponentMenu("TexTransTool/MatAndTexUtils/TTT MatAndTexRelativeSeparator")]
    public class MatAndTexRelativeSeparator : TexTransCallEditorBehavior, IMatAndTexSeparator
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public bool MultiRendererMode = false;
        internal override List<Renderer> GetRenderers => TargetRenderers;
        internal override bool IsPossibleApply => SeparateTarget.Any();
        internal List<MatSlotBool> SeparateTarget = new List<MatSlotBool>();
        public bool IsTextureSeparate;
        public PropertyName PropertyName = PropertyName.DefaultValue;

        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        bool IMatAndTexSeparator.IsTextureSeparate => IsTextureSeparate;
        PropertyName IMatAndTexSeparator.PropertyName => PropertyName;

        List<bool> IMatAndTexSeparator.GetSeparateTarget(IDomain domain, int RendererIndex)
        {
            return SeparateTarget[RendererIndex].BoolList;
        }
    }
    [Serializable]
    internal class MatSlotBool
    {
        public List<bool> BoolList;

        public MatSlotBool(List<bool> boolList)
        {
            BoolList = boolList;
        }

    }
}
