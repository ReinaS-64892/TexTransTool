using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using net.rs64.MultiLayerImage.LayerData;
using net.rs64.MultiLayerImage.Parser.PSD;
using net.rs64.TexTransCore.MipMap;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using Unity.Collections;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [ScriptedImporter(1, new string[] { }, new string[] { "psd" }, AllowCaching = true)]
    public class TexTransToolPSDImporter : ScriptedImporter
    {
        public DownScalingAlgorism PreviewImageDownScalingAlgorism;
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

                var mliImporter = new MultiLayerImageImporter(multiLayerImageCanvas, canvasDescription, ctx, psdBytes, CreatePSDImportedImage, PreviewImageDownScalingAlgorism);
                mliImporter.AddLayers(pSDData.RootLayers);

                EditorUtility.DisplayProgressBar("Import Canvas", "CreatePreview", 0f);
                mliImporter.CreatePreview();
                EditorUtility.DisplayProgressBar("Import Canvas", "SaveSubAsset", 0.5f);
                mliImporter.SaveSubAsset();
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

    [CustomEditor(typeof(TexTransToolPSDImporter))]
    class TexTransToolPSDImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("TexTransToolPSDImporter" + " " + "Common:ExperimentalWarning".GetLocalize(), MessageType.Warning);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewImageDownScalingAlgorism"));

            base.ApplyRevertGUI();
        }
        public override bool HasPreviewGUI() { return true; }
        public override void DrawPreview(Rect previewArea)
        {
            var importer = target as TexTransToolPSDImporter;
            var previewTex = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath).FirstOrDefault(i => i.name == "TTT-CanvasPreviewResult") as Texture2D;
            if (previewTex == null) { return; }

            EditorGUI.DrawTextureTransparent(previewArea, previewTex, ScaleMode.ScaleToFit);
        }

    }







}
