using System.IO;
using net.rs64.MultiLayerImageParser.PSD;
using net.rs64.MultiLayerImageParser.LayerData;
using UnityEditor;
using UnityEngine;
using net.rs64.TexTransTool.ReferenceResolver.MLIResolver;
namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    internal class TexTransToolPSDImporter
    {
        [MenuItem("Assets/TexTransTool/TTT PSD Importer", false)]
        public static void ImportPSD()
        {

            if (!EditorUtility.DisplayDialog(
                "TexTransTool PSD Importer",
"The PSD Importer is an experimental feature and is subject to change or removal without notice".GetLocalize() + "\n" +
"Importing a PSD can take a very long time.".GetLocalize() + "\n" +
"\n" +
"Do you really want to import?".GetLocalize(),
                 "Yes".GetLocalize(), "No".GetLocalize())) { return; }

            foreach (var select in Selection.objects)
            {
                var souseTex2D = select as Texture2D;
                if (souseTex2D == null) { continue; }
                var targetPSDPath = AssetDatabase.GetAssetPath(souseTex2D);
                if (string.IsNullOrWhiteSpace(targetPSDPath)) { continue; }
                if (Path.GetExtension(targetPSDPath) != ".psd") { continue; }

                try
                {
                    EditorUtility.DisplayProgressBar("Parse PSD", "LowLevelParser", 0);
                    var lowPSDData = PSDLowLevelParser.Parse(targetPSDPath);
                    EditorUtility.DisplayProgressBar("Parse PSD", "HighLevelParser", 0.5f);
                    var pSDData = PSDHighLevelParser.Parse(lowPSDData);
                    EditorUtility.DisplayProgressBar("Parse PSD", "End", 1);


                    MultiLayerImageImporter.ImportCanvasData(
                        new MultiLayerImageImporter.HandlerForFolderSaver(targetPSDPath.Replace(".psd", "")), (CanvasData)pSDData,
                        multiLayerImageCanvas => multiLayerImageCanvas.gameObject.AddComponent<AbsoluteTextureResolver>().Texture = souseTex2D
                        );
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }


    }
}
