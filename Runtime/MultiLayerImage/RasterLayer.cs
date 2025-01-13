#nullable enable
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class RasterLayer : AbstractImageLayer
    {
        internal const string ComponentName = "TTT RasterLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public Texture2D? RasterTexture;

        public override void GetImage<TTCE4U>(TTCE4U engine, ITTRenderTexture renderTexture)
        {
            if (RasterTexture == null)
            {
                engine.ColorFill(renderTexture, TexTransCore.Color.Zero);
                return;
            }
            using var ri = engine.Wrapping(RasterTexture);
            engine.LoadTextureWidthAnySize(renderTexture, ri);
        }

        internal override LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engine)
        {
            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;

            if (RasterTexture == null) { return new EmptyLayer<TTCE4U>(Visible, GetAlphaMask(engine), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey)); }

            var ri = engine.Wrapping(RasterTexture);
            return new RasterLayer<TTCE4U>(Visible, GetAlphaMask(engine), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey), ri);
        }

        internal override void LookAtCalling(ILookingObject lookingObject)
        {
            base.LookAtCalling(lookingObject);
            if (RasterTexture != null) lookingObject.LookAt(RasterTexture);
        }
    }
}
