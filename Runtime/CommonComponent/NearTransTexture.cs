#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.TransTexture;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.IslandSelector;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.UVIsland;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class NearTransTexture : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT " + nameof(NearTransTexture);
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.PostProcessing;

        public Renderer? TransSourceRenderer;
        public int SourceMaterialSlot = 0;
        public PropertyName SourcePropertyName = PropertyName.DefaultValue;

        public Renderer? TransTargetRenderer;
        public int TargetMaterialSlot = 0;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;

        [BlendTypeKey] public string BlendTypeKey = ITexTransToolForUnity.BL_KEY_DEFAULT;

        public float FadeStartDistance = 0.01f;
        public float MaxDistance = 0.1f;
        public float Padding = 5f;

        internal override void Apply(IDomain domain)
        {
            if (TransSourceRenderer == null || TransTargetRenderer == null)
            { TTTRuntimeLog.Info("NearTransTexture:info:TargetNotSet"); return; }

            var sourceRenderers = domain.GetDomainsRenderers(TransSourceRenderer);
            var targetRenderers = domain.GetDomainsRenderers(TransTargetRenderer);
            if (sourceRenderers.Count() > 1 || targetRenderers.Count() > 1)
            { TTTRuntimeLog.Info("NearTransTexture:info:FoundTargetIsMultiple"); }

            var sourceRenderer = sourceRenderers.FirstOrDefault();
            var targetRenderer = targetRenderers.FirstOrDefault();
            if (sourceRenderer == null || targetRenderer == null)
            { TTTRuntimeLog.Info("NearTransTexture:info:TargetNotFound"); return; }

            var sourceMeshData = domain.GetMeshData(sourceRenderer);
            var targetMeshData = domain.GetMeshData(targetRenderer);
            if (sourceMeshData.SubMeshCount <= SourceMaterialSlot || targetMeshData.SubMeshCount <= TargetMaterialSlot)
            { TTTRuntimeLog.Error("NearTransTexture:error:SubMeshOverSlotSelected"); return; }


            var sourceMatArray = domain.GetMaterials(sourceRenderer);
            if (sourceMatArray.Length <= SourceMaterialSlot) { TTTRuntimeLog.Error("NearTransTexture:error:SourceRendererIsSmilerMaterialSlots"); return; }
            var sourceMaterial = sourceMatArray[SourceMaterialSlot];
            var targetMatArray = domain.GetMaterials(targetRenderer);
            if (targetMatArray.Length <= TargetMaterialSlot) { TTTRuntimeLog.Error("NearTransTexture:error:TargetRendererIsSmilerMaterialSlots"); return; }
            var targetMaterial = targetMatArray[TargetMaterialSlot];
            if (sourceMaterial == null || targetMaterial == null) { TTTRuntimeLog.Error("NearTransTexture:error:MaterialIsNull"); return; }

            var sourceTexture = sourceMaterial.GetTexture(SourcePropertyName);
            var targetTexture = targetMaterial.GetTexture(TargetPropertyName);
            if (sourceTexture == null || targetTexture == null) { TTTRuntimeLog.Error("NearTransTexture:error:TextureIsNull"); return; }
            domain.LookAt(this);

            using var sourceVertArray = new NativeArray<Vector4>(sourceMeshData.Vertices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            using var targetVertArray = new NativeArray<Vector4>(targetMeshData.Vertices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var sourceExpandHandle = new ExpandJob() { read = sourceMeshData.Vertices, write = sourceVertArray }.Schedule(sourceMeshData.Vertices.Length, 64);
            var targetExpandHandle = new ExpandJob() { read = targetMeshData.Vertices, write = targetVertArray }.Schedule(targetMeshData.Vertices.Length, 64);
            var expandJobHandler = JobHandle.CombineDependencies(sourceExpandHandle, targetExpandHandle);

            var engine = domain.GetTexTransCoreEngineForUnity();

            using var sourceTexRt = engine.WrappingToLoadFullScaleOrUpload(sourceTexture);
            using var targetTexRt = engine.CreateRenderTexture(targetTexture.width, targetTexture.height);

            expandJobHandler.Complete();
            engine.NearTransTexture(
                sourceTexRt,
                targetTexRt,

                sourceMeshData.TriangleIndex[SourceMaterialSlot],
                MemoryMarshal.Cast<Vector4, TTVector4>(sourceVertArray.AsSpan()),
                MemoryMarshal.Cast<Vector2, System.Numerics.Vector2>(sourceMeshData.VertexUV.AsSpan()),

                targetMeshData.TriangleIndex[TargetMaterialSlot],
                MemoryMarshal.Cast<Vector4, TTVector4>(targetVertArray.AsSpan()),
                MemoryMarshal.Cast<Vector2, System.Numerics.Vector2>(targetMeshData.VertexUV.AsSpan()),

                FadeStartDistance,
                MaxDistance,
                Padding
            );

            domain.AddTextureStack(targetTexture, targetTexRt, engine.QueryBlendKey(BlendTypeKey));
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            var renderers = new List<Renderer>(2);
            if (TransSourceRenderer != null) { renderers.Add(TransSourceRenderer); }
            if (TransTargetRenderer != null) { renderers.Add(TransTargetRenderer); }
            return rendererTargeting.GetDomainsRenderers(renderers);
        }


        struct ExpandJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<Vector3> read;
            [WriteOnly]
            public NativeArray<Vector4> write;
            public void Execute(int index) { write[index] = read[index]; }
        }
    }
}
