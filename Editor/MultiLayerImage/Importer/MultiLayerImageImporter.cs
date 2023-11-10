#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.MultiLayerImageParser.LayerData;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using UnityEditor;
namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    public static class MultiLayerImageImporter
    {
        public static MultiLayerImageCanvas ImportCanvasData(ITexture2DHandler ctx, GameObject rootCanvas, CanvasData canvasData)
        {
            var multiLayerImageCanvas = rootCanvas.AddComponent<MultiLayerImageCanvas>();
            multiLayerImageCanvas.TextureSize = canvasData.Size;
            AddLayers(multiLayerImageCanvas.transform, ctx, canvasData.RootLayers);
            return multiLayerImageCanvas;
        }
        public static void AddLayers(Transform thisTransForm, ITexture2DHandler ctx, List<AbstractLayerData> abstractLayers)
        {
            var parent = thisTransForm;
            var count = 0;
            foreach (var layer in abstractLayers.Reverse<AbstractLayerData>())
            {
                var NewLayer = new GameObject(layer.LayerName);
                // var NewLayer = new GameObject(count + "-" + layer.LayerName);
                count += 1;
                NewLayer.transform.SetParent(parent);

                switch (layer)
                {
                    case RasterLayerData rasterLayer:
                        {
                            if (rasterLayer.RasterTexture == null) { UnityEngine.Object.DestroyImmediate(NewLayer); continue; }
                            var rasterLayerComponent = NewLayer.AddComponent<RasterLayer>();
                            rasterLayerComponent.RasterTexture = ctx.AddAsset(rasterLayer.RasterTexture);
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
                    mask.MaskTexture = ctx.AddAsset(mask.MaskTexture);
                }
            }
        }

        public interface ITexture2DHandler
        {
            Texture2D AddAsset(Texture2D tex);
        }

        public class HandlerForAssetImporterContext : ITexture2DHandler
        {
            public AssetImportContext ctx;

            public HandlerForAssetImporterContext(AssetImportContext assetImportContext)
            {
                ctx = assetImportContext;
            }

            public Texture2D AddAsset(Texture2D tex)
            {
                ctx.AddObjectToAsset(tex.name, tex);
                return tex;
            }
        }

        public class HandlerForFolderSaver : ITexture2DHandler
        {
            public string SaveDirectory;

            public const string RasterImageData = "RasterImageData";

            public HandlerForFolderSaver(string v)
            {
                SaveDirectory = v;
            }

            public Texture2D AddAsset(Texture2D tex)
            {
                if (!Directory.Exists(SaveDirectory)) { Directory.CreateDirectory(SaveDirectory); }
                if (!Directory.Exists(Path.Combine(SaveDirectory, RasterImageData))) { Directory.CreateDirectory(Path.Combine(SaveDirectory, RasterImageData)); }
                var path = Path.Combine(SaveDirectory, RasterImageData, tex.name) + ".png";
                File.WriteAllBytes(path, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                importer.maxTextureSize = 512;
                importer.textureCompression = TextureImporterCompression.CompressedLQ;
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                importer.SaveAndReimport();
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }

    }
}
#endif