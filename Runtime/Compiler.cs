#if UNITY_EDITOR
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vector2 = UnityEngine.Vector2;
using System.Linq;

namespace net.rs64.TexTransTool
{
    public enum TexWrapMode
    {
        [Obsolete]
        NotWrap,
        Stretch,
        Loop,
    }
    public static class Compiler
    {
        public const string TransCompilerPath = "Packages/net.rs64.tex-trans-tool/Runtime/ComputeShaders/TransCompiler.compute";

        [Obsolete]
        public static TransTargetTexture TransCompileUseGetPixsel(Texture2D SouseTex, TransMapData AtralsMap, TransTargetTexture TargetTex, TexWrapMode wrapMode, Vector2? OutRange = null)
        {
            var TargetTexSize = TargetTex.DistanceMap.MapSize;
            if (TargetTexSize.x != AtralsMap.Map.MapSize.x && TargetTexSize.y != AtralsMap.Map.MapSize.y) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");

            var lengs = AtralsMap.Map.Array.Length;
            NotFIlterAndReadWritTexture2D(ref SouseTex);
            var TargetPixsel = TargetTex.Texture2D.GetPixels();

            for (int i = 0; i < lengs; i += 1)
            {
                var NawDistans = AtralsMap[i].Distance;
                if (NawDistans > AtralsMap.DefaultPadding && NawDistans > TargetTex.DistanceMap[i])
                {
                    Vector2 SouseTexPos = AtralsMap[i].Pos;

                    Vector2? WarpdPos = GetWarpdPos(wrapMode, SouseTexPos);

                    WarpdPos = IsOutRange(OutRange, SouseTexPos) ? null : WarpdPos;

                    SetPixel(TargetPixsel, SouseTex, i, WarpdPos);
                    TargetTex.DistanceMap[i] = NawDistans;
                }
            }
            TargetTex.Texture2D.SetPixels(TargetPixsel);
            return TargetTex;

        }

        [Obsolete]
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

        private static bool IsOutRange(Vector2? OutRange, Vector2 SouseTexPos)
        {
            if (OutRange.HasValue)
            {
                var outReng = OutRange.Value;
                var OutOfolag = false;
                if (!((outReng.x * -1) < SouseTexPos.x && SouseTexPos.x < (outReng.x + 1))) { OutOfolag = true; }
                if (!((outReng.y * -1) < SouseTexPos.y && SouseTexPos.y < (outReng.y + 1))) { OutOfolag = true; }

                return OutOfolag;
            }

            return false;
        }

        static void SetPixel(Color[] TargetPixel, Texture2D SouseTex, int index, Vector2? SouseTexPos)
        {
            if (!SouseTexPos.HasValue) return;
            var sousPixelColor = SouseTex.GetPixelBilinear(SouseTexPos.Value.x, SouseTexPos.Value.y);
            TargetPixel[index] = sousPixelColor;
        }

        public static void NotFIlterAndReadWritTexture2D(ref Texture2D SouseTex)
        {
            var SouseTexPath = AssetDatabase.GetAssetPath(SouseTex);
            var IsEmpty = string.IsNullOrEmpty(SouseTexPath);
            if (!IsEmpty)
            {
                var TextureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(SouseTex)) as TextureImporter;
                if (TextureImporter == null) return;
                if (TextureImporter.textureType == TextureImporterType.Default && TextureImporter.isReadable) { return; }
            }
            else
            {
                if (SouseTex.isReadable) return;
            }

            byte[] ImageBytes = IsEmpty ? SouseTex.EncodeToPNG() : File.ReadAllBytes(SouseTexPath);
            var pngTex = new Texture2D(2, 2);
            pngTex.LoadImage(ImageBytes);

            var newTex = new Texture2D(pngTex.width, pngTex.height, TextureFormat.RGBA32, false);
            newTex.SetPixels32(pngTex.GetPixels32());
            newTex.Apply();
            SouseTex = newTex;
        }

        public static Vector2Int NativeSize(this Texture2D SouseTex)
        {
            var souseTexPath = AssetDatabase.GetAssetPath(SouseTex);
            Stream stream;
            bool isJPG = false;
            if (string.IsNullOrEmpty(souseTexPath) || AssetDatabase.IsSubAsset(SouseTex))
            {
                //メモリに直接乗っかってる場合かunityアセットの場合、Texture2Dが誤った値を返さない。
                return new Vector2Int(SouseTex.width, SouseTex.height);
            }
            else if (Path.GetExtension(souseTexPath) == ".png")
            {
                stream = File.OpenRead(souseTexPath);
            }
            else if (Path.GetExtension(souseTexPath) == ".jpg" || Path.GetExtension(souseTexPath) == ".jpeg")
            {
                stream = File.OpenRead(souseTexPath);
                isJPG = true;
            }
            else if (Path.GetExtension(souseTexPath) == ".asset")
            {
                return new Vector2Int(SouseTex.width, SouseTex.height);
            }
            else
            {
                stream = new MemoryStream(SouseTex.EncodeToPNG());
            }



            return !isJPG ? PNGtoSize(stream) : JPGtoSize(stream);
        }


        public static Vector2Int JPGtoSize(Stream stream)
        {
            var SOI = new byte[2] { 0xFF, 0xD8 };

            var readSOI = new byte[2];
            stream.Read(readSOI, 0, 2);

            if (!readSOI.SequenceEqual(SOI))
            {
                throw new Exception("JPGファイルではありません");
            }

            var SOFFirstByte = 0xFF;
            var SOFSecondBytes = new byte[] {
                     0xC0 ,  0xC1 , 0xC2 , 0xC3 ,
                     0xC5 , 0xC6 , 0xC7 ,
                     0xC9 , 0xCA , 0xCB ,
                     0xCD , 0xCE , 0xCF
                    };
            var SOS = 0xDA;

            bool SOFHit = false;

            while (!SOFHit)
            {
                if ((Byte)stream.ReadByte() == SOFFirstByte)
                {
                    var SecondByte = (Byte)stream.ReadByte();
                    if (SOFSecondBytes.Contains(SecondByte))
                    {
                        SOFHit = true;
                    }
                    else if (SOS == SecondByte)
                    {
                        throw new Exception("JPGデータから画像サイズを取得できませんでした。");
                    }

                }
            }
            var LFP = new byte[3];
            stream.Read(LFP, 0, 3);


            var heightByte = new byte[2];
            var withByte = new byte[2];

            stream.Read(heightByte, 0, 2);
            stream.Read(withByte, 0, 2);

            if (BitConverter.IsLittleEndian)
            {
                heightByte = heightByte.Reverse().ToArray();
                withByte = withByte.Reverse().ToArray();
            }

            var height = BitConverter.ToUInt16(heightByte, 0);
            var with = BitConverter.ToUInt16(withByte, 0);

            return new Vector2Int((int)with, (int)height);
        }

        public static Vector2Int PNGtoSize(Stream stream)
        {
            var PNGHeader = new byte[8] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            var readByte = new byte[8];
            stream.Read(readByte, 0, 8);

            if (!readByte.SequenceEqual(PNGHeader))
            {
                throw new Exception("PNGファイルではありません");
            }

            var IHDRBytes = new byte[4] { 0x49, 0x48, 0x44, 0x52 };

            var IHDRHit = false;
            while (!IHDRHit)
            {
                foreach (var IHDRByte in IHDRBytes)
                {
                    var Byte = (Byte)stream.ReadByte();
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

            var withByte = new byte[4];
            var heightByte = new byte[4];

            stream.Read(withByte, 0, 4);
            stream.Read(heightByte, 0, 4);

            if (BitConverter.IsLittleEndian)
            {
                withByte = withByte.Reverse().ToArray();
                heightByte = heightByte.Reverse().ToArray();
            }

            var with = BitConverter.ToUInt32(withByte, 0);
            var height = BitConverter.ToUInt32(heightByte, 0);

            return new Vector2Int((int)with, (int)height);
        }

        public static TransTargetTexture TransCompileUseComputeShader(Texture2D SouseTex, TransMapData AtralsMaps, TransTargetTexture targetTex, TexWrapMode wrapMode, Vector2? OutRange = null, ComputeShader CS = null)
        {
            return TransCompileUseComputeSheder(SouseTex, new TransMapData[1] { AtralsMaps }, targetTex, wrapMode, OutRange, CS);
        }
        public static TransTargetTexture TransCompileUseComputeSheder(Texture2D SouseTex, IEnumerable<TransMapData> AtralsMaps, TransTargetTexture targetTex, TexWrapMode wrapMode, Vector2? OutRange = null, ComputeShader CS = null)
        {
            var texSize = targetTex.DistanceMap.MapSize;
            if (AtralsMaps.Any(i => i.Map.MapSize != texSize)) throw new ArgumentException("ターゲットテクスチャとアトラスマップのサイズが一致しません。");
            if (CS == null) CS = AssetDatabase.LoadAssetAtPath<ComputeShader>(TransCompilerPath);

            var sTexSize = SouseTex.NativeSize();

            NotFIlterAndReadWritTexture2D(ref SouseTex);

            var sColors = SouseTex.GetPixels();
            var tColors = targetTex.Texture2D.GetPixels();

            Vector2Int thredGropSize = texSize / 32;
            var kernelIndex = CS.FindKernel(wrapMode.ToString());


            var souseTexCB = new ComputeBuffer(sColors.Length, 16);
            souseTexCB.SetData(sColors);
            CS.SetBuffer(kernelIndex, "Source", souseTexCB);


            CS.SetInts("SourceTexSize", new int[2] { sTexSize.x, sTexSize.y });


            var atlasMapBuffer = new ComputeBuffer(tColors.Length, 12);

            var targetBuffer = new ComputeBuffer(tColors.Length, 16);
            targetBuffer.SetData(tColors);
            CS.SetBuffer(kernelIndex, "Target", targetBuffer);


            var targetDistansBuffer = new ComputeBuffer(tColors.Length, 4);
            targetDistansBuffer.SetData(targetTex.DistanceMap.Array);
            CS.SetBuffer(kernelIndex, "TargetDistansMap", targetDistansBuffer);


            CS.SetInts("TargetTexSize", new int[2] { texSize.x, texSize.y });
            CS.SetBool("IsOutRange", OutRange.HasValue);
            if (OutRange.HasValue)
            {
                CS.SetFloats("OutRange", new float[2] { OutRange.Value.x, OutRange.Value.y });
            }

            foreach (var atralsMap in AtralsMaps)
            {
                atlasMapBuffer.SetData(atralsMap.Map.Array);
                CS.SetBuffer(kernelIndex, "AtlasMap", atlasMapBuffer);

                CS.Dispatch(kernelIndex, thredGropSize.x, thredGropSize.y, 1);
            }


            targetBuffer.GetData(tColors);
            targetTex.Texture2D.SetPixels(tColors);

            targetDistansBuffer.GetData(targetTex.DistanceMap.Array);

            atlasMapBuffer.Release();
            targetDistansBuffer.Release();
            targetBuffer.Release();
            souseTexCB.Release();

            return targetTex;
        }
    }


    [Serializable]
    public class PropAndTexture2D
    {
        public string PropertyName;
        public Texture2D Texture2D;

        public PropAndTexture2D(string propertyName, Texture2D textures)
        {
            PropertyName = propertyName;
            Texture2D = textures;
        }
    }
    [Serializable]
    public class PropAndTexture
    {
        public string PropertyName;
        public Texture Texture2D;

        public PropAndTexture(string propertyName, Texture textures)
        {
            PropertyName = propertyName;
            Texture2D = textures;
        }
    }

}

#endif
