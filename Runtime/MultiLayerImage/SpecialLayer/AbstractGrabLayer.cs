using UnityEngine;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    public abstract class AbstractGrabLayer : AbstractLayer
    {
        public abstract void GetImage(RenderTexture GrabSouse, RenderTexture WriteTarget, IOriginTexture originTexture);

        internal override void EvaluateTexture(CanvasContext canvasContext)
        {
            if (!Visible) { canvasContext.LayerCanvas.AddHiddenLayer(Clipping, true); return; }

            var mask = GetLayerAlphaMod(canvasContext);
            canvasContext.LayerCanvas.GrabCanvas((grab, write) => GetImage(grab, write, canvasContext.TextureManager), mask, BlendTypeKey, Clipping);
        }

    }
}
