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

            using (canvasContext.LayerCanvas.AlphaModScope(GetLayerAlphaMod(canvasContext)))
            {
                if (PassThrough && !Clipping)
                {
                    if (!Visible) { canvasContext.LayerCanvas.AddLayer(BlendLayer.Null(true, Clipping)); return; }

                    //下のレイヤーとクリッピングをできなくする
                    canvasContext.LayerCanvas.AddLayer(new(false, true, false, null, null));
                    foreach (var layer in Layers)
                    {
                        layer.EvaluateTexture(canvasContext);
                    }
                    //上のレイヤーがクリッピングをできなくする
                    canvasContext.LayerCanvas.AddLayer(new(false, true, false, null, null));
                }
                else
                {
                    if (!Visible) { canvasContext.LayerCanvas.AddLayer(BlendLayer.Null(false, Clipping)); return; }

                    var subContext = canvasContext.CreateSubCanvas;
                    foreach (var layer in Layers)
                    {
                        layer.EvaluateTexture(subContext);
                    }

                    var resTex = subContext.LayerCanvas.FinalizeCanvas();

                    if (Clipping)
                    {
                        canvasContext.LayerCanvas.AddLayer(new(false, false, Clipping, resTex, BlendTypeKey));
                    }
                    else
                    {
                        canvasContext.LayerCanvas.AddLayer(new(false, false, Clipping, resTex, PassThrough ? BL_KEY_DEFAULT : BlendTypeKey));
                    }
                }
            }


        }


    }

}