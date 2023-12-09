using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.MultiLayerImageParser.LayerData;
using UnityEngine;
using net.rs64.TexTransCore;
using static net.rs64.MultiLayerImageParser.PSD.PSDLowLevelParser.PSDLowLevelData;
using static net.rs64.MultiLayerImageParser.PSD.ChannelImageDataParser;
using static net.rs64.MultiLayerImageParser.PSD.LayerRecordParser;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.BlendTexture;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using LayerMask = net.rs64.MultiLayerImageParser.LayerData.LayerMaskData;
using System.Buffers;

namespace net.rs64.MultiLayerImageParser.PSD
{
    internal static class PSDHighLevelParser
    {
        public static PSDHighLevelData Parse(PSDLowLevelParser.PSDLowLevelData levelData)
        {
            var PSD = new PSDHighLevelData
            {
                Size = new Vector2Int((int)levelData.width, (int)levelData.height),
                Depth = levelData.Depth,
                channels = levelData.channels
            };

            var rootLayers = new List<AbstractLayerData>();
            var imageDataQueue = new Queue<ChannelImageData>(levelData.LayerInfo.ChannelImageData);
            var imageRecordQueue = new Queue<LayerRecord>(levelData.LayerInfo.LayerRecords);

            var ImageParseTask = new Dictionary<TexCal, RasterLayerData>();
            var ImageMaskParseTask = new Dictionary<TexCal, AbstractLayerData>();

            ParseAsLayers(PSD.Size, rootLayers, imageRecordQueue, imageDataQueue, ImageParseTask, ImageMaskParseTask);

            PSD.RootLayers = rootLayers;
            var image = AwaitImage(ImageParseTask, PSD.Depth, PSD.Size);
            var imageMask = AwaitImageMask(ImageMaskParseTask, PSD.Depth, PSD.Size);
            PSD.Texture2Ds = image.Concat(imageMask).ToArray();
            return PSD;
        }
        public delegate TwoDimensionalMap<Color32> TexCal();

        public static Dictionary<TwoDimensionalMap<Color32>, T> TextureCalculationExecuter<T>(Dictionary<TexCal, T> ImageTask, int? ForceParallelSize = null)
        {
            var parallelSize = ForceParallelSize.HasValue ? ForceParallelSize.Value : Environment.ProcessorCount;
            var results = new Dictionary<TwoDimensionalMap<Color32>, T>(ImageTask.Count);
            var taskQueue = new Queue<KeyValuePair<TexCal, T>>(ImageTask);
            var TaskParallel = new (Task<TwoDimensionalMap<Color32>>, T)[parallelSize];
            while (taskQueue.Count > 0)
            {
                for (int i = 0; TaskParallel.Length > i; i += 1)
                {
                    if (taskQueue.Count > 0)
                    {
                        var task = taskQueue.Dequeue();
                        TaskParallel[i] = (Task.Run(task.Key.Invoke), task.Value);
                    }
                    else
                    {
                        TaskParallel[i] = (null, default);
                    }
                }

                foreach (var task in TaskParallel)
                {
                    if (task.Item1 == null) { continue; }
                    results.Add(TexCalAwaiter(task.Item1).Result, task.Item2);
                }
            }
            return results;
        }

        public static async Task<TwoDimensionalMap<Color32>> TexCalAwaiter(Task<TwoDimensionalMap<Color32>> twoDimensionalMap)
        {
            return await twoDimensionalMap.ConfigureAwait(false);
        }

        private static TwoDimensionalMap<Color32>[] AwaitImage(Dictionary<TexCal, RasterLayerData> ImageParseTask, int Depth, Vector2Int CanvasSize)
        {
            var texture2Ds = new TwoDimensionalMap<Color32>[ImageParseTask.Count];
            var count = 0;
            foreach (var task in TextureCalculationExecuter(ImageParseTask))
            {
                var tex = task.Key;

                texture2Ds[count] = tex;
                task.Value.RasterTexture = tex;
                count += 1;
            }
            return texture2Ds;
        }

        private static TwoDimensionalMap<Color32>[] AwaitImageMask(Dictionary<TexCal, AbstractLayerData> ImageParseTask, int Depth, Vector2Int CanvasSize)
        {
            var texture2Ds = new TwoDimensionalMap<Color32>[ImageParseTask.Count];
            var NameHash = new HashSet<string>();
            var count = 0;
            foreach (var task in TextureCalculationExecuter(ImageParseTask))
            {
                var tex = task.Key;

                texture2Ds[count] = tex;
                task.Value.LayerMask.MaskTexture = tex;
                count += 1;
            }
            return texture2Ds;
        }
        private static void ParseAsLayers(
         Vector2Int CanvasSize,
         List<AbstractLayerData> rootLayers,
         Queue<LayerRecord> imageRecordQueue,
         Queue<ChannelImageData> imageDataQueue,
         Dictionary<TexCal, RasterLayerData> imageParseTask,
         Dictionary<TexCal, AbstractLayerData> imageMaskParseTask
         )
        {
            while (imageRecordQueue.Count != 0)
            {
                var record = imageRecordQueue.Dequeue();

                var sectionDividerSetting = record.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInformationParser.lsct) as AdditionalLayerInformationParser.lsct;
                if (sectionDividerSetting != null && sectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    rootLayers.Add(ParseLayerFolder(record, imageRecordQueue, imageDataQueue, CanvasSize, imageParseTask, imageMaskParseTask));
                }
                else
                {
                    rootLayers.Add(ParseRasterLayer(record, imageDataQueue, CanvasSize, imageParseTask, imageMaskParseTask));
                }

            }
        }

        private static LayerFolderData ParseLayerFolder(
            LayerRecord record,
            Queue<LayerRecord> imageRecordQueue,
            Queue<ChannelImageData> imageDataQueue,
            Vector2Int CanvasSize,
            Dictionary<TexCal, RasterLayerData> imageParseTask,
            Dictionary<TexCal, AbstractLayerData> imageMaskParseTask
        )
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
                    layerFolder.Layers.Add(ParseRasterLayer(imageRecordQueue.Dequeue(), imageDataQueue, CanvasSize, imageParseTask, imageMaskParseTask));
                }
                else if (PeekSectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    layerFolder.Layers.Add(ParseLayerFolder(imageRecordQueue.Dequeue(), imageRecordQueue, imageDataQueue, CanvasSize, imageParseTask, imageMaskParseTask));
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
            layerFolder.BlendTypeKey = PSDLayer.ConvertBlendType(BlendModeKeyEnum).ToString();
            layerFolder.PassThrough = BlendModeKeyEnum == PSDBlendMode.PassThrough;

            var endChannelInfoAndImage = DeuceChannelInfoAndImage(EndFolderRecord, imageDataQueue);
            SetGenerateLayerMask(EndFolderRecord, layerFolder, endChannelInfoAndImage, CanvasSize, imageMaskParseTask);
            return layerFolder;
        }
        private static RasterLayerData ParseRasterLayer(
            LayerRecord record,
            Queue<ChannelImageData> imageDataQueue,
            Vector2Int CanvasSize,
            Dictionary<TexCal, RasterLayerData> imageParseTask,
            Dictionary<TexCal, AbstractLayerData> imageMaskParseTask
        )
        {
            var timer = Stopwatch.StartNew();
            var rasterLayer = new RasterLayerData();
            rasterLayer.CopyFromRecord(record);
            var channelInfoAndImage = DeuceChannelInfoAndImage(record, imageDataQueue);

            if (channelInfoAndImage.ContainsKey(ChannelInformation.ChannelIDEnum.Red)
            && channelInfoAndImage.ContainsKey(ChannelInformation.ChannelIDEnum.Blue)
            && channelInfoAndImage.ContainsKey(ChannelInformation.ChannelIDEnum.Green)
            && record.RectTangle.CalculateRawCompressLength() != 0
            )
            {
                imageParseTask.Add(() => GenerateTexTowDMap(record, CanvasSize, channelInfoAndImage), rasterLayer);
            }

            SetGenerateLayerMask(record, rasterLayer, channelInfoAndImage, CanvasSize, imageMaskParseTask);

            return rasterLayer;
        }

        private static TwoDimensionalMap<Color32> GenerateTexTowDMap(LayerRecord record, Vector2Int size, Dictionary<ChannelInformation.ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            Color32[] pixels = GenerateTexturePixels(record, channelInfoAndImage).Result;
            var TexturePivot = new Vector2Int(record.RectTangle.Left, size.y - record.RectTangle.Bottom);
            return DrawOffsetEvaluateTexture(new TwoDimensionalMap<Color32>(pixels, new Vector2Int(record.RectTangle.GetWidth(), record.RectTangle.GetHeight())), TexturePivot, size, null);
        }

        private static async Task<Color32[]> GenerateTexturePixels(LayerRecord record, Dictionary<ChannelInformation.ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var redImageTask = Task.Run(() => DirectionConvert(channelInfoAndImage[ChannelInformation.ChannelIDEnum.Red].ImageData, record.RectTangle));
            var blueImageTask = Task.Run(() => DirectionConvert(channelInfoAndImage[ChannelInformation.ChannelIDEnum.Blue].ImageData, record.RectTangle));
            var greenImageTask = Task.Run(() => DirectionConvert(channelInfoAndImage[ChannelInformation.ChannelIDEnum.Green].ImageData, record.RectTangle));

            var length = record.RectTangle.GetWidth() * record.RectTangle.GetHeight();
            var pixels = ArrayPool<Color32>.Shared.Rent(length);
            if (channelInfoAndImage.ContainsKey(ChannelInformation.ChannelIDEnum.Transparency))
            {
                var alphaImageTask = Task.Run(() => DirectionConvert(channelInfoAndImage[ChannelInformation.ChannelIDEnum.Transparency].ImageData, record.RectTangle));
                var redImage = await redImageTask.ConfigureAwait(false);
                var blueImage = await blueImageTask.ConfigureAwait(false);
                var greenImage = await greenImageTask.ConfigureAwait(false);
                var alphaImage = await alphaImageTask.ConfigureAwait(false);
                for (var i = 1; length > i; i += 1)
                {
                    pixels[i] = new Color32(redImage[i], greenImage[i], blueImage[i], alphaImage[i]);
                }

                ArrayPool<byte>.Shared.Return(redImage);
                ArrayPool<byte>.Shared.Return(blueImage);
                ArrayPool<byte>.Shared.Return(greenImage);
                ArrayPool<byte>.Shared.Return(alphaImage);
            }
            else
            {
                var redImage = await redImageTask.ConfigureAwait(false);
                var blueImage = await blueImageTask.ConfigureAwait(false);
                var greenImage = await greenImageTask.ConfigureAwait(false);
                for (var i = 1; length > i; i += 1)
                {
                    pixels[i] = new Color32(redImage[i], greenImage[i], blueImage[i], byte.MaxValue);
                }
                ArrayPool<byte>.Shared.Return(redImage);
                ArrayPool<byte>.Shared.Return(blueImage);
                ArrayPool<byte>.Shared.Return(greenImage);
            }

            return pixels;
        }

        private static void SetGenerateLayerMask(LayerRecord record, AbstractLayerData abstractLayer, Dictionary<ChannelInformation.ChannelIDEnum, ChannelImageData> channelInfoAndImage, Vector2Int CanvasSize, Dictionary<TexCal, AbstractLayerData> imageMaskParseTask)
        {
            if (!channelInfoAndImage.ContainsKey(ChannelInformation.ChannelIDEnum.UserLayerMask)) { return; }
            if (record.LayerMaskAdjustmentLayerData.RectTangle.CalculateRawCompressLength() == 0) { return; }

            var MaskPivot = new Vector2Int(record.LayerMaskAdjustmentLayerData.RectTangle.Left, CanvasSize.y - record.LayerMaskAdjustmentLayerData.RectTangle.Bottom);
            var DefaultMaskColor = record.LayerMaskAdjustmentLayerData.DefaultColor / 255;


            abstractLayer.LayerMask = new LayerMask();
            var maskDisabled = record.LayerMaskAdjustmentLayerData.Flag.HasFlag(LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag.MaskDisabled);
            abstractLayer.LayerMask.LayerMaskDisabled = maskDisabled;


            imageMaskParseTask.Add(() => GenerateMaskTexTwoDMap(record, channelInfoAndImage, CanvasSize, MaskPivot, DefaultMaskColor), abstractLayer);
        }

        private static TwoDimensionalMap<Color32> GenerateMaskTexTwoDMap(
            LayerRecord record,
            Dictionary<ChannelInformation.ChannelIDEnum, ChannelImageData> channelInfoAndImage,
            Vector2Int CanvasSize,
            Vector2Int MaskPivot,
            int DefaultMaskColor
        )
        {
            var maskImage = channelInfoAndImage[ChannelInformation.ChannelIDEnum.UserLayerMask].ImageData;
            maskImage = DirectionConvert(maskImage, record.LayerMaskAdjustmentLayerData.RectTangle);

            var length = record.LayerMaskAdjustmentLayerData.RectTangle.GetWidth() * record.LayerMaskAdjustmentLayerData.RectTangle.GetHeight();
            var pixels = ArrayPool<Color32>.Shared.Rent(length);
            for (var i = 1; length > i; i += 1)
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
            ArrayPool<byte>.Shared.Return(maskImage);
            return DrawOffsetEvaluateTexture(new TwoDimensionalMap<Color32>(pixels, new Vector2Int(record.LayerMaskAdjustmentLayerData.RectTangle.GetWidth(), record.LayerMaskAdjustmentLayerData.RectTangle.GetHeight())), MaskPivot, CanvasSize, new Color(1, 1, 1, DefaultMaskColor));
        }

        private static Dictionary<ChannelInformation.ChannelIDEnum, ChannelImageData> DeuceChannelInfoAndImage(
            LayerRecord record,
            Queue<ChannelImageData> imageDataQueue
        )
        {
            var channelInfoAndImage = new Dictionary<ChannelInformation.ChannelIDEnum, ChannelImageData>();
            foreach (var item in record.ChannelInformationArray)
            {
                channelInfoAndImage.Add(item.ChannelID, imageDataQueue.Dequeue());
            }
            return channelInfoAndImage;
        }
        private static byte[] DirectionConvert(byte[] imageData, RectTangle rectTangle)
        {
            return DirectionConvert(imageData, rectTangle.GetWidth(), rectTangle.GetHeight());
        }
        private static byte[] DirectionConvert(byte[] imageData, int width, int height)
        {
            var bytes = ArrayPool<byte>.Shared.Rent(imageData.Length);

            for (var i = 0; height > i; i += 1)
            {
                var startIndex = i * width;

                Array.Copy(imageData, startIndex, bytes, (height - i - 1) * width, width);
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

        public static TwoDimensionalMap<Color32> DrawOffsetEvaluateTexture(
            TwoDimensionalMap<Color32> targetTexture,
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
                return new TwoDimensionalMap<Color32>(targetTexture.Array.AsSpan(0, canvasSize.x * canvasSize.y).ToArray(), canvasSize);
            }
        }

        public static TwoDimensionalMap<Color32> TextureOffset(TwoDimensionalMap<Color32> texture, Vector2Int TargetSize, Vector2Int Pivot, Color32? DefaultColor)
        {
            var sTex2D = texture;
            var tTex2D = new TwoDimensionalMap<Color32>(new Color32[TargetSize.x * TargetSize.y], TargetSize);
            var initColor = DefaultColor.HasValue ? DefaultColor.Value : new Color32(0, 0, 0, 0);
            tTex2D.Array.AsSpan(0, TargetSize.x * TargetSize.y).Fill(initColor);

            var xStart = Mathf.Max(-Pivot.x, 0);
            var xEnd = Mathf.Min(Pivot.x + sTex2D.MapSize.x, TargetSize.x) - Pivot.x;
            var xLength = xEnd - xStart;

            var yStart = Mathf.Max(-Pivot.y, 0);
            var yEnd = Mathf.Min(Pivot.y + sTex2D.MapSize.y, TargetSize.y) - Pivot.y;

            if (xLength < 0)
            {
                ArrayPool<Color32>.Shared.Return(texture.Array);
                return tTex2D;
            }


            for (var yi = yStart; yEnd > yi; yi += 1)
            {
                var sPos = new Vector2Int(xStart, yi);
                var sSpan = sTex2D.Array.AsSpan(sTex2D.GetIndexOn1D(sPos), xLength);
                var tSpan = tTex2D.Array.AsSpan(tTex2D.GetIndexOn1D(sPos + Pivot), xLength);
                sSpan.CopyTo(tSpan);
            }
            ArrayPool<Color32>.Shared.Return(texture.Array);
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
        public TwoDimensionalMap<Color32>[] Texture2Ds;

        public static explicit operator CanvasData(PSDHighLevelData hData) => new CanvasData() { Size = hData.Size, RootLayers = hData.RootLayers };
    }
}