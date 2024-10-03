using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransUnityCore.BlendTexture.TextureBlend;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransUnityCore.BlendTexture;
using net.rs64.TexTransUnityCore;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class LayerFolder : AbstractLayer
    {
        internal const string ComponentName = "TTT LayerFolder";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public bool PassThrough;
        internal override LayerObject GetLayerObject(ITextureManager textureManager)
        {
            var chiles = GetChileLayers().Select(l => l.GetLayerObject(textureManager)).ToList();
            if (PassThrough)
            {
                return new PassThoughtFolder(Visible, GetAlphaMask(textureManager), Clipping, chiles);
            }
            else
            {
                var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
                return new TexTransCore.MultiLayerImageCanvas.LayerFolder(Visible, GetAlphaMask(textureManager), alphaOperator, Clipping, new TTTBlendTypeKey(BlendTypeKey), chiles);
            }
        }
        IEnumerable<AbstractLayer> GetChileLayers() { return MultiLayerImageCanvas.GetChileLayers(transform); }
        internal override void LookAtCalling(ILookingObject lookingObject)
        {
            base.LookAtCalling(lookingObject);
            foreach (var cl in GetChileLayers()) { cl.LookAtCalling(lookingObject); }
        }

    }


}
