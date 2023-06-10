#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rs64.TexTransTool.ShaderSupport;
using UnityEditor;
using UnityEngine;
namespace Rs64.TexTransTool.TexturAtlas
{
    public static class TexturAtlasCompiler
    {
        public static void AtlasSetCompile(AtlasSet Target, ComputeShader TransMapperCS = null)
        {
            var Data = Target.GetCompileData();
            if (Target.Contenar == null) { Target.Contenar = AssetSaveHelper.SaveAsset<CompileDataContenar>(ScriptableObject.CreateInstance<CompileDataContenar>()); }


            var Contenar = Target.Contenar;
            var UVs = Data.GetUVs();
            var IslandPool = IslandUtils.AsyncGeneretIslandPool(Data.meshes, UVs, Data.TargetMeshIndex).Result;


            var NotMovedIslandPool = new IslandPool(IslandPool);

            foreach (var islandI in IslandPool.IslandPoolList)
            {
                var OffSetScaile = Data.Offsets[islandI.MapIndex];
                var island = islandI.island;
                island.Size *= OffSetScaile;
            }

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

        public static void PutData(this CompileDataContenar Contenar, AtlasCompileData Data, List<PropAndAtlasTex> TargetPorpAndAtlasTexs)
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
            Contenar.SetSubAsset<Mesh>(GenereatMesh);
            Contenar.GenereatMeshs = GenereatMesh;
            Contenar.DeletTexture();
            Contenar.SetTextures(TargetPorpAndAtlasTexs.ConvertAll<PropAndTexture>(i => (PropAndTexture)i));
        }

        private static void GenereatMovedIlands(IslandSortingType SortingType, IslandPool IslandPool)
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
                default: throw new ArgumentException();
            }
        }

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
                    foreach (var meshIndex in PropAndSTex.MeshIndex[TexIndex])
                    {
                        if (!(AtlasMapDatas.Length > meshIndex.Index && AtlasMapDatas[meshIndex.Index].Length > meshIndex.SubMeshIndex)) continue;
                        var TransMap = AtlasMapDatas[meshIndex.Index][meshIndex.SubMeshIndex];
                        _ = Compiler.TransCompileUseGetPixsel(SouseTxture, TransMap, TargetTex, TexWrapMode.Stretch);
                    }

                }
            }
        }



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
                var SouseUV = new List<Vector2>();
                var TargetUV = new List<Vector2>();
                Mesh.GetUVs(1, SouseUV);
                Mesh.GetUVs(0, TargetUV);
                var TargetTexScliUV = TransMapper.UVtoTexScale(TargetUV, AtlasTextureSize);
                RetuneData[Index.Index][Index.SubMeshIndex] = TransMapper.TransMapGeneratUseComputeSheder(TransMapperCS, AtlasMapDataI, triangles, TargetTexScliUV, SouseUV, padingType);
            }

            return RetuneData;
        }
    }

    public class AtlasCompileData
    {
        public List<PropAndSouseTexuters> SouseTextures = new List<PropAndSouseTexuters>();
        public List<Mesh> DistMesh = new List<Mesh>();
        public List<Mesh> meshes = new List<Mesh>();
        public List<MeshIndex> TargetMeshIndex = new List<MeshIndex>();
        public List<float> Offsets = new List<float>();
        public Vector2Int AtlasTextureSize;
        public float Pading;
        public PadingType PadingType;
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
                TargetPorpAndAtlasTexs.Add(new PropAndAtlasTex(new TransTargetTexture(new Texture2D(this.AtlasTextureSize.x, this.AtlasTextureSize.y), this.Pading), textures.PropertyName));
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
    }

    public class MeshIndex
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

        public static explicit operator PropAndTexture(PropAndAtlasTex s)
        {
            return new PropAndTexture(s.PropertyName, s.AtlasTexture.Texture2D);
        }
    }


}
#endif