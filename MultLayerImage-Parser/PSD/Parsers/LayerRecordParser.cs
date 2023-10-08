using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static net.rs64.PSD.parser.AdditionalLayerInformationParser;
using static net.rs64.PSD.parser.ChannelImageDataParser;


namespace net.rs64.PSD.parser
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

            public uint ExtraDataFieldLength;
            public LayerMaskAdjustmentLayerData LayerMaskAdjustmentLayerData;
            public LayerBlendingRangesData LayerBlendingRangesData;
            public string LayerName;
            public AdditionalLayerInfo[] AdditionalLayerInformation;
        }
        [Serializable]
        public class RectTangle
        {
            public int Top;
            public int Left;
            public int Bottom;
            public int Right;

            public int CalculateRawCompressLength()
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






        public static LayerRecord PaseLayerRecord(Stream stream)
        {
            var layerRecord = new LayerRecord
            {
                RectTangle = new RectTangle
                {
                    Top = stream.ReadByteToInt32(),
                    Left = stream.ReadByteToInt32(),
                    Bottom = stream.ReadByteToInt32(),
                    Right = stream.ReadByteToInt32(),
                },
                NumberOfChannels = stream.ReadByteToUInt16(),
            };

            var ChannelInformationList = new List<ChannelInformation>();
            for (int i = 0; layerRecord.NumberOfChannels > i; i += 1)
            {
                var channelInfo = new ChannelInformation()
                {
                    ChannelIDRawShort = stream.ReadByteToInt16(),
                    CorrespondingChannelDataLength = stream.ReadByteToInt32()
                };
                channelInfo.ChannelID = (ChannelInformation.ChannelIDEnum)channelInfo.ChannelIDRawShort;
                ChannelInformationList.Add(channelInfo);
            }
            layerRecord.ChannelInformationArray = ChannelInformationList.ToArray();

            if (!stream.ReadBytes(4).SequenceEqual(PSDLowLevelParser.OctBIMSignature)) { return layerRecord; }//throw new Exception(); }

            layerRecord.BlendModeKey = stream.ReadBytes(4).ParseUTF8();
            layerRecord.Opacity = stream.ReadByteToByte();
            layerRecord.Clipping = stream.ReadByteToByte();
            layerRecord.LayerFlag = (LayerRecord.LayerFlagEnum)stream.ReadByte();

            stream.ReadByte();

            layerRecord.ExtraDataFieldLength = stream.ReadByteToUInt32();

            var firstPos = stream.Position;

            var LayerMaskAdjustmentLayerData = new LayerMaskAdjustmentLayerData
            {
                DataSize = stream.ReadByteToUInt32()
            };

            if (LayerMaskAdjustmentLayerData.DataSize != 0)
            {
                LayerMaskAdjustmentLayerData.RectTangle = new RectTangle()
                {
                    Top = stream.ReadByteToInt32(),
                    Left = stream.ReadByteToInt32(),
                    Bottom = stream.ReadByteToInt32(),
                    Right = stream.ReadByteToInt32(),
                };

                LayerMaskAdjustmentLayerData.DefaultColor = stream.ReadByteToByte();
                LayerMaskAdjustmentLayerData.Flag = (LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag)stream.ReadByte();
                if (LayerMaskAdjustmentLayerData.Flag.HasFlag(LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag.UserOrVectorMasksHave))
                {
                    LayerMaskAdjustmentLayerData.MaskParameters = (LayerMaskAdjustmentLayerData.MaskParametersBitFlags)stream.ReadByte();
                    var maskParm = LayerMaskAdjustmentLayerData.MaskParameters.Value;
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.UserDensity))
                    {
                        LayerMaskAdjustmentLayerData.UserMaskDensity = stream.ReadByteToByte();
                    }
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.UserFeather))
                    {
                        LayerMaskAdjustmentLayerData.UserMaskFeather = stream.ReadByteToDouble();
                    }
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.VectorDensity))
                    {
                        LayerMaskAdjustmentLayerData.VectorMaskDensity = stream.ReadByteToByte();
                    }
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.VectorFeather))
                    {
                        LayerMaskAdjustmentLayerData.VectorMaskFeather = stream.ReadByteToDouble();
                    }
                }

                if (LayerMaskAdjustmentLayerData.DataSize == 20) { stream.ReadBytes(2); }
                else
                {
                    LayerMaskAdjustmentLayerData.RealFlag = (LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag)stream.ReadByte();
                    LayerMaskAdjustmentLayerData.RealUserMaskBackground = stream.ReadByteToByte();
                    LayerMaskAdjustmentLayerData.EnclosingLayerMask = new RectTangle()
                    {
                        Top = stream.ReadByteToInt32(),
                        Left = stream.ReadByteToInt32(),
                        Bottom = stream.ReadByteToInt32(),
                        Right = stream.ReadByteToInt32(),
                    };
                }

            }

            layerRecord.LayerMaskAdjustmentLayerData = LayerMaskAdjustmentLayerData;

            var layerBlendingRangesData = new LayerBlendingRangesData
            {
                Length = stream.ReadByteToUInt32()
            };
            if (layerBlendingRangesData.Length != 0)
            {
                var sourSourceAndDestinationRangeList = new List<LayerBlendingRangesData.SourceAndDestinationRange>();
                var sSADRStream = new MemoryStream(stream.ReadBytes(layerBlendingRangesData.Length));
                while (sSADRStream.Position < sSADRStream.Length)
                {
                    sourSourceAndDestinationRangeList.Add(new LayerBlendingRangesData.SourceAndDestinationRange()
                    {
                        CompositeGrayBlendSource1 = sSADRStream.ReadByteToByte(),
                        CompositeGrayBlendSource2 = sSADRStream.ReadByteToByte(),
                        CompositeGrayBlendSource3 = sSADRStream.ReadByteToByte(),
                        CompositeGrayBlendSource4 = sSADRStream.ReadByteToByte(),
                        CompositeGrayBlendDestinationRange = sSADRStream.ReadByteToUInt32()
                    }
                    );
                }
                layerBlendingRangesData.SourceAndDestinationRanges = sourSourceAndDestinationRangeList.ToArray();
            }
            layerRecord.LayerBlendingRangesData = layerBlendingRangesData;


            layerRecord.LayerName = ParserUtility.ReadPascalString(stream);

            var AdditionalLayerInformationBytes = stream.ReadBytes((uint)(layerRecord.ExtraDataFieldLength - (stream.Position - firstPos)));

            layerRecord.AdditionalLayerInformation = PaseAdditionalLayerInfos(new MemoryStream(AdditionalLayerInformationBytes));

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
            public RectTangle EnclosingLayerMask;
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