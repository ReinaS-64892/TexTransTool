#nullable enable
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class RasterLayer : AbstractLayer
    {
        internal const string ComponentName = "TTT RasterLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public Texture2D? RasterTexture;
        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;
            domain.Observe(this);
            domain.Observe(gameObject);

            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            var alphaMask = GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);

            if (RasterTexture == null) { return new EmptyLayer<ITexTransToolForUnity>(Visible, alphaMask, alphaOperator, Clipping, blKey); }

            domain.Observe(RasterTexture);
            var diskTex = engine.Wrapping(RasterTexture);
            return new RasterLayer<ITexTransToolForUnity>(Visible, alphaMask, alphaOperator, Clipping, blKey, diskTex);
        }
    }
}
