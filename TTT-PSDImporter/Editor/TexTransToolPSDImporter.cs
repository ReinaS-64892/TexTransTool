using System.IO;
using net.rs64.TexTransTool.MultiLayerImage.LayerData;
using net.rs64.PSDParser;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.MultiLayerImage.Importer;
using Unity.Collections;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Profiling;
using net.rs64.TexTransTool.PSDParser;
using System;
using System.Linq;

namespace net.rs64.TexTransTool.PSDImporter
{
    /*
    これらの情報が変わると、インポーター再設定だから変わらないように気を付けないといけない。
    AssemblyName: net.rs64.ttt-psd-importer.editor
    FullName: net.rs64.TexTransTool.PSDImporter.TexTransToolPSDImporter
    */
    [ScriptedImporter(2, new string[] { "psb" }, new string[] { "psd" }, AllowCaching = true)]
    public class TexTransToolPSDImporter : ScriptedImporter
    {
        public PSDImportMode ImportMode = PSDImportMode.Auto;
        public override void OnImportAsset(AssetImportContext ctx)
        {
            EditorUtility.DisplayProgressBar("Parse PSD", "ReadBytes", 0.0f);

            Profiler.BeginSample("ParsePSD");
            Profiler.BeginSample("ReadBytes");

            var psdBytes = File.ReadAllBytes(ctx.assetPath);

            Profiler.EndSample();
            Profiler.BeginSample("LowLevel");

            var lowPSDData = PSDLowLevelParser.Parse(psdBytes);

            OutputParseError(lowPSDData);
            Profiler.EndSample();
            Profiler.BeginSample("LowLevel");

            PSDHighLevelParser.PSDImportMode? importMode = ImportMode is PSDImportMode.Auto ? null : (PSDHighLevelParser.PSDImportMode)ImportMode;
            var pSDData = PSDHighLevelParser.Parse(lowPSDData, importMode);

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
                canvasDescription.IsPSB = lowPSDData.IsPSB;
                canvasDescription.name = "CanvasDescription";
                ctx.AddObjectToAsset(canvasDescription.name, canvasDescription);
                multiLayerImageCanvas.tttImportedCanvasDescription = canvasDescription;

                Profiler.EndSample();
                Profiler.BeginSample("CreateLayers");

                var mliImporter = new MultiLayerImageImporter(multiLayerImageCanvas, canvasDescription, ctx, CreatePSDImportedImage);
                mliImporter.AddLayers(pSDData.RootLayers);

                // TODO : これもうちょっと何とかしてもいいかも ... ?
                var imageDataSectionImage = ScriptableObject.CreateInstance<PSDImportedImageDataSectionImage>();
                imageDataSectionImage.CanvasDescription = canvasDescription;
                imageDataSectionImage.ImageDataSectionData = new(lowPSDData);
                imageDataSectionImage.name = "PSDImageDataSectionImage";


                Profiler.EndSample();
                EditorUtility.DisplayProgressBar("Import Canvas", "SaveSubAsset", 0.5f);
                Profiler.BeginSample("SaveSubAssets");

                mliImporter.SaveSubAsset();

                // TODO : 上と同様
                ctx.AddObjectToAsset("PSDImageDataSectionImage", imageDataSectionImage);

                Profiler.EndSample();
                Profiler.EndSample();
                EditorUtility.DisplayProgressBar("Import Canvas", "END", 1f);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void OutputParseError(PSDLowLevelParser.PSDLowLevelData lowPSDData)
        {
            foreach (var LayerRecord in lowPSDData.LayerInfo.LayerRecords)
            {
                foreach (var error in LayerRecord.AdditionalLayerInformation.Where(a => a.ParseError is not null))
                {
                    Debug.LogError(LayerRecord.LayerName + ":" + error.GetType().Name);
                    Debug.LogException(error.ParseError);
                }
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

        public enum PSDImportMode
        {
            Auto = -1,
            Unknown = 0,
            Photoshop = 2,
            ClipStudioPaint = 3,
            SAI = 4,
        }

    }

}
