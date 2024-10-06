using System;
using System.IO;
using net.rs64.MultiLayerImage.LayerData;
using net.rs64.MultiLayerImage.Parser.PSD;
using Unity.Collections;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [ScriptedImporter(1, new string[] { "psb" }, new string[] { "psd" }, AllowCaching = true)]
    public class TexTransToolPSDImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;

            EditorUtility.DisplayProgressBar("Parse PSD", "ReadBytes", 0.0f);

            Profiler.BeginSample("ParsePSD");
            Profiler.BeginSample("ReadBytes");

            var psdBytes = File.ReadAllBytes(ctx.assetPath);

            Profiler.EndSample();
            Profiler.BeginSample("LowLevel");

            var lowPSDData = PSDLowLevelParser.Parse(psdBytes);

            Profiler.EndSample();
            Profiler.BeginSample("LowLevel");

            var pSDData = PSDHighLevelParser.Parse(lowPSDData);

            Profiler.EndSample();
            Profiler.EndSample();

            try
            {
                EditorUtility.DisplayProgressBar("Import Canvas", "Build Layer", 0);
                Profiler.BeginSample("CreateCanvas");
                Profiler.BeginSample("CreateRootObjects");

                var prefabName = Path.GetFileName(ctx.assetPath) + "-Canvas";
                var rootCanvas = new GameObject(prefabName);
                var multiLayerImageCanvas = rootCanvas.AddComponent<MultiLayerImageCanvas>();

                ctx.AddObjectToAsset("RootCanvas", rootCanvas);
                ctx.SetMainObject(rootCanvas);

                var canvasDescription = ScriptableObject.CreateInstance<PSDImportedCanvasDescription>();
                canvasDescription.Width = pSDData.Width;
                canvasDescription.Height = pSDData.Height;
                canvasDescription.BitDepth = pSDData.Depth;
                canvasDescription.name = "CanvasDescription";
                ctx.AddObjectToAsset(canvasDescription.name, canvasDescription);
                multiLayerImageCanvas.tttImportedCanvasDescription = canvasDescription;

                Profiler.EndSample();
                Profiler.BeginSample("CreateLayers");

                var mliImporter = new MultiLayerImageImporter(multiLayerImageCanvas, canvasDescription, ctx, psdBytes, CreatePSDImportedImage);
                mliImporter.AddLayers(pSDData.RootLayers);

                Profiler.EndSample();
                EditorUtility.DisplayProgressBar("Import Canvas", "CreatePreview", 0f);
                Profiler.BeginSample("CreatePreviews");
                try
                {
                    mliImporter.CreatePreview();
                }
                catch (Exception e) { Debug.LogException(e); }

                Profiler.EndSample();
                EditorUtility.DisplayProgressBar("Import Canvas", "SaveSubAsset", 0.5f);
                Profiler.BeginSample("SaveSubAssets");

                mliImporter.SaveSubAsset();

                Profiler.EndSample();
                Profiler.EndSample();
                EditorUtility.DisplayProgressBar("Import Canvas", "END", 1f);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
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

    }







}
