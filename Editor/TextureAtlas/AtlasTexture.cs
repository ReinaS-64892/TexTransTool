#if UNITY_EDITOR
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
using net.rs64.TexTransTool.EditorIsland;
using net.rs64.TexTransTool.TextureAtlas.FineSetting;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [AddComponentMenu("TexTransTool/TTT AtlasTexture")]
    internal class AtlasTexture : TextureTransformer, IMaterialReplaceEventLiner
    {
        public GameObject TargetRoot;
        public List<Renderer> Renderers => FilteredRenderers(TargetRoot);
        public List<MatSelector> SelectMatList = new List<MatSelector>();

        public AtlasSetting AtlasSetting = new AtlasSetting();

        public override bool IsPossibleApply => TargetRoot != null;

        public override List<Renderer> GetRenderers => Renderers;

        public override TexTransPhase PhaseDefine => TexTransPhase.UVModification;

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
        [Obsolete("V0SaveData", true)] public List<AtlasSetting> AtlasSettings = new List<AtlasSetting>() { new AtlasSetting() };
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
            [Obsolete("V1SaveData", true)] public float TextureSizeOffSet;
            #endregion

        }
        struct MatData
        {
            public Material Material;
            public float TextureSizeOffSet;
            public List<PropAndTexture> PropAndTextures;

            public MatData(MatSelector matSelector, List<PropAndTexture> propAndTextures)
            {
                Material = matSelector.Material;
                TextureSizeOffSet = matSelector.AdditionalTextureSizeOffSet;
                PropAndTextures = propAndTextures;
            }
        }

        bool TryCompileAtlasTextures(ITextureManager textureManager, out AtlasData atlasData)
        {
            atlasData = new AtlasData();


            //情報を集めるフェーズ
            var NowContainsMatSet = new HashSet<Material>(RendererUtility.GetMaterials(Renderers));
            var targetMaterialSelectors = SelectMatList.Where(I => NowContainsMatSet.Contains(I.Material)).ToList();
            atlasData.AtlasInMaterials = targetMaterialSelectors;
            var atlasSetting = AtlasSetting;
            var atlasReferenceData = new AtlasReferenceData(targetMaterialSelectors.Select(I => I.Material).ToList(), Renderers);
            var shaderSupports = new AtlasShaderSupportUtils(AtlasSetting.UnknownShaderAtlasAllTexture);


            //ターゲットとなるマテリアルやそのマテリアルが持つテクスチャを引き出すフェーズ
            shaderSupports.BakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
            var matDataList = new List<MatData>();
            foreach (var matSelector in targetMaterialSelectors)
            {
                shaderSupports.AddRecord(matSelector.Material);
            }
            foreach (var matSelector in targetMaterialSelectors)
            {
                matDataList.Add(new MatData(matSelector, shaderSupports.GetTextures(matSelector.Material, textureManager)));
            }
            shaderSupports.ClearRecord();


            //アイランドまわり
            var originIslandPool = atlasReferenceData.GeneratedIslandPool(atlasSetting.UseIslandCache);
            var matDataPools = GetMatDataPool(atlasReferenceData, originIslandPool, matDataList);

            var maxTexturePixelCount = 0;
            foreach (var matSelect in targetMaterialSelectors)
            {
                var tex = matSelect.Material.mainTexture;
                if (tex == null) { continue; }
                maxTexturePixelCount = Mathf.Max(maxTexturePixelCount, tex.width * tex.height);
            }
            var moveIslandPool = new Dictionary<AtlasIslandID, AtlasIsland>(originIslandPool.Count);
            foreach (var matDataPool in matDataPools)
            {
                var tex = matDataPool.Key.Material.mainTexture;
                var defaultTextureSizeOffset = tex != null ? (tex.width * tex.height) / (float)maxTexturePixelCount : 0.01f;
                var sizeOffset = matDataPool.Key.TextureSizeOffSet * defaultTextureSizeOffset;
                foreach (var island in matDataPool.Value.Values)
                {
                    island.Size *= sizeOffset;
                    if (island.Size.x > 0.99) { island.Size *= 0.99f / island.Size.x; }
                    if (island.Size.y > 0.99) { island.Size *= 0.99f / island.Size.y; }
                }
                foreach (var islandKVP in matDataPool.Value) { moveIslandPool.Add(islandKVP.Key, islandKVP.Value); }
            }

            var sorter = AtlasIslandSorterUtility.GetSorter(atlasSetting.SorterName);
            if (sorter == null) { return false; }
            moveIslandPool = sorter.Sorting(moveIslandPool, atlasSetting.GetTexScalePadding);
            var rectTangleMove = sorter.RectTangleMove;


            //新しいUVを持つMeshを生成するフェーズ
            var compiledMeshes = new List<AtlasData.AtlasMeshAndDist>();
            var poolContainsTags = ToIndexTags(moveIslandPool.Keys);
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


                var movedPool = new Dictionary<AtlasIslandID, AtlasIsland>();
                foreach (var tag in meshTags)
                {
                    foreach (var islandKVP in moveIslandPool.Where(i => i.Key.AtlasMeshDataIndex == tag.AtlasMeshDataIndex && i.Key.MaterialSlot == tag.MaterialSlot))
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

            var propertyNames = new HashSet<string>();
            foreach (var matData in matDataList)
            {
                propertyNames.UnionWith(matData.PropAndTextures.ConvertAll(PaT => PaT.PropertyName));
            }


            foreach (var propName in propertyNames)
            {
                var targetRT = RenderTexture.GetTemporary(atlasSetting.AtlasTextureSize, atlasSetting.AtlasTextureSize, 32);
                targetRT.Clear();
                targetRT.name = "AtlasTex" + propName;
                foreach (var matData in matDataList)
                {
                    var souseProp2Tex = matData.PropAndTextures.Find(I => I.PropertyName == propName);
                    if (souseProp2Tex == null) continue;
                    var souseTex = souseProp2Tex.Texture is Texture2D ? textureManager.GetOriginalTexture2D(souseProp2Tex.Texture as Texture2D) : souseProp2Tex.Texture;

                    if (rectTangleMove)
                    {

                        var islandPairs = new Dictionary<Island, Island>();
                        foreach (var islandID in originIslandPool.Keys.Where(tag => atlasReferenceData.GetMaterialReference(tag) == matData.Material))
                        {
                            var Origin = originIslandPool[islandID];
                            var Moved = moveIslandPool[islandID];

                            if (Origin != null && Moved != null) { islandPairs.Add(Origin, Moved); }
                        }

                        TransMoveRectIsland(souseTex, targetRT, islandPairs, atlasSetting.GetTexScalePadding);
                        islandPairs.Clear();
                    }
                    else
                    {
                        foreach (var atlasAMDGroup in originIslandPool
                                            .Where(atlasIsland => atlasReferenceData.GetMaterialReference(atlasIsland.Key) == matData.Material)
                                            .GroupBy(atlasIsland => atlasIsland.Key.AtlasMeshDataIndex)
                                            )
                        {
                            var amd = atlasReferenceData.AtlasMeshDataList[atlasAMDGroup.Key];

                            var transData = new TransData<Vector2>(atlasAMDGroup.SelectMany(value => value.Value.triangles), amd.MovedUV, amd.UV);
                            ForTrans(targetRT, souseTex, transData, atlasSetting.Padding, null, true);
                        }
                    }

                }

                compiledAtlasTextures.Add(new PropAndTexture2D(propName, targetRT.CopyTexture2D()));
                RenderTexture.ReleaseTemporary(targetRT);
            }
            atlasData.Textures = compiledAtlasTextures;

            return true;
        }

        public override void Apply(IDomain domain = null)
        {
            if (!IsPossibleApply)
            {
                Debug.LogWarning("AtlasTexture : アトラス化実行不可能な状態です、設定を見直しましょう。");
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
            var fineSettings = AtlasSetting.GetTextureFineTuning();
            foreach (var fineSetting in fineSettings)
            {
                fineSetting.AddSetting(atlasTexFineTuningTargets);
            }
            TexFineTuningUtility.FinalizeTexFineTuning(atlasTexFineTuningTargets);
            var atlasTexture = TexFineTuningUtility.ConvertForPropAndTexture2D(atlasTexFineTuningTargets);
            domain.transferAssets(atlasTexFineTuningTargets.Select(PaT => PaT.Texture2D));
            foreach (var atlasTexFTData in atlasTexFineTuningTargets)
            {
                var compressSetting = atlasTexFTData.TuningDataList.Find(I => I is CompressionQualityData) as CompressionQualityData;
                var compressSettingTuple = (CompressionQualityApplicant.GetTextureFormat(compressSetting.FormatQualityValue), (int)compressSetting.CompressionQuality);
                domain.TextureCompressDelegation(compressSettingTuple, atlasTexFTData.Texture2D);
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

        private void TransMoveRectIsland(Texture souseTex, RenderTexture targetRT, Dictionary<Island, Island> islandPairs, float padding)
        {
            padding *= 0.5f;
            var sUV = new List<Vector2>();
            var tUV = new List<Vector2>();
            var triangles = new List<TriangleIndex>();

            var nawIndex = 0;
            foreach (var islandPair in islandPairs)
            {
                var (Origin, Moved) = (islandPair.Key, islandPair.Value);
                var originVertexes = Origin.GenerateRectVertexes(padding);
                var movedVertexes = Moved.GenerateRectVertexes(padding);
                var triangleQuad = new List<TriangleIndex>(6)
                {
                    new (nawIndex + 0, nawIndex + 1, nawIndex + 2),
                    new ( nawIndex + 0, nawIndex + 2, nawIndex + 3)
                };
                nawIndex += 4;
                triangles.AddRange(triangleQuad);
                sUV.AddRange(originVertexes);
                tUV.AddRange(movedVertexes);
            }

            TransTexture.ForTrans(targetRT, souseTex, new TransData<Vector2>(triangles, tUV, sUV), argTexWrap: TextureWrap.Loop);

        }

        public static AtlasIdenticalTag? FindIdenticalTag(AtlasReferenceData atlasData, HashSet<AtlasIdenticalTag> poolTags, int findTagMeshRef, int findTagMatSlot, int findTagMatRef)
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

        public static HashSet<AtlasIdenticalTag> ToIndexTags(IEnumerable<AtlasIslandID> tags)
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
            editableTMat.RemoveUnusedProperties();
            shaderSupport.MaterialCustomSetting(editableTMat);
            return editableTMat;
        }



        public List<Renderer> FilteredRenderers(GameObject targetRoot)
        {
            var result = new List<Renderer>();
            foreach (var item in targetRoot.GetComponentsInChildren<Renderer>(AtlasSetting.IncludeDisableRenderer))
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

        public void MaterialReplace(Material souse, Material target)
        {
            var index = SelectMatList.FindIndex(I => I.Material == souse);
            if (index == -1) { return; }
            var selectMat = SelectMatList[index];
            selectMat.Material = target;
            SelectMatList[index] = selectMat;
        }
    }
    internal class AtlasReferenceData
    {
        public OrderedHashSet<Mesh> Meshes;
        public OrderedHashSet<Material> Materials;
        public List<AtlasMeshData> AtlasMeshDataList;
        public List<Renderer> Renderers;
        public AtlasReferenceData(List<Material> targetMaterials, List<Renderer> inputRenderers)
        {
            var targetMatHash = new HashSet<Material>(targetMaterials);
            Meshes = new(); Materials = new(); Renderers = new();

            foreach (var renderer in inputRenderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (targetMatHash.Contains(mat))
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


        public Dictionary<AtlasIslandID, AtlasIsland> GeneratedIslandPool(bool useIslandCache)
        {
            return GeneratedIslandPool(useIslandCache ? new EditorIslandCache() : null);
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
#endif
