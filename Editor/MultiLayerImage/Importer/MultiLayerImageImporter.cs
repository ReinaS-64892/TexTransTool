#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.MultiLayerImageParser.LayerData;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using UnityEditor;
using net.rs64.TexTransCore.TransTextureCore.TransCompute;
using System.Text;
namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    public static class MultiLayerImageImporter
    {
        public static void ImportCanvasData(HandlerForFolderSaver ctx, CanvasData canvasData, Action<MultiLayerImageCanvas> PreSaveCallBack)
        {
            var prefabName = Path.GetFileName(ctx.SaveDirectory) + "-Canvas";
            var rootCanvas = new GameObject(prefabName);
            var multiLayerImageCanvas = rootCanvas.AddComponent<MultiLayerImageCanvas>();
            multiLayerImageCanvas.TextureSize = canvasData.Size;
            AddLayers(multiLayerImageCanvas.transform, ctx, canvasData.RootLayers);
            PreSaveCallBack.Invoke(multiLayerImageCanvas);
            PrefabUtility.SaveAsPrefabAsset(rootCanvas, Path.Combine(ctx.SaveDirectory, prefabName + ".prefab"));
            UnityEngine.Object.DestroyImmediate(rootCanvas);
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
                            if (rasterLayer.RasterTexture.Array == null) { UnityEngine.Object.DestroyImmediate(NewLayer); continue; }
                            var rasterLayerComponent = NewLayer.AddComponent<RasterLayer>();
                            rasterLayerComponent.RasterTexture = ctx.AddAsset(rasterLayer.RasterTexture, layer.LayerName + "_Tex");
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
                    var mask = new LayerMask();
                    mask.LayerMaskDisabled = abstractLayerData.LayerMask.LayerMaskDisabled;
                    abstractLayer.LayerMask = mask;
                    mask.MaskTexture = ctx.AddAsset(abstractLayerData.LayerMask.MaskTexture, abstractLayerData.LayerName + "_Mask");
                }
            }
        }

        public interface ITexture2DHandler
        {
            Texture2D AddAsset(TwoDimensionalMap<Color32> TexMap, string TexName);
        }

        // public class HandlerForAssetImporterContext : ITexture2DHandler
        // {
        //     public AssetImportContext ctx;

        //     public HandlerForAssetImporterContext(AssetImportContext assetImportContext)
        //     {
        //         ctx = assetImportContext;
        //     }

        //     public Texture2D AddAsset(Texture2D tex)
        //     {
        //         ctx.AddObjectToAsset(tex.name, tex);
        //         return tex;
        //     }
        // }

        public class HandlerForFolderSaver : ITexture2DHandler
        {
            public string SaveDirectory;

            public const string RasterImageData = "RasterImageData";

            public HandlerForFolderSaver(string v)
            {
                SaveDirectory = v;
            }

            public Texture2D AddAsset(TwoDimensionalMap<Color32> TexMap, string TexName)
            {
                if (!Directory.Exists(SaveDirectory)) { Directory.CreateDirectory(SaveDirectory); }
                if (!Directory.Exists(Path.Combine(SaveDirectory, RasterImageData))) { Directory.CreateDirectory(Path.Combine(SaveDirectory, RasterImageData)); }
                var path = Path.Combine(SaveDirectory, RasterImageData, TexName) + ".png";
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                var tex2D = new Texture2D(TexMap.MapSize.x, TexMap.MapSize.y, TextureFormat.RGBA32, false);
                tex2D.SetPixelData(TexMap.Array, 0);
                File.WriteAllBytes(path, tex2D.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex2D);

                AssetDatabase.ImportAsset(path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                importer.maxTextureSize = 1024;
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