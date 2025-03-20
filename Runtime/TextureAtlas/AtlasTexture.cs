#nullable enable
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using net.rs64.TexTransTool.TextureAtlas.IslandRelocator;
using UnityEngine.Serialization;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.UVIsland;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
using net.rs64.TexTransCore.AtlasTexture;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [AddComponentMenu(TTTName + "/" + MenuPath)]
    public sealed partial class AtlasTexture : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT AtlasTexture";
        internal const string MenuPath = ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.Optimizing;


        [FormerlySerializedAs("TargetRoot")] public GameObject LimitCandidateMaterials;

        public List<MatSelector> SelectMatList = new List<MatSelector>();
        [Serializable]
        public struct MatSelector
        {
            public Material Material;
            public float MaterialFineTuningValue;

            #region V3SaveData
            [Obsolete("V3SaveData", true)][SerializeField] internal float AdditionalTextureSizeOffSet;
            #endregion
            #region V1SaveData
            [Obsolete("V1SaveData", true)][SerializeField] internal float TextureSizeOffSet;
            #endregion
        }

        public List<MaterialMergeGroup> MergeMaterialGroups = new();
        [Serializable]
        public class MaterialMergeGroup
        {
            public List<Material> Group = new();
            public Material? Reference;
        }
        public Material? AllMaterialMergeReference;



        public AtlasSetting AtlasSetting = new AtlasSetting();

        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);

            if (SelectMatList.Any() is false) { TTLog.Info("AtlasTexture:info:TargetNotSet"); return; }

            var nowRenderers = GetTargetAllowedFilter(domain.EnumerateRenderer());
            var targetMaterials = GetTargetMaterials(domain, nowRenderers).ToHashSet();

            if (targetMaterials.Any() is false) { TTLog.Info("AtlasTexture:info:TargetNotFound"); return; }

            foreach (var mmg in MergeMaterialGroups) { if (mmg.Reference != null) { domain.LookAt(mmg.Reference); } }
            if (AllMaterialMergeReference != null) { domain.LookAt(AllMaterialMergeReference); }


            var sizePriorityDict = targetMaterials
                .ToDictionary(
                    i => i,
                    i => TTMath.Saturate(
                            SelectMatList.First(
                                m => domain.OriginEqual(m.Material, i)
                            ).MaterialFineTuningValue
                        )
                );


            var atlasSetting = AtlasSetting;

            var atlasTargeSize = GetAtlasTextureSize(atlasSetting);


            var targeting = domain as IRendererTargeting;
            var targetRenderers = nowRenderers
                .Where(r => targeting.GetMesh(r) != null)
                .Where(r => targeting.GetMaterials(r)
                    .Where(i => i != null)
                    .Cast<Material>()
                    .Any(targetMaterials.Contains)
                ).ToArray();


            using var atlasContext = new AtlasContext(
                targeting
                , targetRenderers
                , targetMaterials
                , atlasSetting.UsePrimaryMaximumTexture ? null : atlasSetting.PrimaryTextureProperty
                , atlasSetting.AtlasTargetUVChannel
                );



            //アイランドまわり
            if (atlasSetting.PixelNormalize)
            {
                atlasContext.SourceVirtualIslandNormalize();
            }


            var (movedVirtualIslandArray, relocateResult) = IslandProcessing(domain, atlasSetting, atlasContext, sizePriorityDict, out var relocationTime);
            if (relocateResult.IslandRelocationResult is null) { throw new Exception(); }
            if (relocateResult.IslandRelocationResult.IsSuccess is false) { throw new Exception(); }
            var source2MovedVirtualIsland = new Dictionary<IslandTransform, IslandTransform>();
            for (var i = 0; movedVirtualIslandArray.Length > i; i += 1)
            {
                source2MovedVirtualIsland[atlasContext.SourceVirtualIslands[i]] = movedVirtualIslandArray[i];
            }


            //上側を削れるかを見る
            var height = IslandRelocationManager.CalculateIslandsMaxHeight(movedVirtualIslandArray) + atlasSetting.IslandPadding;
            var atlasTextureHeightSize = Mathf.Max(GetNormalizedMinHeightSize(atlasTargeSize.y, height), 4);//4以下はちょっと怪しい挙動しそうだからクランプ
            Debug.Assert(Mathf.IsPowerOfTwo(atlasTextureHeightSize));

            TexTransCore.TTLog.Info("AtlasTexture:info:RelocateResult", 1 - height, relocateResult.PriorityDownScale, relocateResult.OverallDownScale, relocateResult.TotalRelocateCount, relocationTime);
            var atlasedTextureSize = new Vector2Int(atlasTargeSize.x, atlasTextureHeightSize);

            //新しいUVを持つMeshを生成するフェーズ

            // SubSetIndex と対応する
            var atlasedMeshes = atlasContext.GenerateAtlasedMesh(atlasSetting, source2MovedVirtualIsland, atlasedTextureSize);


            //アトラス化したテクスチャーを生成するフェーズ
            var compiledAtlasTextures = new Dictionary<string, ITTRenderTexture>();

            // Profiler.BeginSample("GetGroupedTextures");
            var engine = domain.GetTexTransCoreEngineForUnity();
            using var groupedTextures = atlasContext.GetGroupedDiskOrRenderTextures(engine);
            var containsProperty = atlasContext.MaterialGroupingCtx.GetContainsAllProperties();

            // Profiler.EndSample();
            var loadedDiskTextures = new Dictionary<ITTDiskTexture, ITTRenderTexture>();

            // Profiler.BeginSample("Texture synthesis");
            foreach (var propName in containsProperty)
            {
                var targetRT = engine.CreateRenderTexture(atlasedTextureSize.x, atlasedTextureSize.y);
                engine.ColorFill(targetRT, atlasSetting.BackGroundColor.ToTTCore());
                // TextureUtility.FillColor(targetRT, atlasSetting.BackGroundColor);
                targetRT.Name = "AtlasTex" + propName;
                // Profiler.BeginSample("Draw:" + targetRT.name);
                foreach (var group in groupedTextures.GroupedTextures)
                {
                    if (!group.Value.TryGetValue(propName, out var sourceTexture)) { continue; }
                    var sourceRenderTexture = sourceTexture switch
                    {
                        ITTRenderTexture rt => rt,
                        ITTDiskTexture dt => loadedDiskTextures.ContainsKey(dt) ? loadedDiskTextures[dt] : LoadFullScale(dt),
                        _ => throw new InvalidCastException(),
                    };
                    ITTRenderTexture LoadFullScale(ITTDiskTexture diskTexture)
                    {
                        var loaded = engine.LoadTextureWidthFullScale(diskTexture);
                        loadedDiskTextures[diskTexture] = loaded;
                        return loaded;
                    }

                    var findMaterialID = group.Key;
                    if (relocateResult.IslandRelocationResult.IsRectangleMove)
                    {
                        var findSubIDHash = atlasContext.AtlasSubMeshIndexSetCtx.AtlasSubMeshIndexIDHash
                                                .Where(i => i.MaterialGroupID == findMaterialID).ToHashSet();

                        var drawTargetSourceVirtualIslandsHash = new HashSet<IslandTransform>();
                        foreach (var subID in findSubIDHash)
                        {
                            drawTargetSourceVirtualIslandsHash.UnionWith(
                                atlasContext.AtlasIslandCtx.OriginIslandDict[subID]
                                    .Select(i => atlasContext.AtlasIslandCtx.Origin2VirtualIsland[i])
                                );
                        }
                        var drawTargetSourceVirtualIslands = drawTargetSourceVirtualIslandsHash.ToArray();
                        var drawTargetMovedVirtualIslands = drawTargetSourceVirtualIslands.Select(i => source2MovedVirtualIsland[i]).ToArray();


                        AtlasingUtility.TransMoveRectangle(engine
                            , targetRT
                            , sourceRenderTexture
                            , drawTargetSourceVirtualIslands
                            , drawTargetMovedVirtualIslands
                            , atlasSetting.IslandPadding
                        );
                    }
                    else
                    {
                        throw new NotImplementedException();
                        // for (var subSetIndex = 0; atlasContext.AtlasSubSets.Count > subSetIndex; subSetIndex += 1)
                        // {
                        //     var transTargets = atlasContext.AtlasSubSets[subSetIndex].Where(i => i.HasValue).Where(i => i.Value.MaterialGroupID == findMaterialID).Select(i => i.Value);
                        //     if (!transTargets.Any()) { continue; }

                        //     var triangles = new NativeArray<TriangleIndex>(transTargets.SelectMany(subData => atlasContext.IslandDict[subData].SelectMany(i => i.triangles)).ToArray(), Allocator.TempJob);
                        //     var originUV = atlasContext.MeshDataDict[atlasContext.NormalizeMeshes[atlasContext.Meshes[transTargets.First().MeshID]]].VertexUV;

                        //     var transData = new TransData(triangles, subSetMovedUV[subSetIndex], originUV);
                        //     ForTrans(targetRT, sTexture, transData, atlasSetting.GetTexScalePadding * 0.5f, null, true);

                        //     triangles.Dispose();
                        // }preMesh
                    }

                }
                // Profiler.EndSample();

                // if (atlasSetting.AtlasTextureSize != atlasTextureHeightSize)
                // {
                //     var heightClampRt = TTRt.G(atlasSetting.AtlasTextureSize, atlasTextureHeightSize);
                //     heightClampRt.name = $"{targetRT.name}-heightClamp-TempRt-{heightClampRt.width}x{heightClampRt.height}";
                //     Graphics.CopyTexture(targetRT, 0, 0, 0, 0, heightClampRt.width, heightClampRt.height, heightClampRt, 0, 0, 0, 0);
                //     TTRt.R(targetRT);
                //     targetRT = heightClampRt;
                // }

                // var containsNormalMap = atlasContext.MaterialGroupToAtlasShaderTexDict.SelectMany(i => i.Value).Where(i => i.Key == propName).Any(i => i.Value.IsNormalMap);
                // MipMapUtility.GenerateMips(targetRT, atlasSetting.DownScalingAlgorithm, containsNormalMap);

                // Profiler.BeginSample("Readback");
                compiledAtlasTextures.Add(propName, targetRT);
                // Profiler.EndSample();

                // TTRt.R(targetRT);
            }

            // Profiler.EndSample();

            //     if (atlasSetting.AutoReferenceCopySetting && propertyBakeSetting == PropertyBakeSetting.NotBake)
            //     {
            //         Profiler.BeginSample("AutoReferenceCopySetting");
            //         var prop = containsProperty.ToArray();
            //         var refCopyDict = new Dictionary<string, string>();

            //         var gtHash = containsProperty.ToDictionary(p => p, p => groupedTextures.Where(i => i.Value.ContainsKey(p)).Select(i => i.Value[p]).ToHashSet());

            //         for (var i = 0; prop.Length > i; i += 1)
            //             for (var i2 = 0; prop.Length > i2; i2 += 1)
            //             {preMesh
            //                 if (i == i2) { continue; }

            //                 var copySource = prop[i];
            //                 var copyTarget = prop[i2];

            //                 var sTexHash = gtHash[copySource];
            //                 var tTexHash = gtHash[copyTarget];

            //                 if (sTexHash.SetEquals(tTexHash)) { refCopyDict.Remove(copySource); }

            //                 if (sTexHash.IsSupersetOf(tTexHash) is false) { continue; }
            //                 refCopyDict[copyTarget] = copySource;

            //                 // if (refCopyDict.ContainsKey(copySource) is false) { refCopyDict[copySource] = new(); }
            //                 // refCopyDict[copySource].Add(copyTarget);
            //             }


            //         var isContinue = true;
            //         var values = refCopyDict.Values.ToArray();
            //         while (isContinue)
            //         {
            //             foreach (var s in values)
            //             {
            //                 if (refCopyDict.ContainsKey(s))
            //                 {
            //                     foreach (var t in refCopyDict.Where(i => i.Value == s).ToArray())
            //                     {
            //                         refCopyDict[t.Key] = refCopyDict[s];
            //                     }
            //                     isContinue = true;
            //                 }
            //             }
            //             isContinue = false;
            //         }

            //         atlasData.ReferenceCopyDict = refCopyDict.GroupBy(i => i.Value).ToDictionary(i => i.Key, i => i.Select(k => k.Key).ToList());
            //         Profiler.EndSample();
            //     }
            //     if (atlasSetting.AutoMergeTextureSetting)
            //     {
            //         Profiler.BeginSample("AutoMergeTextureSetting");

            //         var prop = containsProperty.ToArray();
            //         var prop2MatIDs = prop.ToDictionary(p => p, p => groupedTextures.Where(gt => gt.Value.ContainsKey(p)).Select(gt => gt.Key).ToHashSet());

            //         var alreadySetting = new HashSet<string>();
            //         var margeSettingDict = new Dictionary<string, List<string>>();



            //         for (var i = 0; prop.Length > i; i += 1)
            //         {
            //             var mergeParent = prop[i];
            //             var mergedHash = prop2MatIDs[mergeParent].ToHashSet();
            //             var childList = new List<string>();

            //             for (var i2 = i; prop.Length > i2; i2 += 1)
            //             {
            //                 if (i == i2) { continue; }

            //                 var mergeChild = prop[i2];

            //                 if (alreadySetting.Contains(mergeChild)) { continue; }

            //                 var childHash = prop2MatIDs[mergeChild];

            //                 if (mergedHash.Overlaps(childHash)) { continue; }

            //                 childList.Add(mergeChild);
            //                 mergedHash.UnionWith(childHash);
            //                 alreadySetting.Add(mergeChild);
            //             }

            //             if (childList.Any()) { margeSettingDict[mergeParent] = childList; }
            //         }

            //         atlasData.MargeTextureDict = margeSettingDict;
            //         Profiler.EndSample();
            //     }

            //     Profiler.BeginSample("ReleaseGroupeTextures");
            //     foreach (var kv in groupedTextures.Values) { foreach (var tex in kv) { TTRt.R(tex.Value); } }
            //     groupedTextures = null;
            //     Profiler.EndSample();

            //     Profiler.BeginSample("TextureMaxSize");
            //     var texMaxDict = new Dictionary<string, int>();
            //     foreach (var atlasTexKV in atlasContext.MaterialToAtlasShaderTexDict.SelectMany(x => x.Value))
            //     {
            //         if (compiledAtlasTextures.ContainsKey(atlasTexKV.Key) is false) { continue; }
            //         if (texMaxDict.ContainsKey(atlasTexKV.Key) is false) { texMaxDict[atlasTexKV.Key] = 2; }
            //         if (atlasTexKV.Value.Texture == null) { continue; }
            //         texMaxDict[atlasTexKV.Key] = math.max(texMaxDict[atlasTexKV.Key], atlasTexKV.Value.Texture.width);
            //     }
            //     atlasData.SourceTextureMaxSize = texMaxDict;
            //     Profiler.EndSample();

            //     Profiler.BeginSample("Async Readback");
            //     atlasData.Textures = compiledAtlasTextures.ToDictionary(kv => kv.Key, kv => kv.Value.GetTexture2D());
            //     Profiler.EndSample();

            //     atlasData.MaterialID = atlasContext.MaterialGroup.Select(i => i.ToHashSet()).ToArray();
            //     atlasContext.Dispose();
            //     foreach (var movedUV in subSetMovedUV) { movedUV.Dispose(); }

            //     atlasData.BakePropMaxValue = bakePropMaxValue;

            //     return true;


            // Profiler.BeginSample("AtlasShaderSupportUtils:ctor");
            // var shaderSupport = new AtlasShaderSupportUtils();
            // Profiler.EndSample();

            //Mesh Change
            foreach (var renderer in targetRenderers)
            {
                var mesh = domain.GetMesh(renderer);
                if (mesh == null) { continue; }
                if (atlasContext.NormalizedMeshCtx.Origin2NormalizedMesh.ContainsKey(mesh) is false) { continue; }

                var meshID = atlasContext.NormalizedMeshCtx.Normalized2MeshID[atlasContext.NormalizedMeshCtx.Origin2NormalizedMesh[mesh]];
                var matIDs = domain.GetMaterials(renderer).Select(m => m != null ? atlasContext.MaterialGroupingCtx.GetMaterialGroupID(m) : -1).ToArray();

                var subSet = new AtlasSubMeshIndexID?[matIDs.Length];
                for (var i = 0; subSet.Length > i; i += 1)
                {
                    var matID = matIDs[i];
                    if (matID is -1) { subSet[i] = null; }
                    else { subSet[i] = new AtlasSubMeshIndexID(meshID, i, matID); }
                }
                // var atlasMeshHolder = atlasData.Meshes.FindAll(I => I.DistMesh == mesh).Find(I => I.MatIDs.SequenceEqual(matIDs));
                // if (atlasMeshHolder.AtlasMesh == null) { continue; }


                var identicalSubSetID = GetIdenticalSubSet(atlasContext.AtlasSubMeshIndexSetCtx.AtlasSubSets, subSet);

                int GetIdenticalSubSet(List<AtlasSubMeshIndexID?[]> atlasSubSetAll, AtlasSubMeshIndexID?[] findSource)
                {
                    return atlasSubSetAll.FindIndex(subSet =>
                    {

                        if (subSet.Length == findSource.Length && subSet.SequenceEqual(findSource)) { return true; }
                        if (AtlasSubMeshIndexIDSetContext.SubPartEqual(subSet, findSource) is false) { return false; }
                        return subSet.Length < findSource.Length is false;
                    });
                }
                if (identicalSubSetID is -1) { continue; }


                var atlasMesh = atlasedMeshes[identicalSubSetID];
                domain.SetMesh(renderer, atlasMesh);
            }
            domain.TransferAssets(atlasedMeshes);

            // //Texture Fine Tuning
            // var atlasTexFineTuningTargets = TexFineTuningUtility.InitTexFineTuning(atlasData.Textures);
            // SetSizeDataMaxSize(atlasTexFineTuningTargets, atlasData.SourceTextureMaxSize);
            // DefaultMargeTextureDictTuning(atlasTexFineTuningTargets, atlasData.MargeTextureDict);
            // DefaultRefCopyTuning(atlasTexFineTuningTargets, atlasData.ReferenceCopyDict);
            // foreach (var fineTuning in AtlasSetting.TextureFineTuning)
            // {
            //     fineTuning?.AddSetting(atlasTexFineTuningTargets);
            // }
            // var individualApplied = new HashSet<string>();
            // foreach (var individualTuning in AtlasSetting.TextureIndividualFineTuning)
            // {
            //     if (atlasTexFineTuningTargets.ContainsKey(individualTuning.TuningTarget) is false) { continue; }
            //     if (individualApplied.Contains(individualTuning.TuningTarget)) { continue; }
            //     individualApplied.Add(individualTuning.TuningTarget);

            //     var tuningTarget = atlasTexFineTuningTargets[individualTuning.TuningTarget];

            //     if (individualTuning.OverrideReferenceCopy) { tuningTarget.Get<ReferenceCopyData>().CopySource = individualTuning.CopyReferenceSource; }
            //     if (individualTuning.OverrideResize) { tuningTarget.Get<SizeData>().TextureSize = individualTuning.TextureSize; }
            //     if (individualTuning.OverrideCompression) { tuningTarget.Set(individualTuning.CompressionData); }
            //     if (individualTuning.OverrideMipMapRemove) { tuningTarget.Get<MipMapData>().UseMipMap = individualTuning.UseMipMap; }
            //     if (individualTuning.OverrideColorSpace) { tuningTarget.Get<ColorSpaceData>().Linear = individualTuning.Linear; }
            //     if (individualTuning.OverrideRemove) { tuningTarget.Get<RemoveData>().IsRemove = individualTuning.IsRemove; }
            //     if (individualTuning.OverrideMargeTexture) { tuningTarget.Get<MergeTextureData>().MargeParent = individualTuning.MargeRootProperty; }
            // }
            // TexFineTuningUtility.FinalizeTexFineTuning(atlasTexFineTuningTargets, domain.GetTextureManager());
            // var atlasTexture = atlasTexFineTuningTargets.ToDictionary(i => i.Key, i => i.Value.Texture2D);
            // domain.transferAssets(atlasTexture.Select(PaT => PaT.Value));
            var tex = compiledAtlasTextures.ToDictionary(i => i.Key, i => engine.DownloadToTexture2D(i.Value, true));
            foreach (var rt in compiledAtlasTextures.Values) { rt.Dispose(); }
            foreach (var t in tex.Values) { t.Apply(true); }

            //MaterialGenerate And Change
            var atlasMatOption = new AtlasMatGenerateOption() { ForceSetTexture = atlasSetting.ForceSetTexture };
            if (atlasSetting.UnsetTextures.Any())
            {
                var containsAllTexture = targetMaterials.SelectMany(mat => mat.GetAllTextureWithDictionary().Select(i => i.Value));
                atlasMatOption.UnsetTextures = atlasSetting.UnsetTextures.Select(i => i.GetTexture()).SelectMany(ot => containsAllTexture.Where(ct => domain.OriginEqual(ot, ct))).ToHashSet();
            }
            // if (AtlasSetting.MergeMaterials)
            // {
            //     if (AtlasSetting.PropertyBakeSetting != PropertyBakeSetting.NotBake) { atlasMatOption.BakedPropertyReset = AtlasSetting.BakedPropertyWriteMaxValue; }
            //     var mergeMat = AtlasSetting.MergeReferenceMaterial != null ? AtlasSetting.MergeReferenceMaterial : atlasData.AtlasInMaterials.First();
            //     Material generateMat = GenerateAtlasMat(mergeMat, atlasTexture, shaderSupport, atlasData.BakePropMaxValue, atlasMatOption);
            //     var matGroupGenerate = AtlasSetting.MaterialMergeGroups.Where(m => m.GroupMaterials.Any()).ToDictionary(m => m, m => GenerateAtlasMat(m.MergeReferenceMaterial != null ? m.MergeReferenceMaterial : m.GroupMaterials.First(), atlasTexture, shaderSupport, atlasData.BakePropMaxValue, atlasMatOption));

            //     domain.ReplaceMaterials(atlasData.AtlasInMaterials.ToDictionary(x => x, m => FindGroup(m)), false);

            //     Material FindGroup(Material material)
            //     {
            //         foreach (var matGroup in AtlasSetting.MaterialMergeGroups)
            //         {
            //             var index = matGroup.GroupMaterials.FindIndex(m => domain.OriginEqual(m, material));
            //             if (index != -1) { return matGroupGenerate[matGroup]; }
            //         }
            //         return generateMat;
            //     }
            // }
            // else
            {
                var materialMap = new Dictionary<Material, Material>();
                foreach (var distMat in targetMaterials)
                {
                    var generateMat = GenerateAtlasMat(distMat, tex, atlasMatOption);
                    materialMap.Add(distMat, generateMat);
                }
                domain.ReplaceMaterials(materialMap);
            }

            // foreach (var atlasMeshHolder in atlasData.Meshes) { UnityEngine.Object.DestroyImmediate(atlasMeshHolder.NormalizedMesh); }
        }

        private static int GetNormalizedMinHeightSize(int atlasTextureSize, float height)
        {
            switch (height)
            {
                default: return atlasTextureSize;
                case < (1 / 32f): return atlasTextureSize / 32;
                case < (1 / 16f): return atlasTextureSize / 16;
                case < (1 / 8f): return atlasTextureSize / 8;
                case < (1 / 4f): return atlasTextureSize / 4;
                case < (1 / 2f): return atlasTextureSize / 2;
            }
        }
        static Vector2Int GetAtlasTextureSize(AtlasSetting atlasSetting)
        {
            var atlasTargeSize = new Vector2Int(atlasSetting.AtlasTextureSize, atlasSetting.AtlasTextureSize);
            if (atlasSetting.CustomAspect) { atlasTargeSize.y = atlasSetting.AtlasTextureHeightSize; }
            return atlasTargeSize;
        }
        private static (IslandTransform[] virtualIslandArray, IslandRelocationManager.RelocateResult relocateResult) IslandProcessing(
            IReplaceTracking domain
            , AtlasSetting atlasSetting
            , AtlasContext atlasContext
            , Dictionary<Material, float> sizePriorityDict
            , out long relocationTime
        )
        {
            var atlasTargeSize = GetAtlasTextureSize(atlasSetting);
            var virtualIslandArray = new IslandTransform[atlasContext.SourceVirtualIslands.Length];

            for (var i = 0; virtualIslandArray.Length > i; i += 1)
                virtualIslandArray[i] = atlasContext.SourceVirtualIslands[i].Clone();



            atlasContext.PrimaryTextureSizeScaling(virtualIslandArray, atlasTargeSize);

            var refOriginIslands = GetSourceVirtualIslandForRefOriginIslands(atlasContext);
            var islandDescription = GenerateIslandDescription(atlasContext, refOriginIslands);

            var sizePriority = new float[virtualIslandArray.Length]; for (var i = 0; sizePriority.Length > i; i += 1) { sizePriority[i] = 1f; }
            for (var i = 0; virtualIslandArray.Length > i; i += 1)
            {
                var refOriginIsland = refOriginIslands[i];
                var atlasSubMeshIndexID = atlasContext.AtlasIslandCtx.ReverseOriginDict[refOriginIsland];
                var group = atlasContext.MaterialGroupingCtx.GroupMaterials[atlasSubMeshIndexID.MaterialGroupID];

                var materialFineTuningValue = 0f;
                var count = 0;
                foreach (var gm in group)
                    if (sizePriorityDict.TryGetValue(gm, out var priorityValue))
                    {
                        materialFineTuningValue += priorityValue;
                        count += 1;
                    }

                materialFineTuningValue /= count;
                sizePriority[i] = materialFineTuningValue;
            }

            foreach (var islandFineTuner in atlasSetting.IslandFineTuners)
                islandFineTuner?.IslandFineTuning(sizePriority, refOriginIslands, islandDescription, domain);


            for (var i = 0; sizePriority.Length > i; i += 1)
                sizePriority[i] = TTMath.Saturate(sizePriority[i]);


            if (atlasSetting.AtlasIslandRelocator != null) { throw new NotImplementedException(); }
            var islandRelocator = new NFDHPlasFC() as IIslandRelocator;



            var relocateManage = new IslandRelocationManager(islandRelocator);
            relocateManage.Padding = atlasSetting.IslandPadding;
            relocateManage.ForceSizePriority = atlasSetting.ForceSizePriority;
            relocateManage.Height = (float)atlasTargeSize.y / atlasTargeSize.x;

            var timer = System.Diagnostics.Stopwatch.StartNew();
            relocateManage.RelocateLoop(atlasContext, virtualIslandArray, sizePriority, out var relocateResult);
            timer.Stop();
            relocationTime = timer.ElapsedMilliseconds;

            if (relocateResult.IslandRelocationResult is null || relocateResult.IslandRelocationResult.IsSuccess is false)
                TTLog.Error("AtlasTexture:error:RelocationFailed");


            var rectTangleMove = relocateResult.IslandRelocationResult?.IsRectangleMove ?? true;
            Debug.Assert(rectTangleMove is true);


            if (atlasSetting.PixelNormalize)
                if (TTMath.Approximately(atlasSetting.IslandPadding, 0))
                    TTLog.Warning("AtlasTexture:warn:IslandPaddingIsZeroAndPixelNormalizeUsed");

            PostVirtualIslandProcessing(virtualIslandArray, atlasTargeSize, atlasSetting.PixelNormalize);

            return (virtualIslandArray, relocateResult);
        }
        private static Island[] GetSourceVirtualIslandForRefOriginIslands(AtlasContext atlasContext)
        {
            var refOriginIslands = new Island[atlasContext.SourceVirtualIslands.Length];
            for (var i = 0; atlasContext.SourceVirtualIslands.Length > i; i += 1)
            {
                var refOriginIsland = atlasContext.ReverseSourceVirtualIsland2OriginIslands[atlasContext.SourceVirtualIslands[i]].First();
                refOriginIslands[i] = refOriginIsland;
            }

            return refOriginIslands;
        }
        private static IslandSelector.IslandDescription[] GenerateIslandDescription(AtlasContext atlasContext, Island[] refOriginIslands)
        {
            var islandDescription = new IslandSelector.IslandDescription[atlasContext.SourceVirtualIslands.Length];

            for (var i = 0; atlasContext.SourceVirtualIslands.Length > i; i += 1)
            {
                var refOriginIsland = refOriginIslands[i];
                var atlasSubMeshIndexID = atlasContext.AtlasIslandCtx.ReverseOriginDict[refOriginIsland];
                var md = atlasContext.NormalizedMeshCtx.GetMeshDataFromMeshID(atlasSubMeshIndexID.MeshID);

                var vertex = md.Vertices;
                var uv = md.VertexUV;
                var renderer = md.ReferenceRenderer;
                islandDescription[i] = new IslandSelector.IslandDescription(vertex, uv, renderer, atlasSubMeshIndexID.SubMeshIndex);
            }
            return islandDescription;
        }

        private static void PostVirtualIslandProcessing(IslandTransform[] virtualIslandArray, Vector2Int atlasTargeSize, bool pixelNormalize)
        {
            if (pixelNormalize)
            {
                for (var i = 0; virtualIslandArray.Length > i; i += 1)
                {
                    virtualIslandArray[i].Position = AtlasContext.NormalizeMin(atlasTargeSize.x, atlasTargeSize.y, virtualIslandArray[i].Position);
                }
            }

            for (var i = 0; virtualIslandArray.Length > i; i += 1)
            {
                if (virtualIslandArray[i].Size.X <= 0.0001f) { virtualIslandArray[i].Size.X = 0.0001f; }
                if (virtualIslandArray[i].Size.Y <= 0.0001f) { virtualIslandArray[i].Size.Y = 0.0001f; }
            }//Islandが小さすぎると RectTangleMoveのコピーがうまくいかない
        }

        internal List<Material> GetTargetMaterials(IDomain domain, List<Renderer> nowRenderers)
        {
            var nowContainsMatSet = new HashSet<Material>(RendererUtility.GetMaterials(nowRenderers).Where(i => i != null));
            var targetMaterials = nowContainsMatSet.Where(mat => SelectMatList.Any(smat => domain.OriginEqual(smat.Material, mat))).ToList();
            return targetMaterials;
        }

        // internal static void DefaultRefCopyTuning(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, Dictionary<string, List<string>> referenceCopyDict)
        // {
        //     if (referenceCopyDict == null) { return; }

        //     foreach (var fineTuning in referenceCopyDict.Select(i => new ReferenceCopy(new(i.Key), i.Value.Select(i => new PropertyName(i)).ToList())))
        //     {
        //         fineTuning.AddSetting(atlasTexFineTuningTargets);
        //     }
        // }
        // internal static void DefaultMargeTextureDictTuning(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, Dictionary<string, List<string>> margeTextureDict)
        // {
        //     if (margeTextureDict == null) { return; }

        //     foreach (var fineTuning in margeTextureDict.Select(i => new MergeTexture(new(i.Key), i.Value.Select(i => new PropertyName(i)).ToList())))
        //     {
        //         fineTuning.AddSetting(atlasTexFineTuningTargets);
        //     }

        // }
        // internal static void SetSizeDataMaxSize(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, Dictionary<string, int> sourceTextureMaxSize)
        // {
        //     foreach (var texMax in sourceTextureMaxSize)
        //     {
        //         if (texMax.Key == "_MainTex") { continue; }
        //         var sizeData = atlasTexFineTuningTargets[texMax.Key].Get<SizeData>();
        //         sizeData.TextureSize = texMax.Value;
        //     }
        // }

        internal List<Renderer> GetTargetAllowedFilter(IEnumerable<Renderer> domainRenderers) { return domainRenderers.Where(i => AtlasAllowedRenderer(i, AtlasSetting.IncludeDisabledRenderer)).ToList(); }
        private static Material GenerateAtlasMat(Material targetMat, Dictionary<string, Texture2D> atlasTex, AtlasMatGenerateOption option)
        {
            var editableTMat = UnityEngine.Object.Instantiate(targetMat);

            foreach (var texKV in atlasTex)
            {
                // if (supporter.IsConstraintValid(targetMat, texKV.Key) is false) { continue; }
                var tex = editableTMat.GetTexture(texKV.Key);
                if (tex == null) { tex = null; }//これは Unity側の Null を C# の Null にしてるやつ

                if (option.ForceSetTexture is false && tex == null) { continue; }
                if (tex is not Texture2D && tex is not RenderTexture && tex is not null) { continue; }
                // if (tex is RenderTexture rt && TTRt.IsTemp(rt) is false && TTRt2.IsTemp(rt) is false) { continue; }

                if (tex != null && option.UnsetTextures is not null && option.UnsetTextures.Contains(tex)) { continue; }

                editableTMat.SetTexture(texKV.Key, texKV.Value);

                // if (option.TextureScaleOffsetReset)
                // {
                //     editableTMat.SetTextureScale(texKV.Key, Vector2.one);
                //     editableTMat.SetTextureOffset(texKV.Key, Vector2.zero);
                // }
                // if (option.BakedPropertyReset && bakePropMaxValue != null)
                // {
                //     foreach (var bakedPropertyDescriptor in supporter.GetBakePropertyNames(texKV.Key))
                //     {
                //         if (bakePropMaxValue.ContainsKey(bakedPropertyDescriptor.PropertyName) is false || bakedPropertyDescriptor.UseMaxValue is false)
                //         { editableTMat.PropertyReset(bakedPropertyDescriptor.PropertyName); }
                //         else { editableTMat.SetFloat(bakedPropertyDescriptor.PropertyName, bakePropMaxValue[bakedPropertyDescriptor.PropertyName]); }
                //     }
                // }
            }

            // foreach (var postProcess in supporter.AtlasMaterialPostProses) { postProcess.Proses(editableTMat); }

            return editableTMat;
        }
        class AtlasMatGenerateOption
        {
            public bool ForceSetTexture = false;
            public HashSet<Texture>? UnsetTextures = null;
        }



        internal static List<Renderer> FilteredRenderers(GameObject targetRoot, bool includeDisabledRenderer)
        {
            return targetRoot.GetComponentsInChildren<Renderer>(true).Where(r => AtlasAllowedRenderer(r, includeDisabledRenderer)).ToList();
        }

        internal static bool AtlasAllowedRenderer(Renderer item, bool includeDisabledRenderer)
        {
            if (includeDisabledRenderer is false) { if (item.gameObject.activeInHierarchy is false || item.enabled is false) { return false; } }
            if (item.tag == "EditorOnly") return false;
            if (item.GetMesh() == null) return false;
            if (item.GetMesh().uv.Any() == false) return false;
            if (item.sharedMaterials.Length == 0) return false;
            if (item.GetComponent<AtlasExcludeTag>() != null) return false;

            return true;
        }
        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            var nowContainsMatSet = new HashSet<Material>(
                    GetTargetAllowedFilter(rendererTargeting.EnumerateRenderer())
                        .SelectMany(r => rendererTargeting.GetMaterials(r))
                        .Where(i => i != null)
                        .Cast<Material>()
                );
            var selectedMaterials = rendererTargeting.LookAtGet(
                    this
                    , at => at.SelectMatList.Select(sMat => sMat.Material).ToArray()
                    , (l, r) => l.SequenceEqual(r)
                );
            var targetMaterials = nowContainsMatSet
                .Where(mat =>
                    selectedMaterials.Any(sMat => rendererTargeting.OriginEqual(sMat, mat))
                ).ToHashSet();
            return rendererTargeting.RendererFilterForMaterial(targetMaterials);
        }



    }
}
