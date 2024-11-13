using System.Collections.Generic;
using net.rs64.TexTransTool.MultiLayerImage.LayerData;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;
using System.Threading.Tasks;
using UnityEditor.AssetImporters;
using Unity.Collections;
using net.rs64.TexTransCoreEngineForUnity.MipMap;
using Unity.Mathematics;
using UnityEngine.Profiling;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity.Unsafe;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    public class MultiLayerImageImporter
    {
        MultiLayerImageCanvas _multiLayerImageCanvas;
        TTTImportedCanvasDescription _tttImportedCanvasDescription;
        ITTImportedCanvasSource _ttImportedCanvasSource;
        AssetImportContext _ctx;
        List<TTTImportedImage> _tttImportedImages = new();
        CreateImportedImage _imageImporter;
        Dictionary<TTTImportedImage, string> _layerAtPath = new();
        string _path = "";

        public delegate TTTImportedImage CreateImportedImage(ImportRasterImageData importRasterImage);

        public MultiLayerImageImporter(MultiLayerImageCanvas multiLayerImageCanvas, TTTImportedCanvasDescription tttImportedCanvasDescription, AssetImportContext assetImportContext, ITTImportedCanvasSource ttImportedCanvasSource, CreateImportedImage imageImporter)
        {
            _multiLayerImageCanvas = multiLayerImageCanvas;
            _ctx = assetImportContext;
            _imageImporter = imageImporter;
            _tttImportedCanvasDescription = tttImportedCanvasDescription;
            _ttImportedCanvasSource = ttImportedCanvasSource;

        }

        public void AddLayers(List<AbstractLayerData> abstractLayers) { AddLayers(abstractLayers, null); }
        private void AddLayers(List<AbstractLayerData> abstractLayers, Transform parent = null)
        {
            parent ??= _multiLayerImageCanvas.transform;
            var count = 0;
            foreach (var layer in abstractLayers.Reverse<AbstractLayerData>())
            {
                var newLayer = new GameObject(layer.LayerName);
                count += 1;
                newLayer.transform.SetParent(parent);

                switch (layer)
                {
                    case RasterLayerData rasterLayer:
                        {
                            CreateRasterLayer(newLayer, rasterLayer);
                            break;
                        }
                    case LayerFolderData layerFolder:
                        {
                            CreateLayerFolder(newLayer, layerFolder);
                            break;
                        }
                    default:
                        {
                            if (SpecialLayerDataImporterUtil.SpecialLayerDataImporters.ContainsKey(layer.GetType()))
                                SpecialLayerDataImporterUtil.SpecialLayerDataImporters[layer.GetType()].CreateSpecial(CopyFromData, newLayer, layer);
                            break;
                        }
                }
            }

        }

        private void CreateRasterLayer(GameObject newLayer, RasterLayerData rasterLayer)
        {
            if (rasterLayer.RasterTexture == null) { newLayer.name += " is Unsupported Layer"; return; }//データがないならインポートできない扱いをしておく。
            if (rasterLayer is EmptyOrUnsupported) { newLayer.name += " is empty or unsupported layer"; }//空のラスターレイヤーか非対応なものかの判別はつかないから仕方がない。

            var rasterLayerComponent = newLayer.AddComponent<RasterImportedLayer>();
            CopyFromData(rasterLayerComponent, rasterLayer);

            var importedImage = _imageImporter.Invoke(rasterLayer.RasterTexture);
            importedImage.name = rasterLayer.LayerName + "_Tex";
            importedImage.CanvasDescription = _tttImportedCanvasDescription;
            _tttImportedImages.Add(importedImage);
            _layerAtPath[importedImage] = _path;
            rasterLayerComponent.ImportedImage = importedImage;
        }

        private ILayerMask MaskTexture(LayerMaskData maskData, string layerName)
        {
            if (maskData == null) { return new LayerMask(); }

            var importMask = _imageImporter.Invoke(maskData.MaskTexture);
            importMask.name = layerName + "_MaskTex";
            importMask.CanvasDescription = _tttImportedCanvasDescription;
            _tttImportedImages.Add(importMask);
            _layerAtPath[importMask] = _path;
            return new TTTImportedLayerMask(maskData.LayerMaskDisabled, importMask);

        }

        private void CreateLayerFolder(GameObject newLayer, LayerFolderData layerFolder, string path = "")
        {
            var layerFolderComponent = newLayer.AddComponent<LayerFolder>();

            CopyFromData(layerFolderComponent, layerFolder);
            layerFolderComponent.PassThrough = layerFolder.PassThrough;
            var beforePath = _path;
            _path = beforePath + "/" + newLayer.name;
            AddLayers(layerFolder.Layers, newLayer.transform);
            _path = beforePath;
        }

        internal void CopyFromData(AbstractLayer abstractLayer, AbstractLayerData abstractLayerData)
        {
            abstractLayer.BlendTypeKey = abstractLayerData.BlendTypeKey;
            abstractLayer.Opacity = abstractLayerData.Opacity;
            abstractLayer.Clipping = abstractLayerData.Clipping;
            abstractLayer.Visible = abstractLayerData.Visible;
            abstractLayer.LayerMask = MaskTexture(abstractLayerData.LayerMask, abstractLayerData.LayerName);
        }

        public void SaveSubAsset()
        {
            var guid = AssetDatabase.AssetPathToGUID(_ctx.assetPath);
            if (string.IsNullOrWhiteSpace(guid) is false) CanvasImportedImagePreviewManager.InvalidatesCache(guid);
            else CanvasImportedImagePreviewManager.InvalidatesCacheAll();

            foreach (var image in _tttImportedImages.Reverse<TTTImportedImage>()) { _ctx.AddObjectToAsset(_layerAtPath[image] + "/" + image.name, image); }
        }
    }
}
