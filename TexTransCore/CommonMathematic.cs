#nullable enable
using System.Runtime.InteropServices;

namespace net.rs64.TexTransCore
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ColorWOAlpha
    {
        [FieldOffset(0)]
        public float R;
        [FieldOffset(4)]
        public float G;
        [FieldOffset(8)]
        public float B;
        public float[] ToArray() { return new float[] { R, G, B }; }
    }
    /// <summary>
    /// ガンマ色空間の色を表現する
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Color
    {
        [FieldOffset(0)]
        public float R;
        [FieldOffset(4)]
        public float G;
        [FieldOffset(8)]
        public float B;
        [FieldOffset(12)]
        public float A;

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color Zero => new Color(0, 0, 0, 0);

        public float[] ToArray() { return new float[] { R, G, B, A }; }

        public override string ToString()
        {
            return base.ToString() + $"{R}-{G}-{B}-{A}";
        }
    }


    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class Range : System.Attribute
    {
        public float Min;
        public float Max;
        public Range(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
