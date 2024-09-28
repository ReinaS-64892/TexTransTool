using nadena.dev.ndmf;
using net.rs64.TexTransTool.NDMF;
using net.rs64.TexTransTool.Build;
using UnityEngine;
using System.Collections.Generic;
using nadena.dev.ndmf.preview;
using System;
using System.Linq;
using net.rs64.TexTransCore;
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
            .BeforePlugin("net.narazaka.vrchat.floor_adjuster")

            .Run(BeforeUVModificationPass.Instance).Then
            .Run(TexTransBehaviorInsideNestedNonGroupComponentIsDeprecatedWarning.Instance).Then

            .Run(MidwayMergeStackPass.Instance)
            .PreviewingWith(new TexTransDomainFilter(new List<TexTransPhase>() { TexTransPhase.BeforeUVModification }))
            .Then

            .Run(UVModificationPass.Instance).Then
            .Run(AfterUVModificationPass.Instance).Then
            .Run(UnDefinedPass.Instance).Then
            .Run(BeforeOptimizingMergeStackPass.Instance)
            .PreviewingWith(new TexTransDomainFilter(new List<TexTransPhase>() { TexTransPhase.UVModification, TexTransPhase.AfterUVModification, TexTransPhase.UnDefined }));


            InPhase(BuildPhase.Optimizing)
            .BeforePlugin("com.anatawa12.avatar-optimizer")

            .Run(ReFindRenderersPass.Instance).Then

#if CONTAINS_AAO
            .Run(ProvideMeshRemovalToIsland.Instance).Then
#endif
            .Run(OptimizingPass.Instance).Then
            .Run(TTTSessionEndPass.Instance)
            .PreviewingWith(new TexTransDomainFilter(new List<TexTransPhase>() { TexTransPhase.Optimizing }))
            .Then

            .Run(TTTComponentPurgePass.Instance);


        }
        internal static Dictionary<TexTransPhase, TogglablePreviewNode> s_togglablePreviewPhases = new() {
            { TexTransPhase.BeforeUVModification,  TogglablePreviewNode.Create(() => "BeforeUVModification-Phase", "BeforeUVModification", true) },
            { TexTransPhase.UVModification,  TogglablePreviewNode.Create(() => "UVModification-to-UnDefined-Phase", "UVModificationToUnDefined",  true) },
            { TexTransPhase.Optimizing,  TogglablePreviewNode.Create(() => "Optimizing-Phase", "Optimizing", false) },
        };
    }

}
