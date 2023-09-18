#if NDMF
using nadena.dev.ndmf;
using net.rs64.TexTransTool.Build.NDMF;

[assembly: ExportsPlugin(typeof(NDMFPlugin))]

namespace net.rs64.TexTransTool.Build.NDMF
{

    public class NDMFPlugin : Plugin<NDMFPlugin>
    {
        public override string QualifiedName => "net.rs64.tex-trans-tool";
        public override string DisplayName => "TexTransTool";
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming).Run("Build TexTransTool", ctx =>
            {
                AvatarBuildUtils.ProcessAvatar(ctx.AvatarRootObject, ctx.AssetContainer, false);
            });
        }
    }
}
#endif