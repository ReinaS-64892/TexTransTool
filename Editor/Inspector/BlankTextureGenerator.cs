using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor
{

    public class BlankTextureGenerator
    {

        [MenuItem("Assets/Create/TexTransTool/BlankTexture/2048")]
        public static void CreateBlankTexture2048()
        {
            GenerateAndSaveBlankTexture(2048);
        }
        [MenuItem("Assets/Create/TexTransTool/BlankTexture/1024")]
        public static void CreateBlankTexture1024()
        {
            GenerateAndSaveBlankTexture(1024);
        }
        [MenuItem("Assets/Create/TexTransTool/BlankTexture/512")]
        public static void CreateBlankTexture512()
        {
            GenerateAndSaveBlankTexture(512);
        }
        private static void GenerateAndSaveBlankTexture(int size)
        {
            var tex = CoreUtility.CreateFillTexture(size, new Color(0, 0, 0, 0));
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/BlankTexture2K.png");
            var pngByte = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngByte);
            AssetDatabase.ImportAsset(path);
        }
    }
}