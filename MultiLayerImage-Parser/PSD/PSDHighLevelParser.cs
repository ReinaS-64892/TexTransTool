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
using Unity.Collections;
using static net.rs64.MultiLayerImageParser.PSD.ChannelImageDataParser.ChannelInformation;

namespace net.rs64.MultiLayerImageParser.PSD
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
        // public delegate LowMap<Color32> TexCal();

        // public static Dictionary<LowMap<Color32>, T> TextureCalculationExecuter<T>(Dictionary<TexCal, T> ImageTask, int? ForceParallelSize = null)
        // {
        //     var parallelSize = ForceParallelSize.HasValue ? ForceParallelSize.Value : Environment.ProcessorCount;
        //     var results = new Dictionary<LowMap<Color32>, T>(ImageTask.Count);
        //     var taskQueue = new Queue<KeyValuePair<TexCal, T>>(ImageTask);
        //     var TaskParallel = new (Task<LowMap<Color32>>, T)[parallelSize];
        //     while (taskQueue.Count > 0)
        //     {
        //         for (int i = 0; TaskParallel.Length > i; i += 1)
        //         {
        //             if (taskQueue.Count > 0)
        //             {
        //                 var task = taskQueue.Dequeue();
        //                 TaskParallel[i] = (Task.Run(task.Key.Invoke), task.Value);
        //             }
        //             else
        //             {
        //                 TaskParallel[i] = (null, default);
        //             }
        //         }

        //         foreach (var task in TaskParallel)
        //         {
        //             if (task.Item1 == null) { continue; }
        //             results.Add(TexCalAwaiter(task.Item1).Result, task.Item2);
        //         }
        //     }
        //     return results;
        // }

        // public static async Task<LowMap<Color32>> TexCalAwaiter(Task<LowMap<Color32>> twoDimensionalMap)
        // {
        //     return await twoDimensionalMap.ConfigureAwait(false);
        // }

        // private static LowMap<Color32>[] AwaitImage(Dictionary<TexCal, RasterLayerData> ImageParseTask, int Depth, Vector2Int CanvasSize)
        // {
        //     var texture2Ds = new LowMap<Color32>[ImageParseTask.Count];
        //     var count = 0;
        //     foreach (var task in TextureCalculationExecuter(ImageParseTask))
        //     {
        //         var tex = task.Key;

        //         texture2Ds[count] = tex;
        //         task.Value.RasterTexture = tex;
        //         count += 1;
        //     }
        //     return texture2Ds;
        // }

        // private static LowMap<Color32>[] AwaitImageMask(Dictionary<TexCal, AbstractLayerData> ImageParseTask, int Depth, Vector2Int CanvasSize)
        // {
        //     var texture2Ds = new LowMap<Color32>[ImageParseTask.Count];
        //     // var NameHash = new HashSet<string>();
        //     var count = 0;
        //     foreach (var task in TextureCalculationExecuter(ImageParseTask))
        //     {
        //         var tex = task.Key;

        //         texture2Ds[count] = tex;
        //         task.Value.LayerMask.MaskTexture = tex;
        //         count += 1;
        //     }
        //     return texture2Ds;
        // }
        private static void ParseAsLayers(List<AbstractLayerData> rootLayers, Queue<LayerRecord> imageRecordQueue, Queue<ChannelImageData> imageDataQueue)
        {
            while (imageRecordQueue.Count != 0)
            {
                var record = imageRecordQueue.Dequeue();

                var sectionDividerSetting = record.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInformationParser.lsct) as AdditionalLayerInformationParser.lsct;
                if (sectionDividerSetting != null && sectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.BoundingSectionDivider)
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


                var PeekSectionDividerSetting = PeekRecord.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInformationParser.lsct) as AdditionalLayerInformationParser.lsct;

                if (PeekSectionDividerSetting == null)
                {
                    layerFolder.Layers.Add(ParseRasterLayer(imageRecordQueue.Dequeue(), imageDataQueue));
                }
                else if (PeekSectionDividerSetting.SelectionDividerType == AdditionalLayerInformationParser.lsct.SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    layerFolder.Layers.Add(ParseLayerFolder(imageRecordQueue.Dequeue(), imageRecordQueue, imageDataQueue));
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
            layerFolder.LayerMask = ParseLayerMask(EndFolderRecord, endChannelInfoAndImage);

            return layerFolder;
        }
        private static RasterLayerData ParseRasterLayer(LayerRecord record, Queue<ChannelImageData> imageDataQueue)
        {
            var rasterLayer = new RasterLayerData();
            rasterLayer.CopyFromRecord(record);

            var channelInfoAndImage = DeuceChannelInfoAndImage(record, imageDataQueue);

            rasterLayer.RasterTexture = ParseRasterImage(record, channelInfoAndImage);
            rasterLayer.LayerMask = ParseLayerMask(record, channelInfoAndImage);

            return rasterLayer;
        }

        // private static LowMap<Color32> GenerateTexTowDMap(LayerRecord record, Vector2Int size, Dictionary<ChannelInformation.ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        // {
        //     NativeArray<Color32> pixels = GenerateTexturePixels(record, channelInfoAndImage);
        //     var TexturePivot = new Vector2Int(record.RectTangle.Left, record.RectTangle.Top);
        //     return DrawOffsetEvaluateTexture(new LowMap<Color32>(pixels, record.RectTangle.GetWidth(), record.RectTangle.GetHeight()), TexturePivot, size, null);
        // }

        // private static NativeArray<Color32> GenerateTexturePixels(LayerRecord record, Dictionary<ChannelInformation.ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        // {
        //     var length = record.RectTangle.GetWidth() * record.RectTangle.GetHeight();
        //     var pixels = new NativeArray<Color32>(length, Allocator.Persistent);
        //     if (channelInfoAndImage.ContainsKey(ChannelInformation.ChannelIDEnum.Transparency))
        //     {
        //         var redImage = channelInfoAndImage[ChannelInformation.ChannelIDEnum.Red].ImageData;
        //         var blueImage = channelInfoAndImage[ChannelInformation.ChannelIDEnum.Blue].ImageData;
        //         var greenImage = channelInfoAndImage[ChannelInformation.ChannelIDEnum.Green].ImageData;
        //         var alphaImage = channelInfoAndImage[ChannelInformation.ChannelIDEnum.Transparency].ImageData;
        //         for (var i = 1; length > i; i += 1)
        //         {
        //             pixels[i] = new Color32(redImage[i], greenImage[i], blueImage[i], alphaImage[i]);
        //         }
        //     }
        //     else
        //     {
        //         var redImage = channelInfoAndImage[ChannelInformation.ChannelIDEnum.Red].ImageData;
        //         var blueImage = channelInfoAndImage[ChannelInformation.ChannelIDEnum.Blue].ImageData;
        //         var greenImage = channelInfoAndImage[ChannelInformation.ChannelIDEnum.Green].ImageData;
        //         for (var i = 1; length > i; i += 1)
        //         {
        //             pixels[i] = new Color32(redImage[i], greenImage[i], blueImage[i], byte.MaxValue);
        //         }
        //     }

        //     return pixels;
        // }

        private static ImportRasterImageData ParseRasterImage(LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            if (!channelInfoAndImage.ContainsKey(ChannelIDEnum.Red)
                || !channelInfoAndImage.ContainsKey(ChannelIDEnum.Blue)
                || !channelInfoAndImage.ContainsKey(ChannelIDEnum.Green)
                || record.RectTangle.CalculateRawCompressLength() == 0) { return null; }

            var importedRaster = new PSDImportedRasterImageData();
            importedRaster.RectTangle = record.RectTangle;
            importedRaster.R = channelInfoAndImage[ChannelIDEnum.Red];
            importedRaster.G = channelInfoAndImage[ChannelIDEnum.Green];
            importedRaster.B = channelInfoAndImage[ChannelIDEnum.Blue];
            if (channelInfoAndImage.ContainsKey(ChannelIDEnum.Transparency)) { importedRaster.A = channelInfoAndImage[ChannelIDEnum.Transparency]; }

            return importedRaster;
        }
        private static LayerMask ParseLayerMask(LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            if (!channelInfoAndImage.ContainsKey(ChannelIDEnum.UserLayerMask)) { return null; }
            if (record.LayerMaskAdjustmentLayerData.RectTangle.CalculateRawCompressLength() == 0) { return null; }


            var importedMask = new PSDImportedRasterMaskImageData();
            importedMask.RectTangle = record.LayerMaskAdjustmentLayerData.RectTangle;
            importedMask.MaskImage = channelInfoAndImage[ChannelIDEnum.UserLayerMask];
            importedMask.DefaultValue = record.LayerMaskAdjustmentLayerData.DefaultColor;

            var LayerMask = new LayerMask();
            LayerMask.LayerMaskDisabled = record.LayerMaskAdjustmentLayerData.Flag.HasFlag(LayerMaskAdjustmentLayerData.MaskOrAdjustmentFlag.MaskDisabled);
            LayerMask.MaskTexture = importedMask;

            return LayerMask;
        }

        // private static LowMap<Color32> GenerateMaskTexTwoDMap(
        //     LayerRecord record,
        //     Dictionary<ChannelInformation.ChannelIDEnum, ChannelImageData> channelInfoAndImage,
        //     Vector2Int CanvasSize,
        //     Vector2Int MaskPivot,
        //     int DefaultMaskColor
        // )
        // {
        //     var maskImage = channelInfoAndImage[ChannelInformation.ChannelIDEnum.UserLayerMask].ImageData;

        //     var length = record.LayerMaskAdjustmentLayerData.RectTangle.GetWidth() * record.LayerMaskAdjustmentLayerData.RectTangle.GetHeight();
        //     var pixels = new NativeArray<Color32>(length, Allocator.Persistent);
        //     for (var i = 1; length > i; i += 1)
        //     {
        //         try
        //         {
        //             pixels[i] = new Color32(maskImage[i], maskImage[i], maskImage[i], maskImage[i]);
        //         }
        //         catch
        //         {
        //             pixels[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        //         }
        //     }
        //     return DrawOffsetEvaluateTexture(new LowMap<Color32>(pixels, record.LayerMaskAdjustmentLayerData.RectTangle.GetWidth(), record.LayerMaskAdjustmentLayerData.RectTangle.GetHeight()), MaskPivot, CanvasSize, new Color(1, 1, 1, DefaultMaskColor));
        // }

        private static Dictionary<ChannelIDEnum, ChannelImageData> DeuceChannelInfoAndImage(LayerRecord record, Queue<ChannelImageData> imageDataQueue)
        {
            var channelInfoAndImage = new Dictionary<ChannelIDEnum, ChannelImageData>();
            foreach (var item in record.ChannelInformationArray)
            {
                channelInfoAndImage.Add(item.ChannelID, imageDataQueue.Dequeue());
            }
            return channelInfoAndImage;
        }

        // public static TextureFormat DepthToFormat(int depth)
        // {
        //     if (depth <= 8)
        //     {
        //         return TextureFormat.RGBA32;
        //     }
        //     else if (depth <= 16)
        //     {
        //         return TextureFormat.RGBA64;
        //     }
        //     else
        //     {
        //         return TextureFormat.RGBAFloat;
        //     }

        // }

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
            var tTex2D = new LowMap<Color32>(new NativeArray<Color32>(TargetSize.x * TargetSize.y, Allocator.Persistent), TargetSize.x, TargetSize.y);
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

        public static void Fill<T>(this NativeArray<T> values, T val) where T : struct
        {
            for (var i = 0; values.Length > i; i += 1)
            {
                values[i] = val;
            }
        }
        public static void Fill<T>(this NativeSlice<T> values, T val) where T : struct
        {
            for (var i = 0; values.Length > i; i += 1)
            {
                values[i] = val;
            }
        }

        public static void CopyTo<T>(this NativeSlice<T> from, NativeSlice<T> to) where T : struct
        {
            to.CopyFrom(from);
        }
        public static void CopyTo<T>(this NativeArray<T> from, NativeSlice<T> to) where T : struct
        {
            to.CopyFrom(from);
        }

        public static void CopyFrom<T>(this NativeArray<T> to, Span<T> from) where T : struct
        {
            for (var i = 0; to.Length > i; i += 1)
            {
                to[i] = from[i];
            }
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