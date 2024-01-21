using System;
using net.rs64.TexTransTool;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    public class RasterImportedLayer : AbstractImageLayer
    {
        public TTTImportedPng ImportedPNG;
        public override void GetImage(RenderTexture renderTexture, IOriginTexture originTexture)
        {
            originTexture.WriteOriginalTexture(ImportedPNG, renderTexture);
        }
    }
    [Serializable]
    public class TTTImportedPngLayerMask : ILayerMask
    {
        public bool LayerMaskDisabled;
        public TTTImportedPng MaskTexture;

        public TTTImportedPngLayerMask(bool layerMaskDisabled, TTTImportedPng maskPNG)
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