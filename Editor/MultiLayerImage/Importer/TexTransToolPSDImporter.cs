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
namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    public class TexTransToolPSDImporter
    {
        [MenuItem("Assets/TexTransTool/TTT PSD Importer", false)]
        public static void ImportPSD()
        {
            var targetPSDPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrWhiteSpace(targetPSDPath)) { return; }
            if (Path.GetExtension(targetPSDPath) != ".psd") { return; }

            var rootCanvas = new GameObject(Path.GetFileNameWithoutExtension(targetPSDPath) + "-Canvas");

            var pSDData = PSDHighLevelParser.Parse(PSDLowLevelParser.Parse(targetPSDPath));

            var multiLayerImageCanvas = MultiLayerImageImporter.ImportCanvasData(new MultiLayerImageImporter.HandlerForFolderSaver(targetPSDPath.Replace(".psd", "")), rootCanvas, (CanvasData)pSDData);

            PrefabUtility.SaveAsPrefabAsset(rootCanvas, Path.Combine(targetPSDPath.Replace(".psd", ""), Path.GetFileNameWithoutExtension(targetPSDPath) + "-Canvas" + ".prefab"));
        }


    }
}
#endif