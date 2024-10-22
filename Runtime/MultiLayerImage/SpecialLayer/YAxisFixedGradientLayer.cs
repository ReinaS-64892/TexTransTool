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
        public override void GetImage<TTT4U>(TTT4U engine, ITTRenderTexture renderTexture)
        {
            var mat = MatTemp.GetTempMatShader(YAxisFixedGradientShader);
            using (TTRt.U(out var urt, renderTexture.Width, renderTexture.Hight))
            {
                Graphics.Blit(GradientTempTexture.Get(Gradient, 1), urt, mat);
                var tex2D = urt.CopyTexture2D();
                var data = tex2D.GetPixelData<byte>(0);//後々ここら辺はコアに持っていく必要がある
                using (var rt = engine.UploadTexture(tex2D.width, tex2D.height, TTCE4UnityWithTTT4Unity.ToTTCTextureFormat(tex2D.format), false, data))
                    engine.CopyRenderTexture(rt, renderTexture);

                DestroyImmediate(tex2D);
            }
        }

        [TexTransInitialize]
        public static void Init() { YAxisFixedGradientShader = Shader.Find(Y_AXIS_FIXED_GRADIENT); }


        public const string Y_AXIS_FIXED_GRADIENT = "Hidden/YAxisFixedGradient";
        public static Shader YAxisFixedGradientShader;
    }
}
