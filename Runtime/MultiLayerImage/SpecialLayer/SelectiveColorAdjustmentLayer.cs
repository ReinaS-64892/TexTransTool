using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu("TexTransTool/MultiLayer/TTT SelectiveColoringAdjustmentLayer")]
    public class SelectiveColoringAdjustmentLayer : AbstractGrabLayer
    {
        public Vector4 RedsCMYK;
        public Vector4 YellowsCMYK;
        public Vector4 GreensCMYK;
        public Vector4 CyansCMYK;
        public Vector4 BluesCMYK;
        public Vector4 MagentasCMYK;
        public Vector4 WhitesCMYK;
        public Vector4 NeutralsCMYK;
        public Vector4 BlacksCMYK;
        public bool IsAbsolute;

        public override void GetImage(RenderTexture GrabSouse, RenderTexture WriteTarget, IOriginTexture originTexture)
        {
            var mat = new Material(SpecialLayerShaders.SelectiveColorAdjustment);

            mat.SetVector("_RedsCMYK", RedsCMYK);
            mat.SetVector("_YellowsCMYK", YellowsCMYK);
            mat.SetVector("_GreensCMYK", GreensCMYK);
            mat.SetVector("_CyansCMYK", CyansCMYK);
            mat.SetVector("_BluesCMYK", BluesCMYK);
            mat.SetVector("_MagentasCMYK", MagentasCMYK);
            mat.SetVector("_WhitesCMYK", WhitesCMYK);
            mat.SetVector("_NeutralsCMYK", NeutralsCMYK);
            mat.SetVector("_BlacksCMYK", BlacksCMYK);

            mat.SetFloat("_IsAbsolute", IsAbsolute ? 1f : 0f);

            Graphics.Blit(GrabSouse, WriteTarget, mat);
            UnityEngine.Object.DestroyImmediate(mat);
        }
    }
}
