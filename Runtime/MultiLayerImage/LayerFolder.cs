#nullable enable
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using System.Runtime.CompilerServices;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class LayerFolder : AbstractLayer
    {
        internal const string ComponentName = "TTT LayerFolder";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public bool PassThrough;
        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.LookAt(this);
            domain.LookAt(gameObject);

            var layers = GetChileLayers();
            var chiles = new List<LayerObject<ITexTransToolForUnity>>(layers.Capacity);
            foreach (var l in layers) { chiles.Add(l.GetLayerObject(ctx)); }

            var mask = GetAlphaMaskObject(ctx);
            if (PassThrough) { return new PassThoughtFolder<ITexTransToolForUnity>(Visible, mask, Clipping, chiles); }
            else
            {
                var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
                var blKey = engine.QueryBlendKey(BlendTypeKey);
                return new LayerFolder<ITexTransToolForUnity>(Visible, mask, alphaOperator, Clipping, blKey, chiles);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        List<IMultiLayerImageCanvasLayer> GetChileLayers() { return MultiLayerImageCanvas.GetChileLayers(transform); }
    }


}
