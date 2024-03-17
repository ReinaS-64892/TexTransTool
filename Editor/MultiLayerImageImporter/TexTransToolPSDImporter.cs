using System;
using System.IO;
using System.Threading.Tasks;
using net.rs64.MultiLayerImage.LayerData;
using net.rs64.MultiLayerImage.Parser.PSD;
using net.rs64.TexTransCore.TransTextureCore;
using Unity.Collections;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [ScriptedImporter(1, new string[] { }, new string[] { "psd" }, AllowCaching = true)]
    public class TexTransToolPSDImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
            EditorUtility.DisplayProgressBar("Parse PSD", "ReadBytes", 0.2f);

            var psdBytes = File.ReadAllBytes(ctx.assetPath);

            EditorUtility.DisplayProgressBar("Parse PSD", "LowLevelParser", 0.2f);

            var lowPSDData = PSDLowLevelParser.Parse(assetPath);

            EditorUtility.DisplayProgressBar("Parse PSD", "HighLevelParser", 0.6f);

            var pSDData = PSDHighLevelParser.Parse(lowPSDData);

            EditorUtility.DisplayProgressBar("Parse PSD", "End", 1);

            try
            {
                EditorUtility.DisplayProgressBar("Import Canvas", "Build Layer", 0);


                var prefabName = Path.GetFileName(ctx.assetPath) + "-Canvas";
                var rootCanvas = new GameObject(prefabName);
                var multiLayerImageCanvas = rootCanvas.AddComponent<MultiLayerImageCanvas>();

                ctx.AddObjectToAsset("RootCanvas", rootCanvas);
                ctx.SetMainObject(rootCanvas);

                var canvasDescription = ScriptableObject.CreateInstance<PSDImportedCanvasDescription>();
                canvasDescription.Width = pSDData.Size.x;
                canvasDescription.Height = pSDData.Size.y;
                canvasDescription.name = "CanvasDescription";
                ctx.AddObjectToAsset(canvasDescription.name, canvasDescription);
                multiLayerImageCanvas.tttImportedCanvasDescription = canvasDescription;

                var mliImporter = new MultiLayerImageImporter(canvasDescription, ctx, psdBytes, CreatePSDImportedImage);
                mliImporter.AddLayers(multiLayerImageCanvas.transform, pSDData.RootLayers);

                EditorUtility.DisplayProgressBar("Import Canvas", "CreatePreview", 0.1f);
                mliImporter.CreatePreview();
                EditorUtility.DisplayProgressBar("Import Canvas", "CreatePreview", 1f);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        internal TTTImportedImage CreatePSDImportedImage(ImportRasterImageData importRasterImage)
        {
            switch (importRasterImage)
            {
                case PSDImportedRasterImageData pSDImportRasterImage:
                    {
                        var importedRasterImage = ScriptableObject.CreateInstance<PSDImportedRasterImage>();
                        importedRasterImage.RasterImageData = pSDImportRasterImage;
                        return importedRasterImage;
                    }
                case PSDImportedRasterMaskImageData pSDImportRasterMaskImage:
                    {
                        var importedRasterMaskImage = ScriptableObject.CreateInstance<PSDImportedRasterMaskImage>();
                        importedRasterMaskImage.MaskImageData = pSDImportRasterMaskImage;
                        return importedRasterMaskImage;
                    }
                default: return null;
            }
        }

        internal Task<NativeArray<Color32>> GetPreviewImage(byte[] souseBytes, TTTImportedImage importRasterImage)
        {
            var canvasSize = new Vector2Int(importRasterImage.CanvasDescription.Width, importRasterImage.CanvasDescription.Height);
            switch (importRasterImage)
            {
                case PSDImportedRasterImage pSDImportedRasterImage:
                    {
                        var data = pSDImportedRasterImage.RasterImageData;
                        return Task.Run(() => LoadOffsetEvalRasterLayer(souseBytes, data, canvasSize));
                    }
                case PSDImportedRasterMaskImage pSDImportedRasterMaskImage:
                    {
                        var data = pSDImportedRasterMaskImage.MaskImageData;
                        return Task.Run(() => LoadOffsetEvalRasterMask(souseBytes, data, canvasSize));
                    }
                default:
                    throw new ArgumentException();
            }

            static NativeArray<Color32> LoadOffsetEvalRasterLayer(byte[] souseBytes, PSDImportedRasterImageData psdRasterData, Vector2Int canvasSize)
            {
                var image = PSDImportedRasterImage.LoadPSDRasterLayerData(psdRasterData, souseBytes);
                var rawMap = new LowMap<Color32>(image, psdRasterData.RectTangle.GetWidth(), psdRasterData.RectTangle.GetHeight());
                var pivot = new Vector2Int(psdRasterData.RectTangle.Left, canvasSize.y - psdRasterData.RectTangle.Bottom);
                return PSDHighLevelParser.DrawOffsetEvaluateTexture(rawMap, pivot, canvasSize, null).Array;
            }

            static NativeArray<Color32> LoadOffsetEvalRasterMask(byte[] souseBytes, PSDImportedRasterMaskImageData psdMaskData, Vector2Int canvasSize)
            {
                using (var rBytes = PSDImportedRasterMaskImage.LoadPSDMaskImageData(psdMaskData, souseBytes))
                {
                    var bytes = new NativeArray<Color32>(rBytes.Length, Allocator.TempJob);
                    for (var i = 0; bytes.Length > i; i += 1)
                    {
                        var col = rBytes[i];
                        bytes[i] = new Color32(col, col, col, col);
                    }
                    var rawMap = new LowMap<Color32>(bytes, psdMaskData.RectTangle.GetWidth(), psdMaskData.RectTangle.GetHeight());
                    var pivot = new Vector2Int(psdMaskData.RectTangle.Left, canvasSize.y - psdMaskData.RectTangle.Bottom);
                    var defaultValue = new Color32(psdMaskData.DefaultValue, psdMaskData.DefaultValue, psdMaskData.DefaultValue, psdMaskData.DefaultValue);
                    return PSDHighLevelParser.DrawOffsetEvaluateTexture(rawMap, pivot, canvasSize, defaultValue).Array;

                }
            }
        }


    }
}
