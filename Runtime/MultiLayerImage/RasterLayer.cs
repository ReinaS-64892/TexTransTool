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
        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(IDomain domain, ITexTransToolForUnity engine)
        {
            domain.LookAt(this);
            domain.LookAt(gameObject);

            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            var alphaMask = GetAlphaMask(domain, engine);
            var blKey = engine.QueryBlendKey(BlendTypeKey);

            if (RasterTexture == null) { return new EmptyLayer<ITexTransToolForUnity>(Visible, alphaMask, alphaOperator, Clipping, blKey); }

            domain.LookAt(RasterTexture);
            var diskTex = engine.Wrapping(RasterTexture);
            return new RasterLayer<ITexTransToolForUnity>(Visible, alphaMask, alphaOperator, Clipping, blKey, diskTex);
        }
    }
}
