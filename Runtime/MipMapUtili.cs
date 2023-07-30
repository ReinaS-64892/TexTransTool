#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;


namespace Rs64.TexTransTool
{
    public static class MipMapUtili
    {
        public static void GenereatMipMap(this Texture2D Texture)
        {
            var timer = new DebugUtils.DebugTimer();
            var MipMapper = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/net.rs64.tex-trans-tool/Runtime/ComputeShaders/MipMapper.compute");
            var Texsize = new Vector2Int(Texture.width, Texture.height);
            (var Mipcount, var sepCount) = MipCountTarnpos(Mathf.Min(Texsize.x, Texsize.y));

            var FirstTexArray = new TowDMap<Color>(Texture.GetPixels(), Texsize);

            timer.Log("Load");

            var KarnelIndex = MipMapper.FindKernel("SingleMipMapper");
            var KarnelIndex2x2 = MipMapper.FindKernel("SingleMipMapper2x2");


            var FirstTexBuffer = new ComputeBuffer(Texsize.x * Texsize.y, 16);
            FirstTexBuffer.SetData(FirstTexArray.Array);

            MipMapper.SetBuffer(KarnelIndex, "TexFirst", FirstTexBuffer);
            MipMapper.SetBuffer(KarnelIndex2x2, "TexFirst", FirstTexBuffer);
            MipMapper.SetInt("TexFirstSizeX", Texsize.x);
            timer.Log("SetBuffer");

            var MipSize = Texsize;
            for (int i = 1; i <= Mipcount; i++)
            {
                MipSize /= 2;
                var box = new Vector2Int(Texsize.x / MipSize.x, Texsize.y / MipSize.y);
                var mip = new Color[MipSize.x * MipSize.y];
                var buffer = new ComputeBuffer(mip.Length, 16);


                MipMapper.SetInts("BoxSize", new int[] { box.x, box.y });
                MipMapper.SetInt("MipSizeX", MipSize.x);

                if (Mathf.Min(MipSize.x, MipSize.y) < 32)
                {
                    MipMapper.SetBuffer(KarnelIndex2x2, "OutPutMap", buffer);
                    MipMapper.Dispatch(KarnelIndex2x2, Mathf.Max(1, MipSize.x / 2), Mathf.Max(1, MipSize.y / 2), 1);
                }
                else
                {
                    MipMapper.SetBuffer(KarnelIndex, "OutPutMap", buffer);
                    MipMapper.Dispatch(KarnelIndex, MipSize.x / 32, MipSize.y / 32, 1);
                }

                buffer.GetData(mip);
                Texture.SetPixels(mip, i);
                buffer.Release();
                timer.Log("Mip " + i + " mapsixe " + MipSize.x);
            }

            FirstTexBuffer.Release();
            timer.EndLog("End");
        }
        public static void GenereatMipMapPriority(this Texture2D Texture, Texture2D Priority)
        {
            var timer = new DebugUtils.DebugTimer();
            var MipMapper = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/net.rs64.tex-trans-tool/Runtime/ComputeShaders/MipMapper.compute");
            var Texsize = new Vector2Int(Texture.width, Texture.height);
            (var MaxMipCount, var sepCount) = MipCountTarnpos(Mathf.Min(Texsize.x, Texsize.y));

            var SecondTexArray = new TowDMap<Color>(Texture.GetPixels(), Texsize);
            var FirstTexArray = new TowDMap<Color>(Priority.GetPixels(), new Vector2Int(Priority.width, Priority.height));

            timer.Log("Load");



            var KarnelIndex = MipMapper.FindKernel("MipMapper");
            var KarnelIndex2x2 = MipMapper.FindKernel("MipMapper2x2");

            var SecondTexBuffer = new ComputeBuffer(Texture.width * Texture.height, 16);
            SecondTexBuffer.SetData(SecondTexArray.Array);
            var FirstTexBuffer = new ComputeBuffer(Priority.width * Priority.height, 16);
            FirstTexBuffer.SetData(FirstTexArray.Array);

            MipMapper.SetBuffer(KarnelIndex, "TexSecond", SecondTexBuffer);
            MipMapper.SetBuffer(KarnelIndex2x2, "TexSecond", SecondTexBuffer);
            MipMapper.SetInt("TexSecondSizeX", Texture.width);
            MipMapper.SetBuffer(KarnelIndex, "TexFirst", FirstTexBuffer);
            MipMapper.SetBuffer(KarnelIndex2x2, "TexFirst", FirstTexBuffer);
            MipMapper.SetInt("TexFirstSizeX", Priority.width);

            timer.Log("SetBuffer");


            var MipSize = Texsize;
            for (int MipCount = 1; MipCount <= MaxMipCount; MipCount++)
            {
                MipSize /= 2;
                var box = new Vector2Int(Texsize.x / MipSize.x, Texsize.y / MipSize.y);
                var mip = new Color[MipSize.x * MipSize.y];

                var buffer = new ComputeBuffer(mip.Length, 16);


                MipMapper.SetInts("BoxSize", new int[] { box.x, box.y });
                MipMapper.SetInt("MipSizeX", MipSize.x);

                if (Mathf.Min(MipSize.x, MipSize.y) < 32)
                {
                    MipMapper.SetBuffer(KarnelIndex2x2, "OutPutMap", buffer);
                    MipMapper.Dispatch(KarnelIndex2x2, Mathf.Max(1, MipSize.x / 2), Mathf.Max(1, MipSize.y / 2), 1);
                }
                else
                {
                    MipMapper.SetBuffer(KarnelIndex, "OutPutMap", buffer);
                    MipMapper.Dispatch(KarnelIndex, MipSize.x / 32, MipSize.y / 32, 1);
                }

                buffer.GetData(mip);
                Texture.SetPixels(mip, MipCount);
                buffer.Release();

                timer.Log("Mip " + MipCount + " mapsixe " + MipSize.x);

            }

            FirstTexBuffer.Release();
            SecondTexBuffer.Release();


            timer.EndLog("End");
        }
        public static void GenereatMip(this Texture2D Texture)
        {
            var Tex = new TowDMap<Color>(Texture.GetPixels(), new Vector2Int(Texture.width, Texture.height));
            var Mips = new SortedList<int, TowDMap<Color>>();

            var timer = new DebugUtils.DebugTimer();

            var MipCount = 0;
            while (true)
            {
                MipCount += 1;
                var Mip = OneLevelMip(Tex);
                if (Mip != null)
                {
                    Mips.Add(MipCount, Mip);
                    Tex = Mip;
                }
                else
                {
                    break;
                }
                timer.Log("Mip " + MipCount + " mapsixe " + Mip.MapSize.x);
            }
            timer.EndLog("End");

            foreach(var mio in Mips)
            {
                Texture.SetPixels(mio.Value.Array, mio.Key);
            }

        }
        public static TowDMap<Color> OneLevelMip(TowDMap<Color> Texture)
        {
            var MipSize = Texture.MapSize / 2;

            if (MipSize.x <= 0 || MipSize.y <= 0) { return null; }

            var box = new Vector2Int(2, 2);
            var mip = new TowDMap<Color>( MipSize);

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

        public static void GenereatMip(this Texture2D Texture, Texture2D Priority)
        {
            if (Texture.width != Priority.width || Texture.height != Priority.height) { throw new System.Exception("Texture size not match"); }
            var Tex = new TowDMap<Color>(Texture.GetPixels(), new Vector2Int(Texture.width, Texture.height));
            var PrimTex = new TowDMap<Color>(Priority.GetPixels(), new Vector2Int(Priority.width, Priority.height));

            var MipCount = 0;
            while (true)
            {
                MipCount += 1;
                var Mip = OneLevelMip(Tex);
                var Mippr = OneLevelMip(PrimTex);
                if (Mip != null)
                {
                    Texture.SetPixels(MargeMip(Mip, Mippr).Array, MipCount);
                    Tex = Mip;
                    PrimTex = Mippr;

                }
                else
                {
                    break;
                }
            }

        }
        public static TowDMap<Color> MargeMip(TowDMap<Color> Texture, TowDMap<Color> Prim)
        {
            var mip = new TowDMap<Color>(new Color(0, 0, 0, 0), Texture.MapSize);

            for (int X = 0; X < mip.MapSize.x; X++)
            {
                for (int Y = 0; Y < mip.MapSize.y; Y++)
                {
                    mip[X, Y] = Prim[X, Y].a > 0 ? Prim[X, Y] : Texture[X, Y];
                }
            }


            return mip;
        }
        private static SortedList<int, Color[]> GenereatMipMapPrioritys(TowDMap<Color> Secondy, TowDMap<Color> Primary, int SepCount, int MipCount)
        {
            var MipMaps = new SortedList<int, Color[]>();

            var MipSize = Secondy.MapSize;
            for (int i = 1; i <= MipCount; i++)
            {
                MipSize /= 2;
                if (i < SepCount) { continue; }
                var box = new Vector2Int(Secondy.MapSize.x / MipSize.x, Secondy.MapSize.y / MipSize.y);
                var mip = new TowDMap<Color>(new Color(0, 0, 0, 0), MipSize);

                for (int X = 0; X < MipSize.x; X++)
                {
                    for (int Y = 0; Y < MipSize.y; Y++)
                    {
                        var texsel = new Vector2Int(X, Y) * box;

                        var SencdColor = GetMipPixsel(Secondy, texsel, box);
                        var PrimeColor = GetMipPixsel(Primary, texsel, box);

                        mip[X, Y] = Color.Lerp(SencdColor, PrimeColor, Mathf.Ceil(PrimeColor.a));
                    }
                }
                MipMaps.Add(i, mip.Array);
            }

            return MipMaps;

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
        public static (int, int) MipCountTarnpos(int In)
        {
            var value = In;
            var sepcount = In;
            var Count = 0;
            while (value >= 2)
            {
                value /= 2;


                Count += 1;

                if (value / 32 <= 0 && Count < sepcount)
                {
                    sepcount = Count;
                }
            }
            return (Count, sepcount);
        }
    }
}
#endif
