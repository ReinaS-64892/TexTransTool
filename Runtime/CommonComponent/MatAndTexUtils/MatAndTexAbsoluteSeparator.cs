using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using net.rs64.TexTransCore.TransTextureCore.Utils;

namespace net.rs64.TexTransTool.MatAndTexUtils
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class MatAndTexAbsoluteSeparator : TexTransCallEditorBehavior, IMatAndTexSeparator
    {
        internal const string FoldoutName = "MatAndTexUtils";
        internal const string ComponentName = "TTT MatAndTexAbsoluteSeparator";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
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

            var hashSet = new HashSet<Material>(renderer.sharedMaterials);
            var targetMats = hashSet.Where(i => SeparateTarget.Any(m => domain.OriginEqual(m, i))).ToList();

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
