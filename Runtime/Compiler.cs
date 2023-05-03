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
using Rs64.TexTransTool.ShaderSupport;
using UnityEngine.Rendering;

namespace Rs64.TexTransTool
{
    public static class Compiler
    {


        public static TransTargetTexture TransCompileUseGetPixsel(Texture2D SouseTex, TransMapData AtralsMap, TransTargetTexture targetTex)
        {
            if (targetTex.Texture2D.width != AtralsMap.MapSize.x && targetTex.Texture2D.height != AtralsMap.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            var List = Utils.Reange2d(new Vector2Int(targetTex.Texture2D.width, targetTex.Texture2D.height));

            NotFIlterAndReadWritTexture2D(ref SouseTex);
            foreach (var index in List)
            {
                if (AtralsMap.DistansMap[index.x, index.y] > AtralsMap.DefaultPading && AtralsMap.DistansMap[index.x, index.y] > targetTex.DistansMap[index.x, index.y])
                {
                    var souspixselcloro = SouseTex.GetPixelBilinear(AtralsMap.Map[index.x, index.y].x, AtralsMap.Map[index.x, index.y].y);
                    targetTex.Texture2D.SetPixel(index.x, index.y, souspixselcloro);
                }
            }
            return targetTex;
        }

        public static TransTargetTexture TransCompileUseComputeSheder(Texture2D SouseTex, TransMapData AtralsMap, TransTargetTexture targetTex, ComputeShader CS)
        {
            if (targetTex.Texture2D.width != AtralsMap.MapSize.x && targetTex.Texture2D.height != AtralsMap.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            NotFIlterAndReadWritTexture2D(ref SouseTex, true);
            Vector2Int ThredGropSize = AtralsMap.MapSize / 32;
            var KernelIndex = CS.FindKernel("TransCompile");

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

            var TargetDistansArry = TargetDistansList.ToArray();
            TargetDistansBuffer.GetData(TargetDistansArry);
            foreach (var Index in Utils.Reange2d(AtralsMap.MapSize))
            {
                targetTex.DistansMap[Index.x, Index.y] = TargetDistansArry[Utils.TwoDToOneDIndex(Index, AtralsMap.MapSize.x)];
            }
            AtlasMapBuffer.Release();
            TargetDistansBuffer.Release();
            TargetBuffer.Release();
            return targetTex;
        }

        public static void NotFIlterAndReadWritTexture2D(ref Texture2D SouseTex, bool ConvertToLiner = false)
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


}

#endif