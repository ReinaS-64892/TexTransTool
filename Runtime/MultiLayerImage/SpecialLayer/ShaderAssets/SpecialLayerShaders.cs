using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    internal static class SpecialLayerShaders
    {
        public static void Init()
        {
            HSLAdjustmentShader = Shader.Find(HSL_ADJUSTMENT_SHADER);
            LevelAdjustmentShader = Shader.Find(LEVEL_ADJUSTMENT_SHADER);
            SelectiveColorAdjustment = Shader.Find(SELECTIVE_COLOR_ADJUSTMENT_SHADER);
        }

        public const string HSL_ADJUSTMENT_SHADER = "Hidden/HSLAdjustment";
        public static Shader HSLAdjustmentShader;

        public const string LEVEL_ADJUSTMENT_SHADER = "Hidden/LevelAdjustment";
        public static Shader LevelAdjustmentShader;

        public const string SELECTIVE_COLOR_ADJUSTMENT_SHADER = "Hidden/SelectiveColorAdjustment";
        public static Shader SelectiveColorAdjustment;


    }
}
