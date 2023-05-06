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
        public static void AtlasSetCompile(AtlasSetObject Target, ExecuteClient ClientSelect = ExecuteClient.AsyncCPU, ComputeShader TransMapperCS = null)
        {
            var Data = GetCompileData(Target);
            if (Target.Contenar == null) { Target.Contenar = AssetSaveHelper.SaveAsset<CompileDataContenar>(ScriptableObject.CreateInstance<CompileDataContenar>());}

            var Contenar = Target.Contenar;

            var IslandPool = Data.AsyncGeneretIslandPool().Result;

            var UVs = Data.GetUVs();
            var MovedUVs = GenereatNewMovedUVs(Target.SortingType, ClientSelect, IslandPool, UVs);

            var NotMevedUVs = Data.GetUVs();
            Data.SetUVs(MovedUVs, 0);
            Data.SetUVs(NotMevedUVs, 1);

            var AtlasMapDatas = GeneratAtlasMaps(Data.meshes, ClientSelect, TransMapperCS, Data.Pading, Data.AtlasTextureSize, Data.PadingType);

            var TargetPorpAndAtlasTexs = Data.GeneretTargetEmptyTextures();

            AtlasTextureCompile(Data.SouseTextures, AtlasMapDatas, TargetPorpAndAtlasTexs);

            Contenar.PutData(Data, TargetPorpAndAtlasTexs);

            Target.AtlasCompilePostCallBack.Invoke(Contenar);
        }

        public static void PutData(this CompileDataContenar Contenar, CompileData Data, List<PropAndAtlasTex> TargetPorpAndAtlasTexs)
        {
            Contenar.SetSubAsset<Mesh>(Data.meshes);
            Contenar.Meshs = Data.meshes;
            Contenar.DeletTexture();
            Contenar.SetTextures(TargetPorpAndAtlasTexs.ConvertAll<PropAndTexture>(i => (PropAndTexture)i));
        }
        public static CompileData GetCompileData(AtlasSetObject Target)
        {
            CompileData Data = new CompileData();

            List<Renderer> renderers = new List<Renderer>(Target.AtlasTargetMeshs);
            renderers.AddRange(Target.AtlasTargetStaticMeshs);

            Data.SetPropatyAndTexs(renderers, ShaderSupportUtil.GetSupprotInstans());

            var Meshs = Target.AtlasTargetMeshs.ConvertAll<Mesh>(i => i.sharedMesh);
            Meshs.AddRange(Target.AtlasTargetStaticMeshs.ConvertAll<Mesh>(i => i.GetComponent<MeshFilter>().sharedMesh));
            Data.meshes = Meshs.ConvertAll<Mesh>(i => UnityEngine.Object.Instantiate<Mesh>(i));
            Data.AtlasTextureSize = Target.AtlasTextureSize;
            Data.Pading = Target.Pading;
            Data.PadingType = Target.PadingType;
            return Data;
        }
        public static List<List<Vector2>> GenereatNewMovedUVs(IslandSortingType SortingType, ExecuteClient ClientSelect, IslandPool IslandPool, List<List<Vector2>> UVs)
        {
            IslandPool MovedPool = GenereatMovedIlands(SortingType, IslandPool);
            return IslandUtils.UVsMoveAsync(UVs, IslandPool, MovedPool).Result;
        }

        private static IslandPool GenereatMovedIlands(IslandSortingType SortingType, IslandPool IslandPool)
        {
            IslandPool MovedPool;
            switch (SortingType)
            {
                case IslandSortingType.EvenlySpaced:
                    {
                        MovedPool = IslandUtils.IslandPoolEvenlySpaced(IslandPool);
                        break;
                    }
                case IslandSortingType.NextFitDecreasingHeight:
                    {
                        MovedPool = IslandUtils.IslandPoolNextFitDecreasingHeight(IslandPool);
                        break;
                    }
                default: throw new ArgumentException();
            }

            return MovedPool;
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

        [Obsolete]
        public static IslandPool InGenereatIslandPool(this ExecuteClient ClientSelect, CompileData Data)
        {
            IslandPool IslandPool;
            switch (ClientSelect)
            {
                case ExecuteClient.CPU:
                    {
                        IslandPool = Data.GeneretIslandPool();
                        break;
                    }
                case ExecuteClient.AsyncCPU:
                default:
                    {
                        IslandPool = Data.AsyncGeneretIslandPool().Result;
                        break;
                    }
            }

            return IslandPool;
        }
        public static void AtlasTextureCompile(List<PropAndSouseTexuters> SouseList, List<List<TransMapData>> AtlasMapDatas, List<PropAndAtlasTex> TargetPorpAndAtlasTexs)
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
                        var AtlasMapData = AtlasMapDatas[meshIndex.Index][meshIndex.SubMeshIndex];
                        Compiler.TransCompileUseGetPixsel(SouseTxture, AtlasMapData, TargetTex);
                    }

                }
            }
        }



        public static List<List<TransMapData>> GeneratAtlasMaps(List<Mesh> TargetMeshs, ExecuteClient clientSelect, ComputeShader TransMapperCS, float Pading, Vector2Int AtlasTextureSize, PadingType padingType)
        {
            List<List<TransMapData>> Maps = new List<List<TransMapData>>();
            foreach (var Mesh in TargetMeshs)
            {
                List<TransMapData> SubMaps = new List<TransMapData>();
                foreach (var SubMeshIndex in Enumerable.Range(0, Mesh.subMeshCount))
                {
                    var AtlasMapDataI = new TransMapData(Pading, AtlasTextureSize);
                    var triangles = Utils.ToList(Mesh.GetTriangles(SubMeshIndex));
                    var SouseUV = new List<Vector2>();
                    var TargetUV = new List<Vector2>();
                    Mesh.GetUVs(1, SouseUV);
                    Mesh.GetUVs(0, TargetUV);
                    var TargetTexScliUV = TransMapper.UVtoTexScale(TargetUV, AtlasTextureSize);
                    SubMaps.Add(clientSelect.InTransMapGenerat(AtlasMapDataI, triangles, TargetTexScliUV, SouseUV, padingType, TransMapperCS));
                }
                Maps.Add(SubMaps);
            }
            return Maps;
        }
    }

    public class CompileData
    {
        public List<PropAndSouseTexuters> SouseTextures = new List<PropAndSouseTexuters>();
        public List<Mesh> meshes = new List<Mesh>();
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


        public void SetPropatyAndTexs(List<Renderer> renderers, List<IShaderSupport> sappotedListInstans)
        {
            int MeshCount = -1;
            foreach (var Rendera in renderers)
            {
                MeshCount += 1;
                int SubMeshCount = -1;
                foreach (var mat in Rendera.sharedMaterials)
                {
                    SubMeshCount += 1;
                    var MeshIndex = new MeshIndex(MeshCount, SubMeshCount);
                    var SupportShederI = sappotedListInstans.Find(i =>
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