#nullable enable
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class ClipPostScreenLayer : AbstractLayer
    {
        internal const string ComponentName = "TTT " + nameof(ClipPostScreenLayer);
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;

        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;

            domain.Observe(this);
            domain.Observe(gameObject);

            var cps = new ClipPostScreen();

            return new GrabDirectLayer<ITexTransToolForUnity>(Visible, Clipping, cps);
        }
    }
}
