#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{

    public static class TextureUtility
    {


        /// <summary>
        /// いろいろな設定をコピーしたような感じにする。
        /// ただしリサイズだけは行わない。
        /// 戻り値はクローンになる可能性があるため注意。
        /// ならない場合もあるため注意。
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="CopySouse"></param>
        /// <returns></returns>
        public static Texture2D CopySetting(this Texture2D tex, Texture2D CopySouse)
        {
            var TextureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(CopySouse)) as TextureImporter;
            if (TextureImporter != null && TextureImporter.textureType == TextureImporterType.NormalMap) tex = tex.ConvertNormalMap();
            if (tex.mipmapCount > 1 != CopySouse.mipmapCount > 1)
            {
                var newTex = new Texture2D(tex.width, tex.height, tex.format, CopySouse.mipmapCount > 1);
                newTex.SetPixels32(tex.GetPixels32());
                newTex.name = tex.name;
                tex = newTex;
            }
            tex.filterMode = CopySouse.filterMode;
            tex.anisoLevel = CopySouse.anisoLevel;
            tex.alphaIsTransparency = CopySouse.alphaIsTransparency;
            tex.requestedMipmapLevel = CopySouse.requestedMipmapLevel;
            tex.mipMapBias = CopySouse.mipMapBias;
            tex.wrapModeU = CopySouse.wrapModeU;
            tex.wrapModeV = CopySouse.wrapModeV;
            tex.wrapMode = CopySouse.wrapMode;
            if (tex.mipmapCount > 1) { tex.Apply(true); }
            EditorUtility.CompressTexture(tex, CopySouse.format, TextureImporter == null ? 50 : TextureImporter.compressionQuality);

            return tex;
        }
        public static Texture2D ConvertNormalMap(this Texture2D tex)
        {
            throw new NotImplementedException();
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

    }
}
#endif