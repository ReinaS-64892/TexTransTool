using System;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransUnityCore;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class ColorizeLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT ColorizeLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        [ColorUsage(false)] public Color Color = Color.white;
        internal override LayerObject GetLayerObject(TexTransCore.ITexTransToolEngine engine, ITextureManager textureManager)
        {
            var colorize = new Colorize(engine.QueryComputeKey(nameof(Colorize)), Color.ToTTCore());
            return new GrabBlendingAsLayer(Visible, GetAlphaMask(textureManager), Clipping, engine.QueryBlendKey(BlendTypeKey), colorize);
        }
        public override void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture)
        {
            throw new NotSupportedException();
            // var mat = MatTemp.GetTempMatShader(SpecialLayerShaders.ColorizeShader);
            // mat.SetColor("_Color", Color);

            // Graphics.Blit(grabSource, writeTarget, mat);
        }
    }
}
