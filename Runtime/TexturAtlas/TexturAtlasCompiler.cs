#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rs64.TexTransTool.Island;
using Rs64.TexTransTool.ShaderSupport;
using UnityEditor;
using UnityEngine;
namespace Rs64.TexTransTool.TexturAtlas
{
    public static class TexturAtlasCompiler
    {









        /*

                public static void AtlasSetCompile(AtlasSet Target, ComputeShader TransMapperCS = null)
                {

                    var Data = Target.GetCompileData();

                    var Contenar = Target.Contenar;
                    var UVs = Data.GetUVs();

                    IslandPool IslandPool;

                    if (Data.UseIslandCash)
                    {
                        var CacheIslands = AssetSaveHelper.LoadAssets<IslandCache>().ConvertAll(i => i.CacheObject);
                        var diffCacheIslands = new List<IslandCacheObject>(CacheIslands);

                        IslandPool = IslandUtils.AsyncGeneretIslandPool(Data.meshes, UVs, Data.TargetMeshIndex, CacheIslands).Result;

                        AssetSaveHelper.SaveAssets(CacheIslands.Except(diffCacheIslands).Select(i =>
                        {
                            var NI = ScriptableObject.CreateInstance<IslandCache>();
                            NI.CacheObject = i; NI.name = "IslandCache";
                            return NI;
                        }));
                    }
                    else
                    {
                        IslandPool = IslandUtils.AsyncGeneretIslandPool(Data.meshes, UVs, Data.TargetMeshIndex).Result;
                    }


                    var NotMovedIslandPool = new IslandPool(IslandPool);

                    OffSetApply(Data.Offsets, IslandPool);

                    GenereatMovedIlands(Target.SortingType, IslandPool);

                    var MovedUVs = IslandUtils.UVsMoveAsync(UVs, NotMovedIslandPool, IslandPool).Result;


                    var NotMevedUVs = Data.GetUVs();
                    Data.SetUVs(MovedUVs, 0);
                    Data.SetUVs(NotMevedUVs, 1);


                    var AtlasMapDatas = GeneratAtlasMaps(Data.TargetMeshIndex, Data.meshes, TransMapperCS, Data.Pading, Data.AtlasTextureSize, Data.PadingType);

                    var TargetPorpAndAtlasTexs = Data.GeneretTargetEmptyTextures();


                    AtlasTextureCompile(Data.SouseTextures, AtlasMapDatas, TargetPorpAndAtlasTexs);


                    Contenar.PutData(Data, TargetPorpAndAtlasTexs);


                    Target.AtlasCompilePostCallBack.Invoke(Contenar);

                }
                */

        public static void OffSetApply<T>(this TagIslandPool<T> IslandPool, float Offset)
        {
            foreach (var islandI in IslandPool)
            {
                var island = islandI.island;
                island.Size *= Offset;
            }
        }
        /*
                public static void PutData(this TexturAtlasDataContenar Contenar, AtlasCompileData Data, List<PropAndAtlasTex> TargetPorpAndAtlasTexs)
                {
                    var MeshIndexs = Data.TargetMeshIndex.Distinct(new MeshIndex.IndexEqualityl());
                    var DistMesh = new List<Mesh>();
                    var GenereatMesh = new List<Mesh>();
                    foreach (var Index in MeshIndexs)
                    {
                        DistMesh.Add(Data.DistMesh[Index.Index]);
                        GenereatMesh.Add(Data.meshes[Index.Index]);
                    }

                    Contenar.DistMeshs = DistMesh;
                    Contenar.GenereatMeshs = GenereatMesh;
                    Contenar.DeletTexture();
                    Contenar.SetTextures(TargetPorpAndAtlasTexs.ConvertAll<PropAndTexture>(i => (PropAndTexture)i));
                }
        */
        public static void GenereatMovedIlands<T>(IslandSortingType SortingType, TagIslandPool<T> IslandPool)
        {
            switch (SortingType)
            {
                case IslandSortingType.EvenlySpaced:
                    {
                        IslandUtils.IslandPoolEvenlySpaced(IslandPool);
                        break;
                    }
                case IslandSortingType.NextFitDecreasingHeight:
                    {
                        IslandUtils.IslandPoolNextFitDecreasingHeight(IslandPool);
                        break;
                    }
                case IslandSortingType.NextFitDecreasingHeightPlusFloorCeilineg:
                    {
                        IslandUtils.IslandPoolNextFitDecreasingHeightPlusFloorCeilineg(IslandPool);
                        break;
                    }

                default: throw new ArgumentException();
            }
        }
        /*
                [Obsolete]
                private static List<List<Vector2>> InUVsMove(this ExecuteClient ClientSelect, IslandPool IslandPool, List<List<Vector2>> UVs, IslandPool MovedPool)
                {
                    List<List<Vector2>> MovedUVs;
                    switch (ClientSelect)
                    {
                        case ExecuteClient.CPU:
                            {
                                MovedUVs = IslandUtils.UVsMove(UVs, IslandPool, MovedPool);
                                break;
                            }
                        case ExecuteClient.AsyncCPU:
                        default:
                            {
                                MovedUVs = IslandUtils.UVsMoveAsync(UVs, IslandPool, MovedPool).Result;

                                break;

                            }
                    }

                    return MovedUVs;
                }
                */


        [Obsolete]
        public static void AtlasTextureCompile(List<PropAndSouseTexuters> SouseList, TransMapData[][] AtlasMapDatas, List<PropAndAtlasTex> TargetPorpAndAtlasTexs)
        {
            foreach (var PropAndSTex in SouseList)
            {
                int TexIndex = -1;
                string PropertyName = PropAndSTex.PropertyName;
                var TargetTex = TargetPorpAndAtlasTexs.Find(i => i.PropertyName == PropertyName).AtlasTexture;
                foreach (var SouseTxture in PropAndSTex.Texture2Ds)
                {
                    TexIndex += 1;
                    var TransMaps = new List<TransMapData>();
                    foreach (var meshIndex in PropAndSTex.MeshIndex[TexIndex])
                    {
                        if (!(AtlasMapDatas.Length > meshIndex.Index && AtlasMapDatas[meshIndex.Index].Length > meshIndex.SubMeshIndex)) continue;
                        var TransMap = AtlasMapDatas[meshIndex.Index][meshIndex.SubMeshIndex];
                        if (TransMap == null) continue;
                        TransMaps.Add(TransMap);
                    }
                    _ = Compiler.TransCompileUseComputeSheder(SouseTxture, TransMaps, TargetTex, TexWrapMode.Stretch);

                }
            }
        }


        [Obsolete]
        public static TransMapData[][] GeneratAtlasMaps(List<MeshIndex> TargetIndexs, List<Mesh> TargetMeshs, ComputeShader TransMapperCS, float Pading, Vector2Int AtlasTextureSize, PadingType padingType)
        {
            TransMapData[][] RetuneData = new TransMapData[TargetMeshs.Count][];
            int index = -1;
            foreach (var mesh in TargetMeshs)
            {
                index += 1;
                RetuneData[index] = new TransMapData[mesh.subMeshCount];
            }

            foreach (var Index in TargetIndexs)
            {
                var Mesh = TargetMeshs[Index.Index];
                var AtlasMapDataI = new TransMapData(Pading, AtlasTextureSize);
                var triangles = Utils.ToList(Mesh.GetTriangles(Index.SubMeshIndex));
                var capa = Mesh.vertexCount;
                var SouseUV = new List<Vector2>(capa);
                var TargetUV = new List<Vector2>(capa);
                Mesh.GetUVs(1, SouseUV);
                Mesh.GetUVs(0, TargetUV);
                TransMapper.UVtoTexScale(TargetUV, AtlasTextureSize); var TargetTexScliUV = TargetUV;
                RetuneData[Index.Index][Index.SubMeshIndex] = TransMapper.TransMapGeneratUseComputeSheder(TransMapperCS, AtlasMapDataI, triangles, TargetTexScliUV, SouseUV, padingType);
            }

            return RetuneData;
        }
    }
    [Obsolete]
    public class AtlasCompileData
    {
        public List<PropAndSouseTexuters> SouseTextures = new List<PropAndSouseTexuters>();
        public List<Mesh> DistMesh = new List<Mesh>();
        public List<Mesh> meshes = new List<Mesh>();
        public List<MeshIndex> TargetMeshIndex = new List<MeshIndex>();
        public Dictionary<MeshIndex, float> Offsets = new Dictionary<MeshIndex, float>();
        public Vector2Int AtlasTextureSize;
        public float Pading;
        public PadingType PadingType;

        public bool UseIslandCash;
        public void AddTexture(PropAndTexture AddPropAndtex, MeshIndex addIndex)
        {
            var Texture = SouseTextures.Find(i => i.PropertyName == AddPropAndtex.PropertyName);
            if (Texture != null)
            {
                if (!Texture.Texture2Ds.Any(i => i == AddPropAndtex.Texture2D))
                {
                    Texture.Texture2Ds.Add(AddPropAndtex.Texture2D);
                    Texture.MeshIndex.Add(new List<MeshIndex>() { addIndex });
                }
                else
                {
                    var TexIndex = Texture.Texture2Ds.IndexOf(AddPropAndtex.Texture2D);
                    Texture.MeshIndex[TexIndex].Add(addIndex);
                }

            }
            else
            {
                SouseTextures.Add(new PropAndSouseTexuters(AddPropAndtex, new List<List<MeshIndex>>() { new List<MeshIndex>() { addIndex } }));
            }
        }

        public List<PropAndAtlasTex> GeneretTargetEmptyTextures()
        {
            var TargetPorpAndAtlasTexs = new List<PropAndAtlasTex>();
            foreach (var textures in this.SouseTextures)
            {
                TargetPorpAndAtlasTexs.Add(new PropAndAtlasTex(new TransTargetTexture(new Texture2D(this.AtlasTextureSize.x, this.AtlasTextureSize.y), new TowDMap<float>(this.Pading, AtlasTextureSize)), textures.PropertyName));
            }
            return TargetPorpAndAtlasTexs;
        }


        public void SetPropatyAndTexs(List<Renderer> renderers, List<Material> SelectMateril, List<IShaderSupport> sappotedListInstans)
        {
            int MeshCount = -1;
            foreach (var Rendera in renderers)
            {
                if (Rendera.GetMesh() == null) continue;
                MeshCount += 1;
                int SubMeshCount = -1;
                foreach (var mat in Rendera.sharedMaterials)
                {
                    SubMeshCount += 1;
                    if (SelectMateril.Contains(mat))
                    {
                        MaterialTextureAdd(sappotedListInstans, MeshCount, SubMeshCount, mat);
                    }
                }
            }

            void MaterialTextureAdd(List<IShaderSupport> ApportList, int meshCount, int subMeshCount, Material mat)
            {
                var MeshIndex = new MeshIndex(meshCount, subMeshCount);
                var SupportShederI = ApportList.Find(i =>
                {
                    return mat.shader.name.Contains(i.SupprotShaderName);
                });

                if (SupportShederI != null)
                {
                    var textures = SupportShederI.GetPropertyAndTextures(mat);
                    foreach (var Texture in textures)
                    {
                        if (Texture.Texture2D != null)
                        {
                            AddTexture(Texture, MeshIndex);
                        }
                    }
                }
                else
                {
                    var PropertyName = "_MainTex";
                    if (mat.GetTexture(PropertyName) is Texture2D texture2D)
                    {
                        PropAndTexture DefoultTexAndprop = new PropAndTexture(PropertyName, texture2D);
                        AddTexture(DefoultTexAndprop, MeshIndex);
                    }

                }
            }

        }
    }

    public enum IslandSortingType
    {
        EvenlySpaced,
        NextFitDecreasingHeight,
        NextFitDecreasingHeightPlusFloorCeilineg,
    }

    public struct MeshIndex
    {
        public int Index;
        public int SubMeshIndex;

        public MeshIndex(int index, int subMeshIndex)
        {
            Index = index;
            SubMeshIndex = subMeshIndex;
        }

        public class IndexEqualityl : IEqualityComparer<MeshIndex>
        {
            public bool Equals(MeshIndex x, MeshIndex y)
            {
                return x.Index == y.Index;
            }

            public int GetHashCode(MeshIndex obj)
            {
                return obj.Index.GetHashCode();
            }
        }
    }

    public class PropAndSouseTexuters
    {
        public string PropertyName;
        public List<Texture2D> Texture2Ds;
        public List<List<MeshIndex>> MeshIndex;

        public PropAndSouseTexuters(string propertyName, List<Texture2D> textures, List<List<MeshIndex>> meshIndex)
        {
            PropertyName = propertyName;
            Texture2Ds = textures;
            MeshIndex = meshIndex;
        }
        public PropAndSouseTexuters(PropAndTexture textures, List<List<MeshIndex>> meshIndex)
        {
            PropertyName = textures.PropertyName;
            Texture2Ds = new List<Texture2D>() { textures.Texture2D };
            MeshIndex = meshIndex;
        }

    }

    [Serializable]
    public class PropAndAtlasTex
    {
        public string PropertyName;
        public TransTargetTexture AtlasTexture;

        public PropAndAtlasTex(TransTargetTexture texture2D, string propertyName)
        {
            AtlasTexture = texture2D;
            PropertyName = propertyName;
        }
        public PropAndAtlasTex(string propertyName, TransTargetTexture texture2D)
        {
            AtlasTexture = texture2D;
            PropertyName = propertyName;
        }

        public static explicit operator PropAndTexture(PropAndAtlasTex s)
        {
            return new PropAndTexture(s.PropertyName, s.AtlasTexture.Texture2D);
        }
    }


}
#endif