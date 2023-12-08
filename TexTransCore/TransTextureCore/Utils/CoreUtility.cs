using System;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    internal static class CoreUtility
    {
        public static T[] FilledArray<T>(T DefaultValue, int Length)
        {
            var array = new T[Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = DefaultValue;
            }
            return array;
        }
        public static Texture2D CreateFillTexture(int Size, Color FillColor)
        {
            return CreateFillTexture(new Vector2Int(Size, Size), FillColor);
        }
        public static Texture2D CreateFillTexture(Vector2Int Size, Color FillColor)
        {
            var TestTex = new Texture2D(Size.x, Size.y);
            TestTex.SetPixels(FilledArray(FillColor, Size.x * Size.y));
            return TestTex;
        }
    }
}