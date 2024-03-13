using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;
using Island = net.rs64.TexTransCore.Island.Island;
using static net.rs64.TexTransCore.TransTextureCore.TransTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using net.rs64.TexTransTool.TextureAtlas.IslandRelocator;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [AddComponentMenu("TexTransTool/TTT AtlasTexture")]
    public sealed class AtlasTexture : TexTransRuntimeBehavior
    {
        public GameObject TargetRoot;
        public List<Renderer> Renderers => FilteredRenderers(TargetRoot, AtlasSetting.IncludeDisabledRenderer);
        public List<MatSelector> SelectMatList = new List<MatSelector>();

        public AtlasSetting AtlasSetting = new AtlasSetting();

        internal override bool IsPossibleApply => TargetRoot != null;

        internal override List<Renderer> GetRenderers => Renderers;

        internal override TexTransPhase PhaseDefine => TexTransPhase.UVModification;

        #region V0SaveData
        [Obsolete("V0SaveData", true)] public List<AtlasTexture> MigrationV0ObsoleteChannelsRef;
        [Obsolete("V0SaveData", true)] public List<Material> SelectReferenceMat;//OrderedHashSetにしたかったけどシリアライズの都合で
        [Obsolete("V0SaveData", true)] public List<MatSelectorV0> MatSelectors = new List<MatSelectorV0>();
        [Serializable]
        [Obsolete("V0SaveData", true)]
        public class MatSelectorV0
        {
            public Material Material;
            public bool IsTarget = false;
            public int AtlasChannel = 0;
            public float TextureSizeOffSet = 1;
        }
        [Obsolete("V0SaveData", true)][SerializeField] internal List<AtlasSetting> AtlasSettings = new List<AtlasSetting>() { new AtlasSetting() };
        [Obsolete("V0SaveData", true)] public bool UseIslandCache = true;
        #endregion


        // public override bool IsPossibleCompile => TargetRoot != null && AtlasSettings.Count > 0;
        /*
        TargetRenderers 対象となるのはメッシュが存在しマテリアルスロットにNullが含まれていないかつ、無効化されておらず、エディターオンリーではないもの。

        MaterialReference レンダラーから集めた重複のないマテリアルの配列に対してのインデックス。

        MeshReference 上のメッシュ版。

        AtlasMeshData すべてのレンダラーをMeshReferenceとMaterialReferenceに変換し、まったく同じReferenceを持つものを消した物


        AtlasSettings アトラス化するときの細かい設定

        SelectRefsMat インスペクター上で表示されているマテリアルたちの配列。
        MatSelectors SelectRefsMatに含まれているマテリアルの参照を持ち マテリアルがターゲットであるか、大きさのオフセットやどのChannelに属しているかの情報を持っている。

        MatData MatSelectorsをMaterialReferenceとTextureSizeOffSet、PropAndTexturesにしたもの。
        このMaterialReferenceはSelectRefsMatを使ってインデックスに変換している。

        MeshAndMatRef MeshReferenceとマテリアルスロット分のMaterialReferenceをもち、適応するときこれをもとに、マテリアル違いやマテリアルの一部違いなども識別して適応する。

        */

        struct AtlasData
        {
            public List<PropAndTexture2D> Textures;
            public List<AtlasMeshAndDist> Meshes;
            public List<MatSelector> AtlasInMaterials;

            public struct AtlasMeshAndDist
            {
                public Mesh DistMesh;
                public Mesh AtlasMesh;
                public Material[] Mats;

                public AtlasMeshAndDist(Mesh distMesh, Mesh atlasMesh, Material[] mats)
                {
                    DistMesh = distMesh;
                    AtlasMesh = atlasMesh;
                    Mats = mats;
                }
            }
        }
        [Serializable]
        public struct MatSelector
        {
            public Material Material;
            public float AdditionalTextureSizeOffSet;

            #region V1SaveData
            [Obsolete("V1SaveData", true)][SerializeField] internal float TextureSizeOffSet;
            #endregion

        }
        struct MatData
        {
            public Material Material;
            public float TextureSizeOffSet;
            public List<PropAndTexture> PropAndTextures;//ここには Texture2D か TempRendererTextureが入ってる

            public MatData(MatSelector matSelector, List<PropAndTexture> propAndTextures)
            {
                Material = matSelector.Material;
                TextureSizeOffSet = matSelector.AdditionalTextureSizeOffSet;
                PropAndTextures = propAndTextures;
            }
        }

        bool TryCompileAtlasTextures(IDomain domain, out AtlasData atlasData)
        {
            var texManage = domain.GetTextureManager();
            atlasData = new AtlasData();


            //情報を集めるフェーズ
            var NowContainsMatSet = new HashSet<Material>(RendererUtility.GetMaterials(Renderers));
            var targetMaterialSelectors = SelectMatList.Select(matS =>
            {
                matS.Material = domain.TryReplaceQuery(matS.Material, out var rMat) ? (Material)rMat : matS.Material;
                return matS;
            }).Where(I => I.Material != null && NowContainsMatSet.Contains(I.Material)).GroupBy(i => i.Material).Select(i => i.First()).ToList();
            atlasData.AtlasInMaterials = targetMaterialSelectors;
            var atlasSetting = AtlasSetting;
            var atlasReferenceData = new AtlasReferenceData(targetMaterialSelectors.Select(I => I.Material).ToList(), Renderers);
            var shaderSupports = new AtlasShaderSupportUtils();

            //サブメッシュより多いスロットの存在可否
            if (atlasReferenceData.AtlasMeshDataList.Any(i => i.Triangles.Count < i.MaterialIndex.Length)) { TTTRuntimeLog.Warning("AtlasTexture:error:MoreMaterialSlotThanSubMesh"); }


            //ターゲットとなるマテリアルやそのマテリアルが持つテクスチャを引き出すフェーズ
            shaderSupports.BakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
            var materialTextures = new Dictionary<Material, List<PropAndTexture>>();
            foreach (var matSelector in targetMaterialSelectors) { shaderSupports.AddRecord(matSelector.Material); }
            foreach (var matSelector in targetMaterialSelectors) { materialTextures[matSelector.Material] = shaderSupports.GetTextures(matSelector.Material, texManage); }
            shaderSupports.ClearRecord();

            var materialAdditionalTextureOffset = new Dictionary<Material, float>();
            foreach (var matSelector in targetMaterialSelectors) { materialAdditionalTextureOffset[matSelector.Material] = matSelector.AdditionalTextureSizeOffSet; }



            //アイランドまわり
            var originIslandPool = atlasReferenceData.GeneratedIslandPool(domain.GetIslandCacheManager());

            //サブメッシュ間で頂点を共有するアイランドのマージ
            var containsIdenticalIslandForMultipleSubMesh = false;
            for (var amdIndex = 0; atlasReferenceData.AtlasMeshDataList.Count > amdIndex; amdIndex += 1)
            {
                var amd = atlasReferenceData.AtlasMeshDataList[amdIndex];

                var beyondVert = amd.Triangles.Where(i => atlasReferenceData.TargetMaterials.Contains(atlasReferenceData.Materials[amd.MaterialIndex[amd.Triangles.IndexOf(i)]]))
                .Select(i => new HashSet<int>(i.SelectMany(i2 => i2))).SelectMany(i => i)
                .GroupBy(i => i).Select(i => (i.Key, i.Count())).Where(i => i.Item2 > 1).Select(i => i.Key).ToHashSet();

                if (beyondVert.Any()) { containsIdenticalIslandForMultipleSubMesh = true; }
                else { continue; }

                var needMerge = originIslandPool.Where(i => i.Key.AtlasMeshDataIndex == amdIndex).Where(i => i.Value.triangles.SelectMany(i => i).Any(i => beyondVert.Contains(i))).GroupBy(i => i.Key.MaterialSlot).ToList();
                needMerge.Sort((l, r) => l.Key - r.Key);

                var needMergeIslands = needMerge.Select(i => i.ToHashSet()).ToArray();
                var MargeKV = new Dictionary<AtlasIslandID, HashSet<AtlasIslandID>>();

                for (var toIndex = 0; needMergeIslands.Length > toIndex; toIndex += 1)
                {
                    foreach (var island in needMergeIslands[toIndex])
                    {
                        var vertSet = island.Value.triangles.SelectMany(i => i).ToHashSet();

                        for (var fromIndex = toIndex; needMergeIslands.Length > fromIndex; fromIndex += 1)
                        {
                            if (toIndex == fromIndex) { continue; }

                            var margeFrom = needMergeIslands[fromIndex].Where(il => il.Value.triangles.SelectMany(v => v).Any(v => vertSet.Contains(v)));
                            if (margeFrom.Any()) { MargeKV.Add(island.Key, margeFrom.Select(i => i.Key).ToHashSet()); }
                        }
                    }
                }

                foreach (var margeIdKV in MargeKV)
                {
                    var to = originIslandPool[margeIdKV.Key];

                    foreach (var formKey in margeIdKV.Value)
                    {
                        to.triangles.AddRange(originIslandPool[formKey].triangles);
                        originIslandPool.Remove(formKey);
                    }
                }

            }
            if (containsIdenticalIslandForMultipleSubMesh) { TTTRuntimeLog.Warning("AtlasTexture:error:IdenticalIslandForMultipleSubMesh"); }



            if (atlasSetting.PixelNormalize)
            {
                foreach (var islandKV in originIslandPool)
                {
                    var material = materialTextures[atlasReferenceData.GetMaterialReference(islandKV.Key)];
                    var refTex = material.FirstOrDefault(i => i.PropertyName == "_MainTex")?.Texture;
                    if (refTex == null) { continue; }
                    var island = islandKV.Value;
                    island.Pivot.y = Mathf.Round(island.Pivot.y * refTex.height) / refTex.height;
                    island.Pivot.x = Mathf.Round(island.Pivot.x * refTex.width) / refTex.width;
                }
            }

            var islandSizeOffset = new Dictionary<Material, float>();
            foreach (var material in materialAdditionalTextureOffset.Keys)
            {
                var tex = material.mainTexture;
                float defaultTextureSizeOffset;
                if (tex != null)
                {
                    var atlasTexPixelCount = atlasSetting.AtlasTextureSize * atlasSetting.AtlasTextureSize;
                    var texPixelCount = tex.width * tex.height;
                    defaultTextureSizeOffset = texPixelCount / (float)atlasTexPixelCount;
                }
                else { defaultTextureSizeOffset = (float)0.01f; }

                islandSizeOffset[material] = materialAdditionalTextureOffset[material] * Mathf.Sqrt(defaultTextureSizeOffset);
            }

            var islandRectPool = new Dictionary<AtlasIslandID, IslandRect>(originIslandPool.Count);
            foreach (var islandKV in originIslandPool)
            {
                var atlasIslandId = islandKV.Key;
                var islandRect = new IslandRect(islandKV.Value);

                islandRect.Size *= islandSizeOffset[atlasReferenceData.GetMaterialReference(atlasIslandId)];

                // if (islandRect.Size.x > 0.99) { islandRect.Size *= 0.99f / islandRect.Size.x; }//アルゴリズムのほうがよくなったからこんなことしなくてよくなった
                // if (islandRect.Size.y > 0.99) { islandRect.Size *= 0.99f / islandRect.Size.y; }

                islandRectPool[atlasIslandId] = islandRect;
            }

            // foreach (var offset in islandSizeOffset) { Debug.Log(offset.Key.name + "-" + offset.Value.ToString()); }

            IAtlasIslandRelocator relocator = atlasSetting.AtlasIslandRelocator != null ? UnityEngine.Object.Instantiate(atlasSetting.AtlasIslandRelocator) : new NFDHPlasFC();

            relocator.UseUpScaling = atlasSetting.UseUpScaling;
            relocator.Padding = atlasSetting.IslandPadding;

            islandRectPool = relocator.Relocation(islandRectPool, originIslandPool);
            var rectTangleMove = relocator.RectTangleMove;

            if (atlasSetting.PixelNormalize)
            {
                foreach (var key in originIslandPool.Keys)
                {
                    var island = islandRectPool[key];
                    island.Pivot.y = Mathf.Round(island.Pivot.y * atlasSetting.AtlasTextureSize) / atlasSetting.AtlasTextureSize;
                    island.Pivot.x = Mathf.Round(island.Pivot.x * atlasSetting.AtlasTextureSize) / atlasSetting.AtlasTextureSize;
                    islandRectPool[key] = island;
                }
            }

            //上側を削れるかを見る
            var height = IslandRectUtility.CalculateIslandsMaxHeight(islandRectPool.Values);
            var atlasTextureHeightSize = Mathf.Max(GetHeightSize(atlasSetting.AtlasTextureSize, height), 4);//4以下はちょっと怪しい挙動思想だからクランプ

            // var areaSum = IslandRectUtility.CalculateAllAreaSum(islandRectPool.Values);
            // Debug.Log(areaSum + ":AreaSum" + "-" + height + ":height");

            var aspectIslandsRectPool = GetAspectIslandRect(islandRectPool, atlasSetting, atlasTextureHeightSize);


            //新しいUVを持つMeshを生成するフェーズ
            var compiledMeshes = new List<AtlasData.AtlasMeshAndDist>();
            var poolContainsTags = ToIndexTags(islandRectPool.Keys);
            for (int I = 0; I < atlasReferenceData.AtlasMeshDataList.Count; I += 1)
            {
                var atlasMeshData = atlasReferenceData.AtlasMeshDataList[I];

                var distMesh = atlasReferenceData.Meshes[atlasMeshData.ReferenceMesh];
                var newMesh = UnityEngine.Object.Instantiate<Mesh>(distMesh);
                newMesh.name = "AtlasMesh_" + I + "_" + distMesh.name;

                var meshTags = new List<AtlasIdenticalTag>();

                for (var slotIndex = 0; atlasMeshData.MaterialIndex.Length > slotIndex; slotIndex += 1)
                {
                    var thisTag = new AtlasIdenticalTag(I, slotIndex);
                    if (poolContainsTags.Contains(thisTag))
                    {
                        meshTags.Add(thisTag);
                    }
                    else
                    {
                        var thisTagMeshRef = atlasMeshData.ReferenceMesh;
                        var thisTagMatSlot = slotIndex;
                        var thisTagMatRef = atlasMeshData.MaterialIndex[slotIndex];
                        AtlasIdenticalTag? identicalTag = FindIdenticalTag(atlasReferenceData, poolContainsTags, thisTagMeshRef, thisTagMatSlot, thisTagMatRef);

                        if (identicalTag.HasValue)
                        {
                            meshTags.Add(identicalTag.Value);
                        }
                    }
                }


                var movedPool = new Dictionary<AtlasIslandID, IIslandRect>();
                foreach (var tag in meshTags)
                {
                    foreach (var islandKVP in aspectIslandsRectPool.Where(i => i.Key.AtlasMeshDataIndex == tag.AtlasMeshDataIndex && i.Key.MaterialSlot == tag.MaterialSlot))
                    {
                        movedPool.Add(islandKVP.Key, islandKVP.Value);
                    }
                }

                var movedUV = new List<Vector2>(atlasMeshData.UV);
                IslandUtility.IslandPoolMoveUV(atlasMeshData.UV, movedUV, originIslandPool, movedPool);
                atlasMeshData.MovedUV = movedUV;

                newMesh.SetUVs(0, movedUV);
                if (AtlasSetting.WriteOriginalUV) { newMesh.SetUVs(1, atlasMeshData.UV); }

                compiledMeshes.Add(new AtlasData.AtlasMeshAndDist(distMesh, newMesh, atlasMeshData.MaterialIndex.Select(Index => atlasReferenceData.Materials[Index]).ToArray()));
            }
            atlasData.Meshes = compiledMeshes;


            //アトラス化したテクスチャーを生成するフェーズ
            var compiledAtlasTextures = new List<PropAndTexture2D>();

            var propertyNames = materialTextures.Values.SelectMany(i => i).Select(i => i.PropertyName).ToHashSet();


            foreach (var propName in propertyNames)
            {
                var targetRT = RenderTexture.GetTemporary(atlasSetting.AtlasTextureSize, atlasTextureHeightSize, 32);
                targetRT.Clear();
                targetRT.name = "AtlasTex" + propName;
                foreach (var MatPropKV in materialTextures)
                {
                    var souseProp2Tex = MatPropKV.Value.Find(I => I.PropertyName == propName);
                    if (souseProp2Tex == null) continue;
                    var souseTex = souseProp2Tex.Texture is Texture2D ? texManage.GetOriginTempRt(souseProp2Tex.Texture as Texture2D, souseProp2Tex.Texture.width) : souseProp2Tex.Texture;

                    if (rectTangleMove)
                    {

                        var islandPairs = new Dictionary<Island, IslandRect>();
                        foreach (var islandID in originIslandPool.Keys.Where(tag => atlasReferenceData.GetMaterialReference(tag) == MatPropKV.Key))
                        {
                            var Origin = originIslandPool[islandID];
                            if (!islandRectPool.ContainsKey(islandID)) { continue; }
                            var Moved = islandRectPool[islandID];

                            islandPairs.Add(Origin, Moved);
                        }

                        TransMoveRectIsland(souseTex, targetRT, islandPairs, atlasSetting.IslandPadding);
                        islandPairs.Clear();
                    }
                    else
                    {
                        foreach (var atlasAMDGroup in originIslandPool
                                            .Where(atlasIsland => atlasReferenceData.GetMaterialReference(atlasIsland.Key) == MatPropKV.Key)
                                            .GroupBy(atlasIsland => atlasIsland.Key.AtlasMeshDataIndex)
                                            )
                        {
                            var amd = atlasReferenceData.AtlasMeshDataList[atlasAMDGroup.Key];

                            var transData = new TransData<Vector2>(atlasAMDGroup.SelectMany(value => value.Value.triangles), amd.MovedUV, amd.UV);
                            ForTrans(targetRT, souseTex, transData, atlasSetting.GetTexScalePadding * 0.5f, null, true);
                        }
                    }

                    if (souseProp2Tex.Texture is Texture2D && souseTex is RenderTexture tempRt) { RenderTexture.ReleaseTemporary(tempRt); }
                }

                compiledAtlasTextures.Add(new PropAndTexture2D(propName, targetRT.CopyTexture2D()));
                RenderTexture.ReleaseTemporary(targetRT);
            }
            foreach (var matData in materialTextures)
            {
                foreach (var pTex in matData.Value)
                {
                    if (pTex.Texture == null) { continue; }
                    switch (pTex.Texture)
                    {
                        case RenderTexture renderTexture:
                            {
                                RenderTexture.ReleaseTemporary(renderTexture);
                                break;
                            }
                        case Texture2D texture2D:
                        default:
                            { break; }
                    }
                }
            }

            atlasData.Textures = compiledAtlasTextures;

            return true;
        }

        private static Dictionary<AtlasIslandID, IslandRect> GetAspectIslandRect(Dictionary<AtlasIslandID, IslandRect> islandRectPool, AtlasSetting atlasSetting, int atlasTextureHeightSize)
        {
            if (atlasTextureHeightSize != atlasSetting.AtlasTextureSize)
            {
                var heightSizeScale = atlasSetting.AtlasTextureSize / (float)atlasTextureHeightSize;
                var aspectIslands = new Dictionary<AtlasIslandID, IslandRect>();
                foreach (var id in islandRectPool.Keys)
                {
                    var islandRect = islandRectPool[id];
                    islandRect.Pivot.y *= heightSizeScale;
                    islandRect.Size.y *= heightSizeScale;
                    aspectIslands[id] = islandRect;
                }
                return aspectIslands;
            }
            else
            {
                return islandRectPool;
            }
        }

        private static int GetHeightSize(int atlasTextureSize, float height)
        {
            switch (height)
            {
                default: return atlasTextureSize;
                case < (1 / 8f): return atlasTextureSize / 8;
                case < (1 / 4f): return atlasTextureSize / 4;
                case < (1 / 2f): return atlasTextureSize / 2;
            }
        }

        internal override void Apply(IDomain domain = null)
        {
            if (!IsPossibleApply)
            {
                TTTRuntimeLog.Error("AtlasTexture:error:TTTNotExecutable");
                return;
            }

            domain.ProgressStateEnter("AtlasTexture");
            domain.ProgressUpdate("CompileAtlasTexture", 0f);

            if (!TryCompileAtlasTextures(domain, out var atlasData)) { return; }

            domain.ProgressUpdate("MeshChange", 0.5f);

            var nowRenderers = Renderers;

            var shaderSupport = new AtlasShaderSupportUtils();

            //Mesh Change
            foreach (var renderer in nowRenderers)
            {
                var mesh = renderer.GetMesh();
                var mats = renderer.sharedMaterials;
                var atlasMeshAndDist = atlasData.Meshes.FindAll(I => I.DistMesh == mesh).Find(I => I.Mats.SequenceEqual(mats));
                if (atlasMeshAndDist.AtlasMesh == null) { continue; }

                var atlasMesh = atlasMeshAndDist.AtlasMesh;
                domain.SetMesh(renderer, atlasMesh);
                domain.TransferAsset(atlasMesh);
            }

            domain.ProgressUpdate("Texture Fine Tuning", 0.75f);

            //Texture Fine Tuning
            var atlasTexFineTuningTargets = TexFineTuningUtility.ConvertForTargets(atlasData.Textures);
            TexFineTuningUtility.InitTexFineTuning(atlasTexFineTuningTargets);
            foreach (var fineTuning in AtlasSetting.TextureFineTuning)
            {
                fineTuning.AddSetting(atlasTexFineTuningTargets);
            }
            TexFineTuningUtility.FinalizeTexFineTuning(atlasTexFineTuningTargets);
            var atlasTexture = TexFineTuningUtility.ConvertForPropAndTexture2D(atlasTexFineTuningTargets);
            domain.transferAssets(atlasTexture.Select(PaT => PaT.Texture2D));

            //CompressDelegation
            foreach (var atlasTexFTData in atlasTexFineTuningTargets)
            {
                var tex = atlasTexFTData.Texture2D;
                var compressSetting = atlasTexFTData.TuningDataList.Find(I => I is CompressionQualityData) as CompressionQualityData;
                if (compressSetting == null) { continue; }
                var compressSettingTuple = (CompressionQualityApplicant.GetTextureFormat(tex, compressSetting), (int)compressSetting.CompressionQuality);
                domain.GetTextureManager().TextureCompressDelegation(compressSettingTuple, atlasTexFTData.Texture2D);
            }


            domain.ProgressUpdate("MaterialGenerate And Change", 0.9f);

            //MaterialGenerate And Change
            if (AtlasSetting.MergeMaterials)
            {
                var mergeMat = AtlasSetting.MergeReferenceMaterial != null ? AtlasSetting.MergeReferenceMaterial : atlasData.AtlasInMaterials.First().Material;
                Material generateMat = GenerateAtlasMat(mergeMat, atlasTexture, shaderSupport, AtlasSetting.ForceSetTexture);

                domain.ReplaceMaterials(atlasData.AtlasInMaterials.ToDictionary(x => x.Material, _ => generateMat), rendererOnly: true);
            }
            else
            {
                var materialMap = new Dictionary<Material, Material>();
                foreach (var MatSelector in atlasData.AtlasInMaterials)
                {
                    var distMat = MatSelector.Material;
                    var generateMat = GenerateAtlasMat(distMat, atlasTexture, shaderSupport, AtlasSetting.ForceSetTexture);
                    materialMap.Add(distMat, generateMat);
                }
                domain.ReplaceMaterials(materialMap);
            }

            domain.ProgressUpdate("End", 1);
            domain.ProgressStateExit();
        }

        private void TransMoveRectIsland<TIslandRect>(Texture souseTex, RenderTexture targetRT, Dictionary<Island, TIslandRect> notAspectIslandPairs, float uvScalePadding) where TIslandRect : IIslandRect
        {
            uvScalePadding *= 0.5f;
            var targetAspect = targetRT.width / (float)targetRT.height;
            var sUV = new List<Vector2>();
            var tUV = new List<Vector2>();
            var triangles = new List<TriangleIndex>();

            var nawIndex = 0;
            foreach (var islandPair in notAspectIslandPairs)
            {
                var (Origin, Moved) = (islandPair.Key, islandPair.Value);
                var rectScalePadding = Moved.UVScaleToRectScale(uvScalePadding);

                var originVertexes = Origin.GenerateRectVertexes(rectScalePadding);
                var movedVertexes = Moved.GenerateRectVertexes(rectScalePadding);

                triangles.Add(new(nawIndex + 0, nawIndex + 1, nawIndex + 2));
                triangles.Add(new(nawIndex + 0, nawIndex + 2, nawIndex + 3));
                nawIndex += 4;
                sUV.AddRange(originVertexes);
                tUV.AddRange(movedVertexes.Select(v => new Vector2(v.x, v.y * targetAspect)));
            }

            TransTexture.ForTrans(targetRT, souseTex, new TransData<Vector2>(triangles, tUV, sUV), argTexWrap: TextureWrap.Loop);

        }
        internal static float Frac(float v) { return v > 0 ? v - Mathf.Floor(v) : v - Mathf.Ceil(v); }
        internal static AtlasIdenticalTag? FindIdenticalTag(AtlasReferenceData atlasData, HashSet<AtlasIdenticalTag> poolTags, int findTagMeshRef, int findTagMatSlot, int findTagMatRef)
        {
            AtlasIdenticalTag? identicalTag = null;
            foreach (var pTag in poolTags)
            {
                var pTagTargetAMD = atlasData.AtlasMeshDataList[pTag.AtlasMeshDataIndex];
                var pTagMeshRef = pTagTargetAMD.ReferenceMesh;
                var pTagMatSlot = pTag.MaterialSlot;
                var pTagMatRef = pTagTargetAMD.MaterialIndex[pTag.MaterialSlot];

                if (findTagMeshRef == pTagMeshRef && findTagMatSlot == pTagMatSlot && findTagMatRef == pTagMatRef)
                {
                    identicalTag = pTag;
                    break;
                }
            }

            return identicalTag;
        }

        private static Dictionary<MatData, Dictionary<AtlasIslandID, AtlasIsland>> GetMatDataPool(AtlasReferenceData atlasData, Dictionary<AtlasIslandID, AtlasIsland> originIslandPool, List<MatData> matDataList)
        {
            var matDataPairPool = new Dictionary<MatData, Dictionary<AtlasIslandID, AtlasIsland>>();
            foreach (var matData in matDataList)
            {
                var separatePool = atlasData.FindMatIslandPool(originIslandPool, matData.Material, true);
                matDataPairPool.Add(matData, separatePool);
            }

            return matDataPairPool;
        }

        internal static HashSet<AtlasIdenticalTag> ToIndexTags(IEnumerable<AtlasIslandID> tags)
        {
            var indexTag = new HashSet<AtlasIdenticalTag>();
            foreach (var tag in tags)
            {
                indexTag.Add(new(tag.AtlasMeshDataIndex, tag.MaterialSlot));
            }

            return indexTag;
        }

        private static Material GenerateAtlasMat(Material targetMat, List<PropAndTexture2D> atlasTex, AtlasShaderSupportUtils shaderSupport, bool forceSetTexture)
        {
            var editableTMat = UnityEngine.Object.Instantiate(targetMat);

            editableTMat.SetTextures(atlasTex, forceSetTexture);
            //TODO : これどっかでいい感じに誰かがやらないといけない
            //editableTMat.RemoveUnusedProperties();
            shaderSupport.MaterialCustomSetting(editableTMat);
            return editableTMat;
        }



        internal static List<Renderer> FilteredRenderers(GameObject targetRoot, bool includeDisabledRenderer)
        {
            var result = new List<Renderer>();
            foreach (var item in targetRoot.GetComponentsInChildren<Renderer>(includeDisabledRenderer))
            {
                if (item.tag == "EditorOnly") continue;
                if (item.GetMesh() == null) continue;
                if (item.GetMesh().uv.Any() == false) continue;
                if (item.sharedMaterials.Length == 0) continue;
                if (item.sharedMaterials.Any(Mat => Mat == null)) continue;
                if (item.GetComponent<AtlasExcludeTag>() != null) continue;

                result.Add(item);
            }
            return result;
        }

    }
    internal class AtlasReferenceData
    {
        public OrderedHashSet<Mesh> Meshes;
        public HashSet<Material> TargetMaterials;
        public OrderedHashSet<Material> Materials;
        public List<AtlasMeshData> AtlasMeshDataList;
        public List<Renderer> Renderers;
        public AtlasReferenceData(List<Material> targetMaterials, List<Renderer> inputRenderers)
        {
            TargetMaterials = new HashSet<Material>(targetMaterials);
            Meshes = new(); Materials = new(); Renderers = new();

            foreach (var renderer in inputRenderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (TargetMaterials.Contains(mat))
                    {
                        Meshes.Add(renderer.GetMesh());
                        Materials.AddRange(renderer.sharedMaterials);
                        Renderers.Add(renderer);
                        break;
                    }
                }
            }

            AtlasMeshDataList = new();

            foreach (var renderer in Renderers)
            {
                var mesh = renderer.GetMesh();
                var refMesh = Meshes.IndexOf(mesh);
                var materialIndex = renderer.sharedMaterials.Select(Mat => Materials.IndexOf(Mat)).ToArray();

                var index = AtlasMeshDataList.FindIndex(AMD => AMD.ReferenceMesh == refMesh && AMD.MaterialIndex.SequenceEqual(materialIndex));
                if (index == -1)
                {
                    var uv = new List<Vector2>();
                    mesh.GetUVs(0, uv);

                    AtlasMeshDataList.Add(new(
                        refMesh,
                        mesh.GetSubTriangleIndex(),
                        uv,
                        materialIndex
                        ));
                }
            }
        }
        public class AtlasMeshData
        {
            //RefData
            public int ReferenceMesh;
            public int[] MaterialIndex;

            //for Generate
            public readonly List<List<TriangleIndex>> Triangles;
            public List<Vector2> UV;
            public List<Vector2> MovedUV;

            public AtlasMeshData(int referenceMesh, List<List<TriangleIndex>> triangles, List<Vector2> uv, int[] materialIndex)
            {
                ReferenceMesh = referenceMesh;
                Triangles = triangles;
                UV = uv;
                MaterialIndex = materialIndex;
            }
            public AtlasMeshData()
            {
                Triangles = new List<List<TriangleIndex>>();
                UV = new List<Vector2>();
            }
        }


        /// <summary>
        ///  すべてをアイランドにし、同一の物を指すアイランドは排除したものを返します。
        /// </summary>
        /// <param name="islandCache"></param>
        /// <returns></returns>
        public Dictionary<AtlasIslandID, AtlasIsland> GeneratedIslandPool(IIslandCache islandCache)
        {
            var islandPool = new Dictionary<AtlasIslandID, AtlasIsland>();
            var amdCount = AtlasMeshDataList.Count;
            var islandIndex = 0;
            for (int amdIndex = 0; amdIndex < amdCount; amdIndex += 1)
            {
                var atlasMeshData = AtlasMeshDataList[amdIndex];

                for (var SlotIndex = 0; atlasMeshData.MaterialIndex.Length > SlotIndex; SlotIndex += 1)
                {
                    if (!TargetMaterials.Contains(Materials[atlasMeshData.MaterialIndex[SlotIndex]])) { continue; }
                    if (atlasMeshData.Triangles.Count <= SlotIndex) { continue; }

                    var islands = IslandUtility.UVtoIsland(atlasMeshData.Triangles[SlotIndex], atlasMeshData.UV, islandCache);
                    foreach (var island in islands) { islandPool.Add(new AtlasIslandID(amdIndex, SlotIndex, islandIndex), new AtlasIsland(island, atlasMeshData.UV)); islandIndex += 1; }
                }
            }

            var refsHash = new HashSet<(int RefMesh, int MatSlot, int RefMat)>();
            var deleteTags = new HashSet<AtlasIdenticalTag>();
            foreach (var tag in islandPool.Keys.Select(i => new AtlasIdenticalTag(i.AtlasMeshDataIndex, i.MaterialSlot)).Distinct())
            {
                var atlasMeshData = AtlasMeshDataList[tag.AtlasMeshDataIndex];
                var refMesh = atlasMeshData.ReferenceMesh;
                var materialSlot = tag.MaterialSlot;
                var refMat = atlasMeshData.MaterialIndex[tag.MaterialSlot];
                var refs = (refMesh, materialSlot, refMat);

                if (refsHash.Contains(refs)) { deleteTags.Add(tag); }
                else { refsHash.Add(refs); }
            }

            var filteredIslandPool = new Dictionary<AtlasIslandID, AtlasIsland>(islandPool.Count);
            islandIndex = 0;
            foreach (var idPair in islandPool)
            {
                var atlasID = idPair.Key;
                var island = idPair.Value;

                if (deleteTags.Contains(new AtlasIdenticalTag(atlasID.AtlasMeshDataIndex, atlasID.MaterialSlot))) { continue; }

                atlasID.IslandIndex = islandIndex;
                filteredIslandPool.Add(atlasID, island);
                islandIndex += 1;
            }
            return filteredIslandPool;
        }

        public Material GetMaterialReference(AtlasIslandID indexTag)
        {
            return GetMaterialReference(indexTag.AtlasMeshDataIndex, indexTag.MaterialSlot);
        }
        private Material GetMaterialReference(int atlasMeshDataIndex, int materialSlot)
        {
            return Materials[AtlasMeshDataList[atlasMeshDataIndex].MaterialIndex[materialSlot]];
        }


        public Dictionary<AtlasIslandID, AtlasIsland> FindMatIslandPool(Dictionary<AtlasIslandID, AtlasIsland> souse, Material matRef, bool deepClone = true)
        {
            var result = new Dictionary<AtlasIslandID, AtlasIsland>();
            foreach (var islandKVP in souse)
            {
                if (GetMaterialReference(islandKVP.Key) == matRef)
                {
                    result.Add(islandKVP.Key, deepClone ? new AtlasIsland(islandKVP.Value) : islandKVP.Value);
                }
            }
            return result;
        }
    }
    internal struct AtlasIdenticalTag
    {
        public int AtlasMeshDataIndex;
        public int MaterialSlot;

        public AtlasIdenticalTag(int atlasMeshDataIndex, int materialSlot)
        {
            AtlasMeshDataIndex = atlasMeshDataIndex;
            MaterialSlot = materialSlot;
        }

        public static bool operator ==(AtlasIdenticalTag a, AtlasIdenticalTag b)
        {
            return a.AtlasMeshDataIndex == b.AtlasMeshDataIndex && a.MaterialSlot == b.MaterialSlot;
        }
        public static bool operator !=(AtlasIdenticalTag a, AtlasIdenticalTag b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return obj is AtlasIdenticalTag tag && this == tag;
        }
        public override int GetHashCode()
        {
            return AtlasMeshDataIndex.GetHashCode() ^ MaterialSlot.GetHashCode();
        }
    }
    public struct AtlasIslandID
    {
        public int AtlasMeshDataIndex;
        public int MaterialSlot;
        public int IslandIndex;

        public AtlasIslandID(int atlasMeshDataIndex, int materialSlot, int islandIndex)
        {
            AtlasMeshDataIndex = atlasMeshDataIndex;
            MaterialSlot = materialSlot;
            IslandIndex = islandIndex;
        }

        public static bool operator ==(AtlasIslandID a, AtlasIslandID b)
        {
            return a.IslandIndex == b.IslandIndex && a.AtlasMeshDataIndex == b.AtlasMeshDataIndex && a.MaterialSlot == b.MaterialSlot;
        }
        public static bool operator !=(AtlasIslandID a, AtlasIslandID b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return obj is AtlasIslandID tag && this == tag;
        }
        public override int GetHashCode()
        {
            return IslandIndex.GetHashCode() ^ AtlasMeshDataIndex.GetHashCode() ^ MaterialSlot.GetHashCode();
        }
    }
}
