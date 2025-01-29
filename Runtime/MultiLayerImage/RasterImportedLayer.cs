#nullable enable
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class RasterImportedLayer : AbstractLayer
    {
        internal const string ComponentName = "TTT RasterImportedLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public TTTImportedImage? ImportedImage;

        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.LookAt(this);
            domain.LookAt(gameObject);

            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            var alphaMask = GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);

            if (ImportedImage == null) { return new EmptyLayer<ITexTransToolForUnity>(Visible, alphaMask, alphaOperator, Clipping, blKey); }

            domain.LookAt(ImportedImage);
            var diskTex = engine.Wrapping(ImportedImage);
            return new RasterLayer<ITexTransToolForUnity>(Visible, alphaMask, alphaOperator, Clipping, blKey, diskTex);
        }
    }
    [Serializable]
    public class TTTImportedLayerMask : ILayerMask
    {
        public bool LayerMaskDisabled;
        public TTTImportedImage MaskTexture;

        internal TTTImportedLayerMask(bool layerMaskDisabled, TTTImportedImage importedMask)
        {
            LayerMaskDisabled = layerMaskDisabled;
            MaskTexture = importedMask;
        }

        AlphaMask<ITexTransToolForUnity> ILayerMask.GetAlphaMaskObject(GenerateLayerObjectContext ctx, UnityEngine.Object thisObj, Func<UnityEngine.Object, ILayerMask?> getThisToLayerMask)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            var thisLayerMask = domain.LookAtGet(thisObj, o => getThisToLayerMask(o) as TTTImportedLayerMask);
            if (thisLayerMask is null) { return new NoMask<ITexTransToolForUnity>(); }

            var layerMaskDisabled = domain.LookAtGet(thisObj, o => (getThisToLayerMask(o) as TTTImportedLayerMask)!.LayerMaskDisabled);
            if (layerMaskDisabled) { return new NoMask<ITexTransToolForUnity>(); }

            var maskTexture = domain.LookAtGet(thisObj, o => (getThisToLayerMask(o) as TTTImportedLayerMask)!.MaskTexture);
            if (maskTexture == null) { return new NoMask<ITexTransToolForUnity>(); }

            domain.LookAt(maskTexture);
            return new DiskOnlyToMask<ITexTransToolForUnity>(engine.Wrapping(maskTexture!));
        }
    }
}
