#if UNITY_EDITOR
using System.Reflection;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Security;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vector2 = UnityEngine.Vector2;
using Rs.TexturAtlasCompiler.ShaderSupport;

namespace Rs.TexturAtlasCompiler
{
    public static class Compiler
    {
        public static void AtlasSetCompile(AtlasSet Target, ExecuteClient ClientSelect = ExecuteClient.CPU, bool ForesdCompile = false, string ComputeShaderPath = "Assets/Rs/TexturAtlasCompiler/AtlasMapper.compute")
        {
            var Data = GetCompileData(Target);
            if (Target.Contenar == null) { Target.Contenar = CompileDataContenar.CreateCompileDataContenar("Assets/AutoGenerateContenar" + Guid.NewGuid().ToString() + ".asset"); }
            if (!ForesdCompile && Data.Hash == Target.Contenar.Hash) return;


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
            var UVs = Data.GetUVs();
            IslandPool MovedPool;
            switch (Target.SortingType)
            {
                case IslandSortingType.EvenlySpaced:
                    {
                        MovedPool = IslandUtils.IslandPoolEvenlySpaced(IslandPool);
                        break;
                    }
                case IslandSortingType.DescendingOrder:
                    {
                        throw new NotImplementedException();
                        //MovedPool = IslandUtils.IslandPoolEvenlySpaced(IslandPool);
                        //break;
                    }
                default: throw new ArgumentException();
            }

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

            var NotMevedUVs = Data.GetUVs();
            Data.SetUVs(MovedUVs);
            Data.SetUVs(NotMevedUVs, 1);

            var AtlasMapDatas = GeneratAtlasMaps(Data.meshes, ClientSelect, ComputeShaderPath, Data.Pading, Data.AtlasTextureSize, Data.PadingType);

            //var TargetTextureAndDistansMap = new TextureAndDistansMap(TargetTexture, Data.Pading);
            var TargetTexAndDistansMaps = new List<PropatyAndTextureAndDistans>();
            foreach (var textures in Data.Textures)
            {
                TargetTexAndDistansMaps.Add(new PropatyAndTextureAndDistans(new TextureAndDistansMap(new Texture2D(Data.AtlasTextureSize.x, Data.AtlasTextureSize.y), Data.Pading), textures.PropertyName));
            }


            foreach (var PorpAndTex in Data.Textures)
            {
                int TexIndex = -1;
                string PropertyName = PorpAndTex.PropertyName;
                foreach (var SouseTxture in PorpAndTex.Textures)
                {
                    TexIndex += 1;
                    foreach (var meshIndex in PorpAndTex.MeshIndex[TexIndex])
                    {
                        var AtlasMapData = AtlasMapDatas[meshIndex.Index][meshIndex.SubMeshIndex];
                        var TargetTex = TargetTexAndDistansMaps.Find(i => i.PropertyName == PropertyName).TextureAndDistansMap;
                        switch (ClientSelect)
                        {
                            case ExecuteClient.CPU:
                                {
                                    AtlasTextureCompile(SouseTxture, AtlasMapData, TargetTex);
                                    break;
                                }
                            case ExecuteClient.AsyncCPU:
                                {//今はそれ用のコードがないのでCPU
                                    AtlasTextureCompile(SouseTxture, AtlasMapData, TargetTex);
                                    break;
                                }
                            case ExecuteClient.ComputeSheder:
                                {//今はそれ用のコードがないのでCPU
                                    AtlasTextureCompile(SouseTxture, AtlasMapData, TargetTex);
                                    break;
                                }
                        }
                    }

                }
            }

            Target.Contenar.Hash = Data.Hash;
            Target.Contenar.SetMesh(Data);
            Target.Contenar.DeletTexture();
            foreach (var TexAndDist in TargetTexAndDistansMaps)
            {
                Target.Contenar.SetTexture(TexAndDist);
            }
            var mats = GetMaterials(Target);
            Target.Contenar.SetMaterial(mats.ConvertAll<Material>(i => UnityEngine.Object.Instantiate<Material>(i)));






        }
        public static List<List<AtlasMapData>> GeneratAtlasMaps(List<Mesh> TargetMeshs, ExecuteClient clientSelect, string computeShaderPath, float Pading, Vector2Int AtlasTextureSize, PadingType padingType)
        {
            List<List<AtlasMapData>> Maps = new List<List<AtlasMapData>>();
            foreach (var Mesh in TargetMeshs)
            {
                List<AtlasMapData> SubMaps = new List<AtlasMapData>();
                foreach (var SubMeshIndex in Enumerable.Range(0, Mesh.subMeshCount))
                {
                    var AtlasMapDataI = AtlasMapData.CreateAtlasMapData(Pading, AtlasTextureSize);
                    var triangles = AtlasMapper.ToList(Mesh.GetTriangles(SubMeshIndex));
                    var SouseUV = new List<Vector2>();
                    var TargetUV = new List<Vector2>();
                    Mesh.GetUVs(1, SouseUV);
                    Mesh.GetUVs(0, TargetUV);
                    var TargetTexScliUV = AtlasMapper.UVtoTexScale(TargetUV, AtlasTextureSize);
                    switch (clientSelect)
                    {
                        case ExecuteClient.CPU:
                            {
                                SubMaps.Add(AtlasMapper.AtlasMapGenerat(AtlasMapDataI, triangles, TargetTexScliUV, SouseUV, padingType));
                                break;
                            }
                        case ExecuteClient.AsyncCPU:
                            {
                                //throw new NotImplementedException();
                                SubMaps.Add(AtlasMapper.AtlasMapGeneratAsync(AtlasMapDataI, triangles, TargetTexScliUV, SouseUV, padingType).Result);
                                break;
                            }
                        case ExecuteClient.ComputeSheder:
                            {
                                var CS = AssetDatabase.LoadAssetAtPath<ComputeShader>(computeShaderPath);
                                SubMaps.Add(AtlasMapper.UVMappingTableGeneratorComputeShederUsed(CS, AtlasMapDataI, triangles, TargetTexScliUV, SouseUV, padingType));
                                break;
                            }
                    }
                }
                Maps.Add(SubMaps);
            }
            return Maps;
        }

        public static List<Material> GetMaterials(AtlasSet Target)
        {
            List<Renderer> renderers = new List<Renderer>(Target.AtlasTargetMeshs);
            renderers.AddRange(Target.AtlasTargetStaticMeshs);
            List<Material> Mats = new List<Material>();
            foreach (var Rendera in renderers)
            {
                foreach (var mat in Rendera.sharedMaterials)
                {
                    if (mat.mainTexture != null)
                    {
                        Mats.Add(mat);
                    }
                }
            }

            return Mats;
        }
        public static CompileData GetCompileData(AtlasSet Target)
        {

            CompileData Data = new CompileData();

            List<Renderer> renderers = new List<Renderer>(Target.AtlasTargetMeshs);
            renderers.AddRange(Target.AtlasTargetStaticMeshs);

            var SappotedLists = Assembly.GetExecutingAssembly().GetTypes().Where(C => C.GetInterfaces().Any(I => I == typeof(IShaderSupport))).ToList();
            List<IShaderSupport> SappotedListInstans = SappotedLists.ConvertAll<IShaderSupport>(I => Activator.CreateInstance(I) as IShaderSupport);

            SetPropatyAndTexs(Data, renderers, SappotedListInstans);

            var Meshs = Target.AtlasTargetMeshs.ConvertAll<Mesh>(i => i.sharedMesh);
            Meshs.AddRange(Target.AtlasTargetStaticMeshs.ConvertAll<Mesh>(i => i.GetComponent<MeshFilter>().sharedMesh));
            Data.meshes = Meshs.ConvertAll<Mesh>(i => UnityEngine.Object.Instantiate<Mesh>(i));
            Data.AtlasTextureSize = Target.AtlasTextureSize;
            Data.Pading = Target.Pading;
            Data.PadingType = Target.PadingType;
            Data.CreateHash();
            return Data;
        }




        public static TextureAndDistansMap AtlasTextureCompile(Texture2D SouseTex, AtlasMapData AtralsMap, TextureAndDistansMap targetTex)
        {
            if (targetTex.Texture2D.width != AtralsMap.MapSize.x && targetTex.Texture2D.height != AtralsMap.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            var List = Utils.Reange2d(new Vector2Int(targetTex.Texture2D.width, targetTex.Texture2D.height));
            foreach (var index in List)
            {
                //Debug.Log(AtralsMap.DistansMap[index.x, index.y] + " " + AtralsMap.DefaultPading + " " + targetTex.DistansMap[index.x, index.y]);
                if (AtralsMap.DistansMap[index.x, index.y] > AtralsMap.DefaultPading && AtralsMap.DistansMap[index.x, index.y] > targetTex.DistansMap[index.x, index.y])
                {
                    var souspixselcloro = SouseTex.GetPixelBilinear(AtralsMap.Map[index.x, index.y].x, AtralsMap.Map[index.x, index.y].y);
                    //Debug.Log(AtralsMap[index.x, index.y].Value.ToString() + "/" + new Vector2(AtralsMap[index.x, index.y].Value.x * SouseTex.width, AtralsMap[index.x, index.y].Value.y * SouseTex.height).ToString() + "/" + souspixselcloro.ToString());
                    targetTex.Texture2D.SetPixel(index.x, index.y, souspixselcloro);
                    //                    Debug.Log("nya");
                }
            }
            return targetTex;
        }




        static void SetPropatyAndTexs(CompileData Data, List<Renderer> renderers, List<IShaderSupport> sappotedListInstans)
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
                    var SupportShederI = sappotedListInstans.Find(i => mat.shader.name.Contains(i.SupprotShaderName));

                    if (SupportShederI != null)
                    {
                        var textures = SupportShederI.GetPropertyAndTextures(mat);
                        foreach (var Texture in textures)
                        {
                            if (Texture.Texture != null)
                            {
                                Data.AddTexture(Texture, MeshIndex);
                            }
                        }
                    }
                    else
                    {
                        var PropertyName = "_MainTex";
                        if (mat.GetTexture(PropertyName) is Texture2D texture2D)
                        {
                            PropertyAndTextures DefoultTexAndprop = new PropertyAndTextures(PropertyName, texture2D);
                            Data.AddTexture(DefoultTexAndprop, MeshIndex);
                        }

                    }

                }
            }
        }




    }
    public class CompileData
    {
        public List<TexturesAndMeshIndex> Textures = new List<TexturesAndMeshIndex>();
        public List<Mesh> meshes = new List<Mesh>();
        public Vector2Int AtlasTextureSize;
        public float Pading;
        public PadingType PadingType;
        public string Hash;
        public void CreateHash()
        {
            byte[] Bytes = new byte[0];
            Bytes.Concat<byte>(BitConverter.GetBytes(Pading));
            Bytes.Concat<byte>(BitConverter.GetBytes((int)PadingType));
            Bytes.Concat<byte>(BitConverter.GetBytes(AtlasTextureSize.x));
            Bytes.Concat<byte>(BitConverter.GetBytes(AtlasTextureSize.y));
            foreach (var texp in Textures)
            {
                foreach (var tex in texp.Textures)
                {
                    Bytes.Concat<byte>(File.ReadAllBytes(AssetDatabase.GetAssetPath(tex)));
                }
            }
            var Md5Instans = MD5CryptoServiceProvider.Create();
            var Hascode = Md5Instans.ComputeHash(Bytes);
            Hash = BitConverter.ToString(Hascode);
        }

        public void AddTexture(PropertyAndTextures AddPropAndtex, MeshIndex addIndex)
        {
            var Texture = Textures.Find(i => i.PropertyName == AddPropAndtex.PropertyName);
            if (Texture != null)
            {
                if (!Texture.Textures.Any(i => i == AddPropAndtex.Texture))
                {
                    Texture.Textures.Add(AddPropAndtex.Texture);
                    Texture.MeshIndex.Add(new List<MeshIndex>() { addIndex });
                }
                else
                {
                    var TexIndex = Texture.Textures.IndexOf(AddPropAndtex.Texture);
                    Texture.MeshIndex[TexIndex].Add(addIndex);
                }

            }
            else
            {
                Textures.Add(new TexturesAndMeshIndex(AddPropAndtex, new List<List<MeshIndex>>() { new List<MeshIndex>() { addIndex } }));
            }
        }
    }

    public enum IslandSortingType
    {
        EvenlySpaced,
        DescendingOrder,
    }
    [Serializable]
    public class PropertyAndTextures
    {
        public string PropertyName;
        public Texture2D Texture;

        public PropertyAndTextures(string propertyName, Texture2D textures)
        {
            PropertyName = propertyName;
            Texture = textures;
        }
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

    public class TexturesAndMeshIndex
    {
        public string PropertyName;
        public List<Texture2D> Textures;
        public List<List<MeshIndex>> MeshIndex;

        public TexturesAndMeshIndex(string propertyName, List<Texture2D> textures, List<List<MeshIndex>> meshIndex)
        {
            PropertyName = propertyName;
            Textures = textures;
            MeshIndex = meshIndex;
        }
        public TexturesAndMeshIndex(PropertyAndTextures textures, List<List<MeshIndex>> meshIndex)
        {
            PropertyName = textures.PropertyName;
            Textures = new List<Texture2D>() { textures.Texture };
            MeshIndex = meshIndex;
        }

    }
    [Serializable]
    public class PropatyAndTextureAndDistans
    {
        public TextureAndDistansMap TextureAndDistansMap;
        public string PropertyName;

        public PropatyAndTextureAndDistans(TextureAndDistansMap texture2D, string propertyName)
        {
            TextureAndDistansMap = texture2D;
            PropertyName = propertyName;
        }
    }



}



#endif