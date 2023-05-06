#if UNITY_EDITOR
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vector2 = UnityEngine.Vector2;
using System.Runtime.CompilerServices;

namespace Rs64.TexTransTool
{
    public enum TexWrapMode
    {
        NotWrap,
        Stretch,
        Loop,
    }
    public static class Compiler
    {

        [Obsolete]
        public static TransTargetTexture TransCompileUseGetPixsel(Texture2D SouseTex, TransMapData AtralsMap, TransTargetTexture targetTex, TexWrapMode wrapMode)
        {
            if (targetTex.Texture2D.width != AtralsMap.MapSize.x && targetTex.Texture2D.height != AtralsMap.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            var List = Utils.Reange2d(new Vector2Int(targetTex.Texture2D.width, targetTex.Texture2D.height));

            NotFIlterAndReadWritTexture2D(ref SouseTex);
            foreach (var index in List)
            {
                if (AtralsMap.DistansMap[index.x, index.y] > AtralsMap.DefaultPading && AtralsMap.DistansMap[index.x, index.y] > targetTex.DistansMap[index.x, index.y])
                {
                    Vector2 SouseTexPos = AtralsMap.Map[index.x, index.y];
                    Vector2? TWMAppryTexPos = null;
                    switch (wrapMode)
                    {
                        default:
                        case TexWrapMode.NotWrap:
                            {
                                if (!(0 < SouseTexPos.x && SouseTexPos.x < 1) || !(0 < SouseTexPos.y && SouseTexPos.y < 1)) TWMAppryTexPos = null;
                                else TWMAppryTexPos = SouseTexPos;
                                break;
                            }
                        case TexWrapMode.Stretch:
                            {
                                SouseTexPos.x = Mathf.Clamp01(SouseTexPos.x);
                                SouseTexPos.y = Mathf.Clamp01(SouseTexPos.y);
                                TWMAppryTexPos = SouseTexPos;
                                break;
                            }
                        case TexWrapMode.Loop:
                            {
                                TWMAppryTexPos = SouseTexPos;
                                break;
                            }
                    }
                    SetPixsl(SouseTex, targetTex, index, TWMAppryTexPos);
                    targetTex.DistansMap[index.x, index.y] = AtralsMap.DistansMap[index.x, index.y];
                }
            }
            return targetTex;

        }
        [Obsolete]
        static void SetPixsl(Texture2D SouseTex, TransTargetTexture targetTex, Vector2Int index, Vector2? SouseTexPos)
        {
            if (!SouseTexPos.HasValue) return;
            var souspixselcloro = SouseTex.GetPixelBilinear(SouseTexPos.Value.x, SouseTexPos.Value.y);
            targetTex.Texture2D.SetPixel(index.x, index.y, souspixselcloro);
        }
        public static async Task<TransTargetTexture> TransCompileAsync(Texture2D SouseTex, TransMapData AtralsMap, TransTargetTexture targetTex, TexWrapMode wrapMode)
        {
            if (targetTex.Texture2D.width != AtralsMap.MapSize.x && targetTex.Texture2D.height != AtralsMap.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            var TexSize = new Vector2Int(targetTex.Texture2D.width, targetTex.Texture2D.height);
            var sTexSize = new Vector2Int(SouseTex.width, SouseTex.height);
            var List = Utils.Reange2d(TexSize);

            NotFIlterAndReadWritTexture2D(ref SouseTex);
            var SColors = Utils.OneDToTowD(SouseTex.GetPixels(), sTexSize);
            var TColors = Utils.OneDToTowD(targetTex.Texture2D.GetPixels(), TexSize);

            ConfiguredTaskAwaitable[,] Tasks = new ConfiguredTaskAwaitable[TexSize.x, TexSize.y];

            foreach (var index in List)
            {
                Tasks[index.x, index.y] = Task.Run(() => TransCompilePixsl(AtralsMap, targetTex, wrapMode, SColors, TColors, index,sTexSize)).ConfigureAwait(false);
            }
            foreach (var task in Tasks)
            {
                await task;
            }

            var TOneDColors = Utils.TowDtoOneD(TColors, TexSize);
            targetTex.Texture2D.SetPixels(TOneDColors);

            return targetTex;
        }



        static void TransCompilePixsl(TransMapData AtralsMap, TransTargetTexture targetTex, TexWrapMode wrapMode, Color[,] SColors, Color[,] TColors, Vector2Int index,Vector2Int stexSize)
        {
            if (AtralsMap.DistansMap[index.x, index.y] > AtralsMap.DefaultPading && AtralsMap.DistansMap[index.x, index.y] > targetTex.DistansMap[index.x, index.y])
            {
                Vector2 SouseTexPos = AtralsMap.Map[index.x, index.y];
                Vector2? TWMAppryTexPos = null;
                switch (wrapMode)
                {
                    default:
                    case TexWrapMode.NotWrap:
                        {
                            if (!(0 < SouseTexPos.x && SouseTexPos.x < 1) || !(0 < SouseTexPos.y && SouseTexPos.y < 1)) TWMAppryTexPos = null;
                            else TWMAppryTexPos = SouseTexPos;
                            break;
                        }
                    case TexWrapMode.Stretch:
                        {
                            SouseTexPos.x = Mathf.Clamp01(SouseTexPos.x);
                            SouseTexPos.y = Mathf.Clamp01(SouseTexPos.y);
                            TWMAppryTexPos = SouseTexPos;
                            break;
                        }
                    case TexWrapMode.Loop:
                        {
                            SouseTexPos.x %= 1.0f;
                            SouseTexPos.y %= 1.0f;
                            TWMAppryTexPos = SouseTexPos;
                            break;
                        }
                }
                SetPixsl(SColors, TColors, index, TWMAppryTexPos, stexSize);
                targetTex.DistansMap[index.x, index.y] = AtralsMap.DistansMap[index.x, index.y];
            }
        }
        static void SetPixsl(Color[,] SouseTexColors, Color[,] targetTexColors, Vector2Int index, Vector2? SouseTexPos, Vector2Int TexSize)
        {
            if (!SouseTexPos.HasValue) return;
            var souspixselcloro = GetColorBiliner(SouseTexColors, TexSize, SouseTexPos.Value);
            targetTexColors[index.x, index.y] = souspixselcloro;
        }

        public static Color GetColorBiliner(Color[,] Colors, Vector2Int sTexSize, Vector2 Pos)
        {
            sTexSize -= Vector2Int.one;
            Pos *= sTexSize;
            var XC = Mathf.CeilToInt(Pos.x);
            var XF = Mathf.FloorToInt(Pos.x);
            var YC = Mathf.CeilToInt(Pos.y);
            var YF = Mathf.FloorToInt(Pos.y);

            var UpColor = Color.Lerp(Colors[XF, YC], Colors[XC, YC], Pos.x - XF);
            var DownColor = Color.Lerp(Colors[XF, YF], Colors[XC, YF], Pos.x -XF);
            return Color.Lerp(DownColor, UpColor, Pos.y -YF);
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
            if (string.IsNullOrEmpty(SouseTexPath)) throw new ArgumentException("元となる画像のパスが存在しません。");
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