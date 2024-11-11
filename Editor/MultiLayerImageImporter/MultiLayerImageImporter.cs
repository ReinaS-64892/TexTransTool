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

        internal void CreatePreview()
        {
            var texFormat = _tttImportedCanvasDescription.ImportedImageFormat.ToUnityTextureFormat(TexTransCoreTextureChannel.RGBA);
            var ppB = EnginUtil.GetPixelParByte(_tttImportedCanvasDescription.ImportedImageFormat, TexTransCoreTextureChannel.RGBA);
            var length = _tttImportedCanvasDescription.Width * _tttImportedCanvasDescription.Height * ppB;
            using var naBuf = new NativeArray<byte>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var resizing = math.max(_tttImportedCanvasDescription.Width, _tttImportedCanvasDescription.Height) > 1024;

            var fullTexTemp = resizing ? new Texture2D(_tttImportedCanvasDescription.Width, _tttImportedCanvasDescription.Height, texFormat, false) : null;


            foreach (var importedImage in _tttImportedImages)
            {
                Profiler.BeginSample("CreatePreview -" + importedImage.name);
                UnsafeNativeArrayUtility.ClearMemory(naBuf);
                Profiler.BeginSample("LoadImage");

                importedImage.LoadImage(_ttImportedCanvasSource, naBuf);

                Profiler.EndSample();
                Texture2D tex2d;
                if (resizing is false)
                {
                    Profiler.BeginSample("CratePrevTex");

                    tex2d = new Texture2D(_tttImportedCanvasDescription.Width, _tttImportedCanvasDescription.Height, texFormat, false);
                    tex2d.alphaIsTransparency = true;

                    tex2d.LoadRawTextureData(naBuf);
                    EditorUtility.CompressTexture(tex2d, TextureFormat.BC7, 100);

                    Profiler.EndSample();
                }
                else
                {
                    Profiler.BeginSample("CreateAndResizing");
                    fullTexTemp.GetRawTextureData<byte>().CopyFrom(naBuf);
                    tex2d = new Texture2D(1024, 1024, texFormat, false);

                    for (var y = 0; tex2d.height > y; y += 1)
                    {
                        for (var x = 0; tex2d.width > x; x += 1)
                        {
                            tex2d.SetPixel(x, y, fullTexTemp.GetPixelBilinear(x / (float)(tex2d.width - 1), y / (float)(tex2d.height - 1)));
                        }
                    }

                    tex2d.alphaIsTransparency = true;
                    EditorUtility.CompressTexture(tex2d, TextureFormat.BC7, 100);

                    Profiler.EndSample();
                }
                Profiler.BeginSample("SetTexDataAndCompress");

                tex2d.Apply(true, true);
                importedImage.PreviewTexture = tex2d;

                Profiler.EndSample();
                Profiler.EndSample();
            }

            if (fullTexTemp != null) UnityEngine.Object.DestroyImmediate(fullTexTemp);
        }

        public void SaveSubAsset()
        {
            // var NameHash = new HashSet<string>() { "TTT-CanvasPreviewResult", "TTT-CanvasPreviewResult-Material" };
            foreach (var image in _tttImportedImages.Reverse<TTTImportedImage>())
            {
                // var name = image.name;
                // if (NameHash.Contains(name))
                // {
                //     var addCount = 1;
                //     while (NameHash.Contains(name + "-" + addCount)) { addCount += 1; }
                //     name = name + "-" + addCount;
                // }
                // NameHash.Add(name);

                // image.name = name;

                _ctx.AddObjectToAsset(_layerAtPath[image] + "/" + image.name, image);
                try
                {
                    image.PreviewTexture.name = image.name + "_Preview";
                    _ctx.AddObjectToAsset(_layerAtPath[image] + "/" + image.PreviewTexture.name, image.PreviewTexture);
                }
                catch (Exception e) { Debug.LogException(e); }
            }
        }
    }
}
