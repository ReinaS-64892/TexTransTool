#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public abstract class TexTransBehavior : TexTransMonoBaseGameObjectOwned
    {
        internal bool ThisEnable => gameObject.activeInHierarchy;
        internal abstract TexTransPhase PhaseDefine { get; }
        internal const string TTTName = "TexTransTool";

        /// <summary>
        /// Applies this TextureTransformer with that domain
        /// You MUST NOT modify state of this component.
        /// </summary>
        /// <param name="domain">The domain</param>
        internal abstract void Apply(IDomain domain);
        internal abstract IEnumerable<Renderer> TargetRenderers(IDomainReferenceViewer rendererTargeting);
    }
    internal interface IDomainReferenceModifier
    {
        void RegisterDomainReference(IDomainReferenceViewer domainReferenceViewer, IDomainReferenceRegistry registry);
    }
    internal interface IDomainReferenceRegistry
    {
        void RegisterAddMaterials(Renderer renderer, IEnumerable<Material> material);
        void RegisterAddTextures(Material material, IEnumerable<Texture> texture);
    }
    public enum TexTransPhase
    {
        UVDisassembly = 7,
        MaterialModification = 5,

        BeforeUVModification = 1,
        UVModification = 2,
        AfterUVModification = 3,

        PostProcessing = 6,

        UnDefined = 0,


        Optimizing = 4,
    }

    internal static class TexTransPhaseUtility
    {
        public static readonly TexTransPhase[] TexTransPhases = new TexTransPhase[]{
            TexTransPhase.UVDisassembly,
            TexTransPhase.MaterialModification,
            TexTransPhase.BeforeUVModification,
            TexTransPhase.UVModification,
            TexTransPhase.AfterUVModification,
            TexTransPhase.PostProcessing,
            TexTransPhase.UnDefined,

            TexTransPhase.Optimizing,
        };
        public static IEnumerable<TexTransPhase> EnumerateAllPhase()
        {
            return TexTransPhases;
        }
        public static Dictionary<TexTransPhase, TValue> GeneratePhaseDictionary<TValue>()
        where TValue : new()
        {
            var dict = new Dictionary<TexTransPhase, TValue>();
            foreach (var phase in EnumerateAllPhase()) { dict[phase] = new(); }
            return dict;
        }
    }
}
