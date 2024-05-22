using nadena.dev.ndmf;
using net.rs64.TexTransTool.Build.NDMF;
using net.rs64.TexTransTool.Build;
using UnityEngine;

[assembly: ExportsPlugin(typeof(NDMFPlugin))]

namespace net.rs64.TexTransTool.Build.NDMF
{

    internal class NDMFPlugin : Plugin<NDMFPlugin>
    {
        public override string QualifiedName => "net.rs64.tex-trans-tool";
        public override string DisplayName => "TexTransTool";
#if NDMF_1_3_x
        public override Texture2D LogoTexture => TTTImageAssets.Logo;
#endif
        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving)

            .Run(PreviewCancelerPass.Instance).Then
            .Run(ResolvingPass.Instance);


            InPhase(BuildPhase.Transforming)
            .BeforePlugin("io.github.azukimochi.light-limit-changer")

            .Run(FindAtPhasePass.Instance).Then
            .Run(BeforeUVModificationPass.Instance).Then

            .Run(MidwayMergeStackPass.Instance).Then

            .Run(UVModificationPass.Instance).Then
            .Run(AfterUVModificationPass.Instance).Then
            .Run(UnDefinedPass.Instance).Then

            .Run(BeforeOptimizingMergeStackPass.Instance);


            InPhase(BuildPhase.Optimizing)
            .BeforePlugin("com.anatawa12.avatar-optimizer")

            .Run(OptimizingPass.Instance).Then
            .Run(TTTSessionEndPass.Instance);


        }
    }

}
