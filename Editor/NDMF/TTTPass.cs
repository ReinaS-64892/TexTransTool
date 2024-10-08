using System.Linq;
using nadena.dev.ndmf;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Editor.OtherMenuItem;
using static net.rs64.TexTransTool.Build.AvatarBuildUtils;

namespace net.rs64.TexTransTool.NDMF
{
    abstract class TTTPass<T> : Pass<T> where T : Pass<T>, new()
    {
        protected TexTransBuildSession TTTContext(BuildContext context)
        {
            return context.GetState(b => new TexTransBuildSession(new NDMFDomain(b), FindAtPhase(context.AvatarRootObject)));
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
    internal class TexTransBehaviorInsideNestedNonGroupComponentIsDeprecatedWarning : Pass<TexTransBehaviorInsideNestedNonGroupComponentIsDeprecatedWarning>
    {
        protected override void Execute(BuildContext context)
        {
            var warningTarget = context.AvatarRootObject.GetComponentsInChildren<TexTransBehavior>()
               .Where(ttb =>
               {
                   var parent = ttb.transform.parent;
                   if (parent == null) { return false; }
                   return parent.GetComponentsInParent<TexTransBehavior>().Any(pttb =>
                          {
                              switch (pttb)
                              {
                                  case PhaseDefinition:
                                  case TexTransGroup:
                                  case PreviewGroup:
                                      return false;
                                  default:
                                      return true;
                              }
                          });
               }).ToArray();

            if (warningTarget.Any() is false) { return; }

            TTTLog.Warning("Common:error:TexTransBehaviorInsideNestedNonGroupComponentIsDeprecatedWarning", warningTarget);
        }
    }
    internal class BeforeUVModificationPass : TTTPass<BeforeUVModificationPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).ApplyFor(TexTransPhase.BeforeUVModification);
        }
    }
    internal class MidwayMergeStackPass : TTTPass<MidwayMergeStackPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).MidwayMergeStack();
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
    internal class UnDefinedPass : TTTPass<UnDefinedPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).ApplyFor(TexTransPhase.UnDefined);
        }
    }
    internal class BeforeOptimizingMergeStackPass : TTTPass<BeforeOptimizingMergeStackPass>
    {
        protected override void Execute(BuildContext context)
        {
            TTTContext(context).MidwayMergeStack();
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
