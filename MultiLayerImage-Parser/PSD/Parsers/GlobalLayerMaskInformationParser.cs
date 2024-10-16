using System;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static class GlobalLayerMaskInformationParser
    {
        public static GlobalLayerMaskInfo PaseGlobalLayerMaskInformation(ref SubSpanStream stream)
        {
            var globalLayerMaskInfo = new GlobalLayerMaskInfo();
            globalLayerMaskInfo.GlobalLayerMaskInfoLength = stream.ReadUInt32();

            if (globalLayerMaskInfo.GlobalLayerMaskInfoLength == 0) { return globalLayerMaskInfo; }

            var globalLayerInfoStream = stream.ReadSubStream((int)globalLayerMaskInfo.GlobalLayerMaskInfoLength);

            globalLayerMaskInfo.OverlayColorSpace = globalLayerInfoStream.ReadUInt16();

            globalLayerMaskInfo.ColorR = globalLayerInfoStream.ReadUInt16();
            globalLayerMaskInfo.ColorG = globalLayerInfoStream.ReadUInt16();
            globalLayerMaskInfo.ColorB = globalLayerInfoStream.ReadUInt16();
            globalLayerMaskInfo.ColorA = globalLayerInfoStream.ReadUInt16();

            globalLayerMaskInfo.Opacity = globalLayerInfoStream.ReadUInt16();
            globalLayerMaskInfo.Kind = globalLayerInfoStream.ReadByte();


            return globalLayerMaskInfo;
        }

        [Serializable]
        public class GlobalLayerMaskInfo
        {
            public uint GlobalLayerMaskInfoLength;

            public ushort OverlayColorSpace;

            public ushort ColorR;
            public ushort ColorG;
            public ushort ColorB;
            public ushort ColorA;

            public ushort Opacity;
            public byte Kind;
        }

    }
}
