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
using UnityEngine.Rendering;

namespace Rs.TexturAtlasCompiler
{
    public static class Compiler
    {
        public static void AtlasSetCompile(AtlasSet Target, ExecuteClient ClientSelect = ExecuteClient.CPU, bool ForesdCompile = false,
        string AtlasMapperPath = "Packages/rs64.textur-atlas-compiler/Runtime/ComputeShaders/AtlasMapper.compute"
        )
        {
            var Data = GetCompileData(Target);
            if (Target.Contenar == null) { Target.Contenar = CompileDataContenar.CreateCompileDataContenar("Assets/AutoGenerateContenar" + Guid.NewGuid().ToString() + ".asset"); }
            if (!ForesdCompile && Data.Hash == Target.Contenar.Hash) return;
            var Contenar = Target.Contenar;

            var IslandPool = CliantSelectToGenereatIslandPool(ClientSelect, Data);

            var UVs = Data.GetUVs();
            var MovedUVs = GenereatNewUVs(Target.SortingType, ClientSelect, IslandPool, UVs);

            var NotMevedUVs = Data.GetUVs();
            Data.SetUVs(MovedUVs, 0);
            Data.SetUVs(NotMevedUVs, 1);

            var AtlasMapDatas = GeneratAtlasMaps(Data.meshes, ClientSelect, AtlasMapperPath, Data.Pading, Data.AtlasTextureSize, Data.PadingType);

            var TargetPorpAndAtlasTexs = Data.GeneretTargetEmptyTextures();

            AtlasTextureCompile(Data.SouseTextures, AtlasMapDatas, TargetPorpAndAtlasTexs);

            Contenar.PutData(Data, TargetPorpAndAtlasTexs);

            Target.AtlasCompilePostCallBack.Invoke(Contenar);
        }

        public static void PutData(this CompileDataContenar Contenar, CompileData Data, List<PropAndAtlasTex> TargetPorpAndAtlasTexs)
        {
            Contenar.Hash = Data.Hash;
            Contenar.SetSubAsset<Mesh>(Data.meshes);
            Contenar.Meshs = Data.meshes;
            Contenar.DeletTexture();
            Contenar.SetTextures(TargetPorpAndAtlasTexs.ConvertAll<PropAndTexture>(i => (PropAndTexture)i));
        }

        public static IslandPool CliantSelectToGenereatIslandPool(ExecuteClient ClientSelect, CompileData Data)
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

        public static void AtlasTextureCompile(List<PropAndSouseTexuters> SouseList, List<List<AtlasMapData>> AtlasMapDatas, List<PropAndAtlasTex> TargetPorpAndAtlasTexs)
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
                        AtlasTextureCompileUsedUnityGetPixsel(SouseTxture, AtlasMapData, TargetTex);
                    }

                }
            }
        }

        public static List<List<Vector2>> GenereatNewUVs(IslandSortingType SortingType, ExecuteClient ClientSelect, IslandPool IslandPool, List<List<Vector2>> UVs)
        {
            List<List<Vector2>> MovedUVs;
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

        public static List<List<AtlasMapData>> GeneratAtlasMaps(List<Mesh> TargetMeshs, ExecuteClient clientSelect, string computeShaderPath, float Pading, Vector2Int AtlasTextureSize, PadingType padingType)
        {
            List<List<AtlasMapData>> Maps = new List<List<AtlasMapData>>();
            foreach (var Mesh in TargetMeshs)
            {
                List<AtlasMapData> SubMaps = new List<AtlasMapData>();
                foreach (var SubMeshIndex in Enumerable.Range(0, Mesh.subMeshCount))
                {
                    var AtlasMapDataI = new AtlasMapData(Pading, AtlasTextureSize);
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
        public static CompileData GetCompileData(AtlasSet Target)
        {

            CompileData Data = new CompileData();

            List<Renderer> renderers = new List<Renderer>(Target.AtlasTargetMeshs);
            renderers.AddRange(Target.AtlasTargetStaticMeshs);


            SetPropatyAndTexs(Data, renderers, ShaderSupportUtil.GetSupprotInstans());

            var Meshs = Target.AtlasTargetMeshs.ConvertAll<Mesh>(i => i.sharedMesh);
            Meshs.AddRange(Target.AtlasTargetStaticMeshs.ConvertAll<Mesh>(i => i.GetComponent<MeshFilter>().sharedMesh));
            Data.meshes = Meshs.ConvertAll<Mesh>(i => UnityEngine.Object.Instantiate<Mesh>(i));
            Data.AtlasTextureSize = Target.AtlasTextureSize;
            Data.Pading = Target.Pading;
            Data.PadingType = Target.PadingType;
            Data.CreateHash();
            return Data;
        }

        public static AtlasTexture AtlasTextureCompileUsedUnityGetPixsel(Texture2D SouseTex, AtlasMapData AtralsMap, AtlasTexture targetTex)
        {
            if (targetTex.Texture2D.width != AtralsMap.MapSize.x && targetTex.Texture2D.height != AtralsMap.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            var List = Utils.Reange2d(new Vector2Int(targetTex.Texture2D.width, targetTex.Texture2D.height));

            //Debug.Log(SouseTexPath);
            NotFIlterTexture2D(ref SouseTex);
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

        public static AtlasTexture AtlasTextureCompileUsedComputeSheder(Texture2D SouseTex, AtlasMapData AtralsMap, AtlasTexture targetTex, ComputeShader CS)
        {
            if (targetTex.Texture2D.width != AtralsMap.MapSize.x && targetTex.Texture2D.height != AtralsMap.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            NotFIlterTexture2D(ref SouseTex, true);
            Vector2Int ThredGropSize = AtralsMap.MapSize / 32;
            var KernelIndex = CS.FindKernel("AtlasCompile");

            CS.SetTexture(KernelIndex, "Source", SouseTex);

            int BufferSize = AtralsMap.MapSize.x * AtralsMap.MapSize.y;
            var AtlasMapBuffer = new ComputeBuffer(BufferSize, 12);
            var AtlasMapList = new List<Vector3>();
            foreach (var Index in Utils.Reange2d(AtralsMap.MapSize))
            {
                var Map = AtralsMap.Map[Index.x, Index.y];
                var Distans = AtralsMap.DistansMap[Index.x, Index.y];
                AtlasMapList.Add(new Vector3(Map.x, Map.y, Distans));

            }
            AtlasMapBuffer.SetData<Vector3>(AtlasMapList);
            CS.SetBuffer(KernelIndex, "AtlasMap", AtlasMapBuffer);

            var TargetBuffer = new ComputeBuffer(BufferSize, 16);
            var TargetTexColorArry = targetTex.Texture2D.GetPixels();
            TargetBuffer.SetData(TargetTexColorArry);
            CS.SetBuffer(KernelIndex, "Target", TargetBuffer);

            /*
            var TargetRT = new RenderTexture(targetTex.Texture2D.width, targetTex.Texture2D.height, 8, targetTex.Texture2D.graphicsFormat);
            TargetRT.enableRandomWrite = true;
            CS.SetTexture(KernelIndex, "Target", TargetRT);
            */

            var TargetDistansBuffer = new ComputeBuffer(BufferSize, 4);
            var TargetDistansList = new List<float>();
            foreach (var Index in Utils.Reange2d(AtralsMap.MapSize))
            {
                TargetDistansList.Add(targetTex.DistansMap[Index.x, Index.y]);
            }
            TargetDistansBuffer.SetData<float>(TargetDistansList);
            CS.SetBuffer(KernelIndex, "TargetDistansMap", TargetDistansBuffer);
            CS.SetInt("Size", AtralsMap.MapSize.x);


            CS.Dispatch(KernelIndex, ThredGropSize.x, ThredGropSize.y, 1);

            TargetBuffer.GetData(TargetTexColorArry);
            targetTex.Texture2D.SetPixels(TargetTexColorArry);
            targetTex.Texture2D.Apply();
            //RTから計算結果を引き戻すのはどれもなぜかうまくいかなかった...どうして？
            //ReadBackは InvalidOperationException をはいてうまくいかないし WaitForCompletionを実行した後でも...
            //ReadPixelsは真っ黒になる
            /*
            var ReadBackReq = AsyncGPUReadback.Request(TargetRT);
            ReadBackReq.WaitForCompletion();
            var buffer = ReadBackReq.GetData<Color>();
            targetTex.Texture2D.SetPixelData(buffer, 0);
            */
            /*
            var BackUP = RenderTexture.active;
            RenderTexture.active = TargetRT;
            targetTex.Texture2D.ReadPixels(new Rect(0, 0, TargetRT.width, TargetRT.height), 0, 0);
            RenderTexture.active = BackUP;
*/
            targetTex.Texture2D.Apply();

            var TargetDistansArry = TargetDistansList.ToArray();
            TargetDistansBuffer.GetData(TargetDistansArry);
            foreach (var Index in Utils.Reange2d(AtralsMap.MapSize))
            {
                targetTex.DistansMap[Index.x, Index.y] = TargetDistansArry[Utils.TwoDToOneDIndex(Index, AtralsMap.MapSize.x)];
                //Debug.Log(TargetDistansArry[Utils.TwoDToOneDIndex(Index, AtralsMap.MapSize.x)]);
            }
            AtlasMapBuffer.Release();
            TargetDistansBuffer.Release();
            TargetBuffer.Release();
            return targetTex;
        }

        public static void NotFIlterTexture2D(ref Texture2D SouseTex, bool ConvertToLiner = false)
        {
            var SouseTexPath = AssetDatabase.GetAssetPath(SouseTex);
            if (ConvertToLiner)
            {
                SouseTex = new Texture2D(2, 2, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
            }
            else
            {
                SouseTex = new Texture2D(2, 2);
            }
            SouseTex.LoadImage(File.ReadAllBytes(SouseTexPath));
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
                    var SupportShederI = sappotedListInstans.Find(i =>
                    {
                        //Debug.Log(i.SupprotShaderName + " / " + mat.shader.name + " " + mat.shader.name.Contains(i.SupprotShaderName));
                        return mat.shader.name.Contains(i.SupprotShaderName);
                    });

                    if (SupportShederI != null)
                    {
                        var textures = SupportShederI.GetPropertyAndTextures(mat);
                        foreach (var Texture in textures)
                        {
                            if (Texture.Texture2D != null)
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
                            PropAndTexture DefoultTexAndprop = new PropAndTexture(PropertyName, texture2D);
                            Data.AddTexture(DefoultTexAndprop, MeshIndex);
                        }

                    }

                }
            }
        }

    }
    public class CompileData
    {
        public List<PropAndSouseTexuters> SouseTextures = new List<PropAndSouseTexuters>();
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
            foreach (var texp in SouseTextures)
            {
                foreach (var tex in texp.Texture2Ds)
                {
                    Bytes.Concat<byte>(File.ReadAllBytes(AssetDatabase.GetAssetPath(tex)));
                }
            }
            var Md5Instans = MD5CryptoServiceProvider.Create();
            var Hascode = Md5Instans.ComputeHash(Bytes);
            Hash = BitConverter.ToString(Hascode);
        }

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
                TargetPorpAndAtlasTexs.Add(new PropAndAtlasTex(new AtlasTexture(new Texture2D(this.AtlasTextureSize.x, this.AtlasTextureSize.y), this.Pading), textures.PropertyName));
            }
            return TargetPorpAndAtlasTexs;
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
    public class PropAndTexture
    {
        public string PropertyName;
        public Texture2D Texture2D;

        public PropAndTexture(string propertyName, Texture2D textures)
        {
            PropertyName = propertyName;
            Texture2D = textures;
        }
    }

    [Serializable]
    public class PropAndAtlasTex
    {
        public string PropertyName;
        public AtlasTexture AtlasTexture;

        public PropAndAtlasTex(AtlasTexture texture2D, string propertyName)
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