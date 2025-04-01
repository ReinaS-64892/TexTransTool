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
using net.rs64.TexTransTool.TextureAtlas.IslandSizePriorityTuner;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [AddComponentMenu(TTTName + "/" + MenuPath)]
    public sealed partial class AtlasTexture : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT AtlasTexture";
        internal const string MenuPath = ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.Optimizing;

        // targeting
        [FormerlySerializedAs("TargetRoot")] public GameObject? LimitCandidateMaterials;
        public List<Material?> AtlasTargetMaterials = new List<Material?>();

        // material merge
        // public List<MaterialMergeGroup> MergeMaterialGroups = new();
        // [Serializable]
        // public class MaterialMergeGroup
        // {
        //     public List<Material> Group = new();
        //     public Material? Reference;
        // }
        // public Material? AllMaterialMergeReference;

        // IslandSizePriorityTuner
        [SerializeReference, SubclassSelector] internal List<IIslandSizePriorityTuner?> IslandSizePriorityTuner = new();

        // Other atlasing Settings
        public AtlasSetting AtlasSetting = new AtlasSetting();


        internal override void Apply(IDomain domain)
        {
            using var rpf = new PFScope("AtlasTexture");
            using var pf = new PFScope("init");
            domain.LookAt(this);

            if (AtlasTargetMaterials.Where(i => i != null).Any() is false) { TTLog.Info("AtlasTexture:info:TargetNotSet"); return; }

            pf.Split("targeting");

            var nowRenderers = GetTargetAllowedFilter(domain.EnumerateRenderer());
            var targetMaterials = GetTargetMaterials(domain, nowRenderers).ToHashSet();

            if (targetMaterials.Any() is false) { TTLog.Info("AtlasTexture:info:TargetNotFound"); return; }

            pf.Split("looking");

            // foreach (var mmg in MergeMaterialGroups) { if (mmg.Reference != null) { domain.LookAt(mmg.Reference); } }
            // if (AllMaterialMergeReference != null) { domain.LookAt(AllMaterialMergeReference); }

            pf.Split("prepare");

            var engine = domain.GetTexTransCoreEngineForUnity();
            var targeting = domain as IRendererTargeting;
            var targetRenderers = FilterTargetRenderers(targeting, nowRenderers, targetMaterials);
            var atlasSetting = AtlasSetting;

            pf.Split("Atlasing!");
            // Do Atlasing!
            var atlasResult = DoAtlasTexture(domain, engine, targetMaterials, targetRenderers, IslandSizePriorityTuner, atlasSetting);
            using var atlasContext = atlasResult.AtlasContext;
            var atlasedMeshes = atlasResult.AtlasedMeshes;
            var compiledAtlasTextures = atlasResult.CompiledAtlasTextures;

            pf.Split("tex fine tuning");
            //Texture Fine Tuning
            var tunedAtlasTextures = DoTextureFinTuning(engine, atlasContext, atlasSetting, compiledAtlasTextures);

            var tunedAtlasUnityTextures = tunedAtlasTextures.RenderTextures.ToDictionary(i => i.Key, i => engine.GetReferenceRenderTexture(i.Value));
            var usedRT = new HashSet<ITTRenderTexture>(tunedAtlasTextures.TextureDescriptors.Keys);

            pf.Split("replace mesh");
            //Mesh Change
            ReplaceMesh(domain, targetRenderers, atlasContext, atlasedMeshes);

            pf.Split("gen and replace material");
            //MaterialGenerate And Change
            ReplaceAtlasedMaterials(domain, targetMaterials, atlasSetting, tunedAtlasUnityTextures);

            pf.Split("register textures");
            // Register AtlasedTextures
            foreach (var t in compiledAtlasTextures.Values) { if (usedRT.Contains(t) is false) t.Dispose(); }
            foreach (var aTex in tunedAtlasTextures.TextureDescriptors)
                domain.RegisterPostProcessingAndLazyGPUReadBack(aTex.Key, aTex.Value);
        }

        internal static Renderer[] FilterTargetRenderers(IRendererTargeting targeting, List<Renderer> nowRenderers, HashSet<Material> targetMaterials)
        {
            return nowRenderers
                .Where(r => targeting.GetMesh(r) != null)
                .Where(r => targeting.GetMaterials(r)
                    .Where(i => i != null)
                    .Cast<Material>()
                    .Any(targetMaterials.Contains)
                ).ToArray();
        }

        internal static AtlasResult DoAtlasTexture(
            IDomain domain
            , ITexTransToolForUnity engine

            , HashSet<Material> targetMaterials
            , Renderer[] targetRenderers

            , List<IIslandSizePriorityTuner?> islandSizePriorityTuner

            , AtlasSetting atlasSetting
        )
        {
            using var pf = new PFScope("init");
            var targeting = domain as IRendererTargeting;

            var atlasTargeSize = GetAtlasTextureSize(atlasSetting);
            pf.Split("AtlasContext ctr");
            var atlasContext = new AtlasContext(
                               targeting
                               , targetRenderers
                               , targetMaterials
                               , new()
                               {
                                   AtlasTargetUVChannel = atlasSetting.AtlasTargetUVChannel,
                                   PrimaryTexturePropertyOrMaximum = atlasSetting.UsePrimaryMaximumTexture ? null : atlasSetting.PrimaryTextureProperty,
                                   AtlasIslandContextOption = new(),
                               }
                               );



            pf.Split("PixelNormalize");
            //アイランドまわり
            if (atlasSetting.PixelNormalize)
                atlasContext.SourceVirtualIslandNormalize();


            pf.Split("IslandProcessing");
            var (movedVirtualIslandArray, relocateResult) = IslandProcessing(domain, atlasSetting, atlasContext, islandSizePriorityTuner, out var relocationTime);
            if (relocateResult.IslandRelocationResult is null) { throw new Exception(); }
            if (relocateResult.IslandRelocationResult.IsSuccess is false) { throw new Exception(); }
            var source2MovedVirtualIsland = new Dictionary<IslandTransform, IslandTransform>();
            for (var i = 0; movedVirtualIslandArray.Length > i; i += 1)
            {
                source2MovedVirtualIsland[atlasContext.SourceVirtualIslands[i]] = movedVirtualIslandArray[i];
            }


            pf.Split("check hight");
            //上側を削れるかを見る
            var height = IslandRelocationManager.CalculateIslandsMaxHeight(movedVirtualIslandArray) + atlasSetting.IslandPadding;
            var atlasTextureHeightSize = Mathf.Max(GetNormalizedMinHeightSize(atlasTargeSize.y, height), 4);//4以下はちょっと怪しい挙動しそうだからクランプ
            Debug.Assert(Mathf.IsPowerOfTwo(atlasTextureHeightSize));

            TTLog.Info("AtlasTexture:info:RelocateResult", 1 - height, relocateResult.PriorityDownScale, relocateResult.OverallDownScale, relocateResult.TotalRelocateCount, relocationTime);
            var atlasedTextureSize = new Vector2Int(atlasTargeSize.x, atlasTextureHeightSize);

            //新しいUVを持つMeshを生成するフェーズ

            pf.Split("GenerateAtlasedMesh");
            // SubSetIndex と対応する
            var atlasedMeshes = atlasContext.GenerateAtlasedMesh(atlasSetting, source2MovedVirtualIsland, atlasedTextureSize);
            pf.Split("GenerateAtlasedTextures");
            var compiledAtlasTextures = atlasContext.GenerateAtlasedTextures(
                  engine
                  , atlasSetting
                  , atlasedTextureSize
                  , relocateResult.IslandRelocationResult.IsRectangleMove
                  , source2MovedVirtualIsland
              );
            pf.Split("exit");
            return new(atlasContext, atlasedMeshes, compiledAtlasTextures);
        }
        internal record AtlasResult
        {
            // Dispose はきちんとする必要がある。
            // Mesh や RT は後に渡したりとかになるが AtlasContext には気をつけるようにね
            public readonly AtlasContext AtlasContext;
            public readonly Mesh[] AtlasedMeshes;
            public readonly Dictionary<string, ITTRenderTexture> CompiledAtlasTextures;

            public AtlasResult(AtlasContext atlasContext, Mesh[] atlasedMesh, Dictionary<string, ITTRenderTexture> compiledAtlasTextures)
            {
                AtlasContext = atlasContext;
                AtlasedMeshes = atlasedMesh;
                CompiledAtlasTextures = compiledAtlasTextures;
            }
        }
        internal static TexFineTuningResult DoTextureFinTuning(ITexTransToolForUnity engine, AtlasContext atlasContext, AtlasSetting atlasSetting, Dictionary<string, ITTRenderTexture> compiledAtlasTextures)
        {
            var atlasTexFineTuningTargets = TexFineTuningUtility.InitTexFineTuningHolders(compiledAtlasTextures.Keys);

            // AutoTunings
            if (atlasSetting.AutoTextureSizeSetting) SetSizeDataMaxSize(atlasTexFineTuningTargets, atlasContext.MaterialGroupingCtx);
            if (atlasSetting.AutoMergeTextureSetting) DefaultMargeTextureDictTuning(atlasTexFineTuningTargets, atlasContext.MaterialGroupingCtx);
            if (atlasSetting.AutoReferenceCopySetting) DefaultRefCopyTuning(atlasTexFineTuningTargets, atlasContext.MaterialGroupingCtx);

            foreach (var fineTuning in atlasSetting.TextureFineTuning) { fineTuning?.AddSetting(atlasTexFineTuningTargets); }
            IndividualTuningAddSettings(atlasTexFineTuningTargets, atlasSetting.TextureIndividualFineTuning);

            var tunedAtlasTextures = TexFineTuningUtility.ProcessingTextureFineTuning(engine, atlasTexFineTuningTargets, compiledAtlasTextures);
            return tunedAtlasTextures;
        }

        internal static void ReplaceMesh(IDomain domain, Renderer[] targetRenderers, AtlasContext atlasContext, Mesh[] atlasedMeshes)
        {
            var registered = new HashSet<Mesh>();
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

                if (registered.Contains(atlasMesh) is false)
                {
                    domain.RegisterReplace(mesh, atlasMesh);
                    registered.Add(atlasMesh);
                }
            }
            domain.TransferAssets(atlasedMeshes);
        }

        internal static void ReplaceAtlasedMaterials(
            IDomain domain
            , HashSet<Material> targetMaterials
            , AtlasSetting atlasSetting
            , Dictionary<string, RenderTexture> tunedAtlasUnityTextures
            )
        {
            var atlasMatOption = new AtlasMatGenerateOption() { ForceSetTexture = atlasSetting.ForceSetTexture };
            if (atlasSetting.UnsetTextures.Any())
            {
                var containsAllTexture = targetMaterials.SelectMany(mat => mat.GetAllTextureWithDictionary().Select(i => i.Value));
                atlasMatOption.UnsetTextures = atlasSetting.UnsetTextures.Select(i => i.GetTexture()).SelectMany(ot => containsAllTexture.Where(ct => domain.OriginEqual(ot, ct))).ToHashSet();
            }
            var materialMap = GenerateAtlasedMaterials(targetMaterials, tunedAtlasUnityTextures, atlasMatOption);

            domain.ReplaceMaterials(materialMap);
            domain.RegisterReplaces(materialMap);
        }

        private static Dictionary<Material, Material> GenerateAtlasedMaterials<Tex>(HashSet<Material> targetMaterials, Dictionary<string, Tex> tex, AtlasMatGenerateOption atlasMatOption)
        where Tex : Texture
        {
            var materialMap = new Dictionary<Material, Material>();
            foreach (var distMat in targetMaterials)
            {
                materialMap[distMat] = GenerateAtlasMat(distMat, tex, atlasMatOption);
            }
            return materialMap;
        }
        /*
        private static (Dictionary<Material, Material> materialMap, Dictionary<Material, Material> domainsMaterial2ReplaceMaterial)
            GenerateAtlasedMaterials<Tex>(HashSet<Material> targetMaterials, Dictionary<string, Tex> tex, AtlasMatGenerateOption atlasMatOption, NowMaterialGroup[] mergeReferenceMaterial)
            where Tex : Texture
        {
            var materialMap = new Dictionary<Material, Material>();
            var domainsMaterial2ReplaceMaterial = new Dictionary<Material, Material>();
            var mergeRef2MergedMaterial = new Dictionary<Material, Material>();
            foreach (var distMat in targetMaterials)
            {
                var referenceMaterial = mergeReferenceMaterial.FirstOrDefault(g => g.GroupMaterial.Contains(distMat))?.MergeReference;
                if (referenceMaterial == null)
                {
                    domainsMaterial2ReplaceMaterial[distMat] = GenerateAtlasMat(distMat, tex, atlasMatOption);
                    materialMap.Add(distMat, domainsMaterial2ReplaceMaterial[distMat]);
                }
                else
                {
                    if (mergeRef2MergedMaterial.ContainsKey(referenceMaterial) is false)
                    { domainsMaterial2ReplaceMaterial[referenceMaterial] = mergeRef2MergedMaterial[referenceMaterial] = GenerateAtlasMat(referenceMaterial, tex, atlasMatOption); }
                    materialMap.Add(distMat, mergeRef2MergedMaterial[referenceMaterial]);
                }
            }
            return (materialMap, domainsMaterial2ReplaceMaterial);
        }
        private static NowMaterialGroup[] GenerateMergeReference(OriginEqual originEqual, HashSet<Material> targetMaterials, List<MaterialMergeGroup> mergeMaterialGroups, Material? allMaterialMergeReference)
        {
            var matGroupList = new List<NowMaterialGroup>();
            foreach (var mmg in mergeMaterialGroups)
            {
                var domainMaterialGroup = originEqual.GetDomainsMaterialsHashSet(targetMaterials, mmg.Group);
                var domainMaterialMergeRef = mmg.Reference != null ? originEqual.GetDomainsMaterial(targetMaterials, mmg.Reference) ?? mmg.Reference : null;

                matGroupList.Add(new(domainMaterialGroup, domainMaterialMergeRef));
            }

            var domainAllMaterialMergeRef = allMaterialMergeReference != null ? originEqual.GetDomainsMaterial(targetMaterials, allMaterialMergeReference) ?? allMaterialMergeReference : null;
            matGroupList.Add(new(targetMaterials, domainAllMaterialMergeRef));

            return matGroupList.ToArray();
        }
        */
        record NowMaterialGroup
        {
            public HashSet<Material> GroupMaterial;
            public Material? MergeReference;

            public NowMaterialGroup(HashSet<Material> groupMaterial, Material? mergeReference)
            {
                GroupMaterial = groupMaterial;
                MergeReference = mergeReference;
            }
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
            , List<IIslandSizePriorityTuner?> islandSizePriorityTuner
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

            var sizePriority = new float[virtualIslandArray.Length];
            for (var i = 0; sizePriority.Length > i; i += 1) { sizePriority[i] = 1f; }

            foreach (var islandFineTuner in islandSizePriorityTuner)
                islandFineTuner?.Tuning(sizePriority, refOriginIslands, islandDescription, domain);

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

            var sharedMaterialsCache = new Dictionary<Renderer, Material[]>();
            for (var i = 0; atlasContext.SourceVirtualIslands.Length > i; i += 1)
            {
                var refOriginIsland = refOriginIslands[i];
                var atlasSubMeshIndexID = atlasContext.AtlasIslandCtx.ReverseOriginDict[refOriginIsland];
                var md = atlasContext.NormalizedMeshCtx.GetMeshDataFromMeshID(atlasSubMeshIndexID.MeshID);

                var vertex = md.Vertices;
                var uv = md.VertexUV;
                var renderer = md.ReferenceRenderer;

                if (sharedMaterialsCache.TryGetValue(renderer, out var rMats) is false)
                    rMats = sharedMaterialsCache[renderer] = renderer.sharedMaterials;

                islandDescription[i] = new IslandSelector.IslandDescription(vertex, uv, renderer, rMats, atlasSubMeshIndexID.SubMeshIndex);
            }
            return islandDescription;
        }

        internal static void PostVirtualIslandProcessing(IslandTransform[] virtualIslandArray, Vector2Int atlasTargeSize, bool pixelNormalize)
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
        internal static void SetSizeDataMaxSize(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, MaterialGroupingContext materialGroupingCtx)
        {
            foreach (var tuningTarget in atlasTexFineTuningTargets)
            {
                var maxSize = materialGroupingCtx.GroupMaterials.Max(i => i.GroupedTexture.TryGetValue(tuningTarget.Key, out var tex) ? tex.width : 0);
                var sizeData = tuningTarget.Value.Get<SizeData>();
                sizeData.TextureSize = maxSize;
            }
        }
        internal static void DefaultRefCopyTuning(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, MaterialGroupingContext materialGroupingCtx)
        {


            var prop = atlasTexFineTuningTargets.Keys.ToArray();
            var refCopyDict = new Dictionary<string, string>();

            var gtHash = prop.ToDictionary(p => p, p => materialGroupingCtx.GroupMaterials.Where(i => i.GroupedTexture.ContainsKey(p)).Select(i => i.GroupedTexture[p]).ToHashSet());

            for (var i = 0; prop.Length > i; i += 1)
                for (var i2 = 0; prop.Length > i2; i2 += 1)
                {
                    if (i == i2) { continue; }

                    var copySource = prop[i];
                    var copyTarget = prop[i2];

                    var sTexHash = gtHash[copySource];
                    var tTexHash = gtHash[copyTarget];

                    if (sTexHash.SetEquals(tTexHash)) { refCopyDict.Remove(copySource); }

                    if (sTexHash.IsSupersetOf(tTexHash) is false) { continue; }
                    refCopyDict[copyTarget] = copySource;
                }


            var isContinue = true;
            var values = refCopyDict.Values.ToArray();
            while (isContinue)
            {
                foreach (var s in values)
                {
                    if (refCopyDict.ContainsKey(s))
                    {
                        foreach (var t in refCopyDict.Where(i => i.Value == s).ToArray())
                        {
                            refCopyDict[t.Key] = refCopyDict[s];
                        }
                        isContinue = true;
                    }
                }
                isContinue = false;
            }

            var copyRefResult = refCopyDict.GroupBy(i => i.Value).ToDictionary(i => i.Key, i => i.Select(k => k.Key).ToList());

            foreach (var fineTuning in copyRefResult.Select(i => new ReferenceCopy(new(i.Key), i.Value.Select(i => new PropertyName(i)).ToList())))
            {
                (fineTuning as ITextureFineTuning).AddSetting(atlasTexFineTuningTargets);
            }
        }

        internal static void DefaultMargeTextureDictTuning(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, MaterialGroupingContext materialGroupingCtx)
        {

            var prop = atlasTexFineTuningTargets.Keys.ToArray();
            var prop2MatIDs = prop.ToDictionary(
                p => p,
                p => materialGroupingCtx.GroupMaterials
                    .Where(gt => gt.GroupedTexture.ContainsKey(p))
                    .Select(gt => materialGroupingCtx.GetMaterialGroupID(gt))
                    .ToHashSet()
            );

            var alreadySetting = new HashSet<string>();
            var margeSettingDict = new Dictionary<string, List<string>>();


            for (var i = 0; prop.Length > i; i += 1)
            {
                var mergeParent = prop[i];
                var mergedHash = prop2MatIDs[mergeParent].ToHashSet();
                var childList = new List<string>();

                for (var i2 = i; prop.Length > i2; i2 += 1)
                {
                    if (i == i2) { continue; }

                    var mergeChild = prop[i2];

                    if (alreadySetting.Contains(mergeChild)) { continue; }

                    var childHash = prop2MatIDs[mergeChild];

                    if (mergedHash.Overlaps(childHash)) { continue; }

                    childList.Add(mergeChild);
                    mergedHash.UnionWith(childHash);
                    alreadySetting.Add(mergeChild);
                }

                if (childList.Any()) { margeSettingDict[mergeParent] = childList; }
            }

            foreach (var fineTuning in margeSettingDict.Select(i => new MergeTexture(new(i.Key), i.Value.Select(i => new PropertyName(i)).ToList())))
            {
                (fineTuning as ITextureFineTuning).AddSetting(atlasTexFineTuningTargets);
            }
        }

        internal static void IndividualTuningAddSettings(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, List<TextureIndividualTuning> individualTunings)
        {
            var individualApplied = new HashSet<string>();
            foreach (var individualTuning in individualTunings)
            {
                if (atlasTexFineTuningTargets.ContainsKey(individualTuning.TuningTarget) is false) { continue; }
                if (individualApplied.Contains(individualTuning.TuningTarget)) { continue; }
                individualApplied.Add(individualTuning.TuningTarget);

                var tuningTarget = atlasTexFineTuningTargets[individualTuning.TuningTarget];

                if (individualTuning.OverrideReferenceCopy) { tuningTarget.Get<ReferenceCopyData>().CopySource = individualTuning.CopyReferenceSource; }
                if (individualTuning.OverrideResize) { tuningTarget.Get<SizeData>().TextureSize = individualTuning.TextureSize; }
                if (individualTuning.OverrideCompression) { tuningTarget.Get<TextureCompressionTuningData>().CopyFrom(individualTuning.CompressionData); }
                if (individualTuning.OverrideMipMapRemove) { tuningTarget.Get<MipMapData>().UseMipMap = individualTuning.UseMipMap; }
                if (individualTuning.OverrideColorSpace) { tuningTarget.Get<ColorSpaceData>().AsLinear = individualTuning.AsLinear; }
                if (individualTuning.OverrideRemove) { tuningTarget.Get<RemoveData>().IsRemove = individualTuning.IsRemove; }
                if (individualTuning.OverrideMargeTexture) { tuningTarget.Get<MergeTextureData>().MargeParent = individualTuning.MargeRootProperty; }
            }
        }
        internal List<Renderer> GetTargetAllowedFilter(IEnumerable<Renderer> domainRenderers)
        { return domainRenderers.Where(i => AtlasAllowedRenderer(i, AtlasSetting.IncludeDisabledRenderer)).ToList(); }
        private static Material GenerateAtlasMat<Tex>(Material targetMat, Dictionary<string, Tex> atlasTex, AtlasMatGenerateOption option)
        where Tex : Texture
        {
            var editableTMat = UnityEngine.Object.Instantiate(targetMat);
            editableTMat.name = targetMat.name + "(TTT AtlasedMaterial)";

            foreach (var texKV in atlasTex)
            {
                if (editableTMat.HasTexture(texKV.Key) is false) { continue; }
                var tex = editableTMat.GetTexture(texKV.Key);
                if (tex == null) { tex = null; }//これは Unity側の Null を C# の Null にしてるやつ

                if (option.ForceSetTexture is false && tex == null) { continue; }
                if (tex is not Texture2D && tex is not RenderTexture && tex is not null) { continue; }

                if (tex != null && option.UnsetTextures is not null && option.UnsetTextures.Contains(tex)) { continue; }

                editableTMat.SetTexture(texKV.Key, texKV.Value);
            }
            return editableTMat;
        }
        class AtlasMatGenerateOption
        {
            public bool ForceSetTexture = false;
            public HashSet<Texture>? UnsetTextures = null;
        }




        internal List<Material> GetTargetMaterials(IDomain domain, List<Renderer> nowRenderers)
        {
            var nowContainsMatSet = new HashSet<Material>(RendererUtility.GetMaterials(nowRenderers).Where(i => i != null));
            var targetMaterials = nowContainsMatSet.Where(mat => AtlasTargetMaterials.Any(sMat => domain.OriginEqual(sMat, mat))).ToList();
            return targetMaterials;
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
                    , at => at.AtlasTargetMaterials.ToArray()
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
