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
        public override void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture)
        {
            var mat = new Material(SpecialLayerShaders.HSLAdjustmentShader);
            mat.SetFloat("_Hue", Hue);
            mat.SetFloat("_Saturation", Saturation);
            mat.SetFloat("_Lightness", Lightness);

            Graphics.Blit(grabSource, writeTarget, mat);
            UnityEngine.Object.DestroyImmediate(mat);
        }
    }
}
