#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;


namespace net.rs64.TexTransTool
{
    public static class MipMapUtils
    {
        public static SortedList<int, Color[]> GenerateMiplist(this Texture2D Texture)
        {
            var Mips = new SortedList<int, Color[]>();
            var MipMapper = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/net.rs64.tex-trans-tool/Runtime/ComputeShaders/MipMapper.compute");
            var Texsize = new Vector2Int(Texture.width, Texture.height);
            var Mipcount = MipCount(Mathf.Min(Texsize.x, Texsize.y));

            var FirstTexArray = new TowDMap<Color>(Texture.GetPixels(), Texsize);
            var KarnelIndex = MipMapper.FindKernel("MipMapper");


            var TexBuffer = new ComputeBuffer(Texsize.x * Texsize.y, 16);
            TexBuffer.SetData(FirstTexArray.Array);

            MipMapper.SetBuffer(KarnelIndex, "Tex", TexBuffer);
            MipMapper.SetInt("TexSizeX", Texsize.x);

            var MipSize = Texsize;
            for (int i = 1; i <= Mipcount; i++)
            {
                MipSize /= 2;

                var mip = new Color[MipSize.x * MipSize.y];
                var Mipbuffer = new ComputeBuffer(mip.Length, 16);

                MipMapper.SetBuffer(KarnelIndex, "OutPutMap", Mipbuffer);
                MipMapper.SetInt("MipSizeX", MipSize.x);
                MipMapper.Dispatch(KarnelIndex, Mathf.Max(1, MipSize.x / 32), Mathf.Max(1, MipSize.x / 32), 1);



                Mipbuffer.GetData(mip);
                Mips.Add(i, mip);

                TexBuffer.Release();
                TexBuffer = Mipbuffer;

                MipMapper.SetBuffer(KarnelIndex, "Tex", TexBuffer);
                MipMapper.SetInt("TexSizeX", MipSize.x);

            }

            TexBuffer.Release();
            return Mips;
        }
        public static void MergeMip(SortedList<int, Color[]> DistMaps, SortedList<int, Color[]> primMaps)
        {
            for (int mapi = 1; mapi < DistMaps.Count; mapi++)
            {
                var Dist = DistMaps[mapi];
                var prim = primMaps[mapi];

                for (int i = 0; i < Dist.Length; i++)
                {
                    Dist[i] = prim[i].a > 0 ? prim[i] : Dist[i];
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
        public static SortedList<int, Color[]> GenerateMiplistInCPU(this Texture2D Texture)
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
                    var texsel = new Vector2Int(X, Y) * box;
                    mip[X, Y] = GetMipPixsel(Texture, texsel, box);
                }
            }

            return mip;
        }



        private static Color GetMipPixsel(TowDMap<Color> Tex, Vector2Int Texsel, Vector2Int Box)
        {
            var Color = new Color(0, 0, 0, 0);
            var Pixselcount = 0;

            for (int bX = 0; bX < Box.x; bX++)
            {
                for (int bY = 0; bY < Box.y; bY++)
                {
                    var texColor = Tex[Texsel.x + bX, Texsel.y + bY];
                    if (texColor.a > 0)
                    {
                        Color += texColor;
                        Pixselcount += 1;
                    }
                }
            }

            return Pixselcount > 0 ? Color / Pixselcount : new Color(0, 0, 0, 0);
        }


        public static int MipCount(int In)
        {
            var value = In;
            var Count = 0;
            while (value >= 2)
            {
                value /= 2;

                Count += 1;
            }
            return Count;
        }
    }
}
#endif
