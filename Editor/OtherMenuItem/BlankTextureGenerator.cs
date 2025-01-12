using net.rs64.TexTransTool.Utils;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{

    internal class BlankTextureGenerator
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
            var tex = CreateFillTexture(size, new Color(0, 0, 0, 0));
            var path = AssetDatabase.GenerateUniqueAssetPath($"Assets/BlankTexture-{size}.png");
            var pngByte = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngByte);
            AssetDatabase.ImportAsset(path);
        }
        public static Texture2D CreateFillTexture(int size, Color fillColor)
        {
            var newTex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            var na = new NativeArray<Color32>(size * size, Allocator.Temp);
            na.AsSpan().Fill(fillColor);
            newTex.SetPixelData(na, 0);
            return newTex;
        }
    }
}
