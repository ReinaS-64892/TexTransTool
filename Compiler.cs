#if UNITY_EDITOR
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

namespace Rs.TexturAtlasCompiler
{
    public static class Compiler
    {
        public static void AtlasSetCompile(AtlasSet Target, ExecuteClient ClientSelect = ExecuteClient.CPU, bool ForesdCompile = false, string ComputeShaderPath = "Assets/Rs/TexturAtlasCompiler/AtlasMapper.compute")
        {
            var Data = GetCompileData(Target);
            if (Target.Contenar == null) { Target.Contenar = CompileDataContenar.CreateCompileDataContenar("Assets/AutoGenerateContenar" + Guid.NewGuid().ToString() + ".asset"); }
            if (!ForesdCompile && Data.Hash == Target.Contenar.Hash) return;
            Target.Contenar.Hash = Data.Hash;
            Target.Contenar.SetMeshEditableClone(Data);
            Target.Contenar.DeletTexture();
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

            var TargetTexture = new Texture2D(Data.AtlasTextureSize.x, Data.AtlasTextureSize.y);
            var TargetTextureAndDistansMap = new TextureAndDistansMap(TargetTexture, Data.Pading);

            foreach (var Texture in Data.Textures)
            {
                var SouseTxture = Texture.SouseTex;
                var AtlasMapDataI = AtlasMapData.CreateAtlasMapData(Data.Pading, Data.AtlasTextureSize);
                foreach (var Indexs in Texture.Indexs)
                {
                    var TargetMesh = Data.meshes[Indexs.Item1]; //Debug.Log(Indexs);
                    var triangles = AtlasMapper.ToList(TargetMesh.GetTriangles(Indexs.Item2));
                    var SouseUV = new List<Vector2>();
                    var TargetUV = new List<Vector2>();
                    TargetMesh.GetUVs(1, SouseUV);
                    TargetMesh.GetUVs(0, TargetUV);
                    var TargetTexScliUV = AtlasMapper.UVtoTexScale(TargetUV, Data.AtlasTextureSize);
                    switch (ClientSelect)
                    {
                        case ExecuteClient.CPU:
                            {
                                AtlasMapDataI = AtlasMapper.AtlasMapGenerat(AtlasMapDataI, triangles, TargetTexScliUV, SouseUV, Data.PadingType);
                                break;
                            }
                        case ExecuteClient.AsyncCPU:
                            {
                                //throw new NotImplementedException();
                                AtlasMapDataI = AtlasMapper.AtlasMapGeneratAsync(AtlasMapDataI, triangles, TargetTexScliUV, SouseUV, Data.PadingType).Result;
                                break;
                            }
                        case ExecuteClient.ComputeSheder:
                            {
                                var CS = AssetDatabase.LoadAssetAtPath<ComputeShader>(ComputeShaderPath);
                                AtlasMapDataI = AtlasMapper.UVMappingTableGeneratorComputeShederUsed(CS, AtlasMapDataI, triangles, TargetTexScliUV, SouseUV, Data.PadingType);
                                break;
                            }
                    }
                }
                switch (ClientSelect)
                {
                    case ExecuteClient.CPU:
                        {
                            AtlasTextureCompile(SouseTxture, AtlasMapDataI, TargetTextureAndDistansMap);
                            break;
                        }
                    case ExecuteClient.AsyncCPU:
                        {//今はそれ用のコードがないのでCPU
                            AtlasTextureCompile(SouseTxture, AtlasMapDataI, TargetTextureAndDistansMap);
                            break;
                        }
                    case ExecuteClient.ComputeSheder:
                        {//今はそれ用のコードがないのでCPU
                            AtlasTextureCompile(SouseTxture, AtlasMapDataI, TargetTextureAndDistansMap);
                            break;
                        }
                }



            }


            Target.Contenar.SetTexture(TargetTextureAndDistansMap);
            var mats = GetMaterials(Target);
            Target.Contenar.SetMaterial(mats);






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
            int MeshIndex = -1;
            List<Renderer> renderers = new List<Renderer>(Target.AtlasTargetMeshs);
            renderers.AddRange(Target.AtlasTargetStaticMeshs);
            foreach (var Rendera in renderers)
            {
                MeshIndex += 1;
                int SubMeshCount = -1;
                foreach (var mat in Rendera.sharedMaterials)
                {
                    SubMeshCount += 1;
                    if (mat.mainTexture != null && mat.mainTexture is Texture2D texture2D)
                    {
                        if (!Data.Textures.Any(i => i.SouseTex == texture2D))
                        {
                            var texandmeshi = new TextureAndMeshIndex(texture2D);
                            texandmeshi.Indexs.Add((MeshIndex, SubMeshCount));
                            Data.Textures.Add(texandmeshi);
                        }
                        else
                        {
                            var Tex = Data.Textures.Find(i => i.SouseTex == texture2D);
                            Tex.Indexs.Add((MeshIndex, SubMeshCount));
                        }
                    }
                }
            }
            var Meshs = Target.AtlasTargetMeshs.ConvertAll<Mesh>(i => i.sharedMesh);
            Meshs.AddRange(Target.AtlasTargetStaticMeshs.ConvertAll<Mesh>(i => i.GetComponent<MeshFilter>().sharedMesh));
            Data.meshes = Meshs;
            Data.AtlasTextureSize = Target.AtlasTextureSize;
            Data.Pading = Target.Pading;
            Data.PadingType = Target.PadingType;
            Data.CreateHash();
            return Data;
        }


        public static TextureAndDistansMap AtlasTextureCompile(Texture2D SouseTex, AtlasMapData AtralsMap, TextureAndDistansMap targetTex)
        {
            if (targetTex.texture2D.width != AtralsMap.MapSize.x && targetTex.texture2D.height != AtralsMap.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            var List = Utils.Reange2d(new Vector2Int(targetTex.texture2D.width, targetTex.texture2D.height));
            foreach (var index in List)
            {
                //Debug.Log(AtralsMap.DistansMap[index.x, index.y] + " " + AtralsMap.DefaultPading + " " + targetTex.DistansMap[index.x, index.y]);
                if (AtralsMap.DistansMap[index.x, index.y] > AtralsMap.DefaultPading && AtralsMap.DistansMap[index.x, index.y] > targetTex.DistansMap[index.x, index.y])
                {
                    var souspixselcloro = SouseTex.GetPixelBilinear(AtralsMap.Map[index.x, index.y].x, AtralsMap.Map[index.x, index.y].y);
                    //Debug.Log(AtralsMap[index.x, index.y].Value.ToString() + "/" + new Vector2(AtralsMap[index.x, index.y].Value.x * SouseTex.width, AtralsMap[index.x, index.y].Value.y * SouseTex.height).ToString() + "/" + souspixselcloro.ToString());
                    targetTex.texture2D.SetPixel(index.x, index.y, souspixselcloro);
                    //                    Debug.Log("nya");
                }
            }
            return targetTex;
        }






    }
    public class CompileData
    {
        public List<TextureAndMeshIndex> Textures = new List<TextureAndMeshIndex>();
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
            foreach (var tex in Textures)
            {
                Bytes.Concat<byte>(tex.SouseTex.GetRawTextureData());
            }
            var Md5Instans = MD5CryptoServiceProvider.Create();
            var Hascode = Md5Instans.ComputeHash(Bytes);
            Hash = BitConverter.ToString(Hascode);
        }
    }
    public class TextureAndMeshIndex
    {
        public Texture2D SouseTex;
        public List<(int, int)> Indexs = new List<(int, int)>();//MeshIndex , SubMeshCount
        public TextureAndMeshIndex(Texture2D souseTex, List<(int, int)> indexs)
        {
            SouseTex = souseTex;
            Indexs = indexs;
        }
        public TextureAndMeshIndex(Texture2D souseTex)
        {
            SouseTex = souseTex;
        }
    }
    public enum IslandSortingType
    {
        EvenlySpaced,
        DescendingOrder,
    }



}



#endif