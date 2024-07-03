using nadena.dev.ndmf;
using net.rs64.TexTransTool.NDMF;
using net.rs64.TexTransTool.Build;
using UnityEngine;
using System.Collections.Generic;

[assembly: ExportsPlugin(typeof(NDMFPlugin))]

namespace net.rs64.TexTransTool.NDMF
{

    internal class NDMFPlugin : Plugin<NDMFPlugin>
    {
        public override string QualifiedName => "net.rs64.tex-trans-tool";
        public override string DisplayName => "TexTransTool";
        public override Texture2D LogoTexture => TTTImageAssets.Logo;
        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving)

            .Run(PreviewCancelerPass.Instance).Then
            .Run(ResolvingPass.Instance);


            InPhase(BuildPhase.Transforming)
            .BeforePlugin("io.github.azukimochi.light-limit-changer")

            .Run(BeforeUVModificationPass.Instance).Then

            .Run(MidwayMergeStackPass.Instance)
#if NDMF_1_5_x
            .PreviewingWith(new TexTransDomainFilter(new List<TexTransPhase>() { TexTransPhase.BeforeUVModification }))
#endif
            .Then

            .Run(UVModificationPass.Instance).Then
            .Run(AfterUVModificationPass.Instance).Then
            .Run(UnDefinedPass.Instance).Then
            .Run(BeforeOptimizingMergeStackPass.Instance)
#if NDMF_1_5_x
            .PreviewingWith(new TexTransDomainFilter(new List<TexTransPhase>() { TexTransPhase.UVModification, TexTransPhase.AfterUVModification, TexTransPhase.UnDefined }))
#endif
            ;


            InPhase(BuildPhase.Optimizing)
            .BeforePlugin("com.anatawa12.avatar-optimizer")

            .Run(ReFindRenderersPass.Instance).Then

            .Run(OptimizingPass.Instance).Then
            .Run(TTTSessionEndPass.Instance)
#if NDMF_1_5_x
            .PreviewingWith(new TexTransDomainFilter(new List<TexTransPhase>() { TexTransPhase.Optimizing }))
#endif
            .Then

            .Run(TTTComponentPurgePass.Instance);


        }
    }

}
