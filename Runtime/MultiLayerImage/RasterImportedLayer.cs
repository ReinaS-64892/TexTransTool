#nullable enable
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class RasterImportedLayer : AbstractImageLayer
    {
        internal const string ComponentName = "TTT RasterImportedLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public TTTImportedImage? ImportedImage;

        public override void GetImage<TTCE4U>(TTCE4U engine, ITTRenderTexture renderTexture)
        {
            if (ImportedImage == null)
            {
                engine.ColorFill(renderTexture, TexTransCore.Color.Zero);
                return;
            }
            using var ri = engine.Wrapping(ImportedImage);
            engine.LoadTextureWidthAnySize(renderTexture, ri);
        }

        internal override LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engine)
        {
            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;

            if (ImportedImage == null) { return new EmptyLayer<TTCE4U>(Visible, GetAlphaMask(engine), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey)); }

            var ri = engine.Wrapping(ImportedImage);
            return new RasterLayer<TTCE4U>(Visible, GetAlphaMask(engine), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey), ri);
        }
        internal override void LookAtCalling(ILookingObject lookingObject)
        {
            base.LookAtCalling(lookingObject);
            if (ImportedImage != null) lookingObject.LookAt(ImportedImage);
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
