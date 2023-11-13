#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using net.rs64.MultiLayerImageParser.PSD;
using net.rs64.MultiLayerImageParser.LayerData;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using System.Linq;
using net.rs64.TexTransTool.ReferenceResolver.MLIResolver;
namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    public class TexTransToolPSDImporter
    {
        [MenuItem("Assets/TexTransTool/TTT PSD Importer", false)]
        public static void ImportPSD()
        {
            var souseTex2D = Selection.activeObject as Texture2D;
            if (souseTex2D == null) { return; }
            var targetPSDPath = AssetDatabase.GetAssetPath(souseTex2D);
            if (string.IsNullOrWhiteSpace(targetPSDPath)) { return; }
            if (Path.GetExtension(targetPSDPath) != ".psd") { return; }

            if (!EditorUtility.DisplayDialog(
                "TexTransTool PSD Importer",
@"PSDインポーターは、実験的機能で予告なく変更や削除される可能性があり、
PSDのインポートは非常に長い時間がかかる可能性があります。

本当にインポートしますか？".GetLocalize(),
                 "する".GetLocalize(), "しない".GetLocalize())) { return; }

            var pSDData = PSDHighLevelParser.Parse(PSDLowLevelParser.Parse(targetPSDPath));
            MultiLayerImageImporter.ImportCanvasData(
                new MultiLayerImageImporter.HandlerForFolderSaver(targetPSDPath.Replace(".psd", "")), (CanvasData)pSDData,
                multiLayerImageCanvas => multiLayerImageCanvas.gameObject.AddComponent<AbsoluteTextureResolver>().Texture = souseTex2D
                );
        }


    }
}
#endif