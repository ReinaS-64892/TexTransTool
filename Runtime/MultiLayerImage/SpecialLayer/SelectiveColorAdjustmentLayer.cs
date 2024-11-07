using System;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
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
        internal override LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engin)
        {
            return new GrabBlendingAsLayer<TTCE4U>(Visible, GetAlphaMask(engin), Clipping, engin.QueryBlendKey(BlendTypeKey), new SelectiveColorAdjustment(RedsCMYK.ToTTCore(), YellowsCMYK.ToTTCore(), GreensCMYK.ToTTCore(), CyansCMYK.ToTTCore(), BluesCMYK.ToTTCore(), MagentasCMYK.ToTTCore(), WhitesCMYK.ToTTCore(), NeutralsCMYK.ToTTCore(), BlacksCMYK.ToTTCore(), IsAbsolute));
        }
    }
}
