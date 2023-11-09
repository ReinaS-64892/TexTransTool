#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.MultiLayerImageParser.LayerData;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using System.Linq;
using System;
namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    public static class MultiLayerImageImporter
    {
        public static MultiLayerImageCanvas ImportCanvasData(AssetImportContext ctx, GameObject rootCanvas, CanvasData canvasData)
        {
            var multiLayerImageCanvas = rootCanvas.AddComponent<MultiLayerImageCanvas>();
            multiLayerImageCanvas.TextureSize = canvasData.Size;
            AddLayers(multiLayerImageCanvas.transform, ctx, canvasData.RootLayers);
            return multiLayerImageCanvas;
        }
        public static void AddLayers(Transform thisTransForm, AssetImportContext ctx, List<AbstractLayerData> abstractLayers)
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
                            if (rasterLayer.RasterTexture == null) { UnityEngine.Object.DestroyImmediate(NewLayer); continue; }
                            ctx.AddObjectToAsset(rasterLayer.RasterTexture.name, rasterLayer.RasterTexture);
                            var rasterLayerComponent = NewLayer.AddComponent<RasterLayer>();
                            rasterLayerComponent.RasterTexture = rasterLayer.RasterTexture;
                            rasterLayerComponent.BlendMode = layer.BlendMode;
                            rasterLayerComponent.Opacity = layer.Opacity;
                            rasterLayerComponent.Clipping = layer.Clipping;
                            rasterLayerComponent.Visible = layer.Visible;
                            SetMaskTexture(rasterLayerComponent, rasterLayer);
                            break;
                        }
                    case LayerFolderData layerFolder:
                        {

                            var layerFolderComponent = NewLayer.AddComponent<LayerFolder>();
                            layerFolderComponent.PassThrough = layerFolder.PassThrough;
                            layerFolderComponent.BlendMode = layer.BlendMode;
                            layerFolderComponent.Opacity = layer.Opacity;
                            layerFolderComponent.Clipping = layer.Clipping;
                            layerFolderComponent.Visible = layer.Visible;
                            SetMaskTexture(layerFolderComponent, layerFolder);
                            AddLayers(NewLayer.transform, ctx, layerFolder.Layers);
                            break;
                        }
                }
            }

            void SetMaskTexture(AbstractLayer abstractLayer, AbstractLayerData abstractLayerData)
            {
                if (abstractLayerData.LayerMask != null)
                {
                    var mask = abstractLayerData.LayerMask;
                    abstractLayer.LayerMask = mask;
                    ctx.AddObjectToAsset(mask.MaskTexture.name, mask.MaskTexture);
                }
            }
        }

    }
}
#endif