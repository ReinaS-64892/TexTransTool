using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Layer;
using UnityEngine;
using net.rs64.TexTransCore;
using static net.rs64.PSD.parser.PSDLowLevelParser.PSDLowLevelData;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.BlendTexture;

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

            var rootLayers = new List<AbstractLayerData>();
            var imageDataQueue = new Queue<ChannelImageDataParser.ChannelImageData>(levelData.LayerInfo.ChannelImageData);
            var imageRecordQueue = new Queue<LayerRecordParser.LayerRecord>(levelData.LayerInfo.LayerRecords);

            var textures = ParseAsLayers(PSD.Size, PSD.Depth, rootLayers, imageRecordQueue, imageDataQueue);

            PSD.RootLayers = rootLayers;
            PSD.Texture2Ds = textures.Values.ToArray();
            return PSD;
        }

        private static Dictionary<string, Texture2D> ParseAsLayers(Vector2Int Size, int Depth, List<AbstractLayerData> rootLayers, Queue<LayerRecordParser.LayerRecord> imageRecordQueue, Queue<ChannelImageDataParser.ChannelImageData> imageDataQueue)
        {
            var texture2DDict = new Dictionary<string, Texture2D>();
            while (imageRecordQueue.Count != 0)
            {
                var record = imageRecordQueue.Dequeue();

                var sectionDividerSetting = record.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInformationParser.lsct) as AdditionalLayerInformationParser.lsct;
                if (sectionDividerSetting != null && sectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    rootLayers.Add(ParseLayerFolder(record, Depth, imageRecordQueue, imageDataQueue, texture2DDict, Size));
                }
                else
                {
                    rootLayers.Add(ParseRasterLayer(record, Depth, imageDataQueue, texture2DDict, Size));
                }

            }
            return texture2DDict;
        }

        private static LayerFolderData ParseLayerFolder(LayerRecordParser.LayerRecord record, int depth, Queue<LayerRecordParser.LayerRecord> imageRecordQueue, Queue<ChannelImageDataParser.ChannelImageData> imageDataQueue, Dictionary<string, Texture2D> texture2DList, Vector2Int size)
        {
            var layerFolder = new LayerFolderData();
            // layerFolder.CopyFromRecord(record);
            layerFolder.Layers = new List<AbstractLayerData>();
            _ = DeuceChannelInfoAndImage(record, imageDataQueue);
            // SetGenerateLayerMask(record, depth, layerFolder, channelInfoAndImage, texture2DList);
            while (imageRecordQueue.Count != 0)
            {
                var PeekRecord = imageRecordQueue.Peek();


                var PeekSectionDividerSetting = PeekRecord.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInformationParser.lsct) as AdditionalLayerInformationParser.lsct;

                if (PeekSectionDividerSetting == null)
                {
                    layerFolder.Layers.Add(ParseRasterLayer(imageRecordQueue.Dequeue(), depth, imageDataQueue, texture2DList, size));
                }
                else if (PeekSectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    layerFolder.Layers.Add(ParseLayerFolder(imageRecordQueue.Dequeue(), depth, imageRecordQueue, imageDataQueue, texture2DList, size));
                }
                else if (PeekSectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.OpenFolder
                || PeekSectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.ClosedFolder)
                {
                    break;
                }

            }
            var EndFolderRecord = imageRecordQueue.Dequeue();
            layerFolder.CopyFromRecord(EndFolderRecord);

            var lsct = EndFolderRecord.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInformationParser.lsct) as AdditionalLayerInformationParser.lsct;
            var BlendModeKeyEnum = PSDLayer.BlendModeKeyToEnum(lsct.BlendModeKey);
            layerFolder.BlendMode = PSDLayer.ConvertBlendType(BlendModeKeyEnum);
            layerFolder.PassThrough = BlendModeKeyEnum == PSDBlendMode.PassThrough;

            var endChannelInfoAndImage = DeuceChannelInfoAndImage(EndFolderRecord, imageDataQueue);
            SetGenerateLayerMask(EndFolderRecord, depth, layerFolder, endChannelInfoAndImage, texture2DList, size);
            return layerFolder;
        }

        private static RasterLayerData ParseRasterLayer(LayerRecordParser.LayerRecord record, int depth, Queue<ChannelImageDataParser.ChannelImageData> imageDataQueue, Dictionary<string, Texture2D> texture2DList, Vector2Int size)
        {
            var rasterLayer = new RasterLayerData();
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
                tex2D.filterMode = FilterMode.Point;

                rasterLayer.TexturePivot = new Vector2Int(record.RectTangle.Left, size.y - record.RectTangle.Bottom);
                var offsetTex = DrawOffsetEvaluateTexture(tex2D, rasterLayer.TexturePivot, size, null);
                offsetTex.name = record.LayerName + "_Tex";
                offsetTex.filterMode = FilterMode.Point;
                rasterLayer.RasterTexture = offsetTex;
                if (texture2DList.ContainsKey(rasterLayer.RasterTexture.name))
                {
                    var name = rasterLayer.RasterTexture.name;
                    var count = 0;
                    while (texture2DList.ContainsKey($"{name}-{count}"))
                    {
                        count += 1;
                    }
                    rasterLayer.RasterTexture.name = $"{name}-{count}";
                }
                texture2DList.Add(rasterLayer.RasterTexture.name, rasterLayer.RasterTexture);
                if (rasterLayer.RasterTexture != tex2D) { UnityEngine.Object.DestroyImmediate(tex2D, true); }

            }

            SetGenerateLayerMask(record, depth, rasterLayer, channelInfoAndImage, texture2DList, size);

            return rasterLayer;
        }

        private static void SetGenerateLayerMask(LayerRecordParser.LayerRecord record, int depth, AbstractLayerData rasterLayer, Dictionary<ChannelImageDataParser.ChannelInformation.ChannelIDEnum, ChannelImageDataParser.ChannelImageData> channelInfoAndImage, Dictionary<string, Texture2D> texture2DList, Vector2Int size)
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
            tex2D.filterMode = FilterMode.Point;
            tex2D.name = record.LayerName + "_MaskTex";
            rasterLayer.LayerMask = new TexTransCore.Layer.LayerMask();


            var DefaultMaskColor = record.LayerMaskAdjustmentLayerData.DefaultColor / 255;

            var maskDisabled = record.LayerMaskAdjustmentLayerData.Flag.HasFlag(LayerRecordParser.LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag.MaskDisabled);
            rasterLayer.LayerMask.LayerMaskDisabled = maskDisabled;
            var MaskPivot = new Vector2Int(record.LayerMaskAdjustmentLayerData.RectTangle.Left, size.y - record.LayerMaskAdjustmentLayerData.RectTangle.Bottom);

            rasterLayer.LayerMask.MaskTexture = DrawOffsetEvaluateTexture(tex2D, MaskPivot, size, new Color(1, 1, 1, DefaultMaskColor));
            rasterLayer.LayerMask.MaskTexture.filterMode = FilterMode.Point;
            if (texture2DList.ContainsKey(rasterLayer.LayerMask.MaskTexture.name))
            {
                var name = rasterLayer.LayerMask.MaskTexture.name;
                var count = 0;
                while (texture2DList.ContainsKey($"{name}-{count}"))
                {
                    count += 1;
                }
                rasterLayer.LayerMask.MaskTexture.name = $"{name}-{count}";
            }
            texture2DList.Add(rasterLayer.LayerMask.MaskTexture.name, rasterLayer.LayerMask.MaskTexture);
            if (rasterLayer.LayerMask.MaskTexture != tex2D) { UnityEngine.Object.DestroyImmediate(tex2D, true); }

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
            var bytes = new byte[imageData.Length];

            for (var i = 0; height > i; i += 1)
            {
                var startIndex = i * width;

                imageData.AsSpan(startIndex, width).CopyTo(bytes.AsSpan((height - i - 1) * width, width));

            }
            return bytes;
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

        public static Texture2D DrawOffsetEvaluateTexture(Texture2D targetTexture, Vector2Int texturePivot, Vector2Int canvasSize, Color? DefaultColor)
        {
            var RightUpPos = texturePivot + new Vector2Int(targetTexture.width, targetTexture.height);
            var Pivot = texturePivot;
            if (RightUpPos != canvasSize || Pivot != Vector2Int.zero)
            {
                var rt = RenderTexture.GetTemporary(canvasSize.x, canvasSize.y, 0);
                if (DefaultColor.HasValue) { TextureBlendUtils.ColorBlit(rt, DefaultColor.Value); }
                else { TextureBlendUtils.ColorBlit(rt, new Color(0, 0, 0, 0)); }
                TextureOffset(rt, targetTexture, new Vector2((float)RightUpPos.x / canvasSize.x, (float)RightUpPos.y / canvasSize.y), new Vector2((float)Pivot.x / canvasSize.x, (float)Pivot.y / canvasSize.y));
                var tex2D = rt.CopyTexture2D();
                RenderTexture.ReleaseTemporary(rt);
                tex2D.name = targetTexture.name;
                return tex2D;
            }
            else
            {
                return targetTexture;
            }
        }

        public static void TextureOffset(RenderTexture tex, Texture texture, Vector2 RightUpPos, Vector2 Pivot)
        {
            var triangle = new List<TriangleIndex>() { new TriangleIndex(0, 1, 2), new TriangleIndex(2, 1, 3) };
            var souse = new List<Vector2>() { Vector2.zero, new Vector2(0, 1), new Vector2(1, 0), Vector2.one };
            var target = new List<Vector2>() { Pivot, new Vector2(Pivot.x, RightUpPos.y), new Vector2(RightUpPos.x, Pivot.y), RightUpPos };

            var TransData = new TransTexture.TransData(triangle, target, souse);
            TransTexture.TransTextureToRenderTexture(tex, texture, TransData);
        }
    }

    [Serializable]
    public class PSDHighLevelData
    {
        public Vector2Int Size;
        public ushort Depth;
        public ushort channels;
        public List<AbstractLayerData> RootLayers;
        public Texture2D[] Texture2Ds;
    }
}