using System;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
using System.Runtime.CompilerServices;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class LayerFolder : AbstractLayer
    {
        internal const string ComponentName = "TTT LayerFolder";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public bool PassThrough;
        internal override LayerObject<TTT4U> GetLayerObject<TTT4U>(TTT4U engine)
        {
            var layers = GetChileLayers();
            var chiles = new List<LayerObject<TTT4U>>(layers.Capacity);
            foreach (var l in layers) { chiles.Add(l.GetLayerObject(engine)); }

            if (PassThrough)
            {
                return new PassThoughtFolder<TTT4U>(Visible, GetAlphaMask(engine), Clipping, chiles);
            }
            else
            {
                var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
                return new LayerFolder<TTT4U>(Visible, GetAlphaMask(engine), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey), chiles);
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
