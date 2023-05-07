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
        public const string TransCompilerPath = "Packages/rs64.tex-trans-tool/Runtime/ComputeShaders/TransCompiler.compute";

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
                Tasks[index.x, index.y] = Task.Run(() => TransCompilePixsl(AtralsMap, targetTex, wrapMode, SColors, TColors, index, sTexSize)).ConfigureAwait(false);
            }
            foreach (var task in Tasks)
            {
                await task;
            }

            var TOneDColors = Utils.TowDtoOneD(TColors, TexSize);
            targetTex.Texture2D.SetPixels(TOneDColors);

            return targetTex;
        }



        static void TransCompilePixsl(TransMapData AtralsMap, TransTargetTexture targetTex, TexWrapMode wrapMode, Color[,] SColors, Color[,] TColors, Vector2Int index, Vector2Int stexSize)
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
            var DownColor = Color.Lerp(Colors[XF, YF], Colors[XC, YF], Pos.x - XF);
            return Color.Lerp(DownColor, UpColor, Pos.y - YF);
        }


        public static TransTargetTexture TransCompileUseComputeSheder(Texture2D SouseTex, TransMapData AtralsMap, TransTargetTexture targetTex, TexWrapMode wrapMode, ComputeShader CS)
        {
            if (targetTex.Texture2D.width != AtralsMap.MapSize.x && targetTex.Texture2D.height != AtralsMap.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            if (CS == null) CS = AssetDatabase.LoadAssetAtPath<ComputeShader>(TransCompilerPath);
            var TexSize = targetTex.Texture2D.NativeSize();
            var sTexSize = SouseTex.NativeSize();

            NotFIlterAndReadWritTexture2D(ref SouseTex);

            var SColors = SouseTex.GetPixels();
            var TColors = targetTex.Texture2D.GetPixels();

            Vector2Int ThredGropSize = AtralsMap.MapSize / 32;
            var KernelIndex = CS.FindKernel(wrapMode.ToString());


            var SouseTexCB = new ComputeBuffer(SColors.Length, 16);
            SouseTexCB.SetData(SColors);
            CS.SetBuffer(KernelIndex, "Source", SouseTexCB);


            CS.SetInts("SourceTexSize", new int[2] { sTexSize.x, sTexSize.y });
            Debug.Log(sTexSize + " " + SColors.Length);


            var AtlasMapBuffer = new ComputeBuffer(TColors.Length, 12);
            var AtlasMapList = new Vector3[AtralsMap.MapSize.x * AtralsMap.MapSize.y];
            foreach (var Index in Utils.Reange2d(AtralsMap.MapSize))
            {
                var Map = AtralsMap.Map[Index.x, Index.y];
                var Distans = AtralsMap.DistansMap[Index.x, Index.y];
                AtlasMapList[Utils.TwoDToOneDIndex(Index, AtralsMap.MapSize.x)] = new Vector3(Map.x, Map.y, Distans);
            }
            AtlasMapBuffer.SetData(AtlasMapList);
            CS.SetBuffer(KernelIndex, "AtlasMap", AtlasMapBuffer);


            var TargetBuffer = new ComputeBuffer(TColors.Length, 16);
            TargetBuffer.SetData(TColors);
            CS.SetBuffer(KernelIndex, "Target", TargetBuffer);


            var TargetDistansBuffer = new ComputeBuffer(TColors.Length, 4);
            var TargetDistansList = Utils.TowDtoOneD(targetTex.DistansMap, TexSize);
            TargetDistansBuffer.SetData(TargetDistansList);
            CS.SetBuffer(KernelIndex, "TargetDistansMap", TargetDistansBuffer);


            CS.SetInts("TargetTexSize", new int[2] { TexSize.x, TexSize.y });


            CS.Dispatch(KernelIndex, ThredGropSize.x, ThredGropSize.y, 1);

            TargetBuffer.GetData(TColors);
            targetTex.Texture2D.SetPixels(TColors);
            targetTex.Texture2D.Apply();

            TargetDistansBuffer.GetData(TargetDistansList);
            targetTex.DistansMap = Utils.OneDToTowD(TargetDistansList, TexSize);

            AtlasMapBuffer.Release();
            TargetDistansBuffer.Release();
            TargetBuffer.Release();
            SouseTexCB.Release();
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

        public static Vector2Int NativeSize(this Texture2D SouseTex)
        {
            var SouseTexPath = AssetDatabase.GetAssetPath(SouseTex);
            if (string.IsNullOrEmpty(SouseTexPath)) throw new ArgumentException("元となる画像のパスが存在しません。");
            var map = new System.Drawing.Bitmap(SouseTexPath);
            return new Vector2Int(map.Width, map.Height);
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