using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
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

        internal override void LookAtCalling(ILookingObject lookingObject)
        {
            base.LookAtCalling(lookingObject);
            lookingObject.LookAt(ImportedImage);
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
        public void LookAtCalling(ILookingObject lookingObject) { lookingObject.LookAt(MaskTexture); }
    }
}
