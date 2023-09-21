using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static net.rs64.PSD.parser.PSDLowLevelParser.PSDLowLevelData;

namespace net.rs64.PSD.parser
{
    public static class PSDHighLevelParser
    {
        public static PSDHighLevelData Pase(PSDLowLevelParser.PSDLowLevelData levelData)
        {
            var PSD = new PSDHighLevelData
            {
                Size = new Vector2Int((int)levelData.width, (int)levelData.height),
                Depth = levelData.Depth,
                channels = levelData.channels
            };

            var rootLayers = new List<AbstractLayer>();
            var imageDataQueue = new Queue<ChannelImageDataParser.ChannelImageData>(levelData.LayerInfo.ChannelImageData);
            var imageRecordQueue = new Queue<LayerRecordParser.LayerRecord>(levelData.LayerInfo.LayerRecords);

            var textures = ParseAsLayers(PSD.Size, PSD.Depth, rootLayers, imageRecordQueue, imageDataQueue);

            PSD.RootLayers = rootLayers;
            PSD.Texture2Ds = textures;
            return PSD;
        }

        private static List<Texture2D> ParseAsLayers(Vector2Int Size, int Depth, List<AbstractLayer> rootLayers, Queue<LayerRecordParser.LayerRecord> imageRecordQueue, Queue<ChannelImageDataParser.ChannelImageData> imageDataQueue)
        {
            var texture2DList = new List<Texture2D>();
            while (imageRecordQueue.Count != 0)
            {
                var record = imageRecordQueue.Dequeue();

                var sectionDividerSetting = record.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInformationParser.lsct) as AdditionalLayerInformationParser.lsct;
                if (sectionDividerSetting != null && sectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    rootLayers.Add(ParseLayerFolder(record, Depth, imageRecordQueue, imageDataQueue, texture2DList));
                }
                else
                {
                    rootLayers.Add(ParseRasterLayer(record, Depth, imageDataQueue, texture2DList));
                }

            }
            return texture2DList;
        }

        private static LayerFolder ParseLayerFolder(LayerRecordParser.LayerRecord record, int depth, Queue<LayerRecordParser.LayerRecord> imageRecordQueue, Queue<ChannelImageDataParser.ChannelImageData> imageDataQueue, List<Texture2D> texture2DList)
        {
            var layerFolder = new LayerFolder();
            // layerFolder.CopyFromRecord(record);
            layerFolder.Layers = new List<AbstractLayer>();
            _ = DeuceChannelInfoAndImage(record, imageDataQueue);
            // SetGenerateLayerMask(record, depth, layerFolder, channelInfoAndImage, texture2DList);
            while (imageRecordQueue.Count != 0)
            {
                var PeekRecord = imageRecordQueue.Peek();


                var PeekSectionDividerSetting = PeekRecord.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInformationParser.lsct) as AdditionalLayerInformationParser.lsct;

                if (PeekSectionDividerSetting == null)
                {
                    layerFolder.Layers.Add(ParseRasterLayer(imageRecordQueue.Dequeue(), depth, imageDataQueue, texture2DList));
                }
                else if (PeekSectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    layerFolder.Layers.Add(ParseLayerFolder(imageRecordQueue.Dequeue(), depth, imageRecordQueue, imageDataQueue, texture2DList));
                }
                else if (PeekSectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.OpenFolder
                || PeekSectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.ClosedFolder)
                {
                    break;
                }

            }
            var EndFolderRecord = imageRecordQueue.Dequeue();
            layerFolder.CopyFromRecord(EndFolderRecord);
            var endChannelInfoAndImage = DeuceChannelInfoAndImage(EndFolderRecord, imageDataQueue);
            SetGenerateLayerMask(EndFolderRecord, depth, layerFolder, endChannelInfoAndImage, texture2DList);
            return layerFolder;
        }

        private static RasterLayer ParseRasterLayer(LayerRecordParser.LayerRecord record, int depth, Queue<ChannelImageDataParser.ChannelImageData> imageDataQueue, List<Texture2D> texture2DList)
        {
            var rasterLayer = new RasterLayer();
            rasterLayer.CopyFromRecord(record);
            var channelInfoAndImage = DeuceChannelInfoAndImage(record, imageDataQueue);

            if (channelInfoAndImage.ContainsKey(ChannelImageDataParser.ChannelInformation.ChannelIDEnum.Red)
            && channelInfoAndImage.ContainsKey(ChannelImageDataParser.ChannelInformation.ChannelIDEnum.Blue)
            && channelInfoAndImage.ContainsKey(ChannelImageDataParser.ChannelInformation.ChannelIDEnum.Green)
            && record.RectTangle.CalculateRawCompressLength() != 0
            )
            {

                var redImage = DirectionConvert(channelInfoAndImage[ChannelImageDataParser.ChannelInformation.ChannelIDEnum.Red].ImageData, record.RectTangle);
                var blueImage = DirectionConvert(channelInfoAndImage[ChannelImageDataParser.ChannelInformation.ChannelIDEnum.Blue].ImageData, record.RectTangle);
                var greenImage = DirectionConvert(channelInfoAndImage[ChannelImageDataParser.ChannelInformation.ChannelIDEnum.Green].ImageData, record.RectTangle);

                var pixels = new Color32[record.RectTangle.GetWidth() * record.RectTangle.GetHeight()];
                if (channelInfoAndImage.ContainsKey(ChannelImageDataParser.ChannelInformation.ChannelIDEnum.Transparency))
                {
                    var alphaImage = DirectionConvert(channelInfoAndImage[ChannelImageDataParser.ChannelInformation.ChannelIDEnum.Transparency].ImageData, record.RectTangle);
                    for (var i = 1; pixels.Length > i; i += 1)
                    {
                        pixels[i] = new Color32(redImage[i], greenImage[i], blueImage[i], alphaImage[i]);
                    }
                }
                else
                {
                    for (var i = 1; pixels.Length > i; i += 1)
                    {
                        pixels[i] = new Color32(redImage[i], greenImage[i], blueImage[i], byte.MaxValue);
                    }
                }
                var tex2D = new Texture2D(record.RectTangle.GetWidth(), record.RectTangle.GetHeight(), DepthToFormat(depth), false);
                tex2D.SetPixels32(pixels);
                tex2D.Apply();
                tex2D.name = record.LayerName + "_Tex";
                rasterLayer.RasterTexture = tex2D;
                rasterLayer.TexturePivot = new Vector2Int(record.RectTangle.Bottom, record.RectTangle.Left);
                texture2DList.Add(tex2D);
            }

            SetGenerateLayerMask(record, depth, rasterLayer, channelInfoAndImage, texture2DList);

            return rasterLayer;
        }

        private static void SetGenerateLayerMask(LayerRecordParser.LayerRecord record, int depth, AbstractLayer rasterLayer, Dictionary<ChannelImageDataParser.ChannelInformation.ChannelIDEnum, ChannelImageDataParser.ChannelImageData> channelInfoAndImage, List<Texture2D> texture2DList)
        {
            if (!channelInfoAndImage.ContainsKey(ChannelImageDataParser.ChannelInformation.ChannelIDEnum.UserLayerMask)) { return; }
            if (record.LayerMaskAdjustmentLayerData.RectTangle.CalculateRawCompressLength() == 0) { return; }

            var maskImage = channelInfoAndImage[ChannelImageDataParser.ChannelInformation.ChannelIDEnum.UserLayerMask].ImageData;
            maskImage = DirectionConvert(maskImage, record.LayerMaskAdjustmentLayerData.RectTangle);

            var pixels = new Color32[record.LayerMaskAdjustmentLayerData.RectTangle.GetWidth() * record.LayerMaskAdjustmentLayerData.RectTangle.GetHeight()];
            for (var i = 1; pixels.Length > i; i += 1)
            {
                try
                {
                    pixels[i] = new Color32(maskImage[i], maskImage[i], maskImage[i], maskImage[i]);
                }
                catch
                {
                    pixels[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                }
            }
            var tex2D = new Texture2D(record.LayerMaskAdjustmentLayerData.RectTangle.GetWidth(), record.LayerMaskAdjustmentLayerData.RectTangle.GetHeight(), DepthToFormat(depth), false);
            tex2D.SetPixels32(pixels);
            tex2D.Apply();
            tex2D.name = record.LayerName + "_MaskTex";
            rasterLayer.LayerMask = new LayerMask() { MaskTexture = tex2D };
            var maskDisabled = record.LayerMaskAdjustmentLayerData.Flag.HasFlag(LayerRecordParser.LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag.MaskDisabled);
            rasterLayer.LayerMask.LayerMaskDisabled = maskDisabled;
            rasterLayer.LayerMask.MaskPivot = new Vector2Int(record.LayerMaskAdjustmentLayerData.RectTangle.Bottom, record.LayerMaskAdjustmentLayerData.RectTangle.Left);
            texture2DList.Add(tex2D);

        }

        private static Dictionary<ChannelImageDataParser.ChannelInformation.ChannelIDEnum, ChannelImageDataParser.ChannelImageData> DeuceChannelInfoAndImage(LayerRecordParser.LayerRecord record, Queue<ChannelImageDataParser.ChannelImageData> imageDataQueue)
        {
            var channelInfoAndImage = new Dictionary<ChannelImageDataParser.ChannelInformation.ChannelIDEnum, ChannelImageDataParser.ChannelImageData>();
            foreach (var item in record.ChannelInformationArray)
            {
                channelInfoAndImage.Add(item.ChannelID, imageDataQueue.Dequeue());
            }
            return channelInfoAndImage;
        }
        private static byte[] DirectionConvert(byte[] imageData, LayerRecordParser.RectTangle rectTangle)
        {
            return DirectionSlice(imageData, rectTangle.GetWidth(), rectTangle.GetHeight());
        }
        private static byte[] DirectionSlice(byte[] imageData, int width, int height)
        {
            var bytes = new byte[height][];
            for (var i = 0; height > i; i += 1)
            {
                var startIndex = i * width;

                bytes[i] = imageData.AsSpan(startIndex, width).ToArray();

            }
            return bytes.Reverse().SelectMany(I => I).ToArray();
        }
        public static TextureFormat DepthToFormat(int depth)
        {
            if (depth <= 8)
            {
                return TextureFormat.RGBA32;
            }
            else if (depth <= 16)
            {
                return TextureFormat.RGBA64;
            }
            else
            {
                return TextureFormat.RGBAFloat;
            }

        }
    }

    [Serializable]
    public class PSDHighLevelData
    {
        public Vector2Int Size;
        public ushort Depth;
        public ushort channels;
        public List<AbstractLayer> RootLayers;
        public List<Texture2D> Texture2Ds;
    }
}