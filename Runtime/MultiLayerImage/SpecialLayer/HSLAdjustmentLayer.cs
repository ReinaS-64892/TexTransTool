using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu("TexTransTool/MultiLayer/TTT HSLAdjustmentLayer")]
    public class HSLAdjustmentLayer : AbstractGrabLayer
    {
        [Range(-1, 1)] public float Hue;
        [Range(-1, 1)] public float Saturation;
        [Range(-1, 1)] public float Lightness;
        public override void GetImage(RenderTexture GrabSouse, RenderTexture WriteTarget, IOriginTexture originTexture)
        {
            var mat = new Material(SpecialLayerShaders.HSLAdjustmentShader);
            mat.SetFloat("_Hue", Hue);
            mat.SetFloat("_Saturation", Saturation);
            mat.SetFloat("_Lightness", Lightness);

            Graphics.Blit(GrabSouse, WriteTarget, mat);
            UnityEngine.Object.DestroyImmediate(mat);
        }
    }
}
