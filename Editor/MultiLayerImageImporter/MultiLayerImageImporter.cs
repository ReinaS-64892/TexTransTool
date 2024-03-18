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
using UnityEngine.Rendering;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.BlendTexture;
using TextureUtility = net.rs64.TexTransCore.TransTextureCore.Utils.TextureUtility;

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
        DownScalingAlgorism _previewImageDownScalingAlgorism;

        internal delegate TTTImportedImage CreateImportedImage(ImportRasterImageData importRasterImage);
        internal delegate Task<NativeArray<Color32>> GetPreviewImage(byte[] souseBytes, TTTImportedImage importRasterImage);//つまり正方形にオフセットの入った後の画像を取得するやつ RGBA32

        internal MultiLayerImageImporter(MultiLayerImageCanvas multiLayerImageCanvas, TTTImportedCanvasDescription tttImportedCanvasDescription, AssetImportContext assetImportContext, byte[] souseBytes, CreateImportedImage imageImporter, DownScalingAlgorism previewImageDownScalingAlgorism)
        {
            _multiLayerImageCanvas = multiLayerImageCanvas;
            _ctx = assetImportContext;
            _imageImporter = imageImporter;
            _tttImportedCanvasDescription = tttImportedCanvasDescription;
            _souseBytes = souseBytes;
            _previewImageDownScalingAlgorism = previewImageDownScalingAlgorism;

        }

        internal void AddLayers(List<AbstractLayerData> abstractLayers, Transform parent = null)
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
            rasterLayerComponent.ImportedImage = importedImage;
        }

        private ILayerMask MaskTexture(LayerMaskData maskData, string layerName)
        {
            if (maskData == null) { return new LayerMask(); }

            var importMask = _imageImporter.Invoke(maskData.MaskTexture);
            importMask.name = layerName + "_MaskTex";
            importMask.CanvasDescription = _tttImportedCanvasDescription;
            _tttImportedImages.Add(importMask);
            return new TTTImportedLayerMask(maskData.LayerMaskDisabled, importMask);

        }

        private void CreateLayerFolder(GameObject newLayer, LayerFolderData layerFolder)
        {
            var layerFolderComponent = newLayer.AddComponent<LayerFolder>();

            CopyFromData(layerFolderComponent, layerFolder);
            layerFolderComponent.PassThrough = layerFolder.PassThrough;

            AddLayers(layerFolder.Layers, newLayer.transform);
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
            var rt = new RenderTexture(TextureUtility.NormalizePowerOfTwo(_tttImportedCanvasDescription.Width), TextureUtility.NormalizePowerOfTwo(_tttImportedCanvasDescription.Height), 0, RenderTextureFormat.ARGB32);
            rt.enableRandomWrite = true;
            rt.useMipMap = true;
            rt.autoGenerateMips = false;
            foreach (var importedImage in _tttImportedImages)
            {
                rt.Clear();
                importedImage.LoadImage(_souseBytes, rt);
                MipMapUtility.GenerateMips(rt, _previewImageDownScalingAlgorism);

                var mipMapCount = MipMapUtility.MipMapCountFrom(rt.width, 1024);
                var request = AsyncGPUReadback.Request(rt, mipMapCount);
                var tex2d = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);

                request.WaitForCompletion();
                using (var data = request.GetData<byte>()) { tex2d.LoadRawTextureData(data); }

                EditorUtility.CompressTexture(tex2d, TextureFormat.DXT5, 100);
                tex2d.Apply(true, true);

                importedImage.PreviewTexture = tex2d;
            }
            if (rt == RenderTexture.active) { RenderTexture.active = null; }
            UnityEngine.Object.DestroyImmediate(rt);

            var texManager = new TextureManager(true);
            var canvasResult = _multiLayerImageCanvas.EvaluateCanvas(texManager, 1024);
            texManager.DestroyTextures();

            var resultTex = canvasResult.CopyTexture2D(overrideUseMip: true);
            EditorUtility.CompressTexture(resultTex, TextureFormat.DXT5, 100);
            resultTex.name = "TTT-CanvasPreviewResult";
            _ctx.AddObjectToAsset(resultTex.name, resultTex);
            RenderTexture.ReleaseTemporary(canvasResult);

            var quadMesh = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var mesh = quadMesh.GetComponent<MeshFilter>().sharedMesh;
            GameObject.DestroyImmediate(quadMesh);

            var prevGo = new GameObject("TTT-CanvasPreview");
            var hideFlagPatch = prevGo.AddComponent<HideFlagPatch>();
            prevGo.tag ="EditorOnly";
            var quad = prevGo.AddComponent<SkinnedMeshRenderer>();

            quad.transform.SetParent(_multiLayerImageCanvas.transform, false);
            quad.sharedMesh = mesh;
            quad.sharedMaterial = new Material(Shader.Find("Unlit/Texture")) { mainTexture = resultTex, name = "TTT-CanvasPreviewResult-Material" };
            quad.transform.localRotation = Quaternion.Euler(new Vector3(-20f, 60f, 0f));
            quad.transform.localScale = new Vector3(-0.002f, 0.002f, -0.002f);
            quad.localBounds = new Bounds(Vector3.zero, new Vector3(0.31f, 0.31f, 0.001f) * 2f);

            _ctx.AddObjectToAsset("TTT-CanvasPreviewResult-Material", quad.sharedMaterial);

        }

        public void SaveSubAsset()
        {
            var NameHash = new HashSet<string>() { "TTT-CanvasPreviewResult", "TTT-CanvasPreviewResult-Material" };
            foreach (var image in _tttImportedImages.Reverse<TTTImportedImage>())
            {
                var name = image.name;
                if (NameHash.Contains(name))
                {
                    var addCount = 1;
                    while (NameHash.Contains(name + "-" + addCount)) { addCount += 1; }
                    name = name + "-" + addCount;
                }
                NameHash.Add(name);

                image.name = name;
                image.PreviewTexture.name = image.name + "_Preview";
                _ctx.AddObjectToAsset(name, image);
                _ctx.AddObjectToAsset(image.PreviewTexture.name, image.PreviewTexture);
            }
        }
    }
}
