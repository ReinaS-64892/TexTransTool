using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.MultiLayerImage.LayerData;
using UnityEngine;
using net.rs64.TexTransCore;
using static net.rs64.MultiLayerImage.Parser.PSD.PSDLowLevelParser.PSDLowLevelData;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser;
using static net.rs64.MultiLayerImage.Parser.PSD.LayerRecordParser;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.BlendTexture;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using LayerMask = net.rs64.MultiLayerImage.LayerData.LayerMaskData;
using System.Buffers;
using Unity.Collections;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser.ChannelInformation;
using static net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo.lsct;
using net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static class PSDHighLevelParser
    {
        public static PSDHighLevelData Parse(PSDLowLevelParser.PSDLowLevelData levelData)
        {
            var psd = new PSDHighLevelData
            {
                Size = new Vector2Int((int)levelData.width, (int)levelData.height),
                Depth = levelData.Depth,
                channels = levelData.channels,
                RootLayers = new List<AbstractLayerData>()
            };

            var imageDataQueue = new Queue<ChannelImageData>(levelData.LayerInfo.ChannelImageData);
            var imageRecordQueue = new Queue<LayerRecord>(levelData.LayerInfo.LayerRecords);

            ParseAsLayers(psd.RootLayers, imageRecordQueue, imageDataQueue);

            return psd;
        }

        private static void ParseAsLayers(List<AbstractLayerData> rootLayers, Queue<LayerRecord> imageRecordQueue, Queue<ChannelImageData> imageDataQueue)
        {
            while (imageRecordQueue.Count != 0)
            {
                var record = imageRecordQueue.Dequeue();

                var sectionDividerSetting = record.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInfo.lsct) as AdditionalLayerInfo.lsct;
                if (sectionDividerSetting != null && sectionDividerSetting.SelectionDividerType == AdditionalLayerInfo.lsct.SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    rootLayers.Add(ParseLayerFolder(record, imageRecordQueue, imageDataQueue));
                }
                else
                {
                    rootLayers.Add(ParseRasterLayer(record, imageDataQueue));
                }

            }
        }

        private static LayerFolderData ParseLayerFolder(LayerRecord record, Queue<LayerRecord> imageRecordQueue, Queue<ChannelImageData> imageDataQueue)
        {
            var layerFolder = new LayerFolderData();
            layerFolder.Layers = new List<AbstractLayerData>();

            _ = DeuceChannelInfoAndImage(record, imageDataQueue);

            while (imageRecordQueue.Count != 0)
            {
                var PeekRecord = imageRecordQueue.Peek();

                var debugName = PeekRecord.LayerName;

                var PeekSectionDividerSetting = PeekRecord.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInfo.lsct) as AdditionalLayerInfo.lsct;

                if (PeekSectionDividerSetting == null)
                { layerFolder.Layers.Add(ParseRasterLayer(imageRecordQueue.Dequeue(), imageDataQueue)); }
                else if (PeekSectionDividerSetting.SelectionDividerType == SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    layerFolder.Layers.Add(ParseLayerFolder(imageRecordQueue.Dequeue(), imageRecordQueue, imageDataQueue));
                }
                else if (PeekSectionDividerSetting.SelectionDividerType == SelectionDividerTypeEnum.OpenFolder
                || PeekSectionDividerSetting.SelectionDividerType == SelectionDividerTypeEnum.ClosedFolder)
                {
                    break;
                }
                else
                {
                    Debug.Log("AnyOther???" + PeekRecord.LayerName);
                }

            }
            var EndFolderRecord = imageRecordQueue.Dequeue();
            var endChannelInfoAndImage = DeuceChannelInfoAndImage(EndFolderRecord, imageDataQueue);
            layerFolder.CopyFromRecord(EndFolderRecord, endChannelInfoAndImage);

            var lsct = EndFolderRecord.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInfo.lsct) as AdditionalLayerInfo.lsct;
            var BlendModeKeyEnum = PSDLayer.BlendModeKeyToEnum(lsct.BlendModeKey);
            layerFolder.BlendTypeKey = PSDLayer.ConvertBlendType(BlendModeKeyEnum).ToString();
            layerFolder.PassThrough = BlendModeKeyEnum == PSDBlendMode.PassThrough;


            return layerFolder;
        }
        private static AbstractLayerData ParseRasterLayer(LayerRecord record, Queue<ChannelImageData> imageDataQueue)
        {
            var channelInfoAndImage = DeuceChannelInfoAndImage(record, imageDataQueue);

            if (TryParseSpecialLayer(record, channelInfoAndImage, out var abstractLayerData)) { return abstractLayerData; }

            if (!channelInfoAndImage.ContainsKey(ChannelIDEnum.Red) || !channelInfoAndImage.ContainsKey(ChannelIDEnum.Blue) || !channelInfoAndImage.ContainsKey(ChannelIDEnum.Green)
            || record.RectTangle.CalculateRawCompressLength() == 0) { var emptyData = new RasterLayerData(); emptyData.CopyFromRecord(record, channelInfoAndImage); return emptyData; }

            var rasterLayer = new RasterLayerData();
            rasterLayer.CopyFromRecord(record, channelInfoAndImage);

            rasterLayer.RasterTexture = ParseRasterImage(record, channelInfoAndImage);

            return rasterLayer;
        }
        internal static bool TryParseSpecialLayer(LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage, out AbstractLayerData abstractLayerData)
        {
            var addLayerInfoTypes = record.AdditionalLayerInformation.Select(i => i.GetType()).ToHashSet();
            var spPair = SpecialParserDict.FirstOrDefault(i => addLayerInfoTypes.Contains(i.Key));

            if (spPair.Key == null || spPair.Value == null) { abstractLayerData = null; return false; }

            abstractLayerData = spPair.Value.Invoke(record, channelInfoAndImage);
            return true;
        }
        internal delegate AbstractLayerData SpecialLayerParser(LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage);
        internal static Dictionary<Type, SpecialLayerParser> SpecialParserDict = new()
        {
            {typeof(AdditionalLayerInfo.hue2),SpecialHueLayer},
            {typeof(AdditionalLayerInfo.hueOld),SpecialHueLayer},
            {typeof(AdditionalLayerInfo.SoCo), SpecialSolidColorLayer},
            {typeof(AdditionalLayerInfo.levl), SpecialLevelLayer},
        };

        private static AbstractLayerData SpecialSolidColorLayer(LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var solidColorData = new SolidColorLayerData();
            var soCo = record.AdditionalLayerInformation.First(i => i is AdditionalLayerInfo.SoCo) as AdditionalLayerInfo.SoCo;

            solidColorData.CopyFromRecord(record, channelInfoAndImage);
            solidColorData.Color = soCo.Color;

            return solidColorData;
        }

        internal static AbstractLayerData SpecialHueLayer(LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var hueData = new HSVAdjustmentLayerData();
            var hue = record.AdditionalLayerInformation.First(i => i is AdditionalLayerInfo.hue) as AdditionalLayerInfo.hue;

            if (hue.Colorization) { Debug.Log($"Colorization of {record.LayerName} is no supported"); }

            hueData.CopyFromRecord(record, channelInfoAndImage);

            hueData.Hue = hue.Hue / (float)(hue.IsOld ? 100 : 180);
            hueData.Saturation = hue.Saturation / 100f;
            hueData.Lightness = hue.Lightness / 100f;

            return hueData;
        }

        internal static AbstractLayerData SpecialLevelLayer(LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var levelData = new LevelAdjustmentLayerData();
            var levl = record.AdditionalLayerInformation.First(i => i is AdditionalLayerInfo.levl) as AdditionalLayerInfo.levl;

            levelData.CopyFromRecord(record, channelInfoAndImage);

            levelData.RGB = Convert(levl.RGB);
            levelData.Red = Convert(levl.Red);
            levelData.Green = Convert(levl.Green);
            levelData.Blue = Convert(levl.Blue);

            return levelData;

            static LevelAdjustmentLayerData.LevelData Convert(levl.LevelData levelData)
            {
                var data = new LevelAdjustmentLayerData.LevelData();

                data.InputFloor = levelData.InputFloor / 255f;
                data.InputCeiling = levelData.InputCeiling / 255f;
                data.OutputFloor = levelData.OutputFloor / 255f;
                data.OutputCeiling = levelData.OutputCeiling / 255f;
                data.Gamma = levelData.Gamma * 0.01f;

                return data;
            }
        }

        private static ImportRasterImageData ParseRasterImage(LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            if (!channelInfoAndImage.ContainsKey(ChannelIDEnum.Red)) { return null; }
            if (!channelInfoAndImage.ContainsKey(ChannelIDEnum.Blue)) { return null; }
            if (!channelInfoAndImage.ContainsKey(ChannelIDEnum.Green)) { return null; }
            if (record.RectTangle.CalculateRawCompressLength() == 0) { return null; }

            var importedRaster = new PSDImportedRasterImageData();
            importedRaster.RectTangle = record.RectTangle;
            importedRaster.R = channelInfoAndImage[ChannelIDEnum.Red];
            importedRaster.G = channelInfoAndImage[ChannelIDEnum.Green];
            importedRaster.B = channelInfoAndImage[ChannelIDEnum.Blue];
            if (channelInfoAndImage.ContainsKey(ChannelIDEnum.Transparency)) { importedRaster.A = channelInfoAndImage[ChannelIDEnum.Transparency]; }

            return importedRaster;
        }
        internal static LayerMask ParseLayerMask(LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            if (!channelInfoAndImage.ContainsKey(ChannelIDEnum.UserLayerMask)) { return null; }
            //サイズゼロで一色のマスクが存在することがあるっぽくて、画像のあるなしでは判別してはいけないらしい。
            // if (record.LayerMaskAdjustmentLayerData.RectTangle.CalculateRawCompressLength() == 0) { return null; }


            var importedMask = new PSDImportedRasterMaskImageData();
            importedMask.RectTangle = record.LayerMaskAdjustmentLayerData.RectTangle;
            importedMask.MaskImage = channelInfoAndImage[ChannelIDEnum.UserLayerMask];
            importedMask.DefaultValue = record.LayerMaskAdjustmentLayerData.DefaultColor;

            var LayerMask = new LayerMask();
            LayerMask.LayerMaskDisabled = record.LayerMaskAdjustmentLayerData.Flag.HasFlag(LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag.MaskDisabled);
            LayerMask.MaskTexture = importedMask;

            return LayerMask;
        }

        private static Dictionary<ChannelIDEnum, ChannelImageData> DeuceChannelInfoAndImage(LayerRecord record, Queue<ChannelImageData> imageDataQueue)
        {
            var channelInfoAndImage = new Dictionary<ChannelIDEnum, ChannelImageData>();
            foreach (var item in record.ChannelInformationArray)
            {
                channelInfoAndImage.Add(item.ChannelID, imageDataQueue.Dequeue());
            }
            return channelInfoAndImage;
        }

        public static LowMap<Color32> DrawOffsetEvaluateTexture(
            LowMap<Color32> targetTexture,
            Vector2Int texturePivot,
            Vector2Int canvasSize,
            Color? DefaultColor
        )
        {
            var RightUpPos = texturePivot + targetTexture.MapSize;
            var Pivot = texturePivot;
            if (RightUpPos != canvasSize || Pivot != Vector2Int.zero)
            {
                return TextureOffset(targetTexture, canvasSize, Pivot, DefaultColor);
            }
            else
            {
                return targetTexture;
            }
        }

        public static LowMap<Color32> TextureOffset(LowMap<Color32> texture, Vector2Int TargetSize, Vector2Int Pivot, Color32? DefaultColor)
        {
            var sTex2D = texture;
            var tTex2D = new LowMap<Color32>(new NativeArray<Color32>(TargetSize.x * TargetSize.y, Allocator.TempJob), TargetSize.x, TargetSize.y);
            var initColor = DefaultColor.HasValue ? DefaultColor.Value : new Color32(0, 0, 0, 0);
            tTex2D.Array.Fill(initColor);


            var xStart = Mathf.Max(-Pivot.x, 0);
            var xEnd = Mathf.Min(Pivot.x + sTex2D.Width, TargetSize.x) - Pivot.x;
            var xLength = xEnd - xStart;

            var yStart = Mathf.Max(-Pivot.y, 0);
            var yEnd = Mathf.Min(Pivot.y + sTex2D.MapSize.y, TargetSize.y) - Pivot.y;

            if (xLength < 0)
            {
                texture.Dispose();
                return tTex2D;
            }


            for (var yi = yStart; yEnd > yi; yi += 1)
            {
                var sPos = new Vector2Int(xStart, yi);
                var sSpan = sTex2D.Array.Slice(TwoDimensionalMap<Color32>.TwoDToOneDIndex(sPos, sTex2D.Width), xLength);
                var tSpan = tTex2D.Array.Slice(TwoDimensionalMap<Color32>.TwoDToOneDIndex(sPos + Pivot, tTex2D.Width), xLength);
                sSpan.CopyTo(tSpan);
            }
            texture.Dispose();
            return tTex2D;
        }

    }

    [Serializable]
    internal class PSDHighLevelData
    {
        public Vector2Int Size;
        public ushort Depth;
        public ushort channels;
        public List<AbstractLayerData> RootLayers;

        public static explicit operator CanvasData(PSDHighLevelData hData) => new CanvasData() { Size = hData.Size, RootLayers = hData.RootLayers };
    }
}
