using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static net.rs64.MultiLayerImageParser.PSD.AdditionalLayerInformationParser;
using static net.rs64.MultiLayerImageParser.PSD.ChannelImageDataParser;


namespace net.rs64.MultiLayerImageParser.PSD
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

            var ChannelInformationList = new List<ChannelInformation>();
            for (int i = 0; layerRecord.NumberOfChannels > i; i += 1)
            {
                var channelInfo = new ChannelInformation()
                {
                    ChannelIDRawShort = stream.ReadInt16(),
                    CorrespondingChannelDataLength = stream.ReadInt32()
                };
                channelInfo.ChannelID = (ChannelInformation.ChannelIDEnum)channelInfo.ChannelIDRawShort;
                ChannelInformationList.Add(channelInfo);
            }
            layerRecord.ChannelInformationArray = ChannelInformationList.ToArray();

            if (!stream.ReadSubStream(4).Span.SequenceEqual(PSDLowLevelParser.OctBIMSignature)) { return layerRecord; }//throw new Exception(); }

            layerRecord.BlendModeKey = stream.ReadSubStream(4).Span.ParseUTF8();
            layerRecord.Opacity = stream.ReadByte();
            layerRecord.Clipping = stream.ReadByte();
            layerRecord.LayerFlag = (LayerRecord.LayerFlagEnum)stream.ReadByte();

            stream.ReadByte();

            layerRecord.ExtraDataFieldLength = stream.ReadUInt32();

            var firstPos = stream.Position;

            var LayerMaskAdjustmentLayerData = new LayerMaskAdjustmentLayerData
            {
                DataSize = stream.ReadUInt32()
            };

            if (LayerMaskAdjustmentLayerData.DataSize != 0)
            {
                LayerMaskAdjustmentLayerData.RectTangle = new RectTangle()
                {
                    Top = stream.ReadInt32(),
                    Left = stream.ReadInt32(),
                    Bottom = stream.ReadInt32(),
                    Right = stream.ReadInt32(),
                };

                LayerMaskAdjustmentLayerData.DefaultColor = stream.ReadByte();
                LayerMaskAdjustmentLayerData.Flag = (LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag)stream.ReadByte();
                if (LayerMaskAdjustmentLayerData.Flag.HasFlag(LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag.UserOrVectorMasksHave))
                {
                    LayerMaskAdjustmentLayerData.MaskParameters = (LayerMaskAdjustmentLayerData.MaskParametersBitFlags)stream.ReadByte();
                    var maskParm = LayerMaskAdjustmentLayerData.MaskParameters.Value;
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.UserDensity))
                    {
                        LayerMaskAdjustmentLayerData.UserMaskDensity = stream.ReadByte();
                    }
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.UserFeather))
                    {
                        LayerMaskAdjustmentLayerData.UserMaskFeather = stream.ReadDouble();
                    }
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.VectorDensity))
                    {
                        LayerMaskAdjustmentLayerData.VectorMaskDensity = stream.ReadByte();
                    }
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.VectorFeather))
                    {
                        LayerMaskAdjustmentLayerData.VectorMaskFeather = stream.ReadDouble();
                    }
                }

                if (LayerMaskAdjustmentLayerData.DataSize == 20) { stream.ReadSubStream(2); }
                else
                {
                    LayerMaskAdjustmentLayerData.RealFlag = (LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag)stream.ReadByte();
                    LayerMaskAdjustmentLayerData.RealUserMaskBackground = stream.ReadByte();
                    LayerMaskAdjustmentLayerData.EnclosingLayerMask = new RectTangle()
                    {
                        Top = stream.ReadInt32(),
                        Left = stream.ReadInt32(),
                        Bottom = stream.ReadInt32(),
                        Right = stream.ReadInt32(),
                    };
                }

            }

            layerRecord.LayerMaskAdjustmentLayerData = LayerMaskAdjustmentLayerData;

            var layerBlendingRangesData = new LayerBlendingRangesData
            {
                Length = stream.ReadUInt32()
            };
            if (layerBlendingRangesData.Length != 0)
            {
                var sourSourceAndDestinationRangeList = new List<LayerBlendingRangesData.SourceAndDestinationRange>();
                var sSADRStream = stream.ReadSubStream((int)layerBlendingRangesData.Length);
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
            layerRecord.LayerBlendingRangesData = layerBlendingRangesData;


            layerRecord.LayerName = ParserUtility.ReadPascalString(ref stream);

            var AdditionalLayerInformationSpan = stream.ReadSubStream((int)(layerRecord.ExtraDataFieldLength - (stream.Position - firstPos)));

            layerRecord.AdditionalLayerInformation = PaseAdditionalLayerInfos(AdditionalLayerInformationSpan);

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