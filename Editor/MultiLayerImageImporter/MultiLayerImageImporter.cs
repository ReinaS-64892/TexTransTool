using System.Collections.Generic;
using net.rs64.MultiLayerImage.LayerData;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;
using System.Threading.Tasks;
using UnityEditor.AssetImporters;
using Unity.Collections;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    internal class MultiLayerImageImporter
    {
        TTTImportedCanvasDescription _tttImportedCanvasDescription;
        AssetImportContext _ctx;
        List<TTTImportedImage> _tttImportedImages = new();
        CreateImportedImage _imageImporter;
        GetPreviewImage _previewImageTaskGenerator;
        byte[] _souseBytes;

        internal delegate TTTImportedImage CreateImportedImage(ImportRasterImageData importRasterImage);
        internal delegate Task<NativeArray<Color32>> GetPreviewImage(byte[] souseBytes, TTTImportedImage importRasterImage);//つまり正方形にオフセットの入った後の画像を取得するやつ RGBA32

        internal MultiLayerImageImporter(TTTImportedCanvasDescription tttImportedCanvasDescription,
                                         AssetImportContext assetImportContext,
                                         byte[] souseBytes,
                                         CreateImportedImage imageImporter,
                                         GetPreviewImage previewImageTaskGenerator)
        {
            _ctx = assetImportContext;
            _imageImporter = imageImporter;
            _tttImportedCanvasDescription = tttImportedCanvasDescription;
            _souseBytes = souseBytes;
            _previewImageTaskGenerator = previewImageTaskGenerator;
        }

        internal void AddLayers(Transform thisTransForm, List<AbstractLayerData> abstractLayers)
        {
            var parent = thisTransForm;
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
                    case HSVAdjustmentLayerData hSVAdjustmentLayerData:
                        {
                            CreateHSVAdjustmentLayer(newLayer, hSVAdjustmentLayerData);
                            break;
                        }
                }
            }

        }

        private void CreateHSVAdjustmentLayer(GameObject newLayer, HSVAdjustmentLayerData hSVAdjustmentLayerData)
        {
            var HSVAdjustmentLayerComponent = newLayer.AddComponent<HSVAdjustmentLayer>();
            CopyFromData(HSVAdjustmentLayerComponent, hSVAdjustmentLayerData);

            HSVAdjustmentLayerComponent.Hue = hSVAdjustmentLayerData.Hue;
            HSVAdjustmentLayerComponent.Saturation = hSVAdjustmentLayerData.Saturation;
            HSVAdjustmentLayerComponent.Lightness = hSVAdjustmentLayerData.Lightness;
        }

        private void CreateRasterLayer(GameObject newLayer, RasterLayerData rasterLayer)
        {
            if (rasterLayer.RasterTexture == null) { Debug.Log(rasterLayer.LayerName + " is Not RasterLayer"); UnityEngine.Object.DestroyImmediate(newLayer); return; }//ラスターレイヤーじゃないものはインポートできない。
            var rasterLayerComponent = newLayer.AddComponent<RasterImportedLayer>();
            CopyFromData(rasterLayerComponent, rasterLayer);

            var importedImage = _imageImporter.Invoke(rasterLayer.RasterTexture);
            importedImage.name = rasterLayer.LayerName + "_Tex";
            importedImage.CanvasDescription = _tttImportedCanvasDescription;
            _ctx.AddObjectToAsset(importedImage.name, importedImage);
            _tttImportedImages.Add(importedImage);
            rasterLayerComponent.ImportedImage = importedImage;
        }

        private ILayerMask MaskTexture(LayerMaskData maskData, string layerName)
        {
            if (maskData == null) { return new LayerMask(); }

            var importMask = _imageImporter.Invoke(maskData.MaskTexture);
            importMask.name = layerName + "_MaskTex";
            importMask.CanvasDescription = _tttImportedCanvasDescription;
            _ctx.AddObjectToAsset(importMask.name, importMask);
            _tttImportedImages.Add(importMask);
            return new TTTImportedLayerMask(maskData.LayerMaskDisabled, importMask);

        }

        private void CreateLayerFolder(GameObject newLayer, LayerFolderData layerFolder)
        {
            var layerFolderComponent = newLayer.AddComponent<LayerFolder>();

            CopyFromData(layerFolderComponent, layerFolder);
            layerFolderComponent.PassThrough = layerFolder.PassThrough;

            AddLayers(newLayer.transform, layerFolder.Layers);
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
            var imageCount = _tttImportedImages.Count;
            var task = new Task<NativeArray<Color32>>[imageCount];
            for (var i = 0; imageCount > i; i += 1)
            {
                task[i] = _previewImageTaskGenerator.Invoke(_souseBytes, _tttImportedImages[i]);
            }

            var setting = new TextureGenerationSettings(TextureImporterType.Default);
            setting.textureImporterSettings.alphaIsTransparency = true;
            setting.textureImporterSettings.mipmapEnabled = false;
            setting.textureImporterSettings.filterMode = FilterMode.Bilinear;
            setting.textureImporterSettings.readable = false;

            setting.platformSettings.maxTextureSize = 1024;
            setting.platformSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
            setting.platformSettings.textureCompression = TextureImporterCompression.Compressed;
            setting.platformSettings.compressionQuality = 100;

            setting.sourceTextureInformation.width = _tttImportedCanvasDescription.Width;
            setting.sourceTextureInformation.height = _tttImportedCanvasDescription.Height;
            setting.sourceTextureInformation.containsAlpha = true;
            setting.sourceTextureInformation.hdr = false;

            for (var i = 0; imageCount > i; i += 1)
            {
                using (var data = TaskAwaiter(task[i]).Result)
                {
                    var image = _tttImportedImages[i];

                    var output = TextureGenerator.GenerateTexture(setting, data);

                    image.PreviewTexture = output.texture;
                    image.PreviewTexture.name = image.name + "_Preview";
                    _ctx.AddObjectToAsset(output.texture.name, output.texture);
                }
            }

            async static Task<NativeArray<Color32>> TaskAwaiter(Task<NativeArray<Color32>> task)
            {
                return await task.ConfigureAwait(false);
            }


        }

        private static void ParallelExecuter<T>(Action<T> taskExecute, IEnumerable<T> taskData, int? forceParallelSize, Action<float> progressCallBack = null)
        {
            var parallelSize = forceParallelSize.HasValue ? forceParallelSize.Value : Environment.ProcessorCount;
            var taskQueue = new Queue<T>(taskData);
            var taskParallel = new Task[parallelSize];
            var encDataCount = taskQueue.Count; var nowIndex = 0;
            while (taskQueue.Count > 0)
            {
                for (int i = 0; taskParallel.Length > i; i += 1)
                {
                    if (taskQueue.Count > 0)
                    {
                        var task = taskQueue.Dequeue();
                        taskParallel[i] = Task.Run(() => taskExecute.Invoke(task));
                    }
                    else
                    {
                        taskParallel[i] = null;
                        break;
                    }
                }

                foreach (var task in taskParallel)
                {
                    if (task == null) { break; }
                    _ = TaskAwaiter(task).Result;
                    nowIndex += 1;
                    progressCallBack?.Invoke(nowIndex / (float)encDataCount);
                }
            }
        }

        public static async Task<bool> TaskAwaiter(Task task)
        {
            await task.ConfigureAwait(false);
            return true;
        }




    }
}
