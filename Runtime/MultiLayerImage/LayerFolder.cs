using System;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransUnityCore;
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
        internal override LayerObject GetLayerObject(ITexTransToolEngine engine, ITextureManager textureManager)
        {
            var layers = GetChileLayers();
            var chiles = new List<TexTransCore.MultiLayerImageCanvas.LayerObject>(layers.Capacity);
            foreach (var l in layers) { chiles.Add(l.GetLayerObject(engine, textureManager)); }

            if (PassThrough)
            {
                return new PassThoughtFolder(Visible, GetAlphaMask(textureManager), Clipping, chiles);
            }
            else
            {
                var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
                return new TexTransCore.MultiLayerImageCanvas.LayerFolder(Visible, GetAlphaMask(textureManager), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey), chiles);
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
