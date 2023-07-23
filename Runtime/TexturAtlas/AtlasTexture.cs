#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using Rs64.TexTransTool.ShaderSupport;
using Rs64.TexTransTool.Island;
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

        public override void Compile()
        {
            if (!IsPossibleCompile) return;
            if (IsApply) return;
            Container.AtlasTextures = null;
            Container.Meshes = null;
            Container.IsPossibleApply = false;

            var SelectRefsMat = new OrderdHashSet<Material>(SelectRefarensMat);

            var TargetRenderers = Renderers;
            var AtlasDatas = GenereatAtlasMeshDatas(TargetRenderers);
            var ShaderSupports = new ShaderSupportUtili();
            var OriginIslandPool = AtlasDatas.GeneratedIslandPool(UseIslandCash);

            var CompiledAllAtlasTextures = new List<List<PropAndTexture>>();
            var CompiledMeshs = new List<List<AtlasTextureDataContainer.MeshAndMatRef>>();

            var ChannelCount = AtlasSettings.Count;
            for (int Channel = 0; Channel < ChannelCount; Channel++)
            {
                var atlasSetting = AtlasSettings[Channel];
                var TargetMats = MatSelectors.Where(MS => MS.IsTarget && MS.AtlsChannel == Channel).ToArray();
                var Matdatas = new List<MatData>();
                foreach (var MatSelector in TargetMats)
                {
                    var Matdata = new MatData();
                    var MatIndex = SelectRefsMat.IndexOf(MatSelector.Material);
                    Matdata.ThisRefMat = MatIndex;
                    Matdata.Material = AtlasDatas.Materials[MatIndex];
                    Matdata.TextureSizeOffSet = MatSelector.TextureSizeOffSet;
                    Matdata.PropAndTextures = ShaderSupports.GetTextures(Matdata.Material);
                    Matdatas.Add(Matdata);
                }

                var AtlasIslandPool = new TagIslandPool<IndexTagPlusIslandIndex>();
                foreach (var Matdata in Matdatas)
                {
                    var SepalatePool = AtlasDatas.FindMatIslandPool(OriginIslandPool, Matdata.ThisRefMat);
                    SepalatePool.OffSetApply(Matdata.TextureSizeOffSet);
                    AtlasIslandPool.AddRangeIsland(SepalatePool.Islands);
                }

                TexturAtlasCompiler.GenereatMovedIlands(atlasSetting.SortingType, AtlasIslandPool);

                var Tags = AtlasIslandPool.GetTag();
                var IndexTag = new HashSet<IndexTag>();
                foreach (var tag in Tags)
                {
                    IndexTag.Add(new IndexTag(tag.AtlasMeshDataIndex, tag.MaterialSlot));
                }

                var Porps = new HashSet<string>();
                foreach (var matdata in Matdatas)
                {
                    Porps.UnionWith(matdata.PropAndTextures.ConvertAll(PaT => PaT.PropertyName));
                }

                var TransMaps = new Dictionary<int, TransMapData>();

                foreach (var matdata in Matdatas)
                {
                    var transMap = new TransMapData(atlasSetting.Pading, atlasSetting.AtlasTextureSize);
                    var matref = matdata.ThisRefMat;

                    foreach (var itag in IndexTag)
                    {
                        if (AtlasDatas.GetMaterialRefarens(itag) != matref) continue;

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

                foreach (var Porp in Porps)
                {
                    var TargetTex = new TransTargetTexture(atlasSetting.AtlasTextureSize, new Color(0, 0, 0, 0), atlasSetting.Pading);
                    foreach (var matdata in Matdatas)
                    {
                        var SousePorp2Tex = matdata.PropAndTextures.Find(I => I.PropertyName == Porp);
                        if (SousePorp2Tex == null) continue;


                        Compiler.TransCompileUseComputeSheder(SousePorp2Tex.Texture2D, TransMaps[matdata.ThisRefMat], TargetTex, TexWrapMode.Stretch);
                    }
                    CompiledAtlasTextures.Add(new PropAndAtlasTex(Porp, TargetTex));
                }

                CompiledAllAtlasTextures.Add(CompiledAtlasTextures.ConvertAll(I => new PropAndTexture(I.PropertyName, I.AtlasTexture.Texture2D)));

                var NewMeshs = new List<AtlasTextureDataContainer.MeshAndMatRef>();
                for (int I = 0; I < AtlasDatas.AtlasMeshData.Count; I += 1)
                {
                    var AMD = AtlasDatas.AtlasMeshData[I];
                    var newmesh = new AtlasTextureDataContainer.MeshAndMatRef(
                        AMD.RefarensMesh,
                        UnityEngine.Object.Instantiate<Mesh>(AtlasDatas.Meshs[AMD.RefarensMesh]),
                        AMD.MaterialIndex
                        );

                    var thistags = new List<IndexTag>();

                    for (var SlotIndex = 0; AMD.MaterialIndex.Length > SlotIndex; SlotIndex += 1)
                    {
                        thistags.Add(new IndexTag(I, SlotIndex));
                    }

                    var MovedPool = new TagIslandPool<IndexTagPlusIslandIndex>();

                    foreach (var tag in thistags)
                    {
                        AtlasDatas.FindIndexTagIslandPool(AtlasIslandPool, MovedPool, tag, false);
                    }

                    var MovedUV = new List<Vector2>(AMD.UV);
                    IslandUtils.IslandPoolMoveUV(AMD.UV, MovedUV, OriginIslandPool, MovedPool);

                    newmesh.Mesh.SetUVs(0, MovedUV);
                    newmesh.Mesh.SetUVs(1, AMD.UV);

                    NewMeshs.Add(newmesh);
                }

                CompiledMeshs.Add(NewMeshs);

            }

            Container.AtlasTextures = CompiledAllAtlasTextures;
            Container.Meshes = CompiledMeshs;
            Container.IsPossibleApply = true;
        }

        public AvatarDomain RevartDomain;
        public List<List<MeshPea>> RevartMeshs;
        public override void Apply(AvatarDomain avatarMaterialDomain = null)
        {
            if (!IsPossibleApply) return;
            if (_isApply == true) return;
            _isApply = true;
            var NawRendares = Renderers;
            if (avatarMaterialDomain == null) { avatarMaterialDomain = new AvatarDomain(NawRendares); RevartDomain = avatarMaterialDomain; }
            else { RevartDomain = avatarMaterialDomain.GetBackUp(); }
            Container.GenereatMaterials = null;

            var GenereatMaterials = new List<List<Material>>();
            var Nawrevartmeshs = new List<List<MeshPea>>();

            var ShaderSupport = new ShaderSupportUtili();

            var MeshDatas = Container.Meshes;
            var AtlasTexs = Container.AtlasTextures;
            var Materials = GetMaterials(NawRendares);
            var RefarensMesh = GetMeshes(NawRendares);

            if (AtlasSettings.Count == AtlasTexs.Count && AtlasSettings.Count == MeshDatas.Count) { return; }

            var ChannelCount = AtlasSettings.Count;
            for (var Channel = 0; ChannelCount > Channel; Channel += 1)
            {
                var Meshdata = MeshDatas[Channel];
                var AtlasTex = AtlasTexs[Channel];
                var AtlasSetting = AtlasSettings[Channel];

                var nawchannelrevartmeshs = new List<MeshPea>();
                foreach (var Rendare in NawRendares)
                {
                    var Mehs = Rendare.GetMesh();
                    var MeshRef = RefarensMesh.IndexOf(Mehs);
                    var MatRefs = Rendare.sharedMaterials.Select(Mat => Materials.IndexOf(Mat)).ToArray();

                    var TargetMeshdatas = Meshdata.FindAll(MD => MD.RefMesh == MeshRef);
                    var TargetMeshdata = TargetMeshdatas.Find(MD => MD.MatRefs.SequenceEqual(MatRefs));

                    if (TargetMeshdata == null) continue;

                    Rendare.SetMesh(TargetMeshdata.Mesh);
                    nawchannelrevartmeshs.Add(new MeshPea(Mehs, TargetMeshdata.Mesh));
                }

                var ChannnelMatRefs = new HashSet<int>();
                foreach (var md in Meshdata)
                {
                    ChannnelMatRefs.UnionWith(md.MatRefs);
                }

                if (AtlasSetting.IsMargeMaterial)
                {
                    var MargeMat = AtlasSetting.MargeRefarensMaterial != null ? AtlasSetting.MargeRefarensMaterial : Materials[ChannnelMatRefs.First()];
                    var EditableTMat = UnityEngine.Object.Instantiate(MargeMat);

                    EditableTMat.SetTextures(AtlasTex, AtlasSetting.ForseSetTexture);
                    EditableTMat.RemoveUnusedProperties();
                    ShaderSupport.MaterialCustomSetting(EditableTMat);

                    var distmats = ChannnelMatRefs.Select(Matref => Materials[Matref]).ToList();
                    avatarMaterialDomain.SetMaterials(distmats, EditableTMat);

                    GenereatMaterials.Add(new List<Material>(1) { EditableTMat });
                }
                else
                {
                    Dictionary<Material, Material> MaterialMap = new Dictionary<Material, Material>();
                    foreach (var Matref in ChannnelMatRefs)
                    {
                        var Mat = Materials[Matref];

                        var EditableTMat = UnityEngine.Object.Instantiate(Mat);

                        EditableTMat.SetTextures(AtlasTex, AtlasSetting.ForseSetTexture);
                        EditableTMat.RemoveUnusedProperties();
                        ShaderSupport.MaterialCustomSetting(EditableTMat);

                        MaterialMap.Add(Mat, EditableTMat);
                    }

                    avatarMaterialDomain.SetMaterials(MaterialMap);
                    GenereatMaterials.Add(MaterialMap.Values.ToList());
                }
            }

            Container.GenereatMaterials = GenereatMaterials;
            RevartMeshs = Nawrevartmeshs;
        }



        public override void Revart(AvatarDomain avatarMaterialDomain = null)
        {
            if (!IsApply) return;
            _isApply = false;

            RevartDomain.ResetMaterial();
            RevartDomain = null;

            var NawRendares = Renderers;

            var revartmeshdict = new Dictionary<Mesh, Mesh>();
            foreach (var meshpea in RevartMeshs.SelectMany(I => I))
            {
                revartmeshdict.Add(meshpea.SecondMesh, meshpea.Mesh);
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
            var PoolCount = TagIslandPool.Islands.Count;
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
        public int ThisRefMat;
        public Material Material;
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
}
#endif