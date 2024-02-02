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
                    case SolidColorLayerData solidColorLayerData:
                        {
                            CreateSolidColorLayer(newLayer, solidColorLayerData);
                            break;
                        }
                }
            }

        }

        private void CreateSolidColorLayer(GameObject newLayer, SolidColorLayerData solidColorLayerData)
        {
            var SolidColorLayerComponent = newLayer.AddComponent<SolidColorLayer>();
            CopyFromData(SolidColorLayerComponent, solidColorLayerData);

            SolidColorLayerComponent.Color = solidColorLayerData.Color;
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

            foreach (var taskResult in ParallelExecuter<(byte[] souseBytes, TTTImportedImage importRasterImage), NativeArray<Color32>>(
                i => _previewImageTaskGenerator.Invoke(i.souseBytes, i.importRasterImage),
                 _tttImportedImages.Select(i => (_souseBytes, i))))
            {
                using (var data = taskResult.TaskResult)
                {
                    var image = taskResult.TaskData.importRasterImage;

                    var output = TextureGenerator.GenerateTexture(setting, data);

                    image.PreviewTexture = output.texture;
                    image.PreviewTexture.name = image.name + "_Preview";
                    _ctx.AddObjectToAsset(output.texture.name, output.texture);
                }
            }
        }
        private static IEnumerable<(T TaskData, T2 TaskResult)> ParallelExecuter<T, T2>(Func<T, Task<T2>> taskExecute, IEnumerable<T> taskData, int? forceParallelSize = null, Action<float> progressCallBack = null)
        {
            var parallelSize = forceParallelSize.HasValue ? forceParallelSize.Value : Environment.ProcessorCount;
            var taskQueue = new Queue<T>(taskData);
            var taskParallel = new (T TaskData, Task<T2> Task)[parallelSize];
            var encDataCount = taskQueue.Count; var nowIndex = 0;
            while (taskQueue.Count > 0)
            {
                for (int i = 0; taskParallel.Length > i; i += 1)
                {
                    if (taskQueue.Count > 0)
                    {
                        var task = taskQueue.Dequeue();
                        taskParallel[i] = (task, Task.Run(() => taskExecute.Invoke(task)));
                    }
                    else
                    {
                        taskParallel[i] = (default, null);
                        break;
                    }
                }

                foreach (var taskPair in taskParallel)
                {
                    if (taskPair.Task == null) { break; }
                    yield return (taskPair.TaskData, TaskAwaiter(taskPair.Task).Result);
                    nowIndex += 1;
                    progressCallBack?.Invoke(nowIndex / (float)encDataCount);
                }
            }
            static async Task<T3> TaskAwaiter<T3>(Task<T3> task)
            {
                return await task.ConfigureAwait(false);
            }
        }
    }
}
