using System;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class HSLAdjustmentLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT HSLAdjustmentLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        [Range(-1, 1)] public float Hue;
        [Range(-1, 1)] public float Saturation;
        [Range(-1, 1)] public float Lightness;
        internal override LayerObject GetLayerObject(TexTransCore.ITexTransToolEngine engine, ITextureManager textureManager)
        {
            var hsl = new HSLAdjustment(engine.QueryComputeKey(nameof(HSLAdjustment)), Hue, Saturation, Lightness);
            return new GrabBlendingAsLayer(Visible, GetAlphaMask(textureManager), Clipping, engine.QueryBlendKey(BlendTypeKey), hsl);
        }
        public override void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture)
        {
            throw new NotSupportedException();
            // var mat = MatTemp.GetTempMatShader(SpecialLayerShaders.HSLAdjustmentShader);
            // mat.SetFloat("_Hue", Hue);
            // mat.SetFloat("_Saturation", Saturation);
            // mat.SetFloat("_Lightness", Lightness);

            // Graphics.Blit(grabSource, writeTarget, mat);
        }
    }
}
