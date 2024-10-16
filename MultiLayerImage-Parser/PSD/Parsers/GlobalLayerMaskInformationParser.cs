using System;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static class GlobalLayerMaskInformationParser
    {
        public static GlobalLayerMaskInfo PaseGlobalLayerMaskInformation(BinarySectionStream stream)
        {
            var globalLayerMaskInfo = new GlobalLayerMaskInfo();
            var length = stream.ReadUInt32();
            globalLayerMaskInfo.GlobalLayerMaskInfoAddress = stream.PeekToAddress(length);

            if (globalLayerMaskInfo.GlobalLayerMaskInfoAddress.Length == 0) { return globalLayerMaskInfo; }

            var globalLayerInfoStream = stream.ReadSubSection(globalLayerMaskInfo.GlobalLayerMaskInfoAddress.Length);

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
            public BinaryAddress GlobalLayerMaskInfoAddress;

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
