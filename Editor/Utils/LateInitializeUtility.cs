using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
namespace net.rs64.TexTransTool.Utils
{
    internal static class LateInitializeUtility
    {
        [UnityEditor.InitializeOnLoadMethod]
        static void EditorInitDerayCall()
        {
            UnityEditor.EditorApplication.delayCall += EditorInitDerayCaller;
        }
        static void EditorInitDerayCaller()
        {
            Initializer();
            UnityEditor.EditorApplication.delayCall -= EditorInitDerayCaller;
        }
        [UnityEditor.InitializeOnEnterPlayMode]
        public static void Initializer()
        {
            TTTConfig.SettingInitializer();
            Localize.LocalizeInitializer();
            TextureBlend.BlendShadersInit();
            PSDImportedRasterImage.MargeColorAndOffsetShader = Shader.Find(PSDImportedRasterImage.MARGE_COLOR_AND_OFFSET_SHADER);
        }
    }
}