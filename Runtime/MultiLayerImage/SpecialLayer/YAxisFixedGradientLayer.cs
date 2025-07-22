#nullable enable
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCore;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class YAxisFixedGradientLayer : AbstractImageLayer
    {
        internal const string ComponentName = "TTT YAxisFixedGradientLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public Gradient Gradient = new();
        public override void GetImage(ITexTransToolForUnity engine, ITTRenderTexture writeTarget)
        {
            var uniRt = TTRt2.Get(writeTarget.Width, writeTarget.Hight);
            try
            {
                var mat = ShaderTempMaterialManager.GetOrCreateTempMaterial(YAxisFixedGradientShader);
                Graphics.Blit(GradientTempTexture.Get(Gradient, 1), uniRt, mat);

                using var rt = engine.UploadTexture(uniRt);
                engine.CopyRenderTexture(writeTarget, rt);
            }
            finally { TTRt2.Rel(uniRt); }
        }

        [TexTransInitialize]
        public static void Init() { YAxisFixedGradientShader = Shader.Find(Y_AXIS_FIXED_GRADIENT); }


        public const string Y_AXIS_FIXED_GRADIENT = "Hidden/YAxisFixedGradient";
        public static Shader YAxisFixedGradientShader = null!;
    }
}
