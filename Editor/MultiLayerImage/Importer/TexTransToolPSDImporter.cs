#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using net.rs64.MultiLayerImageParser.PSD;
using net.rs64.TexTransCore.LayerData;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using System.Linq;
namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [ScriptedImporter(1, "psd", AutoSelect = false)]
    public class TexTransToolPSDImporter : ScriptedImporter
    {
        public Texture2D DefaultReplaceTexture;

        [MenuItem("Assets/TexTransTool/TTT PSD Importer", false)]
        static void ChangeImporter()
        {
            foreach (var obj in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var ext = Path.GetExtension(path);
                if (ext != ".psd") { continue; }

                var importer = AssetImporter.GetAtPath(path);
                if (importer is TexTransToolPSDImporter)
                {
                    AssetDatabaseExperimental.ClearImporterOverride(path);
                }
                else
                {
                    AssetDatabaseExperimental.SetImporterOverride<TexTransToolPSDImporter>(path);
                }

            }
        }
        public override void OnImportAsset(AssetImportContext ctx)
        {

            var rootCanvas = new GameObject(Path.GetFileNameWithoutExtension(ctx.assetPath));
            ctx.AddObjectToAsset("RootCanvas", rootCanvas);
            ctx.SetMainObject(rootCanvas);

            var pSDData = PSDHighLevelParser.Parse(PSDLowLevelParser.Parse(ctx.assetPath));

            MultiLayerImageImporter.ImportCanvasData(ctx, rootCanvas, (CanvasData)pSDData);
        }


    }
}
#endif