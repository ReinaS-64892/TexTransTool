#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;


namespace net.rs64.TexTransTool
{
    public static class MipMapUtils
    {
        public static SortedList<int, Color[]> GenerateMipList(this Texture2D Texture)
        {
            var mips = new SortedList<int, Color[]>();
            var mipMapper = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/net.rs64.tex-trans-tool/Runtime/ComputeShaders/MipMapper.compute");
            var texSize = new Vector2Int(Texture.width, Texture.height);
            var mipCount = MipCount(Mathf.Min(texSize.x, texSize.y));

            var FirstTexArray = new TowDMap<Color>(Texture.GetPixels(), texSize);
            var KernelIndex = mipMapper.FindKernel("MipMapper");


            var texBuffer = new ComputeBuffer(texSize.x * texSize.y, 16);
            texBuffer.SetData(FirstTexArray.Array);

            mipMapper.SetBuffer(KernelIndex, "Tex", texBuffer);
            mipMapper.SetInt("TexSizeX", texSize.x);

            var MipSize = texSize;
            for (int i = 1; i <= mipCount; i++)
            {
                MipSize /= 2;

                var mip = new Color[MipSize.x * MipSize.y];
                var mipBuffer = new ComputeBuffer(mip.Length, 16);

                mipMapper.SetBuffer(KernelIndex, "OutPutMap", mipBuffer);
                mipMapper.SetInt("MipSizeX", MipSize.x);
                mipMapper.Dispatch(KernelIndex, Mathf.Max(1, MipSize.x / 32), Mathf.Max(1, MipSize.x / 32), 1);



                mipBuffer.GetData(mip);
                mips.Add(i, mip);

                texBuffer.Release();
                texBuffer = mipBuffer;

                mipMapper.SetBuffer(KernelIndex, "Tex", texBuffer);
                mipMapper.SetInt("TexSizeX", MipSize.x);

            }

            texBuffer.Release();
            return mips;
        }
        public static void MergeMip(SortedList<int, Color[]> DistMaps, SortedList<int, Color[]> primMaps)
        {
            for (int mapI = 1; mapI < DistMaps.Count; mapI++)
            {
                var dist = DistMaps[mapI];
                var prim = primMaps[mapI];

                for (int i = 0; i < dist.Length; i++)
                {
                    dist[i] = prim[i].a > 0 ? prim[i] : dist[i];
                }
            }
        }
        public static void ApplyMip(this Texture2D Texture, SortedList<int, Color[]> Maps)
        {
            for (int i = 1; i < Texture.mipmapCount; i++)
            {
                var mip = Maps[i];
                Texture.SetPixels(mip, i);
            }
        }
        public static SortedList<int, Color[]> GenerateMipListInCPU(this Texture2D Texture)
        {
            var Tex = new TowDMap<Color>(Texture.GetPixels(), new Vector2Int(Texture.width, Texture.height));
            var Mips = new SortedList<int, Color[]>();

            var MipCount = 0;
            while (true)
            {
                MipCount += 1;
                var Mip = OneLevelMip(Tex);
                if (Mip != null)
                {
                    Mips.Add(MipCount, Mip.Array);
                    Tex = Mip;
                }
                else
                {
                    break;
                }
            }
            return Mips;
        }
        public static TowDMap<Color> OneLevelMip(TowDMap<Color> Texture)
        {
            var MipSize = Texture.MapSize / 2;

            if (MipSize.x <= 0 || MipSize.y <= 0) { return null; }

            var box = new Vector2Int(2, 2);
            var mip = new TowDMap<Color>(MipSize);

            for (int X = 0; X < MipSize.x; X++)
            {
                for (int Y = 0; Y < MipSize.y; Y++)
                {
                    var texel = new Vector2Int(X, Y) * box;
                    mip[X, Y] = GetMipPixel(Texture, texel, box);
                }
            }

            return mip;
        }



        private static Color GetMipPixel(TowDMap<Color> Tex, Vector2Int texel, Vector2Int Box)
        {
            var color = new Color(0, 0, 0, 0);
            var pixelCount = 0;

            for (int bX = 0; bX < Box.x; bX++)
            {
                for (int bY = 0; bY < Box.y; bY++)
                {
                    var texColor = Tex[texel.x + bX, texel.y + bY];
                    if (texColor.a > 0)
                    {
                        color += texColor;
                        pixelCount += 1;
                    }
                }
            }

            return pixelCount > 0 ? color / pixelCount : new Color(0, 0, 0, 0);
        }


        public static int MipCount(int In)
        {
            var value = In;
            var count = 0;
            while (value >= 2)
            {
                value /= 2;

                count += 1;
            }
            return count;
        }
    }
}
#endif
