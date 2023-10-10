#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using net.rs64.PSD.parser;
using net.rs64.TexTransCore.Layer;
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

            var pSDData = PSDHighLevelParser.Pase(PSDLowLevelParser.Pase(ctx.assetPath));


            var multiLayerImageCanvas = rootCanvas.AddComponent<MultiLayerImageCanvas>();
            multiLayerImageCanvas.TextureSize = pSDData.Size;
            AddLayers(multiLayerImageCanvas.transform, ctx, pSDData.RootLayers);

        }

        public void AddLayers(Transform thisTransForm, AssetImportContext ctx, List<AbstractLayerData> abstractLayers)
        {
            var parent = thisTransForm;
            var count = 0;
            foreach (var layer in abstractLayers.Reverse<AbstractLayerData>())
            {
                var NewLayer = new GameObject(count + "-" + layer.LayerName);
                count += 1;
                NewLayer.transform.SetParent(parent);

                switch (layer)
                {
                    case RasterLayerData rasterLayer:
                        {
                            ctx.AddObjectToAsset(layer.LayerName, rasterLayer.RasterTexture);
                            var rasterLayerComponent = NewLayer.AddComponent<RasterLayer>();
                            rasterLayerComponent.RasterTexture = rasterLayer.RasterTexture;
                            rasterLayerComponent.TexturePivot = rasterLayer.TexturePivot;
                            rasterLayerComponent.BlendMode = layer.BlendMode;
                            rasterLayerComponent.Opacity = layer.Opacity;
                            rasterLayerComponent.Clipping = layer.Clipping;
                            rasterLayerComponent.Visible = layer.Visible;
                            break;
                        }
                    case LayerFolderData layerFolder:
                        {

                            var layerFolderComponent = NewLayer.AddComponent<LayerFolder>();
                            layerFolderComponent.BlendMode = layer.BlendMode;
                            layerFolderComponent.Opacity = layer.Opacity;
                            layerFolderComponent.Clipping = layer.Clipping;
                            layerFolderComponent.Visible = layer.Visible;
                            layerFolderComponent.PassThrough = layerFolder.PassThrough;
                            AddLayers(NewLayer.transform, ctx, layerFolder.Layers);
                            break;
                        }
                }
            }
        }
    }
}
#endif