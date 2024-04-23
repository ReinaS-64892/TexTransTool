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
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class LayerFolder : AbstractLayer
    {
        internal const string ComponentName = "TTT LayerFolder";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public bool PassThrough;
        internal override void EvaluateTexture(CanvasContext canvasContext)
        {
            IEnumerable<AbstractLayer> Layers = GetChileLayers();

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

        internal IEnumerable<AbstractLayer> GetChileLayers()
        {
            return transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Reverse();
        }
    }


}
