using System;
using net.rs64.TexTransTool;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class RasterImportedLayer : AbstractImageLayer
    {
        internal const string ComponentName = "TTT RasterImportedLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public TTTImportedImage ImportedImage;


        public override void GetImage(RenderTexture renderTexture, IOriginTexture originTexture)
        {
            originTexture.WriteOriginalTexture(ImportedImage, renderTexture);
        }
    }
    [Serializable]
    public class TTTImportedLayerMask : ILayerMask
    {
        public bool LayerMaskDisabled;
        [SerializeField] internal TTTImportedImage MaskTexture;

        internal TTTImportedLayerMask(bool layerMaskDisabled, TTTImportedImage maskPNG)
        {
            LayerMaskDisabled = layerMaskDisabled;
            MaskTexture = maskPNG;
        }

        public bool ContainedMask => !LayerMaskDisabled && MaskTexture != null;

        void ILayerMask.WriteMaskTexture(RenderTexture renderTexture, IOriginTexture originTexture)
        {
            originTexture.WriteOriginalTexture(MaskTexture, renderTexture);
        }
    }
}
