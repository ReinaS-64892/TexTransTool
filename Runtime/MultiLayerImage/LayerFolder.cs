using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT LayerFolder")]
    public sealed class LayerFolder : AbstractLayer
    {
        public bool PassThrough;
        internal override void EvaluateTexture(CanvasContext canvasContext)
        {

            var Layers = transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Reverse();

            if (PassThrough && !Clipping)
            {
                if (!Visible) { canvasContext.LayerCanvas.AddHiddenLayer(false, false); return; }

                using (canvasContext.LayerCanvas.UsingLayerScope(GetLayerAlphaMod(canvasContext)))
                {
                    foreach (var layer in Layers)
                    {
                        layer.EvaluateTexture(canvasContext);
                    }
                }
            }
            else
            {
                if (!Visible) { canvasContext.LayerCanvas.AddHiddenLayer(Clipping, false); return; }

                var subContext = canvasContext.CreateSubCanvas;
                foreach (var layer in Layers)
                {
                    layer.EvaluateTexture(subContext);
                }

                var resTex = subContext.LayerCanvas.FinalizeCanvas();
                var mask = GetLayerAlphaMod(canvasContext);

                if (Clipping) { canvasContext.LayerCanvas.AddLayer(new(resTex, BlendTypeKey), mask, Clipping); }
                else { canvasContext.LayerCanvas.AddLayer(new(resTex, PassThrough ? BL_KEY_DEFAULT : BlendTypeKey), mask, false); }

            }
        }


    }


}
