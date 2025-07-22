#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.IslandSelector;
using net.rs64.TexTransTool.Utils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Color = UnityEngine.Color;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class ParallelProjectionWithLilToonDecal : TexTransRuntimeBehavior, IRendererTargetingAffecterWithRuntime
    {
        internal const string ComponentName = "TTT ParallelProjectionWith lilToon Decal";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.MaterialModification;

        public Material? TargetMaterial;
        [ExpandTexture2D] public Texture2D? DecalTexture;

        public Color Color = Color.white;
        public bool MSDFTexture;
        public lilToonCullMode CullMode;
        public lilToonTransparentMode TransparentMode;
        public lilToonBlendingMode ShaderBlendingMode;

        public ReplaceTexture ReplaceTextureTarget = ReplaceTexture.Texture2nd;
        [Range(1, 3)] public int WriteUVTarget = 1;
        public AbstractIslandSelector? IslandSelector;

        public enum lilToonCullMode
        {
            Off,
            Front,
            Back,
        }

        public enum ReplaceTexture
        {
            Texture2nd = 2,
            Texture3rd = 3,
        }
        public enum lilToonTransparentMode
        {
            None,
            Replace,
            Multiply,
            Add,
            Subtract,
        }
        public enum lilToonBlendingMode
        {
            Normal,
            Add,
            Screen,
            Multiply,
        }
        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);
            domain.LookAt(transform.GetParents().Append(transform));

            var targetDomainsMaterials = domain.GetDomainsMaterialsHashSet(TargetMaterial);
            var targetMaterialContainedRenderers = domain.RendererFilterForMaterialFromDomains(targetDomainsMaterials);

            var writeTargetVertexAttribute = GetUnityTexCord();
            var isUVContained = targetMaterialContainedRenderers.Where(r => domain.GetMesh(r)?.HasVertexAttribute(writeTargetVertexAttribute) ?? false).ToArray();
            if (isUVContained.Length is not 0) { TTLog.Info("ParallelProjectionWithLilToonDecal:info:UVHasContained", isUVContained); }

            // UV Writing
            {
                var meshData = targetMaterialContainedRenderers.Select(r => domain.GetMeshData(r)).ToArray();

                var spaceConvertor = GetSpaceConverter();
                var triangleFilter = GetTriangleFilter(domain);

                using var convertedSpace = spaceConvertor.ConvertSpace(meshData);
                using var filteredTriangleHolder = triangleFilter.Filtering(convertedSpace);


                var inParallelVertex = new NativeArray<bool>[meshData.Length];
                var filteredParallelUVVertex = new NativeArray<Vector2>[meshData.Length];

                for (var ri = 0; meshData.Length > ri; ri += 1)
                {
                    inParallelVertex[ri] = new NativeArray<bool>(meshData[ri].VertexUV.Length, Allocator.TempJob);
                    filteredParallelUVVertex[ri] = new NativeArray<Vector2>(meshData[ri].VertexUV.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                }


                var jobHandles = new JobHandle[meshData.Length];
                var triangles = filteredTriangleHolder.GetTriangles();

                for (var ri = 0; meshData.Length > ri; ri += 1)
                {
                    var md = meshData[ri];

                    var refMats = domain.GetMaterials(md.ReferenceRenderer);
                    var mJobHandle = default(JobHandle);
                    for (var si = 0; md.SubMeshCount > si; si += 1)
                    {
                        var refMat = refMats[si];
                        if (refMat == null) { continue; }
                        if (targetDomainsMaterials.Contains(refMat) is false) { continue; }

                        var tri = triangles[ri][si];
                        mJobHandle = new ParallelProjectionInCalculate()
                        {
                            Triangles = tri,
                            IsInParallelProjection = inParallelVertex[ri]
                        }.Schedule(tri.Length, 64, mJobHandle);
                    }
                    jobHandles[ri] = mJobHandle;
                }


                var parallelProjectionUVVertex = (convertedSpace as IDecalSpace).OutputUV();

                for (var ri = 0; meshData.Length > ri; ri += 1)
                {
                    var md = meshData[ri];
                    for (var si = 0; md.SubMeshCount > si; si += 1)
                    {
                        jobHandles[ri] = new FilterParallelProjectionUVVertex()
                        {
                            SourceParallelProjectUVVertex = parallelProjectionUVVertex[ri],
                            IsInParallelProjection = inParallelVertex[ri],
                            OutputParallelProjectUVVertex = filteredParallelUVVertex[ri],
                        }.Schedule(parallelProjectionUVVertex[ri].Length, 64, jobHandles[ri]);
                    }
                }


                for (var ri = 0; meshData.Length > ri; ri += 1)
                {
                    var md = meshData[ri];
                    if (domain.GetMaterials(md.ReferenceRenderer).Any(m => m != null && targetDomainsMaterials.Contains(m)) is false) { continue; }
                    if (triangles[ri].Any(t => t.Length != 0) is false) { continue; }

                    jobHandles[ri].Complete();

                    var writableMesh = Instantiate(domain.GetMesh(md.ReferenceRenderer)!);
                    writableMesh.SetUVs(WriteUVTarget, filteredParallelUVVertex[ri]);
                    domain.SetMesh(md.ReferenceRenderer, writableMesh);
                    domain.TransferAsset(writableMesh);
                }


                for (var ri = 0; meshData.Length > ri; ri += 1)
                {
                    jobHandles[ri].Complete();
                    inParallelVertex[ri].Dispose();
                    filteredParallelUVVertex[ri].Dispose();
                }
            }


            // SetupMaterial
            {
                foreach (var imMat in targetDomainsMaterials)
                {
                    var mutableMat = domain.ToMutable(imMat);
                    ModificationMaterial(mutableMat);
                }
            }
        }

        private void ModificationMaterial(Material mutableMat)
        {
            foreach (var propName in GetProperties(ReplaceTextureTarget)) mutableMat.ResetProperty(propName);

            switch (ReplaceTextureTarget)
            {
                default:
                case ReplaceTexture.Texture2nd:
                    {
                        mutableMat.SetInt(PropName_UseMain2ndTex, 1);
                        mutableMat.SetInt(PropName_Main2ndTexIsDecal, 1);

                        mutableMat.SetTexture(PropName_Main2ndTex, DecalTexture);
                        mutableMat.SetColor(PropName_Color2nd, Color);
                        mutableMat.SetInt(PropName_Main2ndTexIsMSDF, MSDFTexture ? 1 : 0);
                        mutableMat.SetInt(PropName_Main2ndTex_Cull, (int)CullMode);
                        mutableMat.SetInt(PropName_Main2ndTex_UVMode, WriteUVTarget);
                        mutableMat.SetInt(PropName_Main2ndTexAlphaMode, (int)TransparentMode);
                        mutableMat.SetInt(PropName_Main2ndTexBlendMode, (int)ShaderBlendingMode);
                        break;
                    }
                case ReplaceTexture.Texture3rd:
                    {
                        mutableMat.SetInt(PropName_UseMain3rdTex, 1);
                        mutableMat.SetInt(PropName_Main3rdTexIsDecal, 1);

                        mutableMat.SetTexture(PropName_Main3rdTex, DecalTexture);
                        mutableMat.SetColor(PropName_Color3rd, Color);
                        mutableMat.SetInt(PropName_Main3rdTexIsMSDF, MSDFTexture ? 1 : 0);
                        mutableMat.SetInt(PropName_Main3rdTex_Cull, (int)CullMode);
                        mutableMat.SetInt(PropName_Main3rdTex_UVMode, WriteUVTarget);
                        mutableMat.SetInt(PropName_Main3rdTexAlphaMode, (int)TransparentMode);
                        mutableMat.SetInt(PropName_Main3rdTexBlendMode, (int)ShaderBlendingMode);
                        break;
                    }
            }
        }

        UnityEngine.Rendering.VertexAttribute GetUnityTexCord()
        {
            switch (WriteUVTarget)
            {
                default:
                case 1: { return UnityEngine.Rendering.VertexAttribute.TexCoord1; }
                case 2: { return UnityEngine.Rendering.VertexAttribute.TexCoord2; }
                case 3: { return UnityEngine.Rendering.VertexAttribute.TexCoord3; }
            }
        }

        internal ParallelProjectionSpaceConvertor GetSpaceConverter() { return new(transform.worldToLocalMatrix); }
        internal ITrianglesFilter<ParallelProjectionSpace, IFilteredTriangleHolder> GetTriangleFilter(IRendererTargeting targeting)
        {
            if (IslandSelector != null) { return new IslandSelectToPPFilter(IslandSelector, GetFilter(), targeting); }
            return new ParallelProjectionFilter(GetFilter());
        }

        internal JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] GetFilter()
        {
            var filters = new List<JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>>
            {
            };
            return filters.ToArray();
        }
        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            return rendererTargeting.RendererFilterForMaterial(rendererTargeting.LookAtGet(this, i => i.TargetMaterial));
        }

        void IRendererTargetingAffecterWithRuntime.AffectingRendererTargeting(IAffectingRendererTargeting rendererTargetingModification)
        {
            var targetDomainsMaterials = rendererTargetingModification.GetDomainsMaterialsHashSet(TargetMaterial);
            foreach (var mutableMat in targetDomainsMaterials) { ModificationMaterial(mutableMat); }
        }

        static readonly Vector3[] lineList = new Vector3[]{

            new (0f,0f,0f),
            new (0f,0f,1f),


            new (-0.1f,0f,0.9f),
            new (0f,0f,1f),

            new (0f,0f,1f),
            new (0.1f,0f,0.9f),


            new (0f,-0.1f,0.9f),
            new (0f,0f,1f),

            new (0f,0f,1f),
            new (0f,0.1f,0.9f),
        };


        internal void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.black;
            var matrix = transform.localToWorldMatrix;
            Gizmos.matrix = matrix;

            DecalGizmoUtility.DrawGizmoQuad(DecalTexture, UnityEngine.Color.white, matrix);
            Gizmos.DrawLineList(lineList.AsSpan());
        }
        struct ParallelProjectionInCalculate : IJobParallelFor
        {
            [ReadOnly] public NativeArray<TriangleIndex> Triangles;
            [WriteOnly][NativeDisableParallelForRestriction] public NativeArray<bool> IsInParallelProjection;
            public void Execute(int index)
            {
                var tri = Triangles[index];
                IsInParallelProjection[tri.zero] = true;
                IsInParallelProjection[tri.one] = true;
                IsInParallelProjection[tri.two] = true;
            }
        }

        struct FilterParallelProjectionUVVertex : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector2> SourceParallelProjectUVVertex;
            [ReadOnly] public NativeArray<bool> IsInParallelProjection;
            [WriteOnly] public NativeArray<Vector2> OutputParallelProjectUVVertex;

            public void Execute(int index)
            {
                if (IsInParallelProjection[index]) OutputParallelProjectUVVertex[index] = SourceParallelProjectUVVertex[index];
                else OutputParallelProjectUVVertex[index] = new(-0.5f, -0.5f);
            }
        }

        // 力技
        const string PropName_UseMain2ndTex = "_UseMain2ndTex";
        const string PropName_Color2nd = "_Color2nd";
        const string PropName_Main2ndTex = "_Main2ndTex";
        const string PropName_Main2ndTexAngle = "_Main2ndTexAngle";
        const string PropName_Main2ndTex_ScrollRotate = "_Main2ndTex_ScrollRotate";
        const string PropName_Main2ndTex_UVMode = "_Main2ndTex_UVMode";
        const string PropName_Main2ndTex_Cull = "_Main2ndTex_Cull";
        const string PropName_Main2ndTexDecalAnimation = "_Main2ndTexDecalAnimation";
        const string PropName_Main2ndTexDecalSubParam = "_Main2ndTexDecalSubParam";
        const string PropName_Main2ndTexIsDecal = "_Main2ndTexIsDecal";
        const string PropName_Main2ndTexIsLeftOnly = "_Main2ndTexIsLeftOnly";
        const string PropName_Main2ndTexIsRightOnly = "_Main2ndTexIsRightOnly";
        const string PropName_Main2ndTexShouldCopy = "_Main2ndTexShouldCopy";
        const string PropName_Main2ndTexShouldFlipMirror = "_Main2ndTexShouldFlipMirror";
        const string PropName_Main2ndTexShouldFlipCopy = "_Main2ndTexShouldFlipCopy";
        const string PropName_Main2ndTexIsMSDF = "_Main2ndTexIsMSDF";
        const string PropName_Main2ndBlendMask = "_Main2ndBlendMask";
        const string PropName_Main2ndTexBlendMode = "_Main2ndTexBlendMode";
        const string PropName_Main2ndTexAlphaMode = "_Main2ndTexAlphaMode";
        const string PropName_Main2ndEnableLighting = "_Main2ndEnableLighting";
        const string PropName_Main2ndDissolveMask = "_Main2ndDissolveMask";
        const string PropName_Main2ndDissolveNoiseMask = "_Main2ndDissolveNoiseMask";
        const string PropName_Main2ndDissolveNoiseMask_ScrollRotate = "_Main2ndDissolveNoiseMask_ScrollRotate";
        const string PropName_Main2ndDissolveNoiseStrength = "_Main2ndDissolveNoiseStrength";
        const string PropName_Main2ndDissolveColor = "_Main2ndDissolveColor";
        const string PropName_Main2ndDissolveParams = "_Main2ndDissolveParams";
        const string PropName_Main2ndDissolvePos = "_Main2ndDissolvePos";
        const string PropName_Main2ndDistanceFade = "_Main2ndDistanceFade";

        const string PropName_UseMain3rdTex = "_UseMain3rdTex";
        const string PropName_Color3rd = "_Color3rd";
        const string PropName_Main3rdTexAngle = "_Main3rdTexAngle";
        const string PropName_Main3rdTex_ScrollRotate = "_Main3rdTex_ScrollRotate";
        const string PropName_Main3rdTex = "_Main3rdTex";
        const string PropName_Main3rdTex_UVMode = "_Main3rdTex_UVMode";
        const string PropName_Main3rdTex_Cull = "_Main3rdTex_Cull";
        const string PropName_Main3rdTexDecalAnimation = "_Main3rdTexDecalAnimation";
        const string PropName_Main3rdTexDecalSubParam = "_Main3rdTexDecalSubParam";
        const string PropName_Main3rdTexIsDecal = "_Main3rdTexIsDecal";
        const string PropName_Main3rdTexIsLeftOnly = "_Main3rdTexIsLeftOnly";
        const string PropName_Main3rdTexIsRightOnly = "_Main3rdTexIsRightOnly";
        const string PropName_Main3rdTexShouldCopy = "_Main3rdTexShouldCopy";
        const string PropName_Main3rdTexShouldFlipMirror = "_Main3rdTexShouldFlipMirror";
        const string PropName_Main3rdTexShouldFlipCopy = "_Main3rdTexShouldFlipCopy";
        const string PropName_Main3rdTexIsMSDF = "_Main3rdTexIsMSDF";
        const string PropName_Main3rdBlendMask = "_Main3rdBlendMask";
        const string PropName_Main3rdTexBlendMode = "_Main3rdTexBlendMode";
        const string PropName_Main3rdTexAlphaMode = "_Main3rdTexAlphaMode";
        const string PropName_Main3rdEnableLighting = "_Main3rdEnableLighting";
        const string PropName_Main3rdDissolveMask = "_Main3rdDissolveMask";
        const string PropName_Main3rdDissolveNoiseMask = "_Main3rdDissolveNoiseMask";
        const string PropName_Main3rdDissolveNoiseMask_ScrollRotate = "_Main3rdDissolveNoiseMask_ScrollRotate";
        const string PropName_Main3rdDissolveNoiseStrength = "_Main3rdDissolveNoiseStrength";
        const string PropName_Main3rdDissolveColor = "_Main3rdDissolveColor";
        const string PropName_Main3rdDissolveParams = "_Main3rdDissolveParams";
        const string PropName_Main3rdDissolvePos = "_Main3rdDissolvePos";
        const string PropName_Main3rdDistanceFade = "_Main3rdDistanceFade";


        IEnumerable<string> GetProperties(ReplaceTexture replaceTexture)
        {
            switch (replaceTexture)
            {
                default:
                case ReplaceTexture.Texture2nd:
                    {
                        yield return PropName_UseMain2ndTex;
                        yield return PropName_Color2nd;
                        yield return PropName_Main2ndTex;
                        yield return PropName_Main2ndTexAngle;
                        yield return PropName_Main2ndTex_ScrollRotate;
                        yield return PropName_Main2ndTex_UVMode;
                        yield return PropName_Main2ndTex_Cull;
                        yield return PropName_Main2ndTexDecalAnimation;
                        yield return PropName_Main2ndTexDecalSubParam;
                        yield return PropName_Main2ndTexIsDecal;
                        yield return PropName_Main2ndTexIsLeftOnly;
                        yield return PropName_Main2ndTexIsRightOnly;
                        yield return PropName_Main2ndTexShouldCopy;
                        yield return PropName_Main2ndTexShouldFlipMirror;
                        yield return PropName_Main2ndTexShouldFlipCopy;
                        yield return PropName_Main2ndTexIsMSDF;
                        yield return PropName_Main2ndBlendMask;
                        yield return PropName_Main2ndTexBlendMode;
                        yield return PropName_Main2ndTexAlphaMode;
                        yield return PropName_Main2ndEnableLighting;
                        yield return PropName_Main2ndDissolveMask;
                        yield return PropName_Main2ndDissolveNoiseMask;
                        yield return PropName_Main2ndDissolveNoiseMask_ScrollRotate;
                        yield return PropName_Main2ndDissolveNoiseStrength;
                        yield return PropName_Main2ndDissolveColor;
                        yield return PropName_Main2ndDissolveParams;
                        yield return PropName_Main2ndDissolvePos;
                        yield return PropName_Main2ndDistanceFade;
                        break;
                    }
                case ReplaceTexture.Texture3rd:
                    {
                        yield return PropName_UseMain3rdTex;
                        yield return PropName_Color3rd;
                        yield return PropName_Main3rdTexAngle;
                        yield return PropName_Main3rdTex_ScrollRotate;
                        yield return PropName_Main3rdTex;
                        yield return PropName_Main3rdTex_UVMode;
                        yield return PropName_Main3rdTex_Cull;
                        yield return PropName_Main3rdTexDecalAnimation;
                        yield return PropName_Main3rdTexDecalSubParam;
                        yield return PropName_Main3rdTexIsDecal;
                        yield return PropName_Main3rdTexIsLeftOnly;
                        yield return PropName_Main3rdTexIsRightOnly;
                        yield return PropName_Main3rdTexShouldCopy;
                        yield return PropName_Main3rdTexShouldFlipMirror;
                        yield return PropName_Main3rdTexShouldFlipCopy;
                        yield return PropName_Main3rdTexIsMSDF;
                        yield return PropName_Main3rdBlendMask;
                        yield return PropName_Main3rdTexBlendMode;
                        yield return PropName_Main3rdTexAlphaMode;
                        yield return PropName_Main3rdEnableLighting;
                        yield return PropName_Main3rdDissolveMask;
                        yield return PropName_Main3rdDissolveNoiseMask;
                        yield return PropName_Main3rdDissolveNoiseMask_ScrollRotate;
                        yield return PropName_Main3rdDissolveNoiseStrength;
                        yield return PropName_Main3rdDissolveColor;
                        yield return PropName_Main3rdDissolveParams;
                        yield return PropName_Main3rdDissolvePos;
                        yield return PropName_Main3rdDistanceFade;
                        break;
                    }
            }
        }


    }
}
