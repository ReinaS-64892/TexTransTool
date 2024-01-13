using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace net.rs64.TexTransTool.MatAndTexUtils
{
    [AddComponentMenu("TexTransTool/MatAndTexUtils/TTT MatAndTexAbsoluteSeparator")]
    public class MatAndTexAbsoluteSeparator : TexTransCallEditorBehavior, IMatAndTexSeparator
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public bool MultiRendererMode = false;
        internal override List<Renderer> GetRenderers => TargetRenderers;

        internal override bool IsPossibleApply => SeparateTarget.Any();

        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        public List<Material> SeparateTarget = new List<Material>();
        public bool IsTextureSeparate;
        public PropertyName PropertyName = PropertyName.DefaultValue;

        bool IMatAndTexSeparator.IsTextureSeparate => IsTextureSeparate;
        PropertyName IMatAndTexSeparator.PropertyName => PropertyName;

        List<bool> IMatAndTexSeparator.GetSeparateTarget(IDomain domain, int RendererIndex)
        {
            var renderer = TargetRenderers[RendererIndex];

            var targetMats = SeparateTarget.Select(mat => domain.TryReplaceQuery(mat, out var rMat) ? (Material)rMat : mat).ToHashSet();

            return renderer.sharedMaterials.Select(m => targetMats.Contains(m)).ToList();
        }
    }

    internal interface IMatAndTexSeparator
    {
        bool IsTextureSeparate { get; }
        PropertyName PropertyName { get; }
        List<bool> GetSeparateTarget(IDomain domain, int RendererIndex);
    }
}
