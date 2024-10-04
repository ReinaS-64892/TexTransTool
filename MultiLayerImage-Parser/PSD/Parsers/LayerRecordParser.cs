using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser;


namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static class LayerRecordParser
    {

        [Serializable]
        internal class LayerRecord
        {
            public RectTangle RectTangle;
            public ushort NumberOfChannels;
            public ChannelInformation[] ChannelInformationArray;
            public string BlendModeKey;
            public byte Opacity;
            public byte Clipping;
            public LayerFlagEnum LayerFlag;
            [Flags]
            internal enum LayerFlagEnum
            {
                TransparencyProtected = 1,
                NotVisible = 2,
                Obsolete = 4,
                UsefulInformation4Bit = 8,
                NotDocPixelData = 16,
            }

            public uint ExtraDataFieldLength;
            public LayerMaskAdjustmentLayerData LayerMaskAdjustmentLayerData;
            public LayerBlendingRangesData LayerBlendingRangesData;
            public string LayerName;
            public AdditionalLayerInfo.AdditionalLayerInfoBase[] AdditionalLayerInformation;
        }
        [Serializable]
        internal class RectTangle
        {
            public int Top;
            public int Left;
            public int Bottom;
            public int Right;

            public int CalculateRectAreaSize()
            {
                int height = GetHeight();
                int width = GetWidth();
                return height * width;
            }

            public int GetWidth()
            {
                return Right - Left;
            }

            public int GetHeight()
            {
                return Bottom - Top;
            }
        }






        public static LayerRecord PaseLayerRecord(ref SubSpanStream stream)
        {
            var layerRecord = new LayerRecord
            {
                RectTangle = new RectTangle
                {
                    Top = stream.ReadInt32(),
                    Left = stream.ReadInt32(),
                    Bottom = stream.ReadInt32(),
                    Right = stream.ReadInt32(),
                },
                NumberOfChannels = stream.ReadUInt16(),
            };

            var channelInformationList = new List<ChannelInformation>();
            for (int i = 0; layerRecord.NumberOfChannels > i; i += 1)
            {
                var channelInfo = new ChannelInformation()
                {
                    ChannelIDRawShort = stream.ReadInt16(),
                    CorrespondingChannelDataLength = stream.ReadUInt32()
                };
                channelInfo.ChannelID = (ChannelInformation.ChannelIDEnum)channelInfo.ChannelIDRawShort;
                channelInformationList.Add(channelInfo);
            }
            layerRecord.ChannelInformationArray = channelInformationList.ToArray();

            if (!stream.ReadSubStream(4).Span.SequenceEqual(PSDLowLevelParser.OctBIMSignature)) { return layerRecord; }//throw new Exception(); }

            layerRecord.BlendModeKey = stream.ReadSubStream(4).Span.ParseUTF8();
            layerRecord.Opacity = stream.ReadByte();
            layerRecord.Clipping = stream.ReadByte();
            layerRecord.LayerFlag = (LayerRecord.LayerFlagEnum)stream.ReadByte();

            stream.ReadByte();//Filler

            layerRecord.ExtraDataFieldLength = stream.ReadUInt32();

            var extraDataStream = stream.ReadSubStream((int)layerRecord.ExtraDataFieldLength);

            var layerMaskAdjustmentLayerData = layerRecord.LayerMaskAdjustmentLayerData =
                new LayerMaskAdjustmentLayerData { DataSize = extraDataStream.ReadUInt32() };

            if (layerMaskAdjustmentLayerData.DataSize != 0)
            {

                layerMaskAdjustmentLayerData.RectTangle = new RectTangle()
                {
                    Top = extraDataStream.ReadInt32(),
                    Left = extraDataStream.ReadInt32(),
                    Bottom = extraDataStream.ReadInt32(),
                    Right = extraDataStream.ReadInt32(),
                };

                layerMaskAdjustmentLayerData.DefaultColor = extraDataStream.ReadByte();
                layerMaskAdjustmentLayerData.Flag = (LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag)extraDataStream.ReadByte();
                if (layerMaskAdjustmentLayerData.Flag.HasFlag(LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag.UserOrVectorMasksHave))
                {
                    layerMaskAdjustmentLayerData.MaskParameters = (LayerMaskAdjustmentLayerData.MaskParametersBitFlags)extraDataStream.ReadByte();
                    var maskParm = layerMaskAdjustmentLayerData.MaskParameters.Value;
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.UserDensity))
                        layerMaskAdjustmentLayerData.UserMaskDensity = extraDataStream.ReadByte();

                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.UserFeather))
                        layerMaskAdjustmentLayerData.UserMaskFeather = extraDataStream.ReadDouble();

                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.VectorDensity))
                        layerMaskAdjustmentLayerData.VectorMaskDensity = extraDataStream.ReadByte();

                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.VectorFeather))
                        layerMaskAdjustmentLayerData.VectorMaskFeather = extraDataStream.ReadDouble();

                }

                if (layerMaskAdjustmentLayerData.DataSize == 20) { extraDataStream.ReadSubStream(2); }
                else
                {
                    layerMaskAdjustmentLayerData.RealFlag = (LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag)extraDataStream.ReadByte();
                    layerMaskAdjustmentLayerData.RealUserMaskBackground = extraDataStream.ReadByte();
                    layerMaskAdjustmentLayerData.RealRectTangleLayerMask = new RectTangle()
                    {
                        Top = extraDataStream.ReadInt32(),
                        Left = extraDataStream.ReadInt32(),
                        Bottom = extraDataStream.ReadInt32(),
                        Right = extraDataStream.ReadInt32(),
                    };
                }
            }


            var layerBlendingRangesData = layerRecord.LayerBlendingRangesData =
                new LayerBlendingRangesData { Length = extraDataStream.ReadUInt32() };

            if (layerBlendingRangesData.Length != 0)
            {
                var sourSourceAndDestinationRangeList = new List<LayerBlendingRangesData.SourceAndDestinationRange>();
                var sSADRStream = extraDataStream.ReadSubStream((int)layerBlendingRangesData.Length);
                while (sSADRStream.Position < sSADRStream.Length)
                {
                    sourSourceAndDestinationRangeList.Add(new LayerBlendingRangesData.SourceAndDestinationRange()
                    {
                        CompositeGrayBlendSource1 = sSADRStream.ReadByte(),
                        CompositeGrayBlendSource2 = sSADRStream.ReadByte(),
                        CompositeGrayBlendSource3 = sSADRStream.ReadByte(),
                        CompositeGrayBlendSource4 = sSADRStream.ReadByte(),
                        CompositeGrayBlendDestinationRange = sSADRStream.ReadUInt32()
                    }
                    );
                }
                layerBlendingRangesData.SourceAndDestinationRanges = sourSourceAndDestinationRangeList.ToArray();
            }


            layerRecord.LayerName = ParserUtility.ReadPascalStringForPadding4Byte(ref extraDataStream);
            layerRecord.AdditionalLayerInformation = AdditionalLayerInformationParser.PaseAdditionalLayerInfos(extraDataStream);

            var unicodeLayerName = layerRecord.AdditionalLayerInformation.OfType<luni>().FirstOrDefault();
            if (unicodeLayerName is not null) { layerRecord.LayerName = unicodeLayerName.LayerName; }

            return layerRecord;
        }

        [Serializable]
        internal class LayerMaskAdjustmentLayerData
        {
            public uint DataSize;
            public RectTangle RectTangle;
            public byte DefaultColor;

            public MaskOrAdjustmentFlag Flag;
            [Flags]
            internal enum MaskOrAdjustmentFlag
            {
                PosRelToLayer = 1,
                MaskDisabled = 2,
                InvertMask = 4,
                UserMaskActuallyCame = 8,
                UserOrVectorMasksHave = 16,
            }
            public MaskParametersBitFlags? MaskParameters;
            [Flags]
            internal enum MaskParametersBitFlags
            {
                UserDensity = 1,
                UserFeather = 2,
                VectorDensity = 4,
                VectorFeather = 8,
            }
            //上のフラグに依存する
            public byte? UserMaskDensity;
            public double? UserMaskFeather;
            public byte? VectorMaskDensity;
            public double? VectorMaskFeather;

            // 2byte Padding Or...
            public MaskOrAdjustmentFlag? RealFlag;
            public byte? RealUserMaskBackground;
            public RectTangle RealRectTangleLayerMask;
        }
        [Serializable]
        internal class LayerBlendingRangesData
        {
            public uint Length;
            public SourceAndDestinationRange[] SourceAndDestinationRanges;
            [Serializable]
            internal class SourceAndDestinationRange
            {
                public byte CompositeGrayBlendSource1;
                public byte CompositeGrayBlendSource2;
                public byte CompositeGrayBlendSource3;
                public byte CompositeGrayBlendSource4;
                public uint CompositeGrayBlendDestinationRange;
            }
        }

    }
}
