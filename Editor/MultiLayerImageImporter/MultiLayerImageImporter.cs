using System.Collections.Generic;
using net.rs64.MultiLayerImage.LayerData;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;
using System.Threading.Tasks;
using UnityEditor.AssetImporters;
using Unity.Collections;
using net.rs64.TexTransCore.MipMap;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    internal class MultiLayerImageImporter
    {
        MultiLayerImageCanvas _multiLayerImageCanvas;
        TTTImportedCanvasDescription _tttImportedCanvasDescription;
        AssetImportContext _ctx;
        List<TTTImportedImage> _tttImportedImages = new();
        CreateImportedImage _imageImporter;
        byte[] _souseBytes;
        Dictionary<TTTImportedImage, string> _layerAtPath = new();
        string _path = "";

        internal delegate TTTImportedImage CreateImportedImage(ImportRasterImageData importRasterImage);
        internal delegate Task<NativeArray<Color32>> GetPreviewImage(byte[] souseBytes, TTTImportedImage importRasterImage);//つまり正方形にオフセットの入った後の画像を取得するやつ RGBA32

        internal MultiLayerImageImporter(MultiLayerImageCanvas multiLayerImageCanvas, TTTImportedCanvasDescription tttImportedCanvasDescription, AssetImportContext assetImportContext, byte[] souseBytes, CreateImportedImage imageImporter)
        {
            _multiLayerImageCanvas = multiLayerImageCanvas;
            _ctx = assetImportContext;
            _imageImporter = imageImporter;
            _tttImportedCanvasDescription = tttImportedCanvasDescription;
            _souseBytes = souseBytes;

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
                    case HSLAdjustmentLayerData hSVAdjustmentLayerData:
                        {
                            CreateHSLAdjustmentLayer(newLayer, hSVAdjustmentLayerData);
                            break;
                        }
                    case SolidColorLayerData solidColorLayerData:
                        {
                            CreateSolidColorLayer(newLayer, solidColorLayerData);
                            break;
                        }
                    case LevelAdjustmentLayerData levelAdjustmentLayerData:
                        {
                            CreateLevelLayer(newLayer, levelAdjustmentLayerData);
                            break;
                        }
                    case SelectiveColorLayerData selectiveColorLayerData:
                        {
                            CreateSelectiveColorLayer(newLayer, selectiveColorLayerData);
                            break;
                        }

                }
            }

        }

        private void CreateSelectiveColorLayer(GameObject newLayer, SelectiveColorLayerData selectiveColorLayerData)
        {
            var selectiveColoringAdjustmentLayer = newLayer.AddComponent<SelectiveColoringAdjustmentLayer>();
            CopyFromData(selectiveColoringAdjustmentLayer, selectiveColorLayerData);

            selectiveColoringAdjustmentLayer.RedsCMYK = selectiveColorLayerData.RedsCMYK;
            selectiveColoringAdjustmentLayer.YellowsCMYK = selectiveColorLayerData.YellowsCMYK;
            selectiveColoringAdjustmentLayer.GreensCMYK = selectiveColorLayerData.GreensCMYK;
            selectiveColoringAdjustmentLayer.CyansCMYK = selectiveColorLayerData.CyansCMYK;
            selectiveColoringAdjustmentLayer.BluesCMYK = selectiveColorLayerData.BluesCMYK;
            selectiveColoringAdjustmentLayer.MagentasCMYK = selectiveColorLayerData.MagentasCMYK;
            selectiveColoringAdjustmentLayer.WhitesCMYK = selectiveColorLayerData.WhitesCMYK;
            selectiveColoringAdjustmentLayer.NeutralsCMYK = selectiveColorLayerData.NeutralsCMYK;
            selectiveColoringAdjustmentLayer.BlacksCMYK = selectiveColorLayerData.BlacksCMYK;
            selectiveColoringAdjustmentLayer.IsAbsolute = selectiveColorLayerData.IsAbsolute;
        }

        private void CreateSolidColorLayer(GameObject newLayer, SolidColorLayerData solidColorLayerData)
        {
            var SolidColorLayerComponent = newLayer.AddComponent<SolidColorLayer>();
            CopyFromData(SolidColorLayerComponent, solidColorLayerData);

            SolidColorLayerComponent.Color = solidColorLayerData.Color;
        }

        private void CreateHSLAdjustmentLayer(GameObject newLayer, HSLAdjustmentLayerData hSVAdjustmentLayerData)
        {
            var HSVAdjustmentLayerComponent = newLayer.AddComponent<HSLAdjustmentLayer>();
            CopyFromData(HSVAdjustmentLayerComponent, hSVAdjustmentLayerData);

            HSVAdjustmentLayerComponent.Hue = hSVAdjustmentLayerData.Hue;
            HSVAdjustmentLayerComponent.Saturation = hSVAdjustmentLayerData.Saturation;
            HSVAdjustmentLayerComponent.Lightness = hSVAdjustmentLayerData.Lightness;
        }

        private void CreateLevelLayer(GameObject newLayer, LevelAdjustmentLayerData levelAdjustmentLayerData)
        {
            var HSVAdjustmentLayerComponent = newLayer.AddComponent<LevelAdjustmentLayer>();
            CopyFromData(HSVAdjustmentLayerComponent, levelAdjustmentLayerData);

            HSVAdjustmentLayerComponent.RGB = Convert(levelAdjustmentLayerData.RGB);
            HSVAdjustmentLayerComponent.Red = Convert(levelAdjustmentLayerData.Red);
            HSVAdjustmentLayerComponent.Green = Convert(levelAdjustmentLayerData.Green);
            HSVAdjustmentLayerComponent.Blue = Convert(levelAdjustmentLayerData.Blue);

            static LevelAdjustmentLayer.Level Convert(LevelAdjustmentLayerData.LevelData levelData)
            {
                var level = new LevelAdjustmentLayer.Level();

                level.InputFloor = levelData.InputFloor;
                level.InputCeiling = levelData.InputCeiling;
                level.Gamma = levelData.Gamma;
                level.OutputFloor = levelData.OutputFloor;
                level.OutputCeiling = levelData.OutputCeiling;

                return level;
            }
        }

        private void CreateRasterLayer(GameObject newLayer, RasterLayerData rasterLayer)
        {
            if (rasterLayer.RasterTexture == null) { Debug.Log(rasterLayer.LayerName + " is Not RasterLayer"); UnityEngine.Object.DestroyImmediate(newLayer); return; }//ラスターレイヤーじゃないものはインポートできない。
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

            Action nextTaming = () => { };
            Action nextTaming2 = () => { };

            var fullNATex = new NativeArray<Color32>(canvasSize.x * canvasSize.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var fullNAF4Tex = new NativeArray<float4>(canvasSize.x * canvasSize.y, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            foreach (var importedImage in _tttImportedImages)
            {
                Profiler.BeginSample("CreatePreview -" + importedImage.name);
                Profiler.BeginSample("LoadImage");

                var jobResult = importedImage.LoadImage(_souseBytes, fullNATex);

                Profiler.EndSample();
                Profiler.BeginSample("ConvertColor32ToFloat4Job");

                var covF4 = new ConvertColor32ToFloat4Job() { Souse = fullNATex, Target = fullNAF4Tex, };


                var covF4Handle = covF4.Schedule(fullNAF4Tex.Length, 64, jobResult.GetHandle);
                nextTaming();
                covF4Handle.Complete();

                Profiler.EndSample();
                Profiler.BeginSample("CreateMip");

                var mipMapCount = MipMapUtility.MipMapCountFrom(Mathf.Max(canvasSize.x, canvasSize.y), 1024);
                var mipJobResult = MipMapUtility.GenerateAverageMips(fullNAF4Tex, canvasSize, mipMapCount);
                nextTaming2();
                _ = mipJobResult.GetResult;

                Profiler.EndSample();

                Texture2D tex2d = null;
                nextTaming = () =>
                {
                    Profiler.BeginSample("nextTamingCall");
                    Profiler.BeginSample("CratePrevTex");

                    tex2d = new Texture2D(1024, 1024, TextureFormat.RGBAFloat, false);
                    tex2d.alphaIsTransparency = true;

                    tex2d.LoadRawTextureData(mipJobResult.GetResult[mipMapCount]);
                    EditorUtility.CompressTexture(tex2d, TextureFormat.DXT5, 100);

                    Profiler.EndSample();
                    Profiler.EndSample();
                };
                nextTaming2 = () =>
                {
                    Profiler.BeginSample("nextTamingCall2");
                    Profiler.BeginSample("SetTexDataAndCompress");

                    tex2d.Apply(true, true);
                    importedImage.PreviewTexture = tex2d;

                    foreach (var n2da in mipJobResult.GetResult.Skip(1)) { n2da.Dispose(); }

                    Profiler.EndSample();
                    Profiler.EndSample();
                };

                Profiler.EndSample();
            }

            nextTaming();
            nextTaming2();

            fullNAF4Tex.Dispose();
            fullNATex.Dispose();


            // var texManager = new TextureManager(true);
            // var canvasResult = _multiLayerImageCanvas.EvaluateCanvas(texManager, 1024);
            // texManager.DestroyTextures();

            // var resultTex = canvasResult.CopyTexture2D(overrideUseMip: true);
            // EditorUtility.CompressTexture(resultTex, TextureFormat.DXT5, 100);
            // resultTex.name = "TTT-CanvasPreviewResult";
            // _ctx.AddObjectToAsset(resultTex.name, resultTex);
            // RenderTexture.ReleaseTemporary(canvasResult);

            // var quadMesh = GameObject.CreatePrimitive(PrimitiveType.Quad);
            // var mesh = quadMesh.GetComponent<MeshFilter>().sharedMesh;
            // GameObject.DestroyImmediate(quadMesh);

            // var prevGo = new GameObject("TTT-CanvasPreview");
            // var hideFlagPatch = prevGo.AddComponent<HideFlagPatch>();
            // prevGo.tag ="EditorOnly";
            // var quad = prevGo.AddComponent<SkinnedMeshRenderer>();

            // quad.transform.SetParent(_multiLayerImageCanvas.transform, false);
            // quad.sharedMesh = mesh;
            // quad.sharedMaterial = new Material(Shader.Find("Unlit/Texture")) { mainTexture = resultTex, name = "TTT-CanvasPreviewResult-Material" };
            // quad.transform.localRotation = Quaternion.Euler(new Vector3(-20f, 60f, 0f));
            // quad.transform.localScale = new Vector3(-0.002f, 0.002f, -0.002f);
            // quad.localBounds = new Bounds(Vector3.zero, new Vector3(0.31f, 0.31f, 0.001f) * 2f);

            // _ctx.AddObjectToAsset("TTT-CanvasPreviewResult-Material", quad.sharedMaterial);

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

                image.PreviewTexture.name = image.name + "_Preview";
                _ctx.AddObjectToAsset(_layerAtPath[image] + "/" + image.name, image);
                _ctx.AddObjectToAsset(_layerAtPath[image] + "/" + image.PreviewTexture.name, image.PreviewTexture);
            }
        }

        [BurstCompile]
        internal struct ConvertColor32ToFloat4Job : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Color32> Souse;
            [WriteOnly] public NativeArray<float4> Target;
            public void Execute(int index)
            {
                Target[index] = new float4(
                    Souse[index].r / (float)byte.MaxValue,
                    Souse[index].g / (float)byte.MaxValue,
                    Souse[index].b / (float)byte.MaxValue,
                    Souse[index].a / (float)byte.MaxValue
                );
            }
        }

        [BurstCompile]
        internal struct ConvertFloat4ToColor32Job : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float4> Souse;
            [WriteOnly] public NativeArray<Color32> Target;
            public void Execute(int index)
            {
                Target[index] = new Color32(
                    (byte)Mathf.Round(Souse[index].x * (float)byte.MaxValue),
                    (byte)Mathf.Round(Souse[index].y * (float)byte.MaxValue),
                    (byte)Mathf.Round(Souse[index].z * (float)byte.MaxValue),
                    (byte)Mathf.Round(Souse[index].w * (float)byte.MaxValue)
                );
            }
        }
    }
}
