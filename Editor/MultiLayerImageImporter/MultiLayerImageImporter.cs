using System.Collections.Generic;
using net.rs64.MultiLayerImage.LayerData;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;
using System.Threading.Tasks;
using UnityEditor.AssetImporters;
using Unity.Collections;
using net.rs64.TexTransUnityCore.MipMap;
using Unity.Mathematics;
using UnityEngine.Profiling;
using net.rs64.TexTransUnityCore;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    internal class MultiLayerImageImporter
    {
        MultiLayerImageCanvas _multiLayerImageCanvas;
        TTTImportedCanvasDescription _tttImportedCanvasDescription;
        AssetImportContext _ctx;
        List<TTTImportedImage> _tttImportedImages = new();
        CreateImportedImage _imageImporter;
        byte[] _sourceBytes;
        Dictionary<TTTImportedImage, string> _layerAtPath = new();
        string _path = "";

        internal delegate TTTImportedImage CreateImportedImage(ImportRasterImageData importRasterImage);
        internal delegate Task<NativeArray<Color32>> GetPreviewImage(byte[] sourceBytes, TTTImportedImage importRasterImage);//つまり正方形にオフセットの入った後の画像を取得するやつ RGBA32

        internal MultiLayerImageImporter(MultiLayerImageCanvas multiLayerImageCanvas, TTTImportedCanvasDescription tttImportedCanvasDescription, AssetImportContext assetImportContext, byte[] sourceBytes, CreateImportedImage imageImporter)
        {
            _multiLayerImageCanvas = multiLayerImageCanvas;
            _ctx = assetImportContext;
            _imageImporter = imageImporter;
            _tttImportedCanvasDescription = tttImportedCanvasDescription;
            _sourceBytes = sourceBytes;

        }

        internal void AddLayers(List<AbstractLayerData> abstractLayers) { AddLayers(abstractLayers, null); }
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
            var canvasSize = new int2(_tttImportedCanvasDescription.Width, _tttImportedCanvasDescription.Height);

            using (var fullNATex = new NativeArray<Color32>(canvasSize.x * canvasSize.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            {
                foreach (var importedImage in _tttImportedImages)
                {
                    Profiler.BeginSample("CreatePreview -" + importedImage.name);
                    Profiler.BeginSample("LoadImage");

                    var jobResult = importedImage.LoadImage(_sourceBytes, fullNATex);

                    Profiler.EndSample();
                    Texture2D tex2d;
                    if (math.max(canvasSize.x, canvasSize.y) <= 1024)
                    {
                        Profiler.BeginSample("CratePrevTex");

                        tex2d = new Texture2D(canvasSize.x, canvasSize.y, TextureFormat.RGBA32, false);
                        tex2d.alphaIsTransparency = true;

                        tex2d.LoadRawTextureData(jobResult.GetResult);
                        EditorUtility.CompressTexture(tex2d, TextureFormat.BC7, 100);

                        Profiler.EndSample();
                    }
                    else
                    {
                        Profiler.BeginSample("CreateMipDispatch");

                        var mipMapCount = MipMapUtility.MipMapCountFrom(Mathf.Max(canvasSize.x, canvasSize.y), 1024);
                        _ = jobResult.GetResult;
                        var mipJobResult = MipMapUtility.GenerateAverageMips(fullNATex, canvasSize, mipMapCount);

                        Profiler.EndSample();
                        Profiler.BeginSample("CratePrevTex");

                        tex2d = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
                        tex2d.alphaIsTransparency = true;

                        tex2d.LoadRawTextureData(mipJobResult.GetResult[mipMapCount]);
                        EditorUtility.CompressTexture(tex2d, TextureFormat.BC7, 100);
                        foreach (var n2da in mipJobResult.GetResult.Skip(1)) { n2da.Dispose(); }

                        Profiler.EndSample();
                    }
                    Profiler.BeginSample("SetTexDataAndCompress");

                    tex2d.Apply(true, true);
                    importedImage.PreviewTexture = tex2d;

                    Profiler.EndSample();
                    Profiler.EndSample();
                }


            }
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
