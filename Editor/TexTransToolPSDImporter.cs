#if UNITY_EDITOR
using System.IO;
using net.rs64.PSD.parser;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "psd", AutoSelect = false)]
public class TexTransToolPSDImporter : ScriptedImporter
{
    [MenuItem("Assets/TexTransTool/TTT PSD Importer", false)]
    static void ChangeImporter()
    {
        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            var ext = Path.GetExtension(path);
            if (ext == ".psd")
            {
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
    }
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var pSDData = PSDLowLevelParser.Pase(ctx.assetPath);

        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ctx.AddObjectToAsset("Main obj", quad);
        ctx.SetMainObject(quad);

        var count = 0;
        foreach (var channelImageData in pSDData.LayerInfo.ChannelImageData)
        {
            // if (channelImageData.Preview != null)
            // {
            //     channelImageData.Preview.name = "ImageData " + count;
            //     ctx.AddObjectToAsset("ImageData " + count, channelImageData.Preview);
            // }
            count += 1;
        }
    }
}
#endif