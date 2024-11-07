using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity.Utils;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class YAsixFixedGradientLayer : AbstractImageLayer
    {
        internal const string ComponentName = "TTT YAsixFixedGradientLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public Gradient Gradient = new();
        public override void GetImage<TTCE4U>(TTCE4U engine, ITTRenderTexture renderTexture)
        {
            var mat = MatTemp.GetTempMatShader(YAxisFixedGradientShader);
            using (TTRt.U(out var urt, renderTexture.Width, renderTexture.Hight))
            {
                Graphics.Blit(GradientTempTexture.Get(Gradient, 1), urt, mat);

                using (var rt = engine.UploadTexture(urt))
                    engine.CopyRenderTexture(rt, renderTexture);
            }
        }

        [TexTransInitialize]
        public static void Init() { YAxisFixedGradientShader = Shader.Find(Y_AXIS_FIXED_GRADIENT); }


        public const string Y_AXIS_FIXED_GRADIENT = "Hidden/YAxisFixedGradient";
        public static Shader YAxisFixedGradientShader;
    }
}
