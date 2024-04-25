using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool.Preview.Custom

{
    [TTTCustomPreview(typeof(TexTransGroup))]
    internal class TexTransGroupPreview : ITTTCustomPreview
    {
        public void Preview(TexTransBehavior texTransBehavior, IEditorCallDomain editorCallDomain)
        {
            if (texTransBehavior is not TexTransGroup texTransGroup) { return; }
            static IEnumerable<TexTransBehavior> FinedTTGroupBehaviors(TexTransGroup texTransGroup) { return texTransGroup.Targets.Where(i => i is not PhaseDefinition).SelectMany(i => i is TexTransGroup ttg ? FinedTTGroupBehaviors(ttg) : new[] { i }); }

            var phaseOnTf = AvatarBuildUtils.FindAtPhase(texTransGroup.gameObject);
            AvatarBuildUtils.WhiteList(phaseOnTf, new(FinedTTGroupBehaviors(texTransGroup)));
            ExecutePhases(editorCallDomain, phaseOnTf);
        }

        internal static void ExecutePhases(IEditorCallDomain editorCallDomain, Dictionary<TexTransPhase, List<TexTransBehavior>> phaseOnTf)
        {
            foreach (var tf in TexTransGroup.TextureTransformerFilter(phaseOnTf[TexTransPhase.BeforeUVModification])) { tf.Apply(editorCallDomain); }
            if (editorCallDomain is RenderersDomain previewDomain) { previewDomain.MergeStack(); }
            foreach (var tf in TexTransGroup.TextureTransformerFilter(phaseOnTf[TexTransPhase.UVModification])) { tf.Apply(editorCallDomain); }
            foreach (var tf in TexTransGroup.TextureTransformerFilter(phaseOnTf[TexTransPhase.AfterUVModification])) { tf.Apply(editorCallDomain); }
            foreach (var tf in TexTransGroup.TextureTransformerFilter(phaseOnTf[TexTransPhase.UnDefined])) { tf.Apply(editorCallDomain); }
        }
    }
}
