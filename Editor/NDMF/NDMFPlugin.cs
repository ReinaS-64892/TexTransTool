using nadena.dev.ndmf;
using net.rs64.TexTransTool.NDMF;
using net.rs64.TexTransTool.Build;
using UnityEngine;
using System.Collections.Generic;
using nadena.dev.ndmf.preview;
using System;
using System.Linq;
using net.rs64.TexTransCore;

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

            .Run(BeforeUVModificationPass.Instance).Then

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

            .Run(OptimizingPass.Instance).Then
            .Run(TTTSessionEndPass.Instance)
            .PreviewingWith(new TexTransDomainFilter(new List<TexTransPhase>() { TexTransPhase.Optimizing }))
            .Then

            .Run(TTTComponentPurgePass.Instance);


        }
        internal static Dictionary<TexTransPhase, TogglablePreviewNode> s_togglablePreviewPhases = new() {
            { TexTransPhase.BeforeUVModification, new TogglablePreviewNode(() => "BeforeUVModification-Phase", "BeforeUVModification", new(true), true) },
            { TexTransPhase.UVModification, new TogglablePreviewNode(() => "UVModification-to-UnDefined-Phase", "UVModificationToUnDefined", new(true), true) },
            { TexTransPhase.Optimizing, new TogglablePreviewNode(() => "Optimizing-Phase", "Optimizing", new(true), true) },
        };
        internal static Dictionary<Type, TogglablePreviewNode> s_togglablePreviewTexTransBehaviors =
        TexTransInitialize.TexTransToolAssembly().SelectMany(a => a.GetTypes()).Where(t => t.IsAbstract is false)
            .Where(t => typeof(TexTransBehavior).IsAssignableFrom(t))
            .Where(t => typeof(TexTransGroup).IsAssignableFrom(t) is false)
            .Where(t => typeof(PreviewGroup).IsAssignableFrom(t) is false)
            .OrderBy(t => t.Name)
            .ToDictionary(t => t, t => new TogglablePreviewNode(() => t.Name + "-Component", t.Name, new(true), true));
        public override IEnumerable<TogglablePreviewNode> TogglablePreviewNodes => s_togglablePreviewPhases.Values.Concat(s_togglablePreviewTexTransBehaviors.Values);
    }

}
