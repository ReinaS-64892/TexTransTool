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
    public class AtlasTexture : TextureTransformer, IMaterialReplaceEventLiner
    {
        public GameObject TargetRoot;
        public List<Renderer> Renderers => FilteredRenderers(TargetRoot);
        public List<MatSelector> SelectMatList = new List<MatSelector>();
        public List<MatSelector> GetContainedSelectMatList
        {
            get
            {
                var NowContainsMatSet = new HashSet<Material>(RendererUtility.GetMaterials(Renderers));
                return SelectMatList.Where(I => NowContainsMatSet.Contains(I.Material)).ToList();
            }
        }
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

        Channel アトラス化するテクスチャーのチャンネルという感じで、channelごとにUVが違うものになる。
        channel周りではメッシュとマテリアルで扱いが違っていて、
        メッシュはchannel分けでUVを整列するが、サブメッシュ区切りで、別のチャンネルでいじられたUVを持つことになることがあるため、メッシュの情報はchannelごとにならない。
        マテリアルの場合はchannelごとに完全に分かれるため、コンテナの中身は二次元リストとなっている。(テクスチャはマテリアルとほぼ同様の扱い)

        AtlasSettings アトラス化するときのまとめたテクスチャーの大きさなどの情報を持つ。

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
            public float TextureSizeOffSet;
        }
        struct MatData
        {
            public Material Material;
            public float TextureSizeOffSet;
            public List<PropAndTexture> PropAndTextures;

            public MatData(MatSelector matSelector, List<PropAndTexture> propAndTextures)
            {
                Material = matSelector.Material;
                TextureSizeOffSet = matSelector.TextureSizeOffSet;
                PropAndTextures = propAndTextures;
            }
        }
        bool TryCompileAtlasTextures(out AtlasData atlasData)
        {
            atlasData = new AtlasData();


            //情報を集めるフェーズ
            var targetMaterialSelectors = GetContainedSelectMatList;
            var atlasSetting = AtlasSetting;
            var atlasReferenceData = new AtlasReferenceData(targetMaterialSelectors.Select(I => I.Material).ToList(), Renderers);
            var shaderSupports = new AtlasShaderSupportUtils();


            //ターゲットとなるマテリアルやそのマテリアルが持つテクスチャを引き出すフェーズ
            shaderSupports.BakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
            var matDataList = new List<MatData>();
            foreach (var MatSelector in targetMaterialSelectors)
            {
                shaderSupports.AddRecord(MatSelector.Material);
            }
            foreach (var MatSelector in targetMaterialSelectors)
            {
                matDataList.Add(new MatData(MatSelector, shaderSupports.GetTextures(MatSelector.Material)));
            }
            shaderSupports.ClearRecord();


            //アイランドを並び替えるフェーズ
            var originIslandPool = atlasReferenceData.GeneratedIslandPool(atlasSetting.UseIslandCache);
            var matDataPools = GetMatDataPool(atlasReferenceData, originIslandPool, matDataList);
            var moveIslandPool = new TagIslandPool<IndexTagPlusIslandIndex>();
            foreach (var matDataPool in matDataPools)
            {
                matDataPool.Value.IslandPoolSizeOffset(matDataPool.Key.TextureSizeOffSet);
                moveIslandPool.AddRangeIsland(matDataPool.Value);
            }
            IslandSorting.GenerateMovedIslands(atlasSetting.SortingType, moveIslandPool, atlasSetting.GetTexScalePadding);


            //アトラス化したテクスチャーを生成するフェーズ
            var compiledAtlasTextures = new List<PropAndTexture2D>();

            var propertyNames = new HashSet<string>();
            foreach (var MatData in matDataList)
            {
                propertyNames.UnionWith(MatData.PropAndTextures.ConvertAll(PaT => PaT.PropertyName));
            }

            var tags = moveIslandPool.GetTag();

            foreach (var propName in propertyNames)
            {
                var targetRT = new RenderTexture(atlasSetting.AtlasTextureSize, atlasSetting.AtlasTextureSize, 32);
                targetRT.name = "AtlasTex" + propName;
                foreach (var matData in matDataList)
                {
                    var souseProp2Tex = matData.PropAndTextures.Find(I => I.PropertyName == propName);
                    if (souseProp2Tex == null) continue;


                    var islandPairs = new List<(Island, Island)>();
                    foreach (var TargetIndexTag in tags.Where(tag => atlasReferenceData.GetMaterialReference(tag) == matData.Material))
                    {
                        var Origin = originIslandPool.FindTag(TargetIndexTag);
                        var Moved = moveIslandPool.FindTag(TargetIndexTag);

                        if (Origin != null && Moved != null) { islandPairs.Add((Origin, Moved)); }
                    }

                    TransMoveRectIsland(souseProp2Tex.Texture2D, targetRT, islandPairs, atlasSetting.GetTexScalePadding);
                }

                compiledAtlasTextures.Add(new PropAndTexture2D(propName, targetRT.CopyTexture2D()));
                // RenderTexture.ReleaseTemporary(targetRT); なぜかTemporaryなレンダーテクスチャだと壊れる
            }
            atlasData.Textures = compiledAtlasTextures;


            //新しいUVを持つMeshを生成するフェーズ
            var compiledMeshes = new List<AtlasData.AtlasMeshAndDist>();
            for (int I = 0; I < atlasReferenceData.AtlasMeshDataList.Count; I += 1)
            {
                var AMD = atlasReferenceData.AtlasMeshDataList[I];

                var distMesh = atlasReferenceData.Meshes[AMD.ReferenceMesh];
                var NewMesh = UnityEngine.Object.Instantiate<Mesh>(distMesh);
                NewMesh.name = "AtlasMesh_" + NewMesh.name;

                var meshTags = new List<IndexTag>();
                var poolContainsTags = ToIndexTags(moveIslandPool.GetTag());

                for (var slotIndex = 0; AMD.MaterialIndex.Length > slotIndex; slotIndex += 1)
                {
                    var thisTag = new IndexTag(I, slotIndex);
                    if (poolContainsTags.Contains(thisTag))
                    {
                        meshTags.Add(thisTag);
                    }
                    else
                    {
                        var thisTagMeshRef = AMD.ReferenceMesh;
                        var thisTagMatSlot = slotIndex;
                        var thisTagMatRef = AMD.MaterialIndex[slotIndex];
                        IndexTag? identicalTag = FindIdenticalTag(atlasReferenceData, poolContainsTags, thisTagMeshRef, thisTagMatSlot, thisTagMatRef);

                        if (identicalTag.HasValue)
                        {
                            meshTags.Add(identicalTag.Value);
                        }
                    }
                }


                var MovedPool = new TagIslandPool<IndexTagPlusIslandIndex>();
                foreach (var tag in meshTags)
                {
                    atlasReferenceData.FindIndexTagIslandPool(moveIslandPool, MovedPool, tag, false);
                }

                var MovedUV = new List<Vector2>(AMD.UV);
                IslandUtility.IslandPoolMoveUV(AMD.UV, MovedUV, originIslandPool, MovedPool);

                NewMesh.SetUVs(0, MovedUV);
                NewMesh.SetUVs(1, AMD.UV);

                compiledMeshes.Add(new AtlasData.AtlasMeshAndDist(distMesh, NewMesh, AMD.MaterialIndex.Select(Index => atlasReferenceData.Materials[Index]).ToArray()));
            }
            atlasData.Meshes = compiledMeshes;

            return true;
        }

        public override void Apply(IDomain Domain = null)
        {
            if (!IsPossibleApply)
            {
                Debug.LogWarning("AtlasTexture : アトラス化実行不可能な状態です、設定を見直しましょう。");
                return;
            }

            Domain.ProgressStateEnter("AtlasTexture");
            Domain.ProgressUpdate("CompileAtlasTexture", 0f);

            if (!TryCompileAtlasTextures(out var atlasData)) { return; }

            Domain.ProgressUpdate("MeshChange", 0.5f);

            var nowRenderers = Renderers;

            var ShaderSupport = new AtlasShaderSupportUtils();

            //Mesh Change
            foreach (var renderer in nowRenderers)
            {
                var mesh = renderer.GetMesh();
                var mats = renderer.sharedMaterials;
                var atlasMeshAndDist = atlasData.Meshes.FindAll(I => I.DistMesh == mesh).Find(I => I.Mats.SequenceEqual(mats));
                if (atlasMeshAndDist.AtlasMesh == null) { continue; }

                var atlasMesh = atlasMeshAndDist.AtlasMesh;
                Domain.SetMesh(renderer, atlasMesh);
                Domain.TransferAsset(atlasMesh);
            }

            Domain.ProgressUpdate("Texture Fine Tuning", 0.75f);

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
            Domain.transferAssets(atlasTexFineTuningTargets.Select(PaT => PaT.Texture2D));

            Domain.ProgressUpdate("MaterialGenerate And Change", 0.9f);

            //MaterialGenerate And Change
            var targetMats = GetContainedSelectMatList;
            if (AtlasSetting.MergeMaterials)
            {
                var mergeMat = AtlasSetting.MergeReferenceMaterial != null ? AtlasSetting.MergeReferenceMaterial : targetMats.First().Material;
                Material generateMat = GenerateAtlasMat(mergeMat, atlasTexture, ShaderSupport, AtlasSetting.ForceSetTexture);

                Domain.ReplaceMaterials(targetMats.ToDictionary(x => x.Material, _ => generateMat), rendererOnly: true);
            }
            else
            {
                var materialMap = new Dictionary<Material, Material>();
                foreach (var MatSelector in targetMats)
                {
                    var DistMat = MatSelector.Material;
                    var generateMat = GenerateAtlasMat(DistMat, atlasTexture, ShaderSupport, AtlasSetting.ForceSetTexture);
                    materialMap.Add(DistMat, generateMat);
                }
                Domain.ReplaceMaterials(materialMap);
            }

            Domain.ProgressUpdate("End", 1);
            Domain.ProgressStateExit();
        }

        private void TransMoveRectIsland(Texture SouseTex, RenderTexture targetRT, List<(Island, Island)> islandPairs, float padding)
        {
            padding *= 0.5f;
            var SUV = new List<Vector2>();
            var TUV = new List<Vector2>();
            var triangles = new List<TriangleIndex>();

            var nawIndex = 0;
            foreach ((var Origin, var Moved) in islandPairs)
            {
                var originVertexes = Origin.GenerateRectVertexes(padding);
                var movedVertexes = Moved.GenerateRectVertexes(padding);
                var triangleQuad = new List<TriangleIndex>(6)
                {
                    new TriangleIndex(nawIndex + 0, nawIndex + 1, nawIndex + 2),
                    new TriangleIndex( nawIndex + 0, nawIndex + 2, nawIndex + 3)
                };
                nawIndex += 4;
                triangles.AddRange(triangleQuad);
                SUV.AddRange(originVertexes);
                TUV.AddRange(movedVertexes);
            }

            TransTexture.TransTextureToRenderTexture(targetRT, SouseTex, new TransData(triangles, TUV, SUV), TexWrap: TextureWrap.Loop);

        }

        public static IndexTag? FindIdenticalTag(AtlasReferenceData AtlasData, HashSet<IndexTag> PoolTags, int FindTagMeshRef, int FindTagMatSlot, int FindTagMatRef)
        {
            IndexTag? identicalTag = null;
            foreach (var pTag in PoolTags)
            {
                var pTagTargetAMD = AtlasData.AtlasMeshDataList[pTag.AtlasMeshDataIndex];
                var pTagMeshRef = pTagTargetAMD.ReferenceMesh;
                var pTagMatSlot = pTag.MaterialSlot;
                var pTagMatRef = pTagTargetAMD.MaterialIndex[pTag.MaterialSlot];

                if (FindTagMeshRef == pTagMeshRef && FindTagMatSlot == pTagMatSlot && FindTagMatRef == pTagMatRef)
                {
                    identicalTag = pTag;
                    break;
                }
            }

            return identicalTag;
        }

        private static Dictionary<MatData, TagIslandPool<IndexTagPlusIslandIndex>> GetMatDataPool(AtlasReferenceData AtlasData, TagIslandPool<IndexTagPlusIslandIndex> OriginIslandPool, List<MatData> MatDataList)
        {
            var matDataPairPool = new Dictionary<MatData, TagIslandPool<IndexTagPlusIslandIndex>>();
            foreach (var matData in MatDataList)
            {
                var separatePool = AtlasData.FindMatIslandPool(OriginIslandPool, matData.Material);
                matDataPairPool.Add(matData, separatePool);
            }

            return matDataPairPool;
        }

        public static HashSet<IndexTag> ToIndexTags(HashSet<IndexTagPlusIslandIndex> Tags)
        {
            var indexTag = new HashSet<IndexTag>();
            foreach (var tag in Tags)
            {
                indexTag.Add(new IndexTag(tag.AtlasMeshDataIndex, tag.MaterialSlot));
            }

            return indexTag;
        }

        private static Material GenerateAtlasMat(Material TargetMat, List<PropAndTexture2D> AtlasTex, AtlasShaderSupportUtils shaderSupport, bool ForceSetTexture)
        {
            var editableTMat = UnityEngine.Object.Instantiate(TargetMat);

            editableTMat.SetTextures(AtlasTex, ForceSetTexture);
            editableTMat.RemoveUnusedProperties();
            shaderSupport.MaterialCustomSetting(editableTMat);
            return editableTMat;
        }



        public static List<Renderer> FilteredRenderers(GameObject TargetRoot)
        {
            var result = new List<Renderer>();
            foreach (var item in TargetRoot.GetComponentsInChildren<Renderer>())
            {
                if (item.tag == "EditorOnly") continue;
                if (item.enabled == false) continue;
                if (item.GetMesh() == null) continue;
                if (item.GetMesh().uv.Any() == false) continue;
                if (item.sharedMaterials.Length == 0) continue;
                if (item.sharedMaterials.Any(Mat => Mat == null)) continue;

                result.Add(item);
            }
            return result;
        }

        public void AutomaticOffSetSetting()
        {
            var maxTexPixel = 0;

            foreach (var matSelect in SelectMatList)
            {
                var tex = matSelect.Material.mainTexture;
                maxTexPixel = Mathf.Max(maxTexPixel, tex.width * tex.height);
            }

            for (int i = 0; SelectMatList.Count > i; i += 1)
            {
                var MatSelector = SelectMatList[i];
                var tex = MatSelector.Material.mainTexture;
                MatSelector.TextureSizeOffSet = (tex.width * tex.height) / (float)maxTexPixel;
                SelectMatList[i] = MatSelector;
            }
        }

        public void MaterialReplace(Material Souse, Material Target)
        {
            var index = SelectMatList.FindIndex(I => I.Material == Souse);
            if (index == -1) { return; }
            var selectMat = SelectMatList[index];
            selectMat.Material = Target;
            SelectMatList[index] = selectMat;
        }
    }
    public class AtlasReferenceData
    {
        public OrderedHashSet<Mesh> Meshes;
        public OrderedHashSet<Material> Materials;
        public List<AtlasMeshData> AtlasMeshDataList;
        public List<Renderer> Renderers;
        public class AtlasMeshData
        {
            //RefData
            public int ReferenceMesh;
            public int[] MaterialIndex;

            //for Generate
            public readonly List<List<TriangleIndex>> Triangles;
            public List<Vector2> UV;
            public List<Vector2> GeneratedUV;

            public AtlasMeshData(int referenceMesh, List<List<TriangleIndex>> triangles, List<Vector2> uV, int[] materialIndex)
            {
                ReferenceMesh = referenceMesh;
                Triangles = triangles;
                UV = uV;
                MaterialIndex = materialIndex;
            }
            public AtlasMeshData()
            {
                Triangles = new List<List<TriangleIndex>>();
                UV = new List<Vector2>();
            }
        }

        public AtlasReferenceData(List<Material> TargetMaterials, List<Renderer> InputRenderers)
        {
            var targetMatHash = new HashSet<Material>(TargetMaterials);
            Meshes = new OrderedHashSet<Mesh>();
            Materials = new OrderedHashSet<Material>();
            Renderers = new List<Renderer>();
            foreach (var renderer in InputRenderers)
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

            AtlasMeshDataList = new List<AtlasMeshData>();

            foreach (var renderer in Renderers)
            {
                var mesh = renderer.GetMesh();
                var refMesh = Meshes.IndexOf(mesh);
                var materialIndex = renderer.sharedMaterials.Select(Mat => Materials.IndexOf(Mat)).ToArray();

                var index = AtlasMeshDataList.FindIndex(AMD => AMD.ReferenceMesh == refMesh && AMD.MaterialIndex.SequenceEqual(materialIndex));
                if (index == -1)
                {
                    var UV = new List<Vector2>();
                    mesh.GetUVs(0, UV);

                    AtlasMeshDataList.Add(new AtlasMeshData(
                        refMesh,
                        mesh.GetSubTriangleIndex(),
                        UV,
                        materialIndex
                        ));
                }
            }
        }

        public TagIslandPool<IndexTagPlusIslandIndex> GeneratedIslandPool(bool UseIslandCache)
        {
            return GeneratedIslandPool(UseIslandCache ? new EditorIslandCache() : null);
        }
        /// <summary>
        ///  すべてをアイランドにし、同一の物を指すアイランドは排除したものを返します。
        /// </summary>
        /// <param name="islandCache"></param>
        /// <returns></returns>
        public TagIslandPool<IndexTagPlusIslandIndex> GeneratedIslandPool(IIslandCache islandCache)
        {
            var islandPool = new TagIslandPool<IndexTag>();
            var AMDCount = AtlasMeshDataList.Count;
            for (int AMDIndex = 0; AMDIndex < AMDCount; AMDIndex += 1)
            {
                var AMD = AtlasMeshDataList[AMDIndex];

                for (var SlotIndex = 0; AMD.MaterialIndex.Length > SlotIndex; SlotIndex += 1)
                {
                    var tag = new IndexTag(AMDIndex, SlotIndex);
                    var islands = IslandUtility.UVtoIsland(AMD.Triangles[SlotIndex], AMD.UV, islandCache);
                    islandPool.AddRangeIsland(islands, tag);
                }
            }

            var tagSet = islandPool.GetTag();
            var RefMesh_MatSlot_RefMat_Hash = new HashSet<(int, int, int)>();
            var deleteTags = new List<IndexTag>();

            foreach (var tag in tagSet)
            {
                var AMD = AtlasMeshDataList[tag.AtlasMeshDataIndex];
                var refMesh = AMD.ReferenceMesh;
                var materialSlot = tag.MaterialSlot;
                var refMat = AMD.MaterialIndex[tag.MaterialSlot];
                var RMesh_MSlot_RMat = (refMesh, materialSlot, refMat);

                if (RefMesh_MatSlot_RefMat_Hash.Contains(RMesh_MSlot_RMat))
                {
                    deleteTags.Add(tag);
                }
                else
                {
                    RefMesh_MatSlot_RefMat_Hash.Add(RMesh_MSlot_RMat);
                }
            }

            foreach (var deleteTag in deleteTags)
            {
                islandPool.RemoveAll(deleteTag);
            }

            var tagIslandPool = new TagIslandPool<IndexTagPlusIslandIndex>();
            var poolCount = islandPool.Islands.Count;
            for (int poolIndex = 0; poolIndex < poolCount; poolIndex += 1)
            {
                var oldTag = islandPool[poolIndex].tag;
                tagIslandPool.AddIsland(new TagIsland<IndexTagPlusIslandIndex>(islandPool[poolIndex], new IndexTagPlusIslandIndex(oldTag.AtlasMeshDataIndex, oldTag.MaterialSlot, poolIndex)));
            }
            return tagIslandPool;
        }

        public Material GetMaterialReference(IndexTagPlusIslandIndex indexTag)
        {
            return GetMaterialReference(indexTag.AtlasMeshDataIndex, indexTag.MaterialSlot);
        }
        public Material GetMaterialReference(IndexTag indexTag)
        {
            return GetMaterialReference(indexTag.AtlasMeshDataIndex, indexTag.MaterialSlot);
        }
        private Material GetMaterialReference(int atlasMeshDataIndex, int materialSlot)
        {
            return Materials[AtlasMeshDataList[atlasMeshDataIndex].MaterialIndex[materialSlot]];
        }


        public TagIslandPool<IndexTagPlusIslandIndex> FindMatIslandPool(TagIslandPool<IndexTagPlusIslandIndex> Souse, Material MatRef, bool DeepClone = true)
        {
            var result = new TagIslandPool<IndexTagPlusIslandIndex>();
            foreach (var island in Souse)
            {
                if (GetMaterialReference(island.tag) == MatRef)
                {
                    result.AddIsland(DeepClone ? new TagIsland<IndexTagPlusIslandIndex>(island) : island);
                }
            }
            return result;
        }
        public void FindMatIslandPool(TagIslandPool<IndexTagPlusIslandIndex> Souse, TagIslandPool<IndexTagPlusIslandIndex> AddTarget, Material MatRef, bool DeepClone = true)
        {
            foreach (var island in Souse)
            {
                if (GetMaterialReference(island.tag) == MatRef)
                {
                    AddTarget.AddIsland(DeepClone ? new TagIsland<IndexTagPlusIslandIndex>(island) : island);
                }
            }
        }
        public TagIslandPool<IndexTagPlusIslandIndex> FindIndexTagIslandPool(TagIslandPool<IndexTagPlusIslandIndex> Souse, IndexTag Tag, bool DeepClone = true)
        {
            var result = new TagIslandPool<IndexTagPlusIslandIndex>();
            foreach (var Island in Souse)
            {
                if (Island.tag.AtlasMeshDataIndex == Tag.AtlasMeshDataIndex && Island.tag.MaterialSlot == Tag.MaterialSlot)
                {
                    result.AddIsland(DeepClone ? new TagIsland<IndexTagPlusIslandIndex>(Island) : Island);
                }
            }
            return result;
        }
        public void FindIndexTagIslandPool(TagIslandPool<IndexTagPlusIslandIndex> Souse, TagIslandPool<IndexTagPlusIslandIndex> AddTarget, IndexTag Tag, bool DeepClone = true)
        {
            foreach (var Island in Souse)
            {
                if (Island.tag.AtlasMeshDataIndex == Tag.AtlasMeshDataIndex && Island.tag.MaterialSlot == Tag.MaterialSlot)
                {
                    AddTarget.AddIsland(DeepClone ? new TagIsland<IndexTagPlusIslandIndex>(Island) : Island);
                }
            }
        }
    }
    public struct IndexTag
    {
        public int AtlasMeshDataIndex;
        public int MaterialSlot;

        public IndexTag(int atlasMeshDataIndex, int materialSlot)
        {
            AtlasMeshDataIndex = atlasMeshDataIndex;
            MaterialSlot = materialSlot;
        }

        public static bool operator ==(IndexTag a, IndexTag b)
        {
            return a.AtlasMeshDataIndex == b.AtlasMeshDataIndex && a.MaterialSlot == b.MaterialSlot;
        }
        public static bool operator !=(IndexTag a, IndexTag b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return obj is IndexTag tag && this == tag;
        }
        public override int GetHashCode()
        {
            return AtlasMeshDataIndex.GetHashCode() ^ MaterialSlot.GetHashCode();
        }
    }
    public struct IndexTagPlusIslandIndex
    {
        public int AtlasMeshDataIndex;
        public int MaterialSlot;
        public int IslandIndex;

        public IndexTagPlusIslandIndex(int atlasMeshDataIndex, int materialSlot, int islandIndex)
        {
            AtlasMeshDataIndex = atlasMeshDataIndex;
            MaterialSlot = materialSlot;
            IslandIndex = islandIndex;
        }

        public static bool operator ==(IndexTagPlusIslandIndex a, IndexTagPlusIslandIndex b)
        {
            return a.IslandIndex == b.IslandIndex && a.AtlasMeshDataIndex == b.AtlasMeshDataIndex && a.MaterialSlot == b.MaterialSlot;
        }
        public static bool operator !=(IndexTagPlusIslandIndex a, IndexTagPlusIslandIndex b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return obj is IndexTagPlusIslandIndex tag && this == tag;
        }
        public override int GetHashCode()
        {
            return IslandIndex.GetHashCode() ^ AtlasMeshDataIndex.GetHashCode() ^ MaterialSlot.GetHashCode();
        }
    }

    [Serializable]
    public struct MeshPair
    {
        public Mesh Mesh;
        public Mesh SecondMesh;
        public MeshPair(Mesh mesh, Mesh secondMesh)
        {
            Mesh = mesh;
            SecondMesh = secondMesh;
        }

    }

}
#endif
