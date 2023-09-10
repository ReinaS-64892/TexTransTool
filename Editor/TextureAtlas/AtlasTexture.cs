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

namespace net.rs64.TexTransTool.TextureAtlas
{

    [AddComponentMenu("TexTransTool/AtlasTexture")]
    public class AtlasTexture : TextureTransformer
    {
        public GameObject TargetRoot;
        public List<Renderer> Renderers => FilteredRenderers(TargetRoot.GetComponentsInChildren<Renderer>());
        public List<Material> SelectReferenceMat;//OrderedHashSetにしたかったけどシリアライズの都合で
        public List<MatSelector> MatSelectors = new List<MatSelector>();
        public List<AtlasSetting> AtlasSettings = new List<AtlasSetting>() { new AtlasSetting() };
        public bool UseIslandCache = true;

        public override bool IsPossibleApply => TargetRoot != null && AtlasSettings.Count > 0;

        public override List<Renderer> GetRenderers => Renderers;


        // public override bool IsPossibleCompile => TargetRoot != null && AtlasSettings.Count > 0;
        /*
        TargetRenderers 対象となるのはメッシュが存在しマテリアルスロットにNullが含まれていないもの。

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
        public AtlasTextureDataContainer CompileAtlasTextures()
        {
            if (!IsPossibleApply)
            {
                Debug.LogWarning("AtlasTexture : アトラス化可能な状態ではないのためアトラス化ができません。ターゲットルートなどが設定されているかどうかご確認ください。");
                return null;
            }

            //情報を集めるフェーズ
            var selectRefsMat = new OrderedHashSet<Material>(SelectReferenceMat);

            var targetRenderers = Renderers;
            var atlasData = GenerateAtlasMeshData(targetRenderers);
            var shaderSupports = new AtlasShaderSupportUtils();
            var originIslandPool = atlasData.GeneratedIslandPool(UseIslandCache);
            var atlasIslandPool = new TagIslandPool<IndexTagPlusIslandIndex>();

            if (selectRefsMat.Count != atlasData.Materials.Count)
            {
                Debug.LogWarning("AtlasTexture : すでにアトラス化されているためか、マテリアルインデックスがずれているためアトラス化ができません。");
                return null;
            }

            var compiledAllAtlasTextures = new List<List<PropAndTexture2D>>();
            var compiledMeshes = new List<AtlasTextureDataContainer.MeshAndMatRef>();
            var channelsMatRef = new List<List<int>>();

            var channelCount = AtlasSettings.Count;
            for (int channel = 0; channel < channelCount; channel += 1)
            {
                var atlasSetting = AtlasSettings[channel];
                var targetMatSelectors = MatSelectors.Where(MS => MS.IsTarget && MS.AtlasChannel == channel).ToArray();

                //ターゲットとなるマテリアルやそのマテリアルが持つテクスチャを引き出すフェーズ
                shaderSupports.BakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
                var matDataList = new List<MatData>();
                foreach (var MatSelector in targetMatSelectors)
                {
                    var matData = new MatData();
                    var matIndex = selectRefsMat.IndexOf(MatSelector.Material);
                    matData.MaterialReference = matIndex;
                    matData.TextureSizeOffSet = MatSelector.TextureSizeOffSet;
                    shaderSupports.AddRecord(atlasData.Materials[matIndex]);
                    matDataList.Add(matData);
                }

                foreach (var md in matDataList)
                {
                    md.PropAndTextures = shaderSupports.GetTextures(atlasData.Materials[md.MaterialReference]);
                }

                shaderSupports.ClearRecord();

                channelsMatRef.Add(matDataList.Select(MD => MD.MaterialReference).ToList());



                //アイランドを並び替えるフェーズ
                var matDataPools = GetMatDataPool(atlasData, originIslandPool, matDataList);
                var nawChannelAtlasIslandPool = new TagIslandPool<IndexTagPlusIslandIndex>();
                foreach (var matDataPool in matDataPools)
                {
                    matDataPool.Value.IslandPoolSizeOffset(matDataPool.Key.TextureSizeOffSet);
                    nawChannelAtlasIslandPool.AddRangeIsland(matDataPool.Value);
                }


                IslandSorting.GenerateMovedIslands(atlasSetting.SortingType, nawChannelAtlasIslandPool, atlasSetting.GetTexScalePadding);
                atlasIslandPool.AddRangeIsland(nawChannelAtlasIslandPool);


                //アトラス化したテクスチャーを生成するフェーズ
                var compiledAtlasTextures = new List<PropAndTexture2D>();

                var propertyNames = new HashSet<string>();
                foreach (var MatData in matDataList)
                {
                    propertyNames.UnionWith(MatData.PropAndTextures.ConvertAll(PaT => PaT.PropertyName));
                }


                var tags = nawChannelAtlasIslandPool.GetTag();

                foreach (var propName in propertyNames)
                {
                    var targetRT = new RenderTexture(atlasSetting.AtlasTextureSize.x, atlasSetting.AtlasTextureSize.y, 32, RenderTextureFormat.ARGB32);
                    targetRT.name = "AtlasTex" + propName;
                    foreach (var matData in matDataList)
                    {
                        var souseProp2Tex = matData.PropAndTextures.Find(I => I.PropertyName == propName);
                        if (souseProp2Tex == null) continue;


                        var islandPairs = new List<(Island, Island)>();
                        foreach (var TargetIndexTag in tags.Where(tag => atlasData.GetMaterialReference(tag) == matData.MaterialReference))
                        {
                            var Origin = originIslandPool.FindTag(TargetIndexTag);
                            var Moved = nawChannelAtlasIslandPool.FindTag(TargetIndexTag);

                            if (Origin != null && Moved != null) { islandPairs.Add((Origin, Moved)); }
                        }

                        TransMoveRectIsland(souseProp2Tex.Texture2D, targetRT, islandPairs, atlasSetting.GetTexScalePadding);
                    }

                    compiledAtlasTextures.Add(new PropAndTexture2D(propName, targetRT.CopyTexture2D()));
                }

                compiledAllAtlasTextures.Add(compiledAtlasTextures);

            }


            //すべてのチャンネルを加味した新しいUVを持つMeshを生成するフェーズ
            var allChannelMatRefs = channelsMatRef.SelectMany(I => I).Distinct().ToList();
            for (int I = 0; I < atlasData.AtlasMeshData.Count; I += 1)
            {
                var AMD = atlasData.AtlasMeshData[I];
                if (AMD.MaterialIndex.Intersect(allChannelMatRefs).Count() == 0) continue;


                var generateMeshAndMatRef = new AtlasTextureDataContainer.MeshAndMatRef(
                    AMD.ReferenceMesh,
                    UnityEngine.Object.Instantiate<Mesh>(atlasData.Meshes[AMD.ReferenceMesh]),
                    AMD.MaterialIndex
                    );
                generateMeshAndMatRef.Mesh.name = "AtlasMesh_" + generateMeshAndMatRef.Mesh.name;

                var meshTags = new List<IndexTag>();
                var poolContainsTags = ToIndexTags(atlasIslandPool.GetTag());

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
                        IndexTag? identicalTag = FindIdenticalTag(atlasData, poolContainsTags, thisTagMeshRef, thisTagMatSlot, thisTagMatRef);

                        if (identicalTag.HasValue)
                        {
                            meshTags.Add(identicalTag.Value);
                        }
                    }
                }


                var MovedPool = new TagIslandPool<IndexTagPlusIslandIndex>();
                foreach (var tag in meshTags)
                {
                    atlasData.FindIndexTagIslandPool(atlasIslandPool, MovedPool, tag, false);
                }

                var MovedUV = new List<Vector2>(AMD.UV);
                IslandUtility.IslandPoolMoveUV(AMD.UV, MovedUV, originIslandPool, MovedPool);

                generateMeshAndMatRef.Mesh.SetUVs(0, MovedUV);
                generateMeshAndMatRef.Mesh.SetUVs(1, AMD.UV);

                compiledMeshes.Add(generateMeshAndMatRef);
            }

            //保存するフェーズ
            return new AtlasTextureDataContainer
            {
                ChannelsMatRef = channelsMatRef,
                AtlasTextures = compiledAllAtlasTextures,
                GenerateMeshes = compiledMeshes,
            };
        }

        public override void Apply(IDomain Domain)
        {
            var container = CompileAtlasTextures();
            if (container == null) { return; }

            var nowRenderers = Renderers;

            var ShaderSupport = new AtlasShaderSupportUtils();

            var channelMatRef = container.ChannelsMatRef;
            var generateMeshes = container.GenerateMeshes;
            var atlasTextures = container.AtlasTextures;
            var materials = GetMaterials(nowRenderers);
            var referenceMesh = GetMeshes(nowRenderers);

            if (AtlasSettings.Count != atlasTextures.Count || AtlasSettings.Count != channelMatRef.Count) { return; }


            foreach (var renderer in nowRenderers)
            {
                var mesh = renderer.GetMesh();
                var refMesh = referenceMesh.IndexOf(mesh);
                var matRefs = renderer.sharedMaterials.Select(Mat => materials.IndexOf(Mat)).ToArray();

                var targetMeshDataList = generateMeshes.FindAll(MD => MD.RefMesh == refMesh);
                if (targetMeshDataList.Count == 0) continue;
                var targetMeshData = targetMeshDataList.Find(MD => MD.MatRefs.SequenceEqual(matRefs));
                if (targetMeshData == null) continue;

                Domain.SetMesh(renderer, targetMeshData.Mesh);
                Domain.TransferAsset(targetMeshData.Mesh);
            }


            var channelCount = AtlasSettings.Count;
            for (var channel = 0; channelCount > channel; channel += 1)
            {
                var meshData = channelMatRef[channel];
                var atlasSetting = AtlasSettings[channel];
                var channelMatRefs = channelMatRef[channel];

                var AtlasTex = new List<PropAndTexture2D>(atlasTextures[channel].Capacity);
                foreach (var propTex in atlasTextures[channel])
                {
                    AtlasTex.Add(new PropAndTexture2D(propTex.PropertyName, propTex.Texture2D));
                }
                var fineSettings = atlasSetting.GetFineSettings();
                foreach (var fineSetting in fineSettings)
                {
                    fineSetting.FineSetting(AtlasTex);
                }
                Domain.transferAssets(AtlasTex.Select(PaT => PaT.Texture2D));


                if (atlasSetting.MergeMaterials)
                {
                    var mergeMat = atlasSetting.MergeReferenceMaterial != null ? atlasSetting.MergeReferenceMaterial : materials[channelMatRefs.First()];
                    Material generateMat = GenerateAtlasMat(mergeMat, AtlasTex, ShaderSupport, atlasSetting.ForceSetTexture);

                    var distMats = channelMatRefs.Select(MatRef => materials[MatRef]).ToList();
                    Domain.ReplaceMaterials(distMats.ToDictionary(x => x, _ => generateMat), rendererOnly: true);
                }
                else
                {
                    var materialMap = new Dictionary<Material, Material>();

                    foreach (var matRef in channelMatRefs)
                    {
                        var mat = materials[matRef];
                        var generateMat = GenerateAtlasMat(mat, AtlasTex, ShaderSupport, atlasSetting.ForceSetTexture);

                        materialMap.Add(mat, generateMat);
                    }

                    Domain.ReplaceMaterials(materialMap);
                }
            }
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

        public static IndexTag? FindIdenticalTag(AtlasData AtlasData, HashSet<IndexTag> PoolTags, int FindTagMeshRef, int FindTagMatSlot, int FindTagMatRef)
        {
            IndexTag? identicalTag = null;
            foreach (var pTag in PoolTags)
            {
                var pTagTargetAMD = AtlasData.AtlasMeshData[pTag.AtlasMeshDataIndex];
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

        public static Dictionary<MatData, TagIslandPool<IndexTagPlusIslandIndex>> GetMatDataPool(AtlasData AtlasData, TagIslandPool<IndexTagPlusIslandIndex> OriginIslandPool, List<MatData> MatDataList)
        {
            var matDataPairPool = new Dictionary<MatData, TagIslandPool<IndexTagPlusIslandIndex>>();
            foreach (var matData in MatDataList)
            {
                var separatePool = AtlasData.FindMatIslandPool(OriginIslandPool, matData.MaterialReference);
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



        public static List<Renderer> FilteredRenderers(IReadOnlyList<Renderer> renderers)
        {
            var result = new List<Renderer>();
            foreach (var item in renderers)
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
        public static AtlasData GenerateAtlasMeshData(IReadOnlyList<Renderer> renderers)
        {
            OrderedHashSet<Mesh> meshes = GetMeshes(renderers);
            OrderedHashSet<Material> materials = GetMaterials(renderers);
            List<AtlasMeshData> atlasMeshData = new List<AtlasMeshData>();

            foreach (var renderer in renderers)
            {
                var mesh = renderer.GetMesh();
                var refMesh = meshes.IndexOf(mesh);
                var materialIndex = renderer.sharedMaterials.Select(Mat => materials.IndexOf(Mat)).ToArray();

                var index = atlasMeshData.FindIndex(AMD => AMD.ReferenceMesh == refMesh && AMD.MaterialIndex.SequenceEqual(materialIndex));
                if (index == -1)
                {
                    var UV = new List<Vector2>();
                    mesh.GetUVs(0, UV);

                    atlasMeshData.Add(new AtlasMeshData(
                        refMesh,
                        mesh.GetSubTriangleIndex(),
                        UV,
                        materialIndex
                        ));
                }
            }

            return new AtlasData(meshes, materials, atlasMeshData);
        }

        public static OrderedHashSet<Material> GetMaterials(IReadOnlyList<Renderer> renderers)
        {
            OrderedHashSet<Material> materials = new OrderedHashSet<Material>();

            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    materials.Add(mat);
                }
            }

            return materials;
        }

        public static OrderedHashSet<Mesh> GetMeshes(IReadOnlyList<Renderer> renderers)
        {
            OrderedHashSet<Mesh> meshes = new OrderedHashSet<Mesh>();

            foreach (var renderer in renderers)
            {
                var mesh = renderer.GetMesh();
                meshes.Add(mesh);
            }

            return meshes;
        }

        public void AutomaticOffSetSetting()
        {
            var targetMats = MatSelectors.Where(MS => MS.IsTarget).ToArray();

            var maxTexPixel = 0;

            foreach (var matSelect in targetMats)
            {
                var tex = matSelect.Material.mainTexture;
                maxTexPixel = Mathf.Max(maxTexPixel, tex.width * tex.height);
            }

            foreach (var matSelect in targetMats)
            {
                var tex = matSelect.Material.mainTexture;
                matSelect.TextureSizeOffSet = (tex.width * tex.height) / (float)maxTexPixel;
            }
        }
    }
    public class AtlasData
    {
        public OrderedHashSet<Mesh> Meshes;
        public OrderedHashSet<Material> Materials;
        public List<AtlasMeshData> AtlasMeshData;

        public AtlasData(OrderedHashSet<Mesh> meshes, OrderedHashSet<Material> materials, List<AtlasMeshData> atlasMeshData)
        {
            Meshes = meshes;
            Materials = materials;
            AtlasMeshData = atlasMeshData;
        }

        public TagIslandPool<IndexTagPlusIslandIndex> GeneratedIslandPool(bool UseIslandCache)
        {
            return GeneratedIslandPool(UseIslandCache ? new EditorIslandCache() : null);
        }

        public TagIslandPool<IndexTagPlusIslandIndex> GeneratedIslandPool(IIslandCache islandCache)
        {
            var islandPool = new TagIslandPool<IndexTag>();
            var AMDCount = AtlasMeshData.Count;
            for (int AMDIndex = 0; AMDIndex < AMDCount; AMDIndex += 1)
            {
                var AMD = AtlasMeshData[AMDIndex];

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
                var AMD = AtlasMeshData[tag.AtlasMeshDataIndex];
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

        public int GetMaterialReference(IndexTagPlusIslandIndex indexTag)
        {
            return GetMaterialReference(indexTag.AtlasMeshDataIndex, indexTag.MaterialSlot);
        }
        public int GetMaterialReference(IndexTag indexTag)
        {
            return GetMaterialReference(indexTag.AtlasMeshDataIndex, indexTag.MaterialSlot);
        }
        private int GetMaterialReference(int atlasMeshDataIndex, int materialSlot)
        {
            return AtlasMeshData[atlasMeshDataIndex].MaterialIndex[materialSlot];
        }


        public TagIslandPool<IndexTagPlusIslandIndex> FindMatIslandPool(TagIslandPool<IndexTagPlusIslandIndex> Souse, int MatRef, bool DeepClone = true)
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
        public void FindMatIslandPool(TagIslandPool<IndexTagPlusIslandIndex> Souse, TagIslandPool<IndexTagPlusIslandIndex> AddTarget, int MatRef, bool DeepClone = true)
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
    public class AtlasMeshData
    {
        public int ReferenceMesh;
        public readonly List<List<TriangleIndex>> Triangles;
        public List<Vector2> UV;
        public List<Vector2> GeneratedUV;
        public int[] MaterialIndex;

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
    [Serializable]
    public class MatSelector
    {
        public Material Material;
        public bool IsTarget = false;
        public int AtlasChannel = 0;
        public float TextureSizeOffSet = 1;
    }
    [Serializable]
    public class MatData
    {
        public int MaterialReference;
        public float TextureSizeOffSet = 1;
        public List<PropAndTexture> PropAndTextures;
    }
}
#endif
