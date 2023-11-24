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
            EditorUtility.DisplayProgressBar("Import Canvas", "Build Layer", 0);

            var prefabName = Path.GetFileName(ctx.SaveDirectory) + "-Canvas";
            var rootCanvas = new GameObject(prefabName);
            var multiLayerImageCanvas = rootCanvas.AddComponent<MultiLayerImageCanvas>();
            multiLayerImageCanvas.TextureSize = canvasData.Size;
            AddLayers(multiLayerImageCanvas.transform, ctx, canvasData.RootLayers);
            ctx.FinalizeTex2D();
            PreSaveCallBack.Invoke(multiLayerImageCanvas);
            PrefabUtility.SaveAsPrefabAsset(rootCanvas, Path.Combine(ctx.SaveDirectory, prefabName + ".prefab"));
            UnityEngine.Object.DestroyImmediate(rootCanvas);


            EditorUtility.ClearProgressBar();
        }
        public static void AddLayers(Transform thisTransForm, HandlerForFolderSaver ctx, List<AbstractLayerData> abstractLayers)
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
                            ctx.AddTextureCallBack(rasterLayer.RasterTexture, layer.LayerName + "_Tex", (Texture2D tex) => rasterLayerComponent.RasterTexture = tex);
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
                    ctx.AddTextureCallBack(abstractLayerData.LayerMask.MaskTexture, abstractLayerData.LayerName + "_Mask", (Texture2D tex) => abstractLayer.LayerMask.MaskTexture = tex);
                }
            }
        }

        public class HandlerForFolderSaver
        {
            public string SaveDirectory;

            public const string RasterImageData = "RasterImageData";

            public HandlerForFolderSaver(string v)
            {
                SaveDirectory = v;
            }


            public void AddTextureCallBack(TwoDimensionalMap<Color32> rasterTexture, string name, Action<Texture2D> value)
            {
                TexToNameLoad.Add(rasterTexture, (name, value));
            }

            Dictionary<TwoDimensionalMap<Color32>, (string, Action<Texture2D>)> TexToNameLoad = new Dictionary<TwoDimensionalMap<Color32>, (string, Action<Texture2D>)>();

            public void FinalizeTex2D()
            {

                EditorUtility.DisplayProgressBar("Import Canvas", "SavePNG", 0);
                var RasterDataPath = Path.Combine(SaveDirectory, RasterImageData);
                if (Directory.Exists(RasterDataPath)) { Directory.Delete(RasterDataPath, true); }
                Directory.CreateDirectory(RasterDataPath);

                var PathToAction = new Dictionary<string, Action<Texture2D>>();
                var progressesCount = TexToNameLoad.Count;
                var imageIndex = 0;
                foreach (var texToNameLoad in TexToNameLoad)
                {
                    EditorUtility.DisplayProgressBar("Import Canvas", "SavePNG-" + texToNameLoad.Value.Item1, (float)imageIndex / progressesCount);
                    imageIndex += 1;

                    var TexName = texToNameLoad.Value.Item1;
                    var TexMap = texToNameLoad.Key;

                    if (!Directory.Exists(SaveDirectory)) { Directory.CreateDirectory(SaveDirectory); }


                    var path = CreatePath(TexName, imageIndex);


                    var tex2D = new Texture2D(TexMap.MapSize.x, TexMap.MapSize.y, TextureFormat.RGBA32, false);
                    tex2D.SetPixelData(TexMap.Array, 0);
                    File.WriteAllBytes(path, tex2D.EncodeToPNG());
                    File.WriteAllText(path + ".meta", MetaGUIDPre + GUID.Generate().ToString() + MetaGUIDPost);
                    UnityEngine.Object.DestroyImmediate(tex2D);

                    PathToAction.Add(path, texToNameLoad.Value.Item2);
                }
                TexToNameLoad.Clear();


                EditorUtility.DisplayProgressBar("Import Canvas", "AssetDatabase.Refresh();", 0);
                AssetDatabase.Refresh();
                EditorUtility.DisplayProgressBar("Import Canvas", "Refresh End", 1);

                imageIndex = 0;
                foreach (var loadTex2d in PathToAction)
                {
                    EditorUtility.DisplayProgressBar("Import Canvas", "SetUpLayer for PNG-" + Path.GetFileName(loadTex2d.Key), imageIndex / (float)progressesCount);
                    imageIndex += 1;

                    var Tex2D = AssetDatabase.LoadAssetAtPath<Texture2D>(loadTex2d.Key);
                    loadTex2d.Value.Invoke(Tex2D);
                }
                EditorUtility.ClearProgressBar();
            }

            private string CreatePath(string TexName, int count)
            {
                return Path.Combine(SaveDirectory, RasterImageData, TexName + "-" + count)  + ".png";
            }

            public const string MetaGUIDPre =
@"fileFormatVersion: 2
guid: ";
            public const string MetaGUIDPost =
@"
  TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 11
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 1
  lightmap: 0
  compressionQuality: 50
  spriteMode: 0
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 100
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 0
  spriteTessellationDetail: -1
  textureType: 0
  textureShape: 1
  singleChannelComponent: 0
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  applyGammaDecoding: 0
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 1024
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 3
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
  spritePackingTag: 
  pSDRemoveMatte: 0
  pSDShowRemoveMatteOption: 0
  userData: 
  assetBundleName: 
  assetBundleVariant:
";
        }

    }
}
#endif