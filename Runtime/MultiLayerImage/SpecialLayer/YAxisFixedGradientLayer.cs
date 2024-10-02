using net.rs64.TexTransUnityCore.BlendTexture;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
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
            var mat = MatTemp.GetTempMatShader(SpecialLayerShaders.YAxisFixedGradientShader);
            Graphics.Blit(GradientTempTexture.Get(Gradient, 1), renderTexture, mat);
        }
    }
}
