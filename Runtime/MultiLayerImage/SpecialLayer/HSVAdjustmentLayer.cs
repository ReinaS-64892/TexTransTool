using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    public class HSVAdjustmentLayer : AbstractGrabLayer
    {
        [Range(-1, 1)] public float Hue;
        [Range(-1, 1)] public float Saturation;
        [Range(-1, 1)] public float Brightness;
        public override void GetImage(RenderTexture GrabSouse, RenderTexture WriteTarget, IOriginTexture originTexture)
        {
            var mat = new Material(SpecialLayerShaders.HSVAdjustmentShader);
            mat.SetFloat("_Hue", Hue);
            mat.SetFloat("_Saturation", Saturation);
            mat.SetFloat("_Brightness", Brightness);

            Graphics.Blit(GrabSouse, WriteTarget, mat);
            UnityEngine.Object.DestroyImmediate(mat);
        }
    }
}