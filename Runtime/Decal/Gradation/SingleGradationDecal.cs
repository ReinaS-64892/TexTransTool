using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransTool.IslandSelector;
using System;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using net.rs64.TexTransCoreEngineForUnity;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.TransTexture;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class SingleGradationDecal : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT SingleGradationDecal";
        internal const string MenuPath = ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.AfterUVModification;

        public List<Material> TargetMaterials = new();
        public Gradient Gradient;
        [Range(0, 1)] public float Alpha = 1;
        public bool GradientClamp = true;
        public AbstractIslandSelector IslandSelector;
        [BlendTypeKey] public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;
        public float Padding = 5;
        public bool HighQualityPadding = false;


        internal override void Apply([NotNull] IDomain domain)
        {
            domain.LookAt(this);
            domain.LookAt(transform.GetParents().Append(transform));
            if (IslandSelector != null) { IslandSelector.LookAtCalling(domain); }

            if (TargetMaterials.Any() is false) { TTTRuntimeLog.Info("SingleGradationDecal:info:TargetNotSet"); return; }
            var nowTargetMat = GetTargetMaterials(domain.OriginEqual, domain.EnumerateRenderer());
            var ttce = domain.GetTexTransCoreEngineForUnity();

            using var gradDiskTex = ttce.Wrapping(GradientTempTexture.Get(Gradient, Alpha));
            var space = new SingleGradientSpace(transform.worldToLocalMatrix);
            var filter = new IslandSelectFilter(IslandSelector);

            var decalContext = new DecalContext<SingleGradientSpace, IslandSelectFilter>(ttce, space, filter);
            decalContext.TargetPropertyName = TargetPropertyName;
            decalContext.TextureWarp = GradientClamp ? TextureWrap.NotWrap : TextureWrap.Stretch;
            decalContext.NotContainsKeyAutoGenerate = false;
            decalContext.DecalPadding = Padding;
            decalContext.HighQualityPadding = HighQualityPadding;


            var writeable = new Dictionary<Material, TTRenderTexWithDistance>();
            decalContext.GenerateKey(writeable, nowTargetMat);
            var blKey = ttce.QueryBlendKey(BlendTypeKey);

            if (writeable.Any() is false) { TTTRuntimeLog.Info("SingleGradationDecal:info:TargetNotFound"); return; }
            using var gradTex = ttce.LoadTextureWidthFullScale(gradDiskTex);

            foreach (var renderer in domain.EnumerateRenderer())
            {
                if (!renderer.sharedMaterials.Any(mat => nowTargetMat.Contains(mat))) { continue; }
                domain.LookAt(renderer);
                decalContext.WriteDecalTexture(writeable, renderer, gradTex);
            }

            foreach (var m2rt in writeable) { domain.AddTextureStack(m2rt.Key.GetTexture(TargetPropertyName), m2rt.Value.Texture, blKey); }

            foreach (var w in writeable) { w.Value.Dispose(); }
        }

        private HashSet<Material> GetTargetMaterials(OriginEqual originEqual, IEnumerable<Renderer> domainRenderers)
        {
            if (TargetMaterials.Any() is false) { return new(); }
            return RendererUtility.GetFilteredMaterials(domainRenderers.Where(r => r is SkinnedMeshRenderer or MeshRenderer)).Where(m => TargetMaterials.Any(tm => originEqual.Invoke(m, tm))).ToHashSet();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.black;
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawLine(Vector3.zero, Vector3.up);
            IslandSelector?.OnDrawGizmosSelected();
        }
        internal override IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            var targetMat = GetTargetMaterials(replaceTracking, domainRenderers);
            var texHash = targetMat.Select(m => m.HasProperty(TargetPropertyName) ? m.GetTexture(TargetPropertyName) : null).Where(t => t != null).ToHashSet();

            return SimpleDecal.GetTextureReplacedRange(domainRenderers, texHash);
        }
    }

    internal class SingleGradientSpace : IConvertSpace
    {
        Matrix4x4 _world2LocalMatrix;
        MeshData _meshData;

        JobResult<NativeArray<Vector2>> _uv;

        internal MeshData MeshData => _meshData;
        internal JobResult<NativeArray<Vector2>> UV => _uv;

        public SingleGradientSpace(Matrix4x4 w2l)
        {
            _world2LocalMatrix = w2l;
        }
        public void Input(MeshData meshData)
        {
            _meshData = meshData;
            var uvNa = new NativeArray<Vector2>(_meshData.Vertices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var convertJob = new ConvertJob()
            {
                World2Local = _world2LocalMatrix,
                worldVerticals = _meshData.Vertices,
                uv = uvNa
            };

            _uv = new(uvNa, convertJob.Schedule(uvNa.Length, 32));
        }

        public NativeArray<Vector2> OutPutUV()
        {
            if (_uv == null) { return default; }
            return _uv.GetResult;
        }
        public void Dispose()
        {
            _meshData = null;
            _uv.GetResult.Dispose();
            _uv = null;
        }

        [BurstCompile]
        struct ConvertJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> worldVerticals;
            [ReadOnly] public Matrix4x4 World2Local;
            [WriteOnly] public NativeArray<Vector2> uv;
            public void Execute(int index) { uv[index] = new Vector2(World2Local.MultiplyPoint3x4(worldVerticals[index]).y, 0.5f); }
        }

    }

    internal class IslandSelectFilter : ITrianglesFilter<SingleGradientSpace>
    {
        public IIslandSelector IslandSelector;
        MeshData _meshData;
        NativeArray<TriangleIndex>[] _islandSelectedTriangles;

        public IslandSelectFilter(IIslandSelector islandSelector)
        {
            IslandSelector = islandSelector;
        }

        public void SetSpace(SingleGradientSpace space)
        {
            _meshData = space.MeshData;
            _islandSelectedTriangles = new NativeArray<TriangleIndex>[space.MeshData.TriangleIndex.Length];
        }

        NativeArray<TriangleIndex> ITrianglesFilter<SingleGradientSpace>.GetFilteredSubTriangle(int subMeshIndex)
        {
            if (_meshData == null) { return default; }
            if (_islandSelectedTriangles[subMeshIndex].IsCreated) { return _islandSelectedTriangles[subMeshIndex]; }
            var islandSelected = _islandSelectedTriangles[subMeshIndex] = IslandSelectToPPFilter.IslandSelectExecute(IslandSelector, _meshData, subMeshIndex);
            return islandSelected;
        }
        public void Dispose()
        {
            _meshData = null;
            foreach (var na in _islandSelectedTriangles) { na.Dispose(); }
            _islandSelectedTriangles = null;
        }

    }

}
