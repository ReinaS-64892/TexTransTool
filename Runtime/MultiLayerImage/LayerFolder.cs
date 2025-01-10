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
        internal override LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engine)
        {
            var layers = GetChileLayers();
            var chiles = new List<LayerObject<TTCE4U>>(layers.Capacity);
            foreach (var l in layers) { chiles.Add(l.GetLayerObject(engine)); }

            if (PassThrough)
            {
                return new PassThoughtFolder<TTCE4U>(Visible, GetAlphaMask(engine), Clipping, chiles);
            }
            else
            {
                var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
                return new LayerFolder<TTCE4U>(Visible, GetAlphaMask(engine), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey), chiles);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        List<AbstractLayer> GetChileLayers() { return MultiLayerImageCanvas.GetChileLayers(transform); }
        internal override void LookAtCalling(ILookingObject lookingObject)
        {
            base.LookAtCalling(lookingObject);
            foreach (var cl in GetChileLayers()) { cl.LookAtCalling(lookingObject); }
        }

    }


}
