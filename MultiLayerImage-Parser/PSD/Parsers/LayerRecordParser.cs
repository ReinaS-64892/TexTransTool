using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static net.rs64.MultiLayerImageParser.PSD.AdditionalLayerInformationParser;
using static net.rs64.MultiLayerImageParser.PSD.ChannelImageDataParser;


namespace net.rs64.MultiLayerImageParser.PSD
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
            public AdditionalLayerInfo[] AdditionalLayerInformation;
        }
        [Serializable]
        internal class RectTangle
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

            var channelInformationList = new List<ChannelInformation>();
            for (int i = 0; layerRecord.NumberOfChannels > i; i += 1)
            {
                var channelInfo = new ChannelInformation()
                {
                    ChannelIDRawShort = stream.ReadInt16(),
                    CorrespondingChannelDataLength = stream.ReadInt32()
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

            stream.ReadByte();

            layerRecord.ExtraDataFieldLength = stream.ReadUInt32();

            var firstPos = stream.Position;

            var layerMaskAdjustmentLayerData = new LayerMaskAdjustmentLayerData
            {
                DataSize = stream.ReadUInt32()
            };

            if (layerMaskAdjustmentLayerData.DataSize != 0)
            {
                layerMaskAdjustmentLayerData.RectTangle = new RectTangle()
                {
                    Top = stream.ReadInt32(),
                    Left = stream.ReadInt32(),
                    Bottom = stream.ReadInt32(),
                    Right = stream.ReadInt32(),
                };

                layerMaskAdjustmentLayerData.DefaultColor = stream.ReadByte();
                layerMaskAdjustmentLayerData.Flag = (LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag)stream.ReadByte();
                if (layerMaskAdjustmentLayerData.Flag.HasFlag(LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag.UserOrVectorMasksHave))
                {
                    layerMaskAdjustmentLayerData.MaskParameters = (LayerMaskAdjustmentLayerData.MaskParametersBitFlags)stream.ReadByte();
                    var maskParm = layerMaskAdjustmentLayerData.MaskParameters.Value;
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.UserDensity))
                    {
                        layerMaskAdjustmentLayerData.UserMaskDensity = stream.ReadByte();
                    }
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.UserFeather))
                    {
                        layerMaskAdjustmentLayerData.UserMaskFeather = stream.ReadDouble();
                    }
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.VectorDensity))
                    {
                        layerMaskAdjustmentLayerData.VectorMaskDensity = stream.ReadByte();
                    }
                    if (maskParm.HasFlag(LayerMaskAdjustmentLayerData.MaskParametersBitFlags.VectorFeather))
                    {
                        layerMaskAdjustmentLayerData.VectorMaskFeather = stream.ReadDouble();
                    }
                }

                if (layerMaskAdjustmentLayerData.DataSize == 20) { stream.ReadSubStream(2); }
                else
                {
                    layerMaskAdjustmentLayerData.RealFlag = (LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag)stream.ReadByte();
                    layerMaskAdjustmentLayerData.RealUserMaskBackground = stream.ReadByte();
                    layerMaskAdjustmentLayerData.EnclosingLayerMask = new RectTangle()
                    {
                        Top = stream.ReadInt32(),
                        Left = stream.ReadInt32(),
                        Bottom = stream.ReadInt32(),
                        Right = stream.ReadInt32(),
                    };
                }

            }

            layerRecord.LayerMaskAdjustmentLayerData = layerMaskAdjustmentLayerData;

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

            var unicodeLayerName = layerRecord.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInformationParser.luni) as AdditionalLayerInformationParser.luni;
            if (unicodeLayerName != null) { layerRecord.LayerName = unicodeLayerName.LayerName; }

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
            public RectTangle EnclosingLayerMask;
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