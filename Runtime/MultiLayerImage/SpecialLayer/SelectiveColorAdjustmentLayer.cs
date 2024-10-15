using System;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreForUnity;
using UnityEngine;
using Vector4 = UnityEngine.Vector4;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class SelectiveColoringAdjustmentLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT SelectiveColoringAdjustmentLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
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
        internal override LayerObject GetLayerObject(TexTransCore.ITexTransToolEngine engin, ITextureManager textureManager)
        {
            var selective = new SelectiveColorAdjustment(engin.QueryComputeKey(nameof(SelectiveColorAdjustment)), RedsCMYK.ToTTCore(), YellowsCMYK.ToTTCore(), GreensCMYK.ToTTCore(), CyansCMYK.ToTTCore(), BluesCMYK.ToTTCore(), MagentasCMYK.ToTTCore(), WhitesCMYK.ToTTCore(), NeutralsCMYK.ToTTCore(), BlacksCMYK.ToTTCore(), IsAbsolute);
            return new GrabBlendingAsLayer(Visible, GetAlphaMask(textureManager), Clipping, engin.QueryBlendKey(BlendTypeKey), selective);
        }
        public override void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture)
        {
            throw new NotSupportedException();
            // var mat = MatTemp.GetTempMatShader(SpecialLayerShaders.SelectiveColorAdjustment);

            // mat.SetVector("_RedsCMYK", RedsCMYK);
            // mat.SetVector("_YellowsCMYK", YellowsCMYK);
            // mat.SetVector("_GreensCMYK", GreensCMYK);
            // mat.SetVector("_CyansCMYK", CyansCMYK);
            // mat.SetVector("_BluesCMYK", BluesCMYK);
            // mat.SetVector("_MagentasCMYK", MagentasCMYK);
            // mat.SetVector("_WhitesCMYK", WhitesCMYK);
            // mat.SetVector("_NeutralsCMYK", NeutralsCMYK);
            // mat.SetVector("_BlacksCMYK", BlacksCMYK);

            // mat.SetFloat("_IsAbsolute", IsAbsolute ? 1f : 0f);

            // Graphics.Blit(grabSource, writeTarget, mat);
        }
    }
}
