using System.Collections;
using System.Collections.Generic;
using System.IO;
using net.rs64.MultiLayerImageParser.PSD;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.MultiLayerImage.Importer;
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
            EditorUtility.DisplayProgressBar("Parse PSD", "LowLevelParser", 0);
            var lowPSDData = PSDLowLevelParser.Parse(ctx.assetPath);
            EditorUtility.DisplayProgressBar("Parse PSD", "HighLevelParser", 0.5f);
            var pSDData = PSDHighLevelParser.Parse(lowPSDData);
            EditorUtility.DisplayProgressBar("Parse PSD", "End", 1);
            lowPSDData.Dispose();

            try
            {
                EditorUtility.DisplayProgressBar("Import Canvas", "Build Layer", 0);


                var prefabName = Path.GetFileName(ctx.assetPath) + "-Canvas";
                var rootCanvas = new GameObject(prefabName);
                var multiLayerImageCanvas = rootCanvas.AddComponent<MultiLayerImageCanvas>();

                ctx.AddObjectToAsset("RootCanvas", rootCanvas);
                ctx.SetMainObject(rootCanvas);

                MultiLayerImageImporter.AddLayers(multiLayerImageCanvas.transform, ctx, pSDData.RootLayers);

                // deploy.FinalizeTex2D();
            }
            finally
            {
                pSDData.Dispose();
                EditorUtility.ClearProgressBar();
            }

        }
    }
}