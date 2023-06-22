#if UNITY_EDITOR
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vector2 = UnityEngine.Vector2;
using System.Linq;

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


        public static TransTargetTexture TransCompileUseGetPixsel(Texture2D SouseTex, TransMapData AtralsMap, TransTargetTexture TargetTex, TexWrapMode wrapMode, Vector2? OutRenge = null)
        {
            var TargetTexSize = TargetTex.DistansMap.MapSize;
            if (TargetTexSize.x != AtralsMap.Map.MapSize.x && TargetTexSize.y != AtralsMap.Map.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");

            var lengs = AtralsMap.Map.Array.Length;
            NotFIlterAndReadWritTexture2D(ref SouseTex);
            var TargetPixsel = TargetTex.Texture2D.GetPixels();

            for (int i = 0; i < lengs; i += 1)
            {
                var NawDistans = AtralsMap[i].Distans;
                if (NawDistans > AtralsMap.DefaultPading && NawDistans > TargetTex.DistansMap[i])
                {
                    Vector2 SouseTexPos = AtralsMap[i].Pos;

                    Vector2? WarpdPos = GetWarpdPos(wrapMode, SouseTexPos);

                    WarpdPos = IsOutRenge(OutRenge, SouseTexPos) ? null : WarpdPos;

                    SetPixsl(TargetPixsel, SouseTex, i, WarpdPos);
                    TargetTex.DistansMap[i] = NawDistans;
                }
            }
            TargetTex.Texture2D.SetPixels(TargetPixsel);
            TargetTex.Texture2D.Apply();
            return TargetTex;

        }

        private static Vector2? GetWarpdPos(TexWrapMode wrapMode, Vector2 SouseTexPos)
        {
            Vector2? WarpdPos = null;
            switch (wrapMode)
            {
                default:
                case TexWrapMode.NotWrap:
                    {
                        if (!(0 < SouseTexPos.x && SouseTexPos.x < 1) || !(0 < SouseTexPos.y && SouseTexPos.y < 1)) WarpdPos = null;
                        else WarpdPos = SouseTexPos;
                        break;
                    }
                case TexWrapMode.Stretch:
                    {
                        var StrechdPos = new Vector2();
                        StrechdPos.x = Mathf.Clamp01(SouseTexPos.x);
                        StrechdPos.y = Mathf.Clamp01(SouseTexPos.y);
                        WarpdPos = StrechdPos;
                        break;
                    }
                case TexWrapMode.Loop:
                    {
                        WarpdPos = SouseTexPos;
                        break;
                    }
            }

            return WarpdPos;
        }

        private static bool IsOutRenge(Vector2? OutRenge, Vector2 SouseTexPos)
        {
            if (OutRenge.HasValue)
            {
                var outReng = OutRenge.Value;
                var OutOfolag = false;
                if (!((outReng.x * -1) < SouseTexPos.x && SouseTexPos.x < (outReng.x + 1))) { OutOfolag = true; }
                if (!((outReng.y * -1) < SouseTexPos.y && SouseTexPos.y < (outReng.y + 1))) { OutOfolag = true; }

                return OutOfolag;
            }

            return false;
        }

        static void SetPixsl(Color[] TargetPixsel, Texture2D SouseTex, int index, Vector2? SouseTexPos)
        {
            if (!SouseTexPos.HasValue) return;
            var souspixselcloro = SouseTex.GetPixelBilinear(SouseTexPos.Value.x, SouseTexPos.Value.y);
            TargetPixsel[index] = souspixselcloro;
        }

        public static void NotFIlterAndReadWritTexture2D(ref Texture2D SouseTex, bool ConvertToLiner = false)
        {
            var SouseTexPath = AssetDatabase.GetAssetPath(SouseTex);
            byte[] PngBytes = string.IsNullOrEmpty(SouseTexPath) ? SouseTex.EncodeToPNG() : File.ReadAllBytes(SouseTexPath);
            if (ConvertToLiner)
            {
                SouseTex = new Texture2D(2, 2, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
            }
            else
            {
                SouseTex = new Texture2D(2, 2);
            }
            SouseTex.LoadImage(PngBytes);
        }

        public static Vector2Int NativeSize(this Texture2D SouseTex)
        {
            var SouseTexPath = AssetDatabase.GetAssetPath(SouseTex);
            Stream Stream;
            bool IsJPG = false;
            if (string.IsNullOrEmpty(SouseTexPath))
            {
                Stream = new MemoryStream(SouseTex.EncodeToPNG());
            }
            else if (Path.GetExtension(SouseTexPath) == ".png")
            {
                Stream = File.OpenRead(SouseTexPath);
            }
            else if (Path.GetExtension(SouseTexPath) == ".jpg" || Path.GetExtension(SouseTexPath) == ".jpeg")
            {
                Stream = File.OpenRead(SouseTexPath);
                IsJPG = true;
            }
            else
            {
                Stream = new MemoryStream(SouseTex.EncodeToPNG());
            }



            return !IsJPG ? PNGtoSize(Stream) : JPGtoSize(Stream);
        }


        public static Vector2Int JPGtoSize(Stream Stream)
        {
            var SOI = new byte[2] { 0xFF, 0xD8 };

            var ReadSOI = new byte[2];
            Stream.Read(ReadSOI, 0, 2);

            if (!ReadSOI.SequenceEqual(SOI))
            {
                throw new Exception("JPGファイルではありません");
            }

            var SOFFirstByte = 0xFF;
            var SOFSeconndBytes = new byte[] {
                     0xC0 ,  0xC1 , 0xC2 , 0xC3 ,
                     0xC5 , 0xC6 , 0xC7 ,
                     0xC9 , 0xCA , 0xCB ,
                     0xCD , 0xCE , 0xCF
                    };
            var SOS = 0xDA;

            bool SOFHit = false;

            while (!SOFHit)
            {
                if ((Byte)Stream.ReadByte() == SOFFirstByte)
                {
                    var SecoondByte = (Byte)Stream.ReadByte();
                    if (SOFSeconndBytes.Contains(SecoondByte))
                    {
                        SOFHit = true;
                    }
                    else if (SOS == SecoondByte)
                    {
                        throw new Exception("JPGデータから画像サイズを取得できませんでした。");
                    }

                }
            }
            var LFP = new byte[3];
            Stream.Read(LFP, 0, 3);


            var WithByte = new byte[2];
            var HeightByte = new byte[2];

            Stream.Read(WithByte, 0, 2);
            Stream.Read(HeightByte, 0, 2);

            if (BitConverter.IsLittleEndian)
            {
                WithByte = WithByte.Reverse().ToArray();
                HeightByte = HeightByte.Reverse().ToArray();
            }

            var with = BitConverter.ToUInt16(WithByte, 0);
            var height = BitConverter.ToUInt16(HeightByte, 0);

            return new Vector2Int((int)with, (int)height);
        }

        public static Vector2Int PNGtoSize(Stream Stream)
        {
            var PNGHeader = new byte[8] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            var ReadByte = new byte[8];
            Stream.Read(ReadByte, 0, 8);

            if (!ReadByte.SequenceEqual(PNGHeader))
            {
                throw new Exception("PNGファイルではありません");
            }

            var IHDRBytes = new byte[4] { 0x49, 0x48, 0x44, 0x52 };

            var IHDRHit = false;
            while (!IHDRHit)
            {
                foreach (var IHDRByte in IHDRBytes)
                {
                    var Byte = (Byte)Stream.ReadByte();
                    if (IHDRByte == Byte)
                    {
                        IHDRHit = true;
                    }
                    else
                    {
                        IHDRHit = false;
                        break;
                    }
                }
            }

            var WithByte = new byte[4];
            var HeightByte = new byte[4];

            Stream.Read(WithByte, 0, 4);
            Stream.Read(HeightByte, 0, 4);

            if (BitConverter.IsLittleEndian)
            {
                WithByte = WithByte.Reverse().ToArray();
                HeightByte = HeightByte.Reverse().ToArray();
            }

            var with = BitConverter.ToUInt32(WithByte, 0);
            var height = BitConverter.ToUInt32(HeightByte, 0);

            return new Vector2Int((int)with, (int)height);
        }

        public static TransTargetTexture TransCompileUseComputeSheder(Texture2D SouseTex, TransMapData AtralsMaps, TransTargetTexture targetTex, TexWrapMode wrapMode, Vector2? OutRenge = null, ComputeShader CS = null)
        {
            return TransCompileUseComputeSheder(SouseTex, new TransMapData[1] { AtralsMaps }, targetTex, wrapMode, OutRenge, CS);
        }
        public static TransTargetTexture TransCompileUseComputeSheder(Texture2D SouseTex, IEnumerable<TransMapData> AtralsMaps, TransTargetTexture targetTex, TexWrapMode wrapMode, Vector2? OutRenge = null, ComputeShader CS = null)
        {
            var TexSize = targetTex.DistansMap.MapSize;
            if (AtralsMaps.Any(i => i.Map.MapSize != TexSize)) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            if (CS == null) CS = AssetDatabase.LoadAssetAtPath<ComputeShader>(TransCompilerPath);

            var sTexSize = SouseTex.NativeSize();

            NotFIlterAndReadWritTexture2D(ref SouseTex);

            var SColors = SouseTex.GetPixels();
            var TColors = targetTex.Texture2D.GetPixels();

            Vector2Int ThredGropSize = TexSize / 32;
            var KernelIndex = CS.FindKernel(wrapMode.ToString());


            var SouseTexCB = new ComputeBuffer(SColors.Length, 16);
            SouseTexCB.SetData(SColors);
            CS.SetBuffer(KernelIndex, "Source", SouseTexCB);


            CS.SetInts("SourceTexSize", new int[2] { sTexSize.x, sTexSize.y });


            var AtlasMapBuffer = new ComputeBuffer(TColors.Length, 12);

            var TargetBuffer = new ComputeBuffer(TColors.Length, 16);
            TargetBuffer.SetData(TColors);
            CS.SetBuffer(KernelIndex, "Target", TargetBuffer);


            var TargetDistansBuffer = new ComputeBuffer(TColors.Length, 4);
            TargetDistansBuffer.SetData(targetTex.DistansMap.Array);
            CS.SetBuffer(KernelIndex, "TargetDistansMap", TargetDistansBuffer);


            CS.SetInts("TargetTexSize", new int[2] { TexSize.x, TexSize.y });
            CS.SetBool("IsOutRenge", OutRenge.HasValue);
            if (OutRenge.HasValue)
            {
                CS.SetFloats("OutRenge", new float[2] { OutRenge.Value.x, OutRenge.Value.y });
            }

            foreach (var AtralsMap in AtralsMaps)
            {
                AtlasMapBuffer.SetData(AtralsMap.Map.Array);
                CS.SetBuffer(KernelIndex, "AtlasMap", AtlasMapBuffer);

                CS.Dispatch(KernelIndex, ThredGropSize.x, ThredGropSize.y, 1);
            }


            TargetBuffer.GetData(TColors);
            targetTex.Texture2D.SetPixels(TColors);
            targetTex.Texture2D.Apply();

            TargetDistansBuffer.GetData(targetTex.DistansMap.Array);

            AtlasMapBuffer.Release();
            TargetDistansBuffer.Release();
            TargetBuffer.Release();
            SouseTexCB.Release();

            return targetTex;
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