#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor
{

    public class BlankTextureGenereater
    {

        [MenuItem("Assets/Create/TexTransTool/BlankTexture/2048")]
        public static void CreateBlankTexture2048()
        {
            GeneretAndSaveBlanTexture(2048);
        }
        [MenuItem("Assets/Create/TexTransTool/BlankTexture/1024")]
        public static void CreateBlankTexture1024()
        {
            GeneretAndSaveBlanTexture(1024);
        }
        [MenuItem("Assets/Create/TexTransTool/BlankTexture/512")]
        public static void CreateBlankTexture512()
        {
            GeneretAndSaveBlanTexture(512);
        }
        private static void GeneretAndSaveBlanTexture(int size)
        {
            var tex = Utils.CreateFillTexture(size, new Color(0, 0, 0, 0));
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/BlankTexture2K.png");
            var pngbyte = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngbyte);
            AssetDatabase.ImportAsset(path);
        }
    }
}
#endif