#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using Rs64.TexTransTool.ShaderSupport;
using Rs64.TexTransTool.Island;
using static Rs64.TexTransTool.TexturAtlas.AtlasTextureDataContainer;

namespace Rs64.TexTransTool.TexturAtlas
{

    public class AtlasTexture : TextureTransformer
    {
        public GameObject TargetRoot;
        public List<Renderer> Renderers => FilterdRendarer(TargetRoot.GetComponentsInChildren<Renderer>(true));
        public List<Material> SelectRefarensMat;//OrderdHashSetにしたかったけどシリアライズの都合で
        public List<MatSelector> MatSelectors = new List<MatSelector>();
        public List<AtlasSetting> AtlasSettings = new List<AtlasSetting>() { new AtlasSetting() };
        public bool UseIslandCash = true;
        public AtlasTextureDataContainer Container = new AtlasTextureDataContainer();

        [SerializeField] bool _isApply = false;
        public override bool IsApply => _isApply;
        public override bool IsPossibleApply => Container.IsPossibleApply;
        public override bool IsPossibleCompile => TargetRoot != null && AtlasSettings.Count > 0;
        /*
        TargetRenderers 対象となるのはメッシュが存在しマテリアルスロットにNullが含まれていないもの。

        MaterialRefarens レンダラーから集めた重複のないマテリアルの配列に対してのインデックス。

        MeshRefarens 上のメッシュ版。

        AtlasMeshDatas すべてのレンダラーをMeshRefarensとMaterialRefarensに変換し、まったく同じRefanensを持つものを消した物

        Channel アトラス化するテクスチャーのチャンネルという感じで、channelごとにUVが違うものになる。
        channel周りではメッシュとマテリアルで扱いが違っていて、
        メッシュはchannel分けでUVを整列するが、サブメッシュ区切りで、別のチャンネルでいじられたUVを持つことになることがあるため、メッシュの情報はchannelごとにならない。
        マテリアルの場合はchannelごとに完全に分かれるため、コンテナの中身は二次元リストとなっている。(テクスチャはマテリアルとほぼ同様の扱い)

        AtlasSettings アトラス化するときのまとめたテクスチャーの大きさなどの情報を持つ。

        SelectRefsMat インスペクター上で表示されているマテリアルたちの配列。
        MatSelerector SelectRefsMatに含まれているマテリアルの参照を持ち マテリアルがターゲットであるか、大きさのオフセットやどのChannelに属しているかの情報を持っている。

        MatData MatSelerectorをMaterialRefarensとTextureSizeOffSet、PropAndTexturesにしたもの。
        このMaterialRefarensはSelectRefsMatを使ってインデックスに変換している。

        MeshAndMatRef MeshRefarensとマテリアルスロット分のMaterialRefarensをもち、適応するときこれをもとに、マテリアル違いやマテリアルの一部違いなども識別して適応する。

        */
        public override void Compile()
        {
            if (!IsPossibleCompile) return;
            if (IsApply) return;
            Container.AtlasTextures = null;
            Container.GenereatMeshs = null;
            Container.ChannnelsMatRef = null;
            Container.IsPossibleApply = false;

            var SelectRefsMat = new OrderdHashSet<Material>(SelectRefarensMat);

            var TargetRenderers = Renderers;
            var AtlasDatas = GenereatAtlasMeshDatas(TargetRenderers);
            var ShaderSupports = new ShaderSupportUtili();
            var OriginIslandPool = AtlasDatas.GeneratedIslandPool(UseIslandCash);
            var AtlasIslandPool = new TagIslandPool<IndexTagPlusIslandIndex>();


            var CompiledAllAtlasTextures = new List<List<PropAndTexture>>();
            var CompiledMeshs = new List<AtlasTextureDataContainer.MeshAndMatRef>();
            var ChannnelsMatRef = new List<List<int>>();



            var ChannelCount = AtlasSettings.Count;
            for (int Channel = 0; Channel < ChannelCount; Channel += 1)
            {
                var atlasSetting = AtlasSettings[Channel];
                var TargetMatSerectors = MatSelectors.Where(MS => MS.IsTarget && MS.AtlsChannel == Channel).ToArray();

                var Matdatas = new List<MatData>();
                foreach (var MatSelector in TargetMatSerectors)
                {
                    var Matdata = new MatData();
                    var MatIndex = SelectRefsMat.IndexOf(MatSelector.Material);
                    Matdata.MaterialRefarens = MatIndex;
                    Matdata.TextureSizeOffSet = MatSelector.TextureSizeOffSet;
                    Matdata.PropAndTextures = ShaderSupports.GetTextures(AtlasDatas.Materials[MatIndex]);
                    Matdatas.Add(Matdata);
                }

                ChannnelsMatRef.Add(Matdatas.Select(MD => MD.MaterialRefarens).ToList());

                var MatDataPools = GetMatDataPool(AtlasDatas, OriginIslandPool, Matdatas);

                var NawChannnelAtlasIslandPool = new TagIslandPool<IndexTagPlusIslandIndex>();
                foreach (var Matdatapool in MatDataPools)
                {
                    Matdatapool.Value.IslandPoolSizeOffset(Matdatapool.Key.TextureSizeOffSet);
                    NawChannnelAtlasIslandPool.AddRangeIsland(Matdatapool.Value);
                }


                IslandSorting.GenereatMovedIlands(atlasSetting.SortingType, NawChannnelAtlasIslandPool);
                AtlasIslandPool.AddRangeIsland(NawChannnelAtlasIslandPool);

                var Tags = NawChannnelAtlasIslandPool.GetTag();
                var IndexTag = ToIndexTags(Tags);



                var TransMaps = new Dictionary<int, TransMapData>();
                foreach (var matdata in Matdatas)
                {
                    var transMap = new TransMapData(atlasSetting.Pading, atlasSetting.AtlasTextureSize);
                    var matref = matdata.MaterialRefarens;

                    foreach (var itag in IndexTag.Where(tag => AtlasDatas.GetMaterialRefarens(tag) != matref))
                    {
                        var targetAMD = AtlasDatas.AtlasMeshData[itag.AtlasMeshDataIndex];
                        var TargetTrainagles = targetAMD.Traiangles[itag.MaterialSlot];
                        var NotMovedUV = targetAMD.UV;
                        var MovedPool = AtlasDatas.FindIndexTagIslandPool(AtlasIslandPool, itag, false);
                        var MoveUV = new List<Vector2>(NotMovedUV);
                        IslandUtils.IslandPoolMoveUV(NotMovedUV, MoveUV, OriginIslandPool, MovedPool);
                        TransMapper.UVtoTexScale(MoveUV, atlasSetting.AtlasTextureSize);

                        TransMapper.TransMapGeneratUseComputeSheder(null, transMap, TargetTrainagles, MoveUV, NotMovedUV, atlasSetting.PadingType);
                    }

                    TransMaps.Add(matref, transMap);
                }



                var CompiledAtlasTextures = new List<PropAndAtlasTex>();

                var PropatyNames = new HashSet<string>();
                foreach (var matdata in Matdatas)
                {
                    PropatyNames.UnionWith(matdata.PropAndTextures.ConvertAll(PaT => PaT.PropertyName));
                }

                foreach (var Porp in PropatyNames)
                {
                    var TargetTex = new TransTargetTexture(atlasSetting.AtlasTextureSize, new Color(0, 0, 0, 0), atlasSetting.Pading);
                    foreach (var matdata in Matdatas)
                    {
                        var SousePorp2Tex = matdata.PropAndTextures.Find(I => I.PropertyName == Porp);
                        if (SousePorp2Tex == null) continue;

                        Compiler.TransCompileUseComputeSheder(SousePorp2Tex.Texture2D, TransMaps[matdata.MaterialRefarens], TargetTex, TexWrapMode.Stretch);
                    }
                    CompiledAtlasTextures.Add(new PropAndAtlasTex(Porp, TargetTex));
                }

                CompiledAllAtlasTextures.Add(CompiledAtlasTextures.ConvertAll(I => new PropAndTexture(I.PropertyName, I.AtlasTexture.Texture2D)));

            }



            var AllChannelMatrefs = ChannnelsMatRef.SelectMany(I => I).Distinct().ToList();
            for (int I = 0; I < AtlasDatas.AtlasMeshData.Count; I += 1)
            {
                var AMD = AtlasDatas.AtlasMeshData[I];
                if (AMD.MaterialIndex.Intersect(AllChannelMatrefs).Count() == 0) continue;


                var GeneeatMeshAndMatRef = new AtlasTextureDataContainer.MeshAndMatRef(
                    AMD.RefarensMesh,
                    UnityEngine.Object.Instantiate<Mesh>(AtlasDatas.Meshs[AMD.RefarensMesh]),
                    AMD.MaterialIndex
                    );

                var MeshTags = new List<IndexTag>();
                var PoolContainsTags = ToIndexTags(AtlasIslandPool.GetTag());

                for (var SlotIndex = 0; AMD.MaterialIndex.Length > SlotIndex; SlotIndex += 1)
                {
                    var Thistag = new IndexTag(I, SlotIndex);
                    if (PoolContainsTags.Contains(Thistag))
                    {
                        MeshTags.Add(Thistag);
                    }
                    else
                    {
                        var thistagMeshref = AMD.RefarensMesh;
                        var thistagMatSlot = SlotIndex;
                        var thistagMatref = AMD.MaterialIndex[SlotIndex];
                        IndexTag? IdeticalTag = FindIdenticalTag(AtlasDatas, PoolContainsTags, thistagMeshref, thistagMatSlot, thistagMatref);

                        if (IdeticalTag.HasValue)
                        {
                            MeshTags.Add(IdeticalTag.Value);
                        }
                    }
                }


                var MovedPool = new TagIslandPool<IndexTagPlusIslandIndex>();
                foreach (var tag in MeshTags)
                {
                    AtlasDatas.FindIndexTagIslandPool(AtlasIslandPool, MovedPool, tag, false);
                }

                var MovedUV = new List<Vector2>(AMD.UV);
                IslandUtils.IslandPoolMoveUV(AMD.UV, MovedUV, OriginIslandPool, MovedPool);

                GeneeatMeshAndMatRef.Mesh.SetUVs(0, MovedUV);
                GeneeatMeshAndMatRef.Mesh.SetUVs(1, AMD.UV);

                CompiledMeshs.Add(GeneeatMeshAndMatRef);
            }

            Container.ChannnelsMatRef = ChannnelsMatRef;
            Container.AtlasTextures = CompiledAllAtlasTextures;
            Container.GenereatMeshs = CompiledMeshs;
            Container.IsPossibleApply = true;
        }

        public static IndexTag? FindIdenticalTag(AtlasDatas AtlasDatas, HashSet<IndexTag> PoolTags, int FindTagMeshref, int FindTagMatSlot, int FindTagMatref)
        {
            IndexTag? IdeticalTag = null;
            foreach (var ptag in PoolTags)
            {
                var ptagtargetAMD = AtlasDatas.AtlasMeshData[ptag.AtlasMeshDataIndex];
                var ptagtMeshRef = ptagtargetAMD.RefarensMesh;
                var ptagMatSlot = ptag.MaterialSlot;
                var ptagMatref = ptagtargetAMD.MaterialIndex[ptag.MaterialSlot];

                if (FindTagMeshref == ptagtMeshRef && FindTagMatSlot == ptagMatSlot && FindTagMatref == ptagMatref)
                {
                    IdeticalTag = ptag;
                    break;
                }
            }

            return IdeticalTag;
        }

        public static Dictionary<MatData, TagIslandPool<IndexTagPlusIslandIndex>> GetMatDataPool(AtlasDatas AtlasDatas, TagIslandPool<IndexTagPlusIslandIndex> OriginIslandPool, List<MatData> Matdatas)
        {
            var MatDataPeaPool = new Dictionary<MatData, TagIslandPool<IndexTagPlusIslandIndex>>();
            foreach (var Matdata in Matdatas)
            {
                var SepalatePool = AtlasDatas.FindMatIslandPool(OriginIslandPool, Matdata.MaterialRefarens);
                MatDataPeaPool.Add(Matdata, SepalatePool);
            }

            return MatDataPeaPool;
        }

        public static HashSet<IndexTag> ToIndexTags(HashSet<IndexTagPlusIslandIndex> Tags)
        {
            var IndexTag = new HashSet<IndexTag>();
            foreach (var tag in Tags)
            {
                IndexTag.Add(new IndexTag(tag.AtlasMeshDataIndex, tag.MaterialSlot));
            }

            return IndexTag;
        }

        public AvatarDomain RevartDomain;
        public List<MeshPea> RevartMeshs;
        public override void Apply(AvatarDomain avatarMaterialDomain = null)
        {
            if (!IsPossibleApply) return;
            if (_isApply == true) return;
            var NawRendares = Renderers;
            if (avatarMaterialDomain == null) { avatarMaterialDomain = new AvatarDomain(NawRendares); RevartDomain = avatarMaterialDomain; }
            else { RevartDomain = avatarMaterialDomain.GetBackUp(); }
            Container.GenereatMaterials = null;

            var GenereatMaterials = new List<List<Material>>();

            var ShaderSupport = new ShaderSupportUtili();

            var ChannnelMatRef = Container.ChannnelsMatRef;
            var GenereatMeshs = Container.GenereatMeshs;
            var AtlasTexs = Container.AtlasTextures;
            var Materials = GetMaterials(NawRendares);
            var RefarensMesh = GetMeshes(NawRendares);

            if (AtlasSettings.Count != AtlasTexs.Count || AtlasSettings.Count != ChannnelMatRef.Count) { return; }


            var nawchannelrevartmeshs = new List<MeshPea>();
            var MMref = ChannnelMatRef.SelectMany(I => I).ToList();
            foreach (var Rendare in NawRendares)
            {
                var Mehs = Rendare.GetMesh();
                var RefMesh = RefarensMesh.IndexOf(Mehs);
                var MatRefs = Rendare.sharedMaterials.Select(Mat => Materials.IndexOf(Mat)).ToArray();

                var TargetMeshdatas = GenereatMeshs.FindAll(MD => MD.RefMesh == RefMesh);
                var TargetMeshdata = TargetMeshdatas.Find(MD => MD.MatRefs.SequenceEqual(MatRefs));

                if (TargetMeshdata == null) continue;

                Rendare.SetMesh(TargetMeshdata.Mesh);
                nawchannelrevartmeshs.Add(new MeshPea(Mehs, TargetMeshdata.Mesh));
            }


            var ChannelCount = AtlasSettings.Count;
            for (var Channel = 0; ChannelCount > Channel; Channel += 1)
            {
                var Meshdata = ChannnelMatRef[Channel];
                var AtlasTex = AtlasTexs[Channel];
                var AtlasSetting = AtlasSettings[Channel];

                var ChannnelMatRefs = ChannnelMatRef[Channel];

                if (AtlasSetting.IsMargeMaterial)
                {
                    var MargeMat = AtlasSetting.MargeRefarensMaterial != null ? AtlasSetting.MargeRefarensMaterial : Materials[ChannnelMatRefs.First()];
                    Material GenereatMat = GenereatAtlasMat(MargeMat, AtlasTex, ShaderSupport, AtlasSetting.ForseSetTexture);

                    var DistMats = ChannnelMatRefs.Select(Matref => Materials[Matref]).ToList();
                    avatarMaterialDomain.SetMaterials(DistMats, GenereatMat);

                    GenereatMaterials.Add(new List<Material>(1) { GenereatMat });
                }
                else
                {
                    Dictionary<Material, Material> MaterialMap = new Dictionary<Material, Material>();
                    foreach (var Matref in ChannnelMatRefs)
                    {
                        var Mat = Materials[Matref];
                        var GenereatMat = GenereatAtlasMat(Mat, AtlasTex, ShaderSupport, AtlasSetting.ForseSetTexture);

                        MaterialMap.Add(Mat, GenereatMat);
                    }

                    avatarMaterialDomain.SetMaterials(MaterialMap);
                    GenereatMaterials.Add(MaterialMap.Values.ToList());
                }
            }

            Container.GenereatMaterials = GenereatMaterials;
            RevartMeshs = nawchannelrevartmeshs;
            _isApply = true;
        }

        private static Material GenereatAtlasMat(Material TargetMat, List<PropAndTexture> AtlasTex, ShaderSupportUtili ShaderSupport, bool ForseSetTexture)
        {
            var EditableTMat = UnityEngine.Object.Instantiate(TargetMat);

            EditableTMat.SetTextures(AtlasTex, ForseSetTexture);
            EditableTMat.RemoveUnusedProperties();
            ShaderSupport.MaterialCustomSetting(EditableTMat);
            return EditableTMat;
        }

        public override void Revart(AvatarDomain avatarMaterialDomain = null)
        {
            if (!IsApply) return;
            _isApply = false;

            RevartDomain.ResetMaterial();
            RevartDomain = null;

            var NawRendares = Renderers;

            var revartmeshdict = new Dictionary<Mesh, Mesh>();
            foreach (var meshpea in RevartMeshs)
            {
                if (!revartmeshdict.ContainsKey(meshpea.SecondMesh))
                {
                    revartmeshdict.Add(meshpea.SecondMesh, meshpea.Mesh);
                }
            }


            foreach (var Rendare in NawRendares)
            {
                var mesh = Rendare.GetMesh();
                if (revartmeshdict.ContainsKey(mesh))
                {
                    Rendare.SetMesh(revartmeshdict[mesh]);
                }
            }
        }



        public static List<Renderer> FilterdRendarer(IReadOnlyList<Renderer> renderers)
        {
            var result = new List<Renderer>();
            foreach (var item in renderers)
            {
                if (item.GetMesh() == null) continue;
                if (item.sharedMaterials.Length == 0) continue;
                if (item.sharedMaterials.Any(Mat => Mat == null)) continue;

                result.Add(item);
            }
            return result;
        }
        public static AtlasDatas GenereatAtlasMeshDatas(IReadOnlyList<Renderer> renderers)
        {
            OrderdHashSet<Mesh> Meshs = GetMeshes(renderers);
            OrderdHashSet<Material> Materials = GetMaterials(renderers);
            List<AtlasMeshData> AtlasMeshData = new List<AtlasMeshData>();

            foreach (var Renderer in renderers)
            {
                var mesh = Renderer.GetMesh();
                var RefMesh = Meshs.IndexOf(mesh);
                var MaterialIndex = Renderer.sharedMaterials.Select(Mat => Materials.IndexOf(Mat)).ToArray();

                var Index = AtlasMeshData.FindIndex(AMD => AMD.RefarensMesh == RefMesh && AMD.MaterialIndex.SequenceEqual(MaterialIndex));
                if (Index == -1)
                {
                    var UV = new List<Vector2>();
                    mesh.GetUVs(0, UV);

                    AtlasMeshData.Add(new AtlasMeshData(
                        RefMesh,
                        mesh.GetSubTraiangel(),
                        UV,
                        MaterialIndex
                        ));
                }
            }

            return new AtlasDatas(Meshs, Materials, AtlasMeshData);
        }

        public static OrderdHashSet<Material> GetMaterials(IReadOnlyList<Renderer> renderers)
        {
            OrderdHashSet<Material> Materials = new OrderdHashSet<Material>();

            foreach (var Renderer in renderers)
            {
                foreach (var Mat in Renderer.sharedMaterials)
                {
                    Materials.Add(Mat);
                }
            }

            return Materials;
        }

        public static OrderdHashSet<Mesh> GetMeshes(IReadOnlyList<Renderer> renderers)
        {
            OrderdHashSet<Mesh> Meshs = new OrderdHashSet<Mesh>();

            foreach (var Renderer in renderers)
            {
                var mesh = Renderer.GetMesh();
                Meshs.Add(mesh);
            }

            return Meshs;
        }

        public void AutomaticOffSetSetting()
        {
            var TargetMats = MatSelectors.Where(MS => MS.IsTarget).ToArray();

            var MaxTexPixsel = 0;

            foreach (var MatSelect in TargetMats)
            {
                var Tex = MatSelect.Material.mainTexture;
                MaxTexPixsel = Mathf.Max(MaxTexPixsel, Tex.width * Tex.height);
            }

            foreach (var MatSelect in TargetMats)
            {
                var Tex = MatSelect.Material.mainTexture;
                MatSelect.TextureSizeOffSet = (Tex.width * Tex.height) / (float)MaxTexPixsel;
            }
        }
    }
    public class AtlasDatas
    {
        public OrderdHashSet<Mesh> Meshs;
        public OrderdHashSet<Material> Materials;
        public List<AtlasMeshData> AtlasMeshData;

        public AtlasDatas(OrderdHashSet<Mesh> meshs, OrderdHashSet<Material> materials, List<AtlasMeshData> atlasMeshData)
        {
            Meshs = meshs;
            Materials = materials;
            AtlasMeshData = atlasMeshData;
        }

        public TagIslandPool<IndexTagPlusIslandIndex> GeneratedIslandPool(bool UseIslandCash)
        {
            if (UseIslandCash)
            {
                var CacheIslands = AssetSaveHelper.LoadAssets<IslandCache>().ConvertAll(i => i.CacheObject);
                var diffCacheIslands = new List<IslandCacheObject>(CacheIslands);

                var IslandPool = GeneratedIslandPool(CacheIslands);

                AssetSaveHelper.SaveAssets(CacheIslands.Except(diffCacheIslands).Select(i =>
                {
                    var NI = ScriptableObject.CreateInstance<IslandCache>();
                    NI.CacheObject = i; NI.name = "IslandCache";
                    return NI;
                }));

                return IslandPool;
            }
            else
            {
                return GeneratedIslandPool(null);
            }

        }

        public TagIslandPool<IndexTagPlusIslandIndex> GeneratedIslandPool(List<IslandCacheObject> islandCache)
        {
            var IslandPool = new TagIslandPool<IndexTag>();
            var AMDCount = AtlasMeshData.Count;
            for (int AMDIndex = 0; AMDIndex < AMDCount; AMDIndex += 1)
            {
                var AMD = AtlasMeshData[AMDIndex];

                for (var SlotIndex = 0; AMD.MaterialIndex.Length > SlotIndex; SlotIndex += 1)
                {
                    var Tag = new IndexTag(AMDIndex, SlotIndex);
                    var Islands = IslandUtils.UVtoIsland(AMD.Traiangles[SlotIndex], AMD.UV, islandCache);
                    IslandPool.AddRangeIsland(Islands, Tag);
                }
            }

            var tagset = IslandPool.GetTag();
            var RefMesh_MatSlot_RefMat_Hash = new HashSet<(int, int, int)>();
            var DeleteTags = new List<IndexTag>();

            foreach (var tag in tagset)
            {
                var AMD = AtlasMeshData[tag.AtlasMeshDataIndex];
                var RefMesh = AMD.RefarensMesh;
                var MaterialSlot = tag.MaterialSlot;
                var RefMat = AMD.MaterialIndex[tag.MaterialSlot];
                var RMesh_MSlot_RMat = (RefMesh, MaterialSlot, RefMat);

                if (RefMesh_MatSlot_RefMat_Hash.Contains(RMesh_MSlot_RMat))
                {
                    DeleteTags.Add(tag);
                }
                else
                {
                    RefMesh_MatSlot_RefMat_Hash.Add(RMesh_MSlot_RMat);
                }
            }

            foreach (var DeletTag in DeleteTags)
            {
                IslandPool.RemoveAll(DeletTag);
            }

            var TagIslandPool = new TagIslandPool<IndexTagPlusIslandIndex>();
            var PoolCount = IslandPool.Islands.Count;
            for (int PoolIndex = 0; PoolIndex < PoolCount; PoolIndex += 1)
            {
                var OldTag = IslandPool[PoolIndex].tag;
                TagIslandPool.AddIsland(new TagIsland<IndexTagPlusIslandIndex>(IslandPool[PoolIndex], new IndexTagPlusIslandIndex(OldTag.AtlasMeshDataIndex, OldTag.MaterialSlot, PoolIndex)));
            }
            return TagIslandPool;
        }

        public int GetMaterialRefarens(IndexTagPlusIslandIndex indexTag)
        {
            return AtlasMeshData[indexTag.AtlasMeshDataIndex].MaterialIndex[indexTag.MaterialSlot];

        }
        public int GetMaterialRefarens(IndexTag indexTag)
        {
            return AtlasMeshData[indexTag.AtlasMeshDataIndex].MaterialIndex[indexTag.MaterialSlot];
        }

        public TagIslandPool<IndexTagPlusIslandIndex> FindMatIslandPool(TagIslandPool<IndexTagPlusIslandIndex> Souse, int MatRef, bool DeepClone = true)
        {
            var result = new TagIslandPool<IndexTagPlusIslandIndex>();
            foreach (var Island in Souse)
            {
                if (GetMaterialRefarens(Island.tag) == MatRef)
                {
                    result.AddIsland(DeepClone ? new TagIsland<IndexTagPlusIslandIndex>(Island) : Island);
                }
            }
            return result;
        }
        public void FindMatIslandPool(TagIslandPool<IndexTagPlusIslandIndex> Souse, TagIslandPool<IndexTagPlusIslandIndex> AddTarget, int MatRef, bool DeepClone = true)
        {
            foreach (var Island in Souse)
            {
                if (GetMaterialRefarens(Island.tag) == MatRef)
                {
                    AddTarget.AddIsland(DeepClone ? new TagIsland<IndexTagPlusIslandIndex>(Island) : Island);
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
        public int RefarensMesh;
        public IReadOnlyList<IReadOnlyList<TraiangleIndex>> Traiangles;
        public List<Vector2> UV;
        public List<Vector2> GeneratedUV;
        public int[] MaterialIndex;

        public AtlasMeshData(int refarensMesh, IReadOnlyList<IReadOnlyList<TraiangleIndex>> traiangles, List<Vector2> uV, int[] materialIndex)
        {
            RefarensMesh = refarensMesh;
            Traiangles = traiangles;
            UV = uV;
            MaterialIndex = materialIndex;
        }
        public AtlasMeshData()
        {
            Traiangles = new List<List<TraiangleIndex>>();
            UV = new List<Vector2>();
        }
    }
    [Serializable]
    public class MatSelector
    {
        public Material Material;
        public bool IsTarget = false;
        public int AtlsChannel = 0;
        public float TextureSizeOffSet = 1;
    }
    [Serializable]
    public class MatData
    {
        public int MaterialRefarens;
        public float TextureSizeOffSet = 1;
        public List<PropAndTexture> PropAndTextures;
    }
    [Serializable]
    public class AtlasSetting
    {
        public bool IsMargeMaterial;
        public Material MargeRefarensMaterial;
        public bool ForseSetTexture;
        public Vector2Int AtlasTextureSize = new Vector2Int(2048, 2048);
        public PadingType PadingType = PadingType.EdgeBase;
        public float Pading = -10;
        public IslandSortingType SortingType = IslandSortingType.NextFitDecreasingHeightPlusFloorCeilineg;
    }
    [Serializable]
    public struct MeshPea
    {
        public Mesh Mesh;
        public Mesh SecondMesh;
        public MeshPea(Mesh mesh, Mesh secondMesh)
        {
            Mesh = mesh;
            SecondMesh = secondMesh;
        }



    }
    [Serializable]
    public class SubListMeshPea : SubList<MeshPea>
    {
        public SubListMeshPea(List<MeshPea> subListInstans) : base(subListInstans)
        {
        }
        public static List<SubListMeshPea> ConvartSubList(List<List<MeshPea>> SubListMeshPeas)
        {
            return SubListMeshPeas.ConvertAll(I => new SubListMeshPea(I));
        }
        public static List<List<MeshPea>> ConvartSubList(List<SubListMeshPea> SubListMeshPeas)
        {
            return SubListMeshPeas.ConvertAll(I => I.SubListInstans);
        }

    }
}
#endif