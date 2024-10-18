using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser;


namespace net.rs64.MultiLayerImage.Parser.PSD
{
    public static class LayerRecordParser
    {

        [Serializable]
        public class LayerRecord
        {
            public RectTangle RectTangle;
            public ushort NumberOfChannels;
            public ChannelInformation[] ChannelInformationArray;
            public string BlendModeKey;
            public byte Opacity;
            public byte Clipping;
            public LayerFlagEnum LayerFlag;
            [Flags]
            public enum LayerFlagEnum
            {
                TransparencyProtected = 1,
                NotVisible = 2,
                Obsolete = 4,
                UsefulInformation4Bit = 8,
                NotDocPixelData = 16,
            }

            public BinaryAddress ExtraDataField;
            public LayerMaskAdjustmentLayerData LayerMaskAdjustmentLayerData;
            public LayerBlendingRangesData LayerBlendingRangesData;
            public string LayerName;
            public AdditionalLayerInfo.AdditionalLayerInfoBase[] AdditionalLayerInformation;
        }
        [Serializable]
        public class RectTangle
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






        public static LayerRecord PaseLayerRecord(bool isPSB, BinarySectionStream stream)
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
                    CorrespondingChannelDataLength = isPSB is false ? stream.ReadUInt32() : stream.ReadUInt64()
                };
                channelInfo.ChannelID = (ChannelInformation.ChannelIDEnum)channelInfo.ChannelIDRawShort;
                channelInformationList.Add(channelInfo);
            }
            layerRecord.ChannelInformationArray = channelInformationList.ToArray();

            if (stream.Signature(PSDLowLevelParser.OctBIMSignature) is false) { return layerRecord; }//throw new Exception(); }

            Span<byte> blKeySpan = stackalloc byte[4];
            stream.ReadToSpan(blKeySpan);
            layerRecord.BlendModeKey = blKeySpan.ParseASCII();

            layerRecord.Opacity = stream.ReadByte();
            layerRecord.Clipping = stream.ReadByte();
            layerRecord.LayerFlag = (LayerRecord.LayerFlagEnum)stream.ReadByte();

            stream.ReadByte();//Filler

            var extraDataFieldLength = stream.ReadUInt32();
            layerRecord.ExtraDataField = stream.PeekToAddress(extraDataFieldLength);

            var extraDataStream = stream.ReadSubSection(layerRecord.ExtraDataField.Length);

            var layerMaskAdjustmentLayerData = layerRecord.LayerMaskAdjustmentLayerData =
                new LayerMaskAdjustmentLayerData { DataSize = extraDataStream.ReadUInt32() };

            if (layerMaskAdjustmentLayerData.DataSize != 0)
            {
                var lmStream = extraDataStream.ReadSubSection(layerMaskAdjustmentLayerData.DataSize);

                layerMaskAdjustmentLayerData.RectTangle = new RectTangle()
                {
                    Top = lmStream.ReadInt32(),
                    Left = lmStream.ReadInt32(),
                    Bottom = lmStream.ReadInt32(),
                    Right = lmStream.ReadInt32(),
                };

                layerMaskAdjustmentLayerData.DefaultColor = lmStream.ReadByte();
                layerMaskAdjustmentLayerData.Flag = (LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag)lmStream.ReadByte();
                if (layerMaskAdjustmentLayerData.Flag.HasFlag(LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag.UserOrVectorMasksHave))
                {
                    layerMaskAdjustmentLayerData.MaskParameters = (LayerMaskAdjustmentLayerData.MaskParametersBitFlags)lmStream.ReadByte();
                    var maskParm = layerMaskAdjustmentLayerData.MaskParameters.Value;
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.UserDensity))
                        layerMaskAdjustmentLayerData.UserMaskDensity = lmStream.ReadByte();

                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.UserFeather))
                        layerMaskAdjustmentLayerData.UserMaskFeather = lmStream.ReadDouble();

                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.VectorDensity))
                        layerMaskAdjustmentLayerData.VectorMaskDensity = lmStream.ReadByte();

                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.VectorFeather))
                        layerMaskAdjustmentLayerData.VectorMaskFeather = lmStream.ReadDouble();

                }

                if (layerMaskAdjustmentLayerData.DataSize == 20) { /*lmStream.ReadSubStream(); // 上段のストリームが何とかしてるのでこのストリームを最後まで読む必要はなくスキップ*/ }
                else
                {
                    layerMaskAdjustmentLayerData.RealFlag = (LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag)lmStream.ReadByte();
                    layerMaskAdjustmentLayerData.RealUserMaskBackground = lmStream.ReadByte();
                    layerMaskAdjustmentLayerData.RealRectTangleLayerMask = new RectTangle()
                    {
                        Top = lmStream.ReadInt32(),
                        Left = lmStream.ReadInt32(),
                        Bottom = lmStream.ReadInt32(),
                        Right = lmStream.ReadInt32(),
                    };
                }
            }


            var layerBlendingRangesData = layerRecord.LayerBlendingRangesData =
                new LayerBlendingRangesData { Length = extraDataStream.ReadUInt32() };

            if (layerBlendingRangesData.Length != 0)
            {
                var sourSourceAndDestinationRangeList = new List<LayerBlendingRangesData.SourceAndDestinationRange>();
                var sSADRStream = extraDataStream.ReadSubSection(layerBlendingRangesData.Length);
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


            layerRecord.LayerName = ParserUtility.ReadPascalStringForPadding4Byte(extraDataStream);
            layerRecord.AdditionalLayerInformation = AdditionalLayerInformationParser.PaseAdditionalLayerInfos(isPSB, extraDataStream);

            return layerRecord;
        }

        [Serializable]
        public class LayerMaskAdjustmentLayerData
        {
            public uint DataSize;
            public RectTangle RectTangle;
            public byte DefaultColor;

            public MaskOrAdjustmentFlag Flag;
            [Flags]
            public enum MaskOrAdjustmentFlag
            {
                PosRelToLayer = 1,
                MaskDisabled = 2,
                InvertMask = 4,
                UserMaskActuallyCame = 8,
                UserOrVectorMasksHave = 16,
            }
            public MaskParametersBitFlags? MaskParameters;
            [Flags]
            public enum MaskParametersBitFlags
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
        public class LayerBlendingRangesData
        {
            public uint Length;
            public SourceAndDestinationRange[] SourceAndDestinationRanges;
            [Serializable]
            public class SourceAndDestinationRange
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
