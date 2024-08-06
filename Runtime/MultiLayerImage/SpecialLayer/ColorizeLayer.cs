using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class ColorizeLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT ColorizeLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        [ColorUsage(false)] public Color Color = Color.white;

        public override void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture)
        {
            var mat = MatTemp.GetTempMatShader(SpecialLayerShaders.ColorizeShader);
            mat.SetColor("_Color", Color);

            Graphics.Blit(grabSource, writeTarget, mat);
        }
    }
}