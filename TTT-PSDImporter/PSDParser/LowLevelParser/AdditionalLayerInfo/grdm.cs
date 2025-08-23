using System;
using System.Diagnostics;
using System.Linq;
using net.rs64.ParserUtility;

namespace net.rs64.PSDParser.AdditionalLayerInfo
{
    [Serializable, AdditionalLayerInfoParser("grdm")]
    public class grdm : AdditionalLayerInfoBase
    {
        public ushort Version;
        public bool IsGradientReversed;
        public bool IsGradientDithered;

        // v3

        public string GradientInteropMethodKey;
        public uint GradientNameLength;
        public string GradientName;
        public ushort ColorKeyCount;
        public ColorKeyV3[] ColorKeys;
        public ushort TransparencyKeyCount;
        public TransParencyKeyV3[] TransparencyKeys;
        public ushort Unknown;
        public ushort Smoothens;// 0-4096

        public override void ParseAddLY(bool isPSB, BinarySectionStream stream)
        {
            // var bin = new byte[(int)stream.Length];
            // stream.ReadToSpan(bin);
            // var fileName = string.Join("-", new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(bin).Select(b => b.ToString()));
            // System.IO.File.WriteAllBytes("/home/Reina/Downloads/grdm-bin/" + fileName + ".bin", bin);
            // return;

            Version = stream.ReadUInt16();
            Debug.Assert(Version is 3);

            IsGradientReversed = stream.ReadByte() != 0;
            IsGradientDithered = stream.ReadByte() != 0;

            Span<byte> imSpanKey = stackalloc byte[4];
            stream.ReadToSpan(imSpanKey);
            GradientInteropMethodKey = ParserUtil.ParseASCII(imSpanKey);

            GradientNameLength = stream.ReadUInt32();
            Span<byte> gradientName = new byte[GradientNameLength * 2];
            stream.ReadToSpan(gradientName);
            GradientName = ParserUtil.ParseBigUTF16(gradientName);

            ColorKeyCount = stream.ReadUInt16();
            ColorKeys = new ColorKeyV3[ColorKeyCount];
            for (var i = 0; ColorKeyCount > i; i += 1)
            {
                var colKey = new ColorKeyV3();
                colKey.Unknown = stream.ReadUInt16();
                colKey.KeyLocation = stream.ReadUInt16();
                colKey.Unknown2 = stream.ReadUInt16();
                colKey.MidLocation = stream.ReadUInt16();
                colKey.Unknown3 = stream.ReadUInt16();
                colKey.Red = stream.ReadUInt16();
                colKey.Green = stream.ReadUInt16();
                colKey.Blue = stream.ReadUInt16();
                colKey.Unknown4 = stream.ReadUInt32();
                ColorKeys[i] = colKey;
            }

            TransparencyKeyCount = stream.ReadUInt16();
            TransparencyKeys = new TransParencyKeyV3[TransparencyKeyCount];
            for (var i = 0; TransparencyKeyCount > i; i += 1)
            {
                var tpKey = new TransParencyKeyV3();
                tpKey.Unknown = stream.ReadUInt16();
                tpKey.KeyLocation = stream.ReadUInt16();
                tpKey.Unknown2 = stream.ReadUInt16();
                tpKey.MidLocation = stream.ReadUInt16();
                tpKey.Transparency = stream.ReadUInt16();
                TransparencyKeys[i] = tpKey;
            }

            Unknown = stream.ReadUInt16();
            Smoothens = stream.ReadUInt16();
        }
        [Serializable]
        public struct ColorKeyV3
        {
            public ushort Unknown;
            public ushort KeyLocation; // 0-4096
            public ushort Unknown2;
            public ushort MidLocation; // 0-100
            public ushort Unknown3;
            public ushort Red; // 0-ushort.max
            public ushort Green; // 0-ushort.max
            public ushort Blue; // 0-ushort.max
            public uint Unknown4;
        }
        [Serializable]
        public struct TransParencyKeyV3
        {
            public ushort Unknown;
            public ushort KeyLocation; // 0-4096
            public ushort Unknown2;
            public ushort MidLocation; // 0~100
            public ushort Transparency; // 0-255
        }

        /*
        グラデーションマップレイヤー。

        バージョンが3つ存在するらしい ... ?
        けれど仕様書にあるバージョンを確認したことはないため謎です。

        現行確認されているバージョンは 3 。
        */
    }
}
