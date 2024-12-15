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

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class SimpleDecal : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT SimpleDecal";
        internal const string MenuPath = ComponentName;
        public RendererSelectMode SelectMode;
        public List<Renderer> TargetRenderers = new List<Renderer> { null };

        [ExpandTexture2D] public Texture2D DecalTexture;
        [BlendTypeKey] public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;

        public UnityEngine.Color Color = UnityEngine.Color.white;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;
        public float Padding = 5;
        public bool HighQualityPadding = false;
        public bool FixedAspect = true;
        [FormerlySerializedAs("SideChek")][FormerlySerializedAs("SideCulling")] public bool BackCulling = true;
        public bool PolygonOutOfCulling = true;

        public AbstractIslandSelector IslandSelector;
        public MultiLayerImageCanvas OverrideDecalTextureWithMultiLayerImageCanvas;
        public bool UseDepth;
        public bool DepthInvert;

        #region V5SaveData
        public bool MultiRendererMode = false;
        #endregion V5SaveData
        #region V3SaveData
        [Obsolete("V3SaveData", true)][FormerlySerializedAs("PolygonCaling")][SerializeField] internal PolygonCulling PolygonCulling = PolygonCulling.Vertex;
        [Obsolete("V3SaveData", true)][SerializeField] internal bool IslandCulling = false;
        [Obsolete("V3SaveData", true)][SerializeField] internal Vector2 IslandSelectorPos = new Vector2(0.5f, 0.5f);
        [Obsolete("V3SaveData", true)][SerializeField] internal float IslandSelectorRange = 1;
        #endregion V3SaveData


        #region V1SaveData
        [Obsolete("Replaced with BlendTypeKey", true)][HideInInspector][SerializeField] internal BlendType BlendType = BlendType.Normal;
        #endregion
        #region V0SaveData
        [Obsolete("V0SaveData", true)][HideInInspector] public bool MigrationV0ClearTarget;
        [Obsolete("V0SaveData", true)][HideInInspector] public GameObject MigrationV0DataMatAndTexSeparatorGameObject;
        // [Obsolete("V0SaveData", true)][HideInInspector] public MatAndTexUtils.MatAndTexRelativeSeparator MigrationV0DataMatAndTexSeparator;
        [Obsolete("V0SaveData", true)][HideInInspector] public SimpleDecal MigrationV0DataAbstractDecal;
        [Obsolete("V0SaveData", true)][HideInInspector] public bool IsSeparateMatAndTexture;
        [Obsolete("V0SaveData", true)][HideInInspector] public bool FastMode = true;
        #endregion

        internal override TexTransPhase PhaseDefine => TexTransPhase.AfterUVModification;
        internal bool? GetUseDepthOrInvert => UseDepth ? new bool?(DepthInvert) : null;
        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);
            if (SelectMode is RendererSelectMode.Manual && TargetRenderers.Any() is false) { TTTRuntimeLog.Info("SimpleDecal:info:TargetNotSet"); return; }
            var targetRenderers = GetTargetRenderers(domain.EnumerateRenderer(), domain.OriginEqual);
            var decalCompiledTextures = CompileDecal(targetRenderers, domain);

            domain.LookAt(transform.GetParents().Append(transform));
            domain.LookAt(decalCompiledTextures.Keys);
            if (IslandSelector != null) { IslandSelector.LookAtCalling(domain); }

            var blKey = domain.GetTexTransCoreEngineForUnity().QueryBlendKey(BlendTypeKey);

            foreach (var matAndTex in decalCompiledTextures)
            {
                domain.AddTextureStack(matAndTex.Key, matAndTex.Value.Texture, blKey);
            }

            if (decalCompiledTextures.Keys.Any() is false) { TTTRuntimeLog.Info("SimpleDecal:info:TargetNotFound"); }
            foreach (var t in decalCompiledTextures.Values) { t.Dispose(); }
        }
        internal Dictionary<Texture, TTRenderTexWithDistance> CompileDecal(IEnumerable<Renderer> targetRenderers, IDomain domain)
        {
            var ttce = domain.GetTexTransCoreEngineForUnity();
            ITTRenderTexture mulDecalTexture = null;
            try
            {
                if (OverrideDecalTextureWithMultiLayerImageCanvas == null)
                {
                    Profiler.BeginSample("GetMultipleDecalTexture");
                    if (DecalTexture != null)
                    {
                        domain.LookAt(DecalTexture);
                        var decalTexDisk = ttce.Wrapping(DecalTexture);
                        mulDecalTexture = ttce.LoadTextureWidthFullScale(decalTexDisk);
                        ttce.ColorMultiply(mulDecalTexture, Color.ToTTCore());
                    }
                    else
                    {
                        mulDecalTexture = ttce.CreateRenderTexture(64, 64);
                        ttce.ColorFill(mulDecalTexture, Color.ToTTCore());
                    }
                    Profiler.EndSample();
                }
                else
                {
                    Profiler.BeginSample("Rendering MultiLayerImageCanvas");
                    OverrideDecalTextureWithMultiLayerImageCanvas.LookAtCallingCanvas(domain);
                    mulDecalTexture = OverrideDecalTextureWithMultiLayerImageCanvas.EvaluateCanvas(domain.GetTexTransCoreEngineForUnity(), 2048, 2048);
                    Profiler.EndSample();
                }

                var decalContext = new DecalContext<ParallelProjectionSpace, ITrianglesFilter<ParallelProjectionSpace>>(ttce, GetSpaceConverter(), GetTriangleFilter());
                decalContext.TargetPropertyName = TargetPropertyName;
                decalContext.IsTextureStretch = false;
                decalContext.DecalPadding = Padding;
                decalContext.HighQualityPadding = HighQualityPadding;
                decalContext.UseDepthOrInvert = GetUseDepthOrInvert;

                var decalCompiledRenderTextures = new Dictionary<Texture, TTRenderTexWithDistance>();
                domain.LookAt(targetRenderers);
                foreach (var renderer in targetRenderers)
                {
                    Profiler.BeginSample("CreateDecalTexture");
                    decalContext.WriteDecalTexture(decalCompiledRenderTextures, renderer, mulDecalTexture);
                    Profiler.EndSample();
                }
                return decalCompiledRenderTextures;
            }
            finally { mulDecalTexture?.Dispose(); }
        }
        internal override IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            var targetRenderers = GetTargetRenderers(domainRenderers, replaceTracking);

            foreach (var tr in targetRenderers)
            {
                if (tr is not (SkinnedMeshRenderer or MeshRenderer)) { continue; }
                if (tr.GetMesh() == null) { continue; }
                foreach (var mat in tr.sharedMaterials)
                {
                    if (mat == null) { continue; }
                    var targetTex = mat.HasProperty(TargetPropertyName) ? mat.GetTexture(TargetPropertyName) : null;
                    if (targetTex == null) { continue; }
                    yield return tr;
                    break;
                }
            }
        }

        private IEnumerable<Renderer> GetTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            IEnumerable<Renderer> targetRenderers;
            switch (SelectMode)
            {
                default:
                case RendererSelectMode.Auto:
                    {
                        targetRenderers = GetIntersectRenderers(domainRenderers);
                        break;
                    }
                case RendererSelectMode.Manual:
                    {
                        targetRenderers = replaceTracking.GetDomainsRenderers(domainRenderers, TargetRenderers);
                        break;
                    }
            }

            return targetRenderers;
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

        internal ParallelProjectionSpace GetSpaceConverter() { return new ParallelProjectionSpace(transform.worldToLocalMatrix); }
        internal ITrianglesFilter<ParallelProjectionSpace> GetTriangleFilter()
        {
            if (IslandSelector != null) { return new IslandSelectToPPFilter(IslandSelector, GetFilter()); }
            return new ParallelProjectionFilter(GetFilter());
        }

        internal JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] GetFilter()
        {
            var filters = new List<JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>>
            {
                TriangleFilterUtility.FarStruct.GetJobChain(1, true),
                TriangleFilterUtility.NearStruct.GetJobChain(0, true)
            };
            if (BackCulling) filters.Add(TriangleFilterUtility.SideStruct.GetJobChain(false));
            if (PolygonOutOfCulling) filters.Add(TriangleFilterUtility.OutOfPolygonStruct.GetJobChain(0, 1, true));

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

    }
}
