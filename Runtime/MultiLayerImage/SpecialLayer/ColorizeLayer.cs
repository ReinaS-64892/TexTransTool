#nullable enable
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class ColorizeLayer : AbstractLayer
    {
        internal const string ComponentName = "TTT ColorizeLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        [ColorUsage(false)] public Color Color = Color.white;

        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(IDomain domain, ITexTransToolForUnity engine)
        {
            domain.LookAt(this);
            domain.LookAt(gameObject);

            var lm = GetAlphaMask(domain, engine);
            var blKey = engine.QueryBlendKey(BlendTypeKey);
            var colorize = new Colorize(Color.ToTTCore());

            return new GrabBlendingAsLayer<ITexTransToolForUnity>(Visible, lm, Clipping, blKey, colorize);
        }
    }
}
