using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.MultiLayerImage.LayerData;
using net.rs64.TexTransCore;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser;
using static net.rs64.MultiLayerImage.Parser.PSD.LayerRecordParser;
using LayerMask = net.rs64.MultiLayerImage.LayerData.LayerMaskData;
using static net.rs64.MultiLayerImage.Parser.PSD.ChannelImageDataParser.ChannelInformation;
using static net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo.lsct;
using net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static class PSDHighLevelParser
    {
        public static PSDHighLevelData Parse(PSDLowLevelParser.PSDLowLevelData levelData, PSDImportMode? importMode = null)
        {
            var psd = new PSDHighLevelData
            {
                Width = (int)levelData.Width,
                Height = (int)levelData.Height,
                Depth = levelData.BitDepth,
                channels = levelData.Channels,
                RootLayers = new List<AbstractLayerData>()
            };

            var ctx = new HighLevelParserContext();

            ctx.RootLayers = psd.RootLayers;

            importMode ??= levelData.ImageResources.FindIndex(ir => ir.UniqueIdentifier == 1060) == -1 ? PSDImportMode.ClipStudioPaint : PSDImportMode.Photoshop;
            ctx.ImportMode = importMode.Value;

            ctx.ImageDataQueue = new Queue<ChannelImageData>(levelData.LayerInfo.ChannelImageData ?? new());
            ctx.ImageRecordQueue = new Queue<LayerRecord>(levelData.LayerInfo.LayerRecords ?? new());
            ctx.CanvasTypeAdditionalLayerInfo = levelData.CanvasTypeAdditionalLayerInfo;

            CollectAdditionalLayer(ctx);

            ParseAsLayers(ctx);

            ResolveBlendTypeKeyWithImportMode(ctx, ctx.RootLayers);
            ResolveClippingAndPassThrough(ctx, ctx.RootLayers);

            return psd;
        }

        class HighLevelParserContext
        {
            internal PSDImportMode ImportMode;
            internal Queue<ChannelImageData> ImageDataQueue;
            internal Queue<LayerRecord> ImageRecordQueue;
            internal AdditionalLayerInfoBase[] CanvasTypeAdditionalLayerInfo;
            internal List<AbstractLayerData> RootLayers;
            internal Dictionary<AbstractLayerData, List<LayerRecord>> SourceLayerRecode = new();
        }

        private static void CollectAdditionalLayer(HighLevelParserContext context)
        {
            foreach (var al in context.CanvasTypeAdditionalLayerInfo)
            {
                if (al is Lr32 lr)
                {
                    foreach (var l in lr.AdditionalLayerInformation.LayerRecords) context.ImageRecordQueue.Enqueue(l);
                    foreach (var c in lr.AdditionalLayerInformation.ChannelImageData) context.ImageDataQueue.Enqueue(c);
                }
            }
        }

        public enum PSDImportMode
        {
            Unknown = 0,
            Photoshop = 2,
            ClipStudioPaint = 3,
        }


        private static void ResolveBlendTypeKeyWithImportMode(HighLevelParserContext ctx, List<AbstractLayerData> layerData)
        {
            switch (ctx.ImportMode)
            {
                case PSDImportMode.ClipStudioPaint:
                    {
                        foreach (var layer in layerData)
                        {
                            layer.BlendTypeKey = PSDLayer.ResolveGlow(layer.BlendTypeKey, ctx.SourceLayerRecode[layer].LastOrDefault()?.AdditionalLayerInformation);
                            if (s_clipBlendModeDict.TryGetValue(layer.BlendTypeKey, out var actualModeKey))
                            { layer.BlendTypeKey = actualModeKey; }
                            if (layer is LayerFolderData layerFolderData)
                            { ResolveBlendTypeKeyWithImportMode(ctx, layerFolderData.Layers); }
                        }
                        break;
                    }
            }
        }
        private static void ResolveClippingAndPassThrough(HighLevelParserContext ctx, List<AbstractLayerData> layerData)
        {
            Action defferApplyAction = () => { };
            for (var i = 0; layerData.Count > i; i += 1)
            {
                var layer = layerData[i];

                switch (ctx.ImportMode)
                {
                    case PSDImportMode.Photoshop:
                        {
                            // Photosohp の挙動に、色調補正系 つまり Grab系 が存在する通過レイヤーフォルダーに対する クリッピングを行うと、そのレイヤーフォルダーは通過ではなくる仕様の修正
                            // TTT は 通過レイヤーフォルダーに Grab系が存在しても、クリッピングされたときに挙動を変えるみたいな奇妙なことをしないので、
                            if (layer is LayerFolderData lf && lf.PassThrough)
                            {
                                var above = i + 1;
                                var clippingLayer = layerData.Count > above ? layerData[above] : null;

                                if (clippingLayer.Clipping)
                                {
                                    if (lf.Layers.Any(l => l is IGrabTag))
                                    {
                                        defferApplyAction += () => { lf.PassThrough = false; };
                                    }
                                }
                            }

                            break;
                        }
                    case PSDImportMode.ClipStudioPaint:
                        {
                            // ClipStudioPaint で 灰色の表示になっているクリッピングが無効な状態を、TTT ではセーブデータレベルで取り消す。
                            // TTT ではすべてのレイヤーがクリッピングの対象として可能な概念になっているから
                            // Grab 系の中でも 色調補正系はクリッピングを反映しない状態になるし、 フォルダはまぁいい感じに適用するので。
                            // 一番下のレイヤーはちょっと違うけど、念のため無効化。

                            //ちなみに後でクリッピングを無効化するのは、クリッピング対象を探るときにすぐに無効化されてしまうとクリッピングが複数枚連なっていた場合に辿れなくなってしまうから。
                            var clippingTargetIndex = FindClippingTarget(layerData, i);
                            if (clippingTargetIndex != -1)
                            {
                                var clippingTarget = layerData[clippingTargetIndex];
                                if (clippingTarget is IGrabTag)
                                {
                                    defferApplyAction += () => { layer.Clipping = false; };
                                }
                                else if (clippingTarget is LayerFolderData lf)
                                {
                                    if (lf.PassThrough)
                                    {
                                        defferApplyAction += () => { layer.Clipping = false; };
                                    }
                                }
                            }
                            else
                            {
                                defferApplyAction += () => { layer.Clipping = false; };
                            }
                            break;
                        }
                }

                if (layer is LayerFolderData layerFolderData) { ResolveClippingAndPassThrough(ctx, layerFolderData.Layers); }
            }

            defferApplyAction();

            static int FindClippingTarget(List<AbstractLayerData> layerData, int entryIndex)
            {
                var clippingTarget = entryIndex - 1;
                if (clippingTarget >= 0)
                {
                    if (layerData[clippingTarget].Clipping)
                    {
                        return FindClippingTarget(layerData, clippingTarget);
                    }
                    else
                    {
                        return clippingTarget;
                    }
                }
                else
                {
                    return -1;
                }
            }
        }
        static Dictionary<string, string> s_clipBlendModeDict = new()
        {
            {"Normal","Clip/Normal"},
            {"Mul","Clip/Mul"},
            {"Screen","Clip/Screen"},
            {"Overlay","Clip/Overlay"},
            {"HardLight","Clip/HardLight"},
            {"SoftLight","Clip/SoftLight"},
            {"ColorDodge","Clip/ColorDodge"},
            {"ColorBurn","Clip/ColorBurn"},
            {"LinearBurn","Clip/LinearBurn"},
            {"VividLight","Clip/VividLight"},
            {"LinearLight","Clip/LinearLight"},
            {"Divide","Clip/Divide"},
            {"Addition","Clip/Addition"},
            {"Subtract","Clip/Subtract"},
            {"Difference","Clip/Difference"},
            {"DarkenOnly","Clip/DarkenOnly"},
            {"LightenOnly","Clip/LightenOnly"},
            {"Hue","Clip/Hue"},
            {"Saturation","Clip/Saturation"},
            {"Color","Clip/Color"},
            {"Luminosity","Clip/Luminosity"},
            {"Exclusion","Clip/Exclusion"},
            {"DarkenColorOnly","Clip/DarkenColorOnly"},
            {"LightenColorOnly","Clip/LightenColorOnly"},
            {"PinLight","Clip/PinLight"},
            {"HardMix","Clip/HardMix"},
            {"AdditionGlow","Clip/AdditionGlow"},
            {"ColorDodgeGlow","Clip/ColorDodgeGlow"},
        };

        private static void ParseAsLayers(HighLevelParserContext ctx)
        {
            while (ctx.ImageRecordQueue.Count != 0)
            {
                var record = ctx.ImageRecordQueue.Dequeue();

                var sectionDividerSetting = record.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInfo.lsct) as AdditionalLayerInfo.lsct;
                if (sectionDividerSetting != null && sectionDividerSetting.SelectionDividerType == AdditionalLayerInfo.lsct.SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    ctx.RootLayers.Add(ParseLayerFolder(ctx, record));
                }
                else
                {
                    ctx.RootLayers.Add(ParseRasterLayer(ctx, record));
                }

            }
        }

        private static LayerFolderData ParseLayerFolder(HighLevelParserContext ctx, LayerRecord record)
        {
            var layerFolder = new LayerFolderData();
            layerFolder.Layers = new List<AbstractLayerData>();
            ctx.SourceLayerRecode[layerFolder] = new() { record };

            _ = DeuceChannelInfoAndImage(record, ctx.ImageDataQueue);

            while (ctx.ImageRecordQueue.Count != 0)
            {
                var PeekRecord = ctx.ImageRecordQueue.Peek();

                var debugName = PeekRecord.LayerName;

                var PeekSectionDividerSetting = PeekRecord.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInfo.lsct) as AdditionalLayerInfo.lsct;

                if (PeekSectionDividerSetting == null)
                { layerFolder.Layers.Add(ParseRasterLayer(ctx, ctx.ImageRecordQueue.Dequeue())); }
                else if (PeekSectionDividerSetting.SelectionDividerType == SelectionDividerTypeEnum.BoundingSectionDivider)
                {
                    layerFolder.Layers.Add(ParseLayerFolder(ctx, ctx.ImageRecordQueue.Dequeue()));
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
            var EndFolderRecord = ctx.ImageRecordQueue.Dequeue();
            ctx.SourceLayerRecode[layerFolder].Add(EndFolderRecord);
            var endChannelInfoAndImage = DeuceChannelInfoAndImage(EndFolderRecord, ctx.ImageDataQueue);
            layerFolder.CopyFromRecord(EndFolderRecord, endChannelInfoAndImage);

            var lsct = EndFolderRecord.AdditionalLayerInformation.FirstOrDefault(I => I is AdditionalLayerInfo.lsct) as AdditionalLayerInfo.lsct;
            var BlendModeKeyEnum = PSDLayer.BlendModeKeyToEnum(lsct.BlendModeKey);
            layerFolder.BlendTypeKey = BlendModeKeyEnum.ToString();
            layerFolder.PassThrough = BlendModeKeyEnum == PSDBlendMode.PassThrough;


            return layerFolder;
        }
        private static AbstractLayerData ParseRasterLayer(HighLevelParserContext ctx, LayerRecord record)
        {
            var channelInfoAndImage = DeuceChannelInfoAndImage(record, ctx.ImageDataQueue);

            if (TryParseSpecialLayer(record, channelInfoAndImage, out var abstractLayerData))
            {
                ctx.SourceLayerRecode[abstractLayerData] = new() { record };
                return abstractLayerData;
            }

            if (!channelInfoAndImage.ContainsKey(ChannelIDEnum.Red) || !channelInfoAndImage.ContainsKey(ChannelIDEnum.Blue) || !channelInfoAndImage.ContainsKey(ChannelIDEnum.Green) || record.RectTangle.CalculateRectAreaSize() == 0)
            {//この判定だと...空のラスターレイヤーか、非対応なタイプのレイヤーなのかがわからない...どうすればいい...?
                var emptyData = new EmptyOrUnsupported();
                ctx.SourceLayerRecode[emptyData] = new() { record };
                emptyData.CopyFromRecord(record, channelInfoAndImage);
                emptyData.RasterTexture = ParseRasterImage(record, channelInfoAndImage);
                return emptyData;
            }

            var rasterLayer = new RasterLayerData();
            ctx.SourceLayerRecode[rasterLayer] = new() { record };
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
            {typeof(AdditionalLayerInfo.selc), SpecialSelectiveColorLayer},
        };

        private static AbstractLayerData SpecialSelectiveColorLayer(LayerRecord record, Dictionary<ChannelIDEnum, ChannelImageData> channelInfoAndImage)
        {
            var selectiveColorData = new SelectiveColorLayerData();
            var selc = record.AdditionalLayerInformation.First(i => i is selc) as selc;

            selectiveColorData.CopyFromRecord(record, channelInfoAndImage);

            selectiveColorData.RedsCMYK = selc.RedsCMYK;
            selectiveColorData.YellowsCMYK = selc.YellowsCMYK;
            selectiveColorData.GreensCMYK = selc.GreensCMYK;
            selectiveColorData.CyansCMYK = selc.CyansCMYK;
            selectiveColorData.BluesCMYK = selc.BluesCMYK;
            selectiveColorData.MagentasCMYK = selc.MagentasCMYK;
            selectiveColorData.WhitesCMYK = selc.WhitesCMYK;
            selectiveColorData.NeutralsCMYK = selc.NeutralsCMYK;
            selectiveColorData.BlacksCMYK = selc.BlacksCMYK;

            selectiveColorData.IsAbsolute = selc.IsAbsolute;

            return selectiveColorData;
        }

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
            var hueData = new HSLAdjustmentLayerData();
            var hue = record.AdditionalLayerInformation.First(i => i is AdditionalLayerInfo.hue) as AdditionalLayerInfo.hue;

            if (hue.Colorization) { Debug.Log($"Colorization of {record.LayerName} is no supported"); }

            hueData.CopyFromRecord(record, channelInfoAndImage);

            hueData.Hue = hue.Hue / (float)(hue.IsOld is false ? 180f : 100f);
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

            {//重複した色チャンネルを持つと主張する、治安の悪いPSDに対するワークアラウンド (確認されたケースはレイヤーフォルダーの開始を意味する空レイヤーですべて赤色という記述だった)
                var channelInfos = record.ChannelInformationArray;
                var channelIDs = channelInfos.Select(i => i.ChannelID);
                if (channelIDs.Distinct().Count() != channelIDs.Count())
                {
                    var id = -1;
                    for (var i = 0; channelInfos.Length > i; i += 1)
                    {
                        channelInfos[i].ChannelID = (ChannelInformation.ChannelIDEnum)id;
                        id += 1;
                    }
                }
            }

            foreach (var item in record.ChannelInformationArray)
            {
                channelInfoAndImage.Add(item.ChannelID, imageDataQueue.Dequeue());
            }
            return channelInfoAndImage;
        }

        // public static NativeArrayMap<Color32> DrawOffsetEvaluateTexture(
        //     NativeArrayMap<Color32> targetTexture,
        //     Vector2Int texturePivot,
        //     Vector2Int canvasSize,
        //     Color? DefaultColor
        // )
        // {
        //     var RightUpPos = texturePivot + targetTexture.MapSize;
        //     var Pivot = texturePivot;
        //     if (RightUpPos != canvasSize || Pivot != Vector2Int.zero)
        //     {
        //         return TextureOffset(targetTexture, canvasSize, Pivot, DefaultColor);
        //     }
        //     else
        //     {
        //         return targetTexture;
        //     }
        // }

        // public static NativeArrayMap<Color32> TextureOffset(NativeArrayMap<Color32> texture, Vector2Int TargetSize, Vector2Int Pivot, Color32? DefaultColor)
        // {
        //     var sTex2D = texture;
        //     var tTex2D = new NativeArrayMap<Color32>(new NativeArray<Color32>(TargetSize.x * TargetSize.y, Allocator.TempJob), TargetSize.x, TargetSize.y);
        //     var initColor = DefaultColor.HasValue ? DefaultColor.Value : new Color32(0, 0, 0, 0);
        //     tTex2D.Array.Fill(initColor);


        //     var xStart = Mathf.Max(-Pivot.x, 0);
        //     var xEnd = Mathf.Min(Pivot.x + sTex2D.Width, TargetSize.x) - Pivot.x;
        //     var xLength = xEnd - xStart;

        //     var yStart = Mathf.Max(-Pivot.y, 0);
        //     var yEnd = Mathf.Min(Pivot.y + sTex2D.MapSize.y, TargetSize.y) - Pivot.y;

        //     if (xLength < 0)
        //     {
        //         texture.Dispose();
        //         return tTex2D;
        //     }


        //     for (var yi = yStart; yEnd > yi; yi += 1)
        //     {
        //         var sSpan = sTex2D.Array.Slice(NativeArrayMap<int>.Convert1D(xStart, yi, sTex2D.Width), xLength);
        //         var tSpan = tTex2D.Array.Slice(NativeArrayMap<int>.Convert1D(xStart + Pivot.x, yi + Pivot.y, tTex2D.Width), xLength);
        //         sSpan.CopyTo(tSpan);
        //     }
        //     texture.Dispose();
        //     return tTex2D;
        // }

    }

    [Serializable]
    internal class PSDHighLevelData
    {
        public int Width;
        public int Height;
        public ushort Depth;
        public ushort channels;
        public List<AbstractLayerData> RootLayers;

        public static explicit operator CanvasData(PSDHighLevelData hData) => new CanvasData() { Width = hData.Width, Height = hData.Height, RootLayers = hData.RootLayers };
    }
}
