using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCoreEngineForUnity;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class YAsixFixedGradientLayer : AbstractImageLayer
    {
        internal const string ComponentName = "TTT YAsixFixedGradientLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public Gradient Gradient = new();
        public override void GetImage(RenderTexture renderTexture, IOriginTexture originTexture)
        {
            var mat = MatTemp.GetTempMatShader(YAxisFixedGradientShader);
            Graphics.Blit(GradientTempTexture.Get(Gradient, 1), renderTexture, mat);
        }


        [TexTransInitialize]
        public static void Init() { YAxisFixedGradientShader = Shader.Find(Y_AXIS_FIXED_GRADIENT); }
        public const string Y_AXIS_FIXED_GRADIENT = "Hidden/YAxisFixedGradient";
        public static Shader YAxisFixedGradientShader;
    }
}
