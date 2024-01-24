using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using net.rs64.MultiLayerImageParser.LayerData;
using net.rs64.MultiLayerImageParser.PSD;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.MultiLayerImage.Importer;
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

                var mliImporter = new MultiLayerImageImporter(canvasDescription, ctx, psdBytes, CreatePSDImportedImage, GetPreviewImage);
                mliImporter.AddLayers(multiLayerImageCanvas.transform, pSDData.RootLayers);
                mliImporter.CreatePreview();

                //PSDの特定のポイントにあることを示すオブジェクトに変えて随時ロードに書き換えよう。
                //ダウンスケーリングは最後にやるように書き換えたいな
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
                    var bytes = new NativeArray<Color32>(rBytes.Length, Allocator.Persistent);
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