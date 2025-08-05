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

            domain.Observe(this);
            domain.Observe(gameObject);

            var layers = GetChileLayers();
            var chiles = new List<LayerObject<ITexTransToolForUnity>>(layers.Capacity);
            foreach (var l in layers) { chiles.Add(l.GetLayerObject(ctx)); }

            var mask = GetAlphaMaskObject(ctx);
            if (Clipping is false && PassThrough)
            {
                // Clipping と PassThrough が共存することは、 Photoshop では存在しない。クリスタだと PassThrough が無効化される。
                // その仕様のため TTT は クリスタの PassThrough の無効と言う挙動を取ります。
                return new PassThoughtFolder<ITexTransToolForUnity>(Visible, mask, false, chiles);
            }
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
