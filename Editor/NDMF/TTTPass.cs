using System.Linq;
using nadena.dev.ndmf;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Editor.OtherMenuItem;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Profiling;
using static net.rs64.TexTransTool.Build.AvatarBuildUtils;

namespace net.rs64.TexTransTool.NDMF
{
    abstract class TTTPass<T> : Pass<T> where T : Pass<T>, new()
    {
        protected TexTransBuildSession TTTContext(BuildContext context)
        {
            return context.GetState(b =>
            {
                using var pf = new PFScope("FindAtPhase");

                var p2b = FindAtPhase(context.AvatarRootObject);

                pf.Split("TexTransBuildSession.ctr");
// #if CONTAINS_TTCE_WGPU
//                 var wgpuCtx = TTCEWgpuDeviceWithTTT4UnityHolder.Device().GetTTCEWgpuContext();
//                 var bs = new TexTransBuildSession(context.AvatarRootObject, new NDMFDomain(b, null, wgpuCtx), p2b);
// #else
                var bs = new TexTransBuildSession(context.AvatarRootObject, new NDMFDomain(b), p2b);
// #endif
                return bs;
            });
        }
    }
    internal class PreviewCancelerPass : Pass<PreviewCancelerPass>
    {
        protected override void Execute(BuildContext context)
        {
            if (!PreviewUtility.IsPreviewContains) { return; }
            PreviewUtility.ExitPreviews();
            TTTLog.Error("Common:error:BuildWasRunDuringPreviewing");
            throw new TTTNotExecutable();
        }
    }
    internal class MaterialModificationPass : TTTPass<MaterialModificationPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).ApplyFor(TexTransPhase.MaterialModification);
        }
    }
    internal class BeforeUVModificationPass : TTTPass<BeforeUVModificationPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).ApplyFor(TexTransPhase.BeforeUVModification);
        }
    }
    internal class UVModificationPass : TTTPass<UVModificationPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).ApplyFor(TexTransPhase.UVModification);
        }
    }
    internal class AfterUVModificationPass : TTTPass<AfterUVModificationPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).ApplyFor(TexTransPhase.AfterUVModification);
        }
    }
    internal class PostProcessingPass : TTTPass<PostProcessingPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).ApplyFor(TexTransPhase.PostProcessing);
        }
    }
    internal class UnDefinedPass : TTTPass<UnDefinedPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).ApplyFor(TexTransPhase.UnDefined);
        }
    }
    internal class ReFindRenderersPass : TTTPass<ReFindRenderersPass>
    {
        protected override void Execute(BuildContext context)
        {
            if (TTTContext(context).Domain is NDMFDomain domain) { domain.ReFindRenderers(); }
        }
    }
    internal class OptimizingPass : TTTPass<OptimizingPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).ApplyFor(TexTransPhase.Optimizing);
        }
    }
    internal class TTTSessionEndPass : TTTPass<TTTSessionEndPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).TTTSessionEnd();
        }
    }
    internal class TTTComponentPurgePass : TTTPass<TTTComponentPurgePass>
    {
        protected override void Execute(BuildContext context)
        {
            DestroyITexTransToolTags(context.AvatarRootObject);
        }
    }
}
