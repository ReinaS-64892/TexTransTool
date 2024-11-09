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

        public override void GetImage<TTT4U>(TTT4U engine, ITTRenderTexture renderTexture)
        {
            using var ii = engine.Wrapping(ImportedImage);
            engine.LoadTexture(renderTexture, ii);
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
        public void LookAtCalling(ILookingObject lookingObject) { lookingObject.LookAt(MaskTexture); }
        public void WriteMaskTexture<TTCE4U>(TTCE4U engine, ITTRenderTexture renderTexture)
        where TTCE4U : ITexTransToolForUnity
        , ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            using var im = engine.Wrapping(MaskTexture);
            engine.LoadTexture(renderTexture, im);
        }
    }
}
