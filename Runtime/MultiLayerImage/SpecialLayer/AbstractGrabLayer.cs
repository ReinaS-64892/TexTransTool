using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    public abstract class AbstractGrabLayer : AbstractLayer
    {
        //GrabSouseには絶対に書き込まないように
        public abstract void GetImage(RenderTexture GrabSouse, RenderTexture WriteTarget, IOriginTexture originTexture);

        internal override void EvaluateTexture(CanvasContext canvasContext)
        {
            if (!Visible) { canvasContext.LayerCanvas.AddLayer(BlendLayer.Null(true, Clipping)); return; }

            var rTex = RenderTexture.GetTemporary(canvasContext.CanvasSize, canvasContext.CanvasSize, 0); rTex.Clear();
            var grabTex = canvasContext.LayerCanvas.GrabCanvas(Clipping);

            if (grabTex == null) { canvasContext.LayerCanvas.AddLayer(BlendLayer.Null(true, true)); return; }

            GetImage(grabTex, rTex, canvasContext.TextureManager);


            using (canvasContext.LayerCanvas.AlphaModScope(GetLayerAlphaMod(canvasContext)))
            {
                canvasContext.LayerCanvas.AddLayer(new(false, true, Clipping, rTex, BlendTypeKey));
            }
        }

    }
}