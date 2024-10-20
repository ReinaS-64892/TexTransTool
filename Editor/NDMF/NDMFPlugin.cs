using nadena.dev.ndmf;
using net.rs64.TexTransTool.NDMF;
using net.rs64.TexTransTool.Build;
using UnityEngine;
using System.Collections.Generic;
using nadena.dev.ndmf.preview;
using System;
using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.NDMF.AAO;

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
            .Run(PreviewCancelerPass.Instance);


            InPhase(BuildPhase.Transforming)
            .BeforePlugin("io.github.azukimochi.light-limit-changer")
#if CONTAINS_AAO
            .Run(NegotiateAAOPass.Instance).Then
#endif

            .Run(BeforeUVModificationPass.Instance).PreviewingWith(new TexTransDomainFilter(TexTransPhase.BeforeUVModification)).Then

            .Run(UVModificationPass.Instance).PreviewingWith(new TexTransDomainFilter(TexTransPhase.UVModification)).Then
            .Run(AfterUVModificationPass.Instance).PreviewingWith(new TexTransDomainFilter(TexTransPhase.AfterUVModification)).Then
            .Run(UnDefinedPass.Instance).PreviewingWith(new TexTransDomainFilter(TexTransPhase.UnDefined));


            InPhase(BuildPhase.Optimizing)
            .BeforePlugin("com.anatawa12.avatar-optimizer")

            .Run(ReFindRenderersPass.Instance).Then

            .Run(OptimizingPass.Instance).Then
            .Run(TTTSessionEndPass.Instance).PreviewingWith(new TexTransDomainFilter(TexTransPhase.Optimizing)).Then

            .Run(TTTComponentPurgePass.Instance);
        }
        internal static Dictionary<TexTransPhase, TogglablePreviewNode> s_togglablePreviewPhases = new() {
            { TexTransPhase.BeforeUVModification,  TogglablePreviewNode.Create(() => "BeforeUVModification-Phase", "BeforeUVModification", true) },
            { TexTransPhase.UVModification,  TogglablePreviewNode.Create(() => "UVModification-Phase", "UVModification",  true) },
            { TexTransPhase.AfterUVModification,  TogglablePreviewNode.Create(() => "AfterUVModification-Phase", "AfterUVModification",  true) },
            { TexTransPhase.UnDefined,  TogglablePreviewNode.Create(() => "UnDefined-Phase", "UnDefined",  true) },
            { TexTransPhase.Optimizing,  TogglablePreviewNode.Create(() => "Optimizing-Phase", "Optimizing", false) },
        };
    }

}
