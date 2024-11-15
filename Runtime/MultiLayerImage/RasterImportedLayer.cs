using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
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

        public override void GetImage<TTCE4U>(TTCE4U engine, ITTRenderTexture renderTexture)
        {
            using var ri = engine.Wrapping(ImportedImage);
            engine.LoadTextureWidthAnySize(renderTexture, ri);
        }

        internal override LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engine)
        {
            var ri = engine.Wrapping(ImportedImage);
            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            return new RasterLayer<TTCE4U>(Visible, GetAlphaMask(engine), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey), ri);
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

        public bool ContainedMask => LayerMaskDisabled is false && MaskTexture != null;
        public void LookAtCalling(ILookingObject lookingObject) { lookingObject.LookAt(MaskTexture); }
    }
}
