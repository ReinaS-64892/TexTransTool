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
        public Texture2D RasterTexture;

        public override void GetImage<TTCE4U>(TTCE4U engine, ITTRenderTexture renderTexture)
        {
            using var ri = engine.Wrapping(RasterTexture);
            engine.LoadTextureWidthAnySize(renderTexture, ri);
        }

        internal override LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engine)
        {
            var ri = engine.Wrapping(RasterTexture);
            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            return new RasterLayer<TTCE4U>(Visible, GetAlphaMask(engine), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey), ri);
        }

        internal override void LookAtCalling(ILookingObject lookingObject)
        {
            base.LookAtCalling(lookingObject);
            lookingObject.LookAt(RasterTexture);
        }
    }
}
