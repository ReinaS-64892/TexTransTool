#nullable enable
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
using Vector4 = UnityEngine.Vector4;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class SelectiveColoringAdjustmentLayer : AbstractLayer
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
        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.Observe(this);
            domain.Observe(gameObject);

            var lm = GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);
            var sca = new SelectiveColorAdjustment(
                RedsCMYK.ToSysNum()
                , YellowsCMYK.ToSysNum()
                , GreensCMYK.ToSysNum()
                , CyansCMYK.ToSysNum()
                , BluesCMYK.ToSysNum()
                , MagentasCMYK.ToSysNum()
                , WhitesCMYK.ToSysNum()
                , NeutralsCMYK.ToSysNum()
                , BlacksCMYK.ToSysNum()
                , IsAbsolute
                );

            return new GrabBlendingAsLayer<ITexTransToolForUnity>(Visible, lm, Clipping, blKey, sca);
        }
    }
}
