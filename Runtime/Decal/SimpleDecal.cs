#nullable enable
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.IslandSelector;
using System;
using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine.Profiling;
using Unity.Collections;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransCore.TransTexture;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class SimpleDecal : TexTransRuntimeBehavior, ICanBehaveAsLayer , ITexTransToolStableComponent
    {
        internal const string ComponentName = "TTT SimpleDecal";
        internal const string MenuPath = ComponentName;
        public int StabilizeSaveDataVersion => TTTDataVersion_0_10_X;
        internal override TexTransPhase PhaseDefine => TexTransPhase.AfterUVModification;

        public DecalRendererSelector RendererSelector = new();

        [ExpandTexture2D] public Texture2D? DecalTexture;
        [BlendTypeKey] public string BlendTypeKey = ITexTransToolForUnity.BL_KEY_DEFAULT;

        public UnityEngine.Color Color = UnityEngine.Color.white;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;
        public float Padding = 5;
        public string DownScaleAlgorithm = ITexTransToolForUnity.DS_ALGORITHM_DEFAULT;
        public bool FixedAspect = true;
        [FormerlySerializedAs("SideChek")][FormerlySerializedAs("SideCulling")] public bool BackCulling = true;
        public AbstractIslandSelector? IslandSelector;


        #region V6SaveData
        [Obsolete("V6SaveData", true)][SerializeField] internal bool HighQualityPadding = false;
        [Obsolete("V6SaveData", true)][SerializeField] internal SimpleDecalExperimentalFeature? MigrationTemporaryExperimentalFeature;
        [Obsolete("V6SaveData", true)][SerializeField] internal MultiLayerImageCanvas? OverrideDecalTextureWithMultiLayerImageCanvas;
        [Obsolete("V6SaveData", true)][SerializeField] internal bool UseDepth;
        [Obsolete("V6SaveData", true)][SerializeField] internal bool DepthInvert;
        #endregion V6SaveData

        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);
            if (RendererSelector.IsTargetNotSet()) { TTLog.Info("SimpleDecal:info:TargetNotSet"); return; }
            var decalCompiledTextures = CompileDecal(domain);

            domain.LookAt(transform.GetParents().Append(transform));
            domain.LookAt(decalCompiledTextures.Keys);
            if (IslandSelector != null) { IslandSelector.LookAtCalling(domain); }
            domain.LookAtGetComponent<SimpleDecalExperimentalFeature>(gameObject);
            if (GetExperimentalFeature != null) domain.LookAt(GetExperimentalFeature);


            var blKey = domain.GetTexTransCoreEngineForUnity().QueryBlendKey(BlendTypeKey);

            foreach (var matAndTex in decalCompiledTextures)
            {
                domain.AddTextureStack(matAndTex.Key, matAndTex.Value.Texture, blKey);
            }

            if (decalCompiledTextures.Keys.Any() is false) { TTLog.Info("SimpleDecal:info:TargetNotFound"); }
            foreach (var t in decalCompiledTextures.Values) { t.Dispose(); }
        }
        internal Dictionary<Texture, TTRenderTexWithPaddingDistance> CompileDecal(IDomain domain)
        {
            var ttce = domain.GetTexTransCoreEngineForUnity();


            Profiler.BeginSample("GetMultipleDecalTexture");
            using var mulDecalTexture = GetDecalSourceTexture(domain, ttce);
            Profiler.EndSample();

            var targetRenderers = ModificationTargetRenderers(domain);
            var decalContext = GenerateDecalCtx(domain, ttce);
            decalContext.DrawMaskMaterials = RendererSelector.GetOrNullAutoMaterialHashSet(domain);

            domain.LookAt(targetRenderers);
            return decalContext.WriteDecalTexture<Texture>(domain, targetRenderers, mulDecalTexture, TargetPropertyName) ?? new();

        }
        internal SimpleDecalExperimentalFeature? ExperimentalFeatureCash;
        internal SimpleDecalExperimentalFeature? GetExperimentalFeature
        {
            get
            {
                if (ExperimentalFeatureCash == null) ExperimentalFeatureCash = GetComponent<SimpleDecalExperimentalFeature>();
                return ExperimentalFeatureCash;
            }
        }

        internal DecalContext<ParallelProjectionSpaceConvertor, ParallelProjectionSpace, ITrianglesFilter<ParallelProjectionSpace, IFilteredTriangleHolder>, IFilteredTriangleHolder>
            GenerateDecalCtx(IDomain domain, ITexTransToolForUnity ttce)
        {
            var decalContext = new DecalContext
                <ParallelProjectionSpaceConvertor, ParallelProjectionSpace, ITrianglesFilter<ParallelProjectionSpace, IFilteredTriangleHolder>, IFilteredTriangleHolder>
                (ttce, GetSpaceConverter(), GetTriangleFilter(domain));
            decalContext.IsTextureStretch = false;
            decalContext.DecalPadding = Padding;
            decalContext.UseDepthOrInvert = GetExperimentalFeature?.DepthInvert ?? null;
            return decalContext;
        }

        internal ITTRenderTexture GetDecalSourceTexture(IDomain domain, ITexTransToolForUnity ttce)
        {
            if (GetExperimentalFeature?.OverrideDecalTextureWithMultiLayerImageCanvas != null)
                return GetExperimentalFeature.OverrideDecalTextureWithMultiLayerImageCanvas.EvaluateCanvas(new(domain, (2048, 2048), null));

            if (DecalTexture == null)
            {
                var solid = ttce.CreateRenderTexture(64, 64);
                ttce.ColorFill(solid, Color.ToTTCore());
                return solid;
            }

            domain.LookAt(DecalTexture);
            var decalTexDisk = ttce.Wrapping(DecalTexture);
            var mulDecalTexture = ttce.LoadTextureWidthFullScale(decalTexDisk);
            ttce.ColorMultiply(mulDecalTexture, Color.ToTTCore());
            return mulDecalTexture;
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            return DecalContextUtility.FilterDecalTarget(rendererTargeting, RendererSelector.GetSelectedOrIncludingAll(rendererTargeting, this, GetDRS, out var _), TargetPropertyName);
            DecalRendererSelector GetDRS(SimpleDecal simpleDecal) => simpleDecal.RendererSelector;
        }
        public List<Renderer> GetIntersectRenderers(IEnumerable<Renderer> renderers)
        {
            var decalAABB = GetDecalAABB();
            var intersected = new List<Renderer>();

            foreach (var r in renderers)
            {
                if (r == null) { continue; }
                if (r.GetMesh() == null) { continue; }
                var md = r.GetToMemorizedMeshData();

                var result = md.AxisAlignedBoundingBox.IsIntersect(decalAABB);
                if (result) { intersected.Add(r); }
            }
            return intersected;
        }

        internal ParallelProjectionSpaceConvertor GetSpaceConverter() { return new(transform.worldToLocalMatrix); }
        internal ITrianglesFilter<ParallelProjectionSpace, IFilteredTriangleHolder> GetTriangleFilter(IRendererTargeting originEqual)
        {
            if (IslandSelector != null) { return new IslandSelectToPPFilter(IslandSelector, GetFilter(), originEqual); }
            return new ParallelProjectionFilter(GetFilter());
        }

        internal JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] GetFilter()
        {
            var filters = new List<JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>>(2);

            if (BackCulling) filters.Add(TriangleFilterUtility.SideStruct.GetJobChain(false));
            filters.Add(TriangleFilterUtility.OutOfPolygonStruct.GetJobChain(new TexTransUnityAABB(Vector3.zero).AddVertex(Vector3.one)));

            return filters.ToArray();
        }
        internal void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.black;
            var matrix = transform.localToWorldMatrix;

            Gizmos.matrix = matrix;

            var centerPos = Vector3.zero;
            Gizmos.DrawWireCube(centerPos + new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));//基準となる四角形


            DecalGizmoUtility.DrawGizmoQuad(DecalTexture, Color, matrix);
            if (IslandSelector != null) { IslandSelector.OnDrawGizmosSelected(); }
        }
        static readonly Vector3[] s_decalBoxVertexes = new Vector3[]{
            new(-0.5f,-0.5f,0),
            new(0.5f,-0.5f,0),
            new(-0.5f,0.5f,0),
            new(0.5f,0.5f,0),

            new(-0.5f,-0.5f,1f),
            new(0.5f,-0.5f,1f),
            new(-0.5f,0.5f,1f),
            new(0.5f,0.5f,1f),
        };

        internal TexTransUnityAABB GetDecalAABB()
        {
            var matrix = transform.localToWorldMatrix;
            var box = new Vector3[s_decalBoxVertexes.Length];
            for (var i = 0; box.Length > i; i += 1)
            {
                box[i] = matrix.MultiplyPoint3x4(s_decalBoxVertexes[i]);
            }

            var aabb = new TexTransUnityAABB(box[0]);
            for (var i = 1; box.Length > i; i += 1)
            {
                aabb.AddVertex(box[i]);
            }
            return aabb;
        }

        bool ICanBehaveAsLayer.HaveBlendTypeKey => true;
        LayerObject<ITexTransToolForUnity> ICanBehaveAsLayer.GetLayerObject(GenerateLayerObjectContext ctx, AsLayer asLayer)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.LookAt(this);
            var alphaMask = asLayer.GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);
            var alphaOp = asLayer.Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;

            if (ctx.TargetContainedMaterials is null)
            {
                TTLog.Error("SimpleDecal:error:CanNotAsLayerWhenUnsupportedContext");
                return new EmptyLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey);
            }

            domain.LookAt(transform.GetParents().Append(transform));
            using var decalSourceTex = GetDecalSourceTexture(domain, engine);
            var decalWriteTarget = ctx.Engine.CreateRenderTexture(ctx.CanvasSize.x, ctx.CanvasSize.y);

            var decalRenderTarget = ctx.Domain.RendererFilterForMaterialFromDomains(ctx.TargetContainedMaterials);
            domain.LookAt(decalRenderTarget);
            var decalContext = GenerateDecalCtx(domain, engine);
            decalContext.DrawMaskMaterials = ctx.TargetContainedMaterials;

            if (decalContext.WriteDecalTextureWithSingleTexture(domain, decalRenderTarget, decalWriteTarget, decalSourceTex))
                return new RasterLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey, decalWriteTarget);
            else { return new EmptyLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey); }

        }
    }
}
